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
using System.Threading;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;

namespace PhoneApp1
{
    public partial class MainPage : PhoneApplicationPage
    {
        ReadmillClient client;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            booksList.Items.Add(new ListBoxItem()
            {
                Content = new TextBlock() 
                { TextAlignment = System.Windows.TextAlignment.Center, 
                    FontSize = 26, Padding = new Thickness(20), Text = "loading books..." }
            });

            client = new ReadmillClient("3f2116709bb1f330084b9cd9f1045961");
            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            IDictionary<string, Book> readableBooks = new Dictionary<string, Book>();

            BooksQueryOptions booksOptions = new BooksQueryOptions() { CountValue = 100 };
            ReadingsQueryOptions readingsOptions = new ReadingsQueryOptions() { CountValue = 100 };

            //Task<List<Book>> booksTask = client.Books.GetBooksAsync(booksOptions);
            Task<List<Reading>> readingsTask = client.Readings.GetReadingsAsync(readingsOptions);

            readingsTask.ContinueWith(task =>
            {
                foreach (Reading r in task.Result)
                {
                   if(!readableBooks.ContainsKey(r.Book.Id))
                    readableBooks.Add(r.Book.Id, r.Book);
                }

                booksList.Items.RemoveAt(0);
                booksList.ItemsSource = readableBooks.Values; //filterTask.Result;               
            }, uiTaskScheduler);

        }

        private void booksList_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void booksList_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }

        private void booksList_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Book selectedBook = (Book)booksList.SelectedItem;

            if (PhoneApplicationService.Current.State.ContainsKey("SelectedBook"))
                PhoneApplicationService.Current.State.Remove("SelectedBook");

            PhoneApplicationService.Current.State.Add("SelectedBook", selectedBook);

            NavigationService.Navigate(new Uri("/BookDetailsPage.xaml", UriKind.Relative));
        }

        private void searchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            { 
                TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

                //IDictionary<string, Book> readableBooks = new Dictionary<string, Book>();

                BooksQueryOptions booksOptions = new BooksQueryOptions() { SearchStringValue = searchBox.Text, CountValue = 100 };
                //ReadingsQueryOptions readingsOptions = new ReadingsQueryOptions() { CountValue = 100 };

                Task<List<Book>> booksTask = client.Books.GetBooksAsync(booksOptions);
                //Task<List<Reading>> readingsTask = client.Readings.GetReadingsAsync(readingsOptions);

                booksTask.ContinueWith(task =>
                {
                    //booksList.Items.RemoveAt(0);
                    booksList.ItemsSource = task.Result; //readableBooks.Values; //filterTask.Result;
                    booksList.Focus();

                }, uiTaskScheduler);
            }
        }
    }
}