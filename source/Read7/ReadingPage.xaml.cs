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
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Book book = (Book)PhoneApplicationService.Current.State["SelectedBook"];
            bookTitle.Text = book.Title.ToLower() + "\nby " + book.Author.ToLower();
            
            highlightsListBox.Items.Add(new ListBoxItem()
            {
                Content = new TextBlock()
                {
                    TextWrapping = System.Windows.TextWrapping.Wrap, //TextAlignment = System.Windows.TextAlignment.Center,
                    FontSize = 24,
                    Padding = new Thickness(10,30,10,10),
                    Text = "loading book from readmill..."
                }
            });

            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            ReadmillClient client = new ReadmillClient("3f2116709bb1f330084b9cd9f1045961");

            IDictionary<decimal, Highlight> highlights = new Dictionary<decimal, Highlight>();

            ReadingsQueryOptions readingOptions = new ReadingsQueryOptions() { CountValue = 100 };
            HighlightsQueryOptions highlightOptions = new HighlightsQueryOptions() { CountValue = 100 };

            //Get all readings
            Task.Factory.StartNew(() =>
                {
                    List<Reading> readings = client.Books.GetBookReadingsAsync(book.Id, readingOptions).Result;

                    foreach (Reading reading in readings)
                    {
                        //Foreach reading, Get all Highlights

                        foreach (Highlight h in client.Readings.GetReadingHighlightsAsync(reading.Id, highlightOptions).Result)
                        {
                            if (!highlights.ContainsKey(h.Locators.Position))
                                highlights.Add(h.Locators.Position, h);
                        }
                    }

                    Task.Factory.StartNew(() =>
                    {
                        List<Highlight> h = highlights.Values.ToList<Highlight>();
                        highlightsListBox.Items.RemoveAt(0);

                        if (h.Count <= 0)
                        {
                            highlightsListBox.Items.Add(new ListBoxItem()
                            {
                                Content = new TextBlock()
                                {
                                    TextWrapping = System.Windows.TextWrapping.Wrap, //TextAlignment = System.Windows.TextAlignment.Center,
                                    FontSize = 24,
                                    Padding = new Thickness(10,30,10,10),
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
    }
}