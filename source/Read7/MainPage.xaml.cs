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
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization.Json;

namespace PhoneApp1
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ReadmillClient client;
        private DataContractJsonSerializer ser;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            //Show progress bar
            booksProgressBar.IsIndeterminate = true;
            booksProgressBar.Visibility = System.Windows.Visibility.Visible;

            //try loading user access token
            //ToDo: Handle when the token is not valid - this will only be detected at API call time
            ser = new DataContractJsonSerializer(typeof(AccessToken));
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    using (var stream = new
                                        IsolatedStorageFileStream("token.ser",
                                                                    FileMode.Open,
                                                                    FileAccess.Read,
                                                                    store))
                    {
                        AppConstants.Token = (AccessToken)ser.ReadObject(stream);
                    }
                }
                catch (IsolatedStorageException ex)
                {
                    //no-op: we'll ask for authorization when page loads
                }
            }
            

            client = new ReadmillClient(AppConstants.ClientId);
            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            IDictionary<string, Book> readableBooks = new Dictionary<string, Book>();

            BooksQueryOptions booksOptions = new BooksQueryOptions() { CountValue = 100 };
            ReadingsQueryOptions readingsOptions = new ReadingsQueryOptions() { CountValue = 100 };

            //Task<List<Book>> booksTask = client.Books.GetBooksAsync(booksOptions);
            Task<List<Reading>> readingsTask = client.Readings.GetReadingsAsync(readingsOptions);

            //Task<List<Highlight>> getTask = client.Highlights.GetHighlightsAsync();

            readingsTask.ContinueWith(task =>
            {
                foreach (Reading r in task.Result)
                {
                    if (!readableBooks.ContainsKey(r.Book.Id))
                        readableBooks.Add(r.Book.Id, r.Book);
                }

                //hide progress-bar
                booksProgressBar.Visibility = System.Windows.Visibility.Collapsed;

                booksList.ItemsSource = readableBooks.Values;

            }, uiTaskScheduler);

        }
        
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {            
            //If this is the first time the app is being run, show log-in screen
            if (AppConstants.Token == null)
                NavigationService.Navigate(new Uri("/LogInPage.xaml", UriKind.Relative));
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
            //NavigationService.Navigate(new Uri("/LogInPage.xaml", UriKind.Relative));
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