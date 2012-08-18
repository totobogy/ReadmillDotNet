using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Com.Readmill.Api;
using Com.Readmill.Api.DataContracts;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using System.Threading;

namespace PhoneApp1
{
    public partial class ReadingPage : PhoneApplicationPage
    {
        public ReadingPage()
        {
            InitializeComponent();

            var gl = GestureService.GetGestureListener(this);
            gl.Flick += new EventHandler<Microsoft.Phone.Controls.FlickGestureEventArgs>(gl_Flick);

            Book book = (Book)PhoneApplicationService.Current.State["SelectedBook"];
            bookTitlePanel.DataContext = book;
            bookTitle.Text = book.Title + "\nby " + book.Author;

            //show progress bar
            highlightsProgressBar.IsIndeterminate = true;
            highlightsProgressBar.Visibility = System.Windows.Visibility.Visible;

            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            ReadmillClient client = new ReadmillClient(AppConstants.ClientId);
            IDictionary<decimal, Highlight> highlights = new Dictionary<decimal, Highlight>();
            ReadingsQueryOptions readingOptions = new ReadingsQueryOptions() { CountValue = 100 };
            RangeQueryOptions highlightOptions = new RangeQueryOptions() { CountValue = 100 };

            //Get all readings
            Task.Factory.StartNew(() =>
            {
                List<Reading> readings = client.Books.GetBookReadingsAsync(book.Id, readingOptions).Result;

                foreach (Reading reading in readings)
                {
                    //foreach reading, Get all Highlights
                    foreach (Highlight h in client.Readings.GetReadingHighlightsAsync(reading.Id, highlightOptions).Result)
                    {
                        if (!highlights.ContainsKey(h.Locators.Position))
                            highlights.Add(h.Locators.Position, h);
                    }
                }

                Task.Factory.StartNew(() =>
                {
                    List<Highlight> h = highlights.Values.ToList<Highlight>();
                    
                    //hide progress bar
                    highlightsProgressBar.Visibility = System.Windows.Visibility.Collapsed;

                    if (h.Count <= 0)
                    {
                        highlightsListBox.Items.Add(new ListBoxItem()
                        {
                            Content = new TextBlock()
                            {
                                TextWrapping = System.Windows.TextWrapping.Wrap, //TextAlignment = System.Windows.TextAlignment.Center,
                                FontSize = 24,
                                Padding = new Thickness(10, 30, 10, 10),
                                Text = "sorry, no one seems to have highlighted this book yet."
                            }
                        });
                    }
                    else
                    {
                        highlightsListBox.ItemsSource = from highlight in h
                                                        orderby highlight.Position
                                                        select highlight;
                    }
                    
                }, CancellationToken.None, TaskCreationOptions.None, uiTaskScheduler);
            });           
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);            
        }

        private void gl_Flick(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).Parent != ContentPanel
                && ((FrameworkElement)e.OriginalSource).Parent != TitlePanel)
                return;

            NavigationService.Navigate(new Uri("/MyReadingsPage.xaml", UriKind.Relative));

        }

        private void Like_Click(object sender, RoutedEventArgs e)
        {
            Button likeButton = (Button)sender;
            likeButton.Content = "unlike";

            string highlightId = (string) likeButton.Tag;
        }
    }
}