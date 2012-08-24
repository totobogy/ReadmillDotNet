﻿using System;
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
using PhoneApp1.ViewModels;
using System.Runtime.Serialization.Json;
using System.IO.IsolatedStorage;
using System.IO;
using System.Threading.Tasks;
using Com.Readmill.Api.DataContracts;
using Microsoft.Phone.Shell;

namespace PhoneApp1.Views
{
    public partial class Home : PhoneApplicationPage
    {
        BookListViewModel bookListVM;
        CollectionsViewModel collectionsVM;

        public Home()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(HomePage_Loaded);
            this.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(HomePage_BackKeyPress);

            //If the token is not found, we route to the login-page on load
            //TryInitializeAccessToken();

            bookListVM = new BookListViewModel();
            booksPanoramaItem.DataContext = bookListVM;

            collectionsVM = new CollectionsViewModel();
            collectionsPanoramaItem.DataContext = collectionsVM;
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            //If this is the first time the app is being run, show log-in screen
            if (!AppContext.IsConnected)
                MessageBox.Show(AppStrings.NotConnectedMsg, AppStrings.NotConnectedMsgTitle, MessageBoxButton.OK);

            if (AppContext.AccessToken == null)
                NavigationService.Navigate(new Uri("/Views/LogInPage.xaml", UriKind.Relative));
        }

        void HomePage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
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
                    if (booksProgressBar.Visibility == System.Windows.Visibility.Visible)
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

            NavigationService.Navigate(new Uri("/Views/BookDetailsPage.xaml", UriKind.Relative));
        }

        private void searchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

                booksList.ItemsSource = null;
                booksList.InvalidateArrange();

                bookListVM.SearchBooksAsync(searchBox.Text).ContinueWith(task =>
                {
                    booksList.ItemsSource = bookListVM.BookList;
                    booksList.Focus();
                }, uiTaskScheduler);
            }
        }

        private void collectionsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //Load Books and Tiles
            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            
            collectionsVM.LoadCollectedBooksAsync().ContinueWith(task =>
            {
                if (collectionsVM.CollectedBooks != null && collectionsVM.CollectedBooks.Count > 0)
                {
                    bookTile1.DataContext = collectionsVM.CollectedBooks[0];
                    bookTile1.IsEnabled = true;
                    if (bookTile1.IsFrozen)
                        HubTileService.UnfreezeHubTile(bookTile1);

                    if (collectionsVM.CollectedBooks.Count > 1)
                    {
                        bookTile2.DataContext = collectionsVM.CollectedBooks[1];
                        bookTile2.IsEnabled = true;
                        if (bookTile2.IsFrozen)
                            HubTileService.UnfreezeHubTile(bookTile2);
                    }
                }
                else
                {
                    //Empty list handling - good for now?
                    bookTile1.Message = AppStrings.NoCollectedBooksMsg;
                    bookTile1.IsEnabled = false;
                    HubTileService.FreezeHubTile(bookTile1);
                    
                    bookTile2.Message = AppStrings.NoCollectedBooksMsg;
                    bookTile2.IsEnabled = false;
                    HubTileService.FreezeHubTile(bookTile2);
                }
            }, uiTaskScheduler);

            //Load Highlights
        }

        private void bookTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            HubTile bookTile = sender as HubTile;
            Book selectedBook = bookTile.DataContext as Book;

            //ToDo: Do we need to synchronize access to Current.State?
            if (PhoneApplicationService.Current.State.ContainsKey("SelectedBook"))
                PhoneApplicationService.Current.State.Remove("SelectedBook");

            PhoneApplicationService.Current.State.Add("SelectedBook", selectedBook);

            NavigationService.Navigate(new Uri("/Views/BookDetailsPage.xaml", UriKind.Relative));
        }
    }
}