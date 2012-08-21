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
using PhoneApp1.ViewModels;

namespace PhoneApp1
{
    public partial class MainPage : PhoneApplicationPage
    {
        BookListViewModel bookListVM;

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            this.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(MainPage_BackKeyPress);

            //If the token is not found, we route to the login-page on load
            TryInitializeAccessToken();
            
            bookListVM = new BookListViewModel();
            this.DataContext = bookListVM;                   
        }

        private void TryInitializeAccessToken()
        {
            //ToDo: Handle when the token is not valid - this will only be detected at API call time
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AccessToken));
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
        }
        
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //If this is the first time the app is being run, show log-in screen
            if (AppConstants.Token == null)
                NavigationService.Navigate(new Uri("/LogInPage.xaml", UriKind.Relative));
        }

        void MainPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //We shouldn't exit the page, except from RecentlyRead state
            if (bookListVM.ListState != BookListViewModel.State.RecentlyRead)
            {
                e.Cancel = true;

                //Load RecentlyRead instead

                //Show progress bar if the list is empty
                if (bookListVM.ListState == BookListViewModel.State.Unloaded)
                {
                    booksProgressBar.IsIndeterminate = true;
                    booksProgressBar.Visibility = System.Windows.Visibility.Visible;
                }

                TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                bookListVM.LoadRecentlyReadBooksAsync().ContinueWith(displayList =>
                {
                    //hide progress-bar
                    if (booksProgressBar.Visibility == System.Windows.Visibility.Visible)
                        booksProgressBar.Visibility = System.Windows.Visibility.Collapsed;

                    booksList.ItemsSource = bookListVM.BookList;
                }, uiTaskScheduler);   
            }
        }

        private void booksList_Loaded(object sender, RoutedEventArgs e)
        {
            if (bookListVM.ListState == BookListViewModel.State.Unloaded)
            {
                //Show progress bar
                booksProgressBar.IsIndeterminate = true;
                booksProgressBar.Visibility = System.Windows.Visibility.Visible; 

                TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                bookListVM.LoadRecentlyReadBooksAsync().ContinueWith(displayList =>
                {
                    //hide progress-bar
                    if(booksProgressBar.Visibility == System.Windows.Visibility.Visible)
                        booksProgressBar.Visibility = System.Windows.Visibility.Collapsed;

                    booksList.ItemsSource = bookListVM.BookList;
                }, uiTaskScheduler);   
            }
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

                bookListVM.SearchBooksAsync(searchBox.Text).ContinueWith(task =>
                {
                    booksList.ItemsSource = bookListVM.BookList;
                    booksList.Focus();
                }, uiTaskScheduler);
            }
        }
    }
}