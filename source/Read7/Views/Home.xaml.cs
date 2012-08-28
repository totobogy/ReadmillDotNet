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
using PhoneApp1.ViewModels;
using System.Runtime.Serialization.Json;
using System.IO.IsolatedStorage;
using System.IO;
using System.Threading.Tasks;
using Com.Readmill.Api.DataContracts;
using Microsoft.Phone.Shell;
using System.Threading;

namespace PhoneApp1.Views
{
    public partial class Home : PhoneApplicationPage
    {
        /************
         * ToDo
         * **********
         * Timeouts for collections??
         * Timeout pop-up asks before cancelling??
         * Post cancelling, UI state / text etc.
         * First time exp - loading text etc.
         * Show collection control only after load. Flipped text on tiles
         * Make sure back is always available
         * Refresh (after timeout or otherwise)??
         * book and highlighter name in highlight
         * 
         * story not working?
         * search on separate page?
         * 
         * All books and All highlights pages
         * Latest Highlights
         * Settings (e.g. timeouts - not needed if refresh is available)?
         */

        BookListViewModel bookListVM;
        AutoResetEvent bookListLoadTimeoutHandle;

        CollectionsViewModel collectionsVM;

        private bool viewModelsInvalidated;

        private CancellationTokenSource cancel;

        public Home()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(HomePage_Loaded);
            this.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(HomePage_BackKeyPress);

            viewModelsInvalidated = true;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                
            }
            else
            {
                State["BooksViewModel"] = bookListVM;
                State["CollectionsViewModel"] = collectionsVM;
            }

            bookListLoadTimeoutHandle.Dispose();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string navigatedUri = e.Uri.ToString();
            if (navigatedUri.Contains("from_login=true"))
            {
                //Remove login page(s) from back-stack - home should be the first page
                while(NavigationService.CanGoBack)
                    NavigationService.RemoveBackEntry();
            }

            //collectionsGrid.Visibility = System.Windows.Visibility.Collapsed;
            if (viewModelsInvalidated)
            {
                if (State.ContainsKey("BooksViewModel"))
                    bookListVM = State["BooksViewModel"] as BookListViewModel;
                else
                    bookListVM = new BookListViewModel();

                booksPanoramaItem.DataContext = bookListVM;

                if (State.ContainsKey("CollectionsViewModel"))
                    collectionsVM = State["CollectionsViewModel"] as CollectionsViewModel;
                else
                    collectionsVM = new CollectionsViewModel();

                collectionsPanoramaItem.DataContext = collectionsVM;

                cancel = new CancellationTokenSource();

                viewModelsInvalidated = false;
            }

            bookListLoadTimeoutHandle = new AutoResetEvent(false);
        }

        void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        void HomePage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //If we are returning from a search AND the user is on search page
            if (bookListVM.ListState == BookListViewModel.State.SearchResult
                && panoramaRoot.SelectedItem == booksPanoramaItem)
            {
                e.Cancel = true;

                //Load RecentlyRead instead
                booksPanoramaItem.Header = AppStrings.RecentlyReadPageTitle;
                searchBox.Text = string.Empty;

                //Show progress bar if the list is empty
                if (bookListVM.ListState == BookListViewModel.State.Unloaded)
                {
                    booksProgressBar.IsIndeterminate = true;
                    booksProgressBar.Visibility = System.Windows.Visibility.Visible;
                }

                TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                bookListVM.LoadRecentlyReadBooksAsync(cancel.Token).ContinueWith(displayList =>
                {
                    //hide progress-bar
                    if (booksProgressBar.Visibility == System.Windows.Visibility.Visible)
                        booksProgressBar.Visibility = System.Windows.Visibility.Collapsed;

                    booksList.ItemsSource = bookListVM.BookList;
                }, uiTaskScheduler);
            }
            else
            {
                try
                {
                    if (!cancel.IsCancellationRequested)
                        cancel.Cancel();
                }
                catch (OperationCanceledException ex)
                {
                    //no op
                }
                catch (AggregateException ex)
                {
                    ex.Handle(err =>
                    {
                        return (err is OperationCanceledException || err is TaskCanceledException);
                    });
                }
            }
        }

        /*bool ShowTimeoutMessage()
        {
            this.Dispatcher.BeginInvoke(
               () =>
               {
                   MessageBoxResult res = MessageBox.Show(
                       AppStrings.TimeoutMsg,
                       AppStrings.TimeoutMsgTitle,
                       MessageBoxButton.OKCancel);
                   if (!(res == MessageBoxResult.OK))
                       return true;
                   else
                       return false;
               });
        }*/

        private void booksList_Loaded(object sender, RoutedEventArgs e)
        {
            if (bookListVM.ListState == BookListViewModel.State.Unloaded)
            {
                //bar interactions till the list has actually loaded
                booksList.IsEnabled = false;

                //Show progress bar
                booksProgressBar.IsIndeterminate = true;
                booksProgressBar.Visibility = System.Windows.Visibility.Visible;

                //Timer
                ThreadPool.RegisterWaitForSingleObject(
                    bookListLoadTimeoutHandle,
                    new WaitOrTimerCallback(
                        (cancelToken, timedOut) =>
                        {
                            if (timedOut)
                            {
                                //ShowTimeoutMessage();
                            }
                        }),
                    cancel,
                    TimeSpan.FromSeconds(15),
                    true);


                TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                bookListVM.LoadRecentlyReadBooksAsync(cancel.Token).ContinueWith(displayList =>
                {
                    //signal the event - we don't want to initiate a timeout cancellation now
                    bookListLoadTimeoutHandle.Set();

                    //hide progress-bar
                    if (booksProgressBar.Visibility == System.Windows.Visibility.Visible)
                        booksProgressBar.Visibility = System.Windows.Visibility.Collapsed;

                    if (!displayList.IsCanceled && !displayList.IsFaulted)
                    {
                        booksList.ItemsSource = bookListVM.BookList;
                        booksList.IsEnabled = true;
                    }
                    else
                    {
                        //Error
                    }

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

                booksPanoramaItem.Header = AppStrings.SearchPageTitle;

                booksList.ItemsSource = null;
                booksList.InvalidateArrange();

                bookListVM.SearchBooksAsync(searchBox.Text, cancel.Token).ContinueWith(task =>
                {
                    //searchBox.Text = searchBox.Hint;
                    booksList.ItemsSource = bookListVM.BookList;
                    booksList.Focus();

                }, uiTaskScheduler);
            }
        }

        private void RefreshBookTiles()
        {
            if (collectionsVM.CollectedBooks != null && collectionsVM.CollectedBooks.Count > 0)
            {
                Random randomGen = new Random();

                int index1 = randomGen.Next(0, collectionsVM.CollectedBooks.Count);
                int index2 = randomGen.Next(0, collectionsVM.CollectedBooks.Count);
                while (index1 == index2)
                {
                    index2 = randomGen.Next(0, collectionsVM.CollectedBooks.Count);
                }

                bookTile1.DataContext = collectionsVM.CollectedBooks[index1];
                bookTile1.IsEnabled = true;
                if (bookTile1.IsFrozen)
                    HubTileService.UnfreezeHubTile(bookTile1);

                if (collectionsVM.CollectedBooks.Count > 1)
                {                    
                    bookTile2.DataContext = collectionsVM.CollectedBooks[index2];
                    bookTile2.IsEnabled = true;
                    if (bookTile2.IsFrozen)
                        HubTileService.UnfreezeHubTile(bookTile2);
                }
                else
                {
                    bookTile2.Message = AppStrings.NoCollectedBooksMsg;
                    bookTile2.IsEnabled = false;
                    //HubTileService.FreezeHubTile(bookTile2);
                }
            }
            else
            {
                //Empty list handling - good for now?
                bookTile1.Message = AppStrings.NoCollectedBooksMsg;
                bookTile1.IsEnabled = false;
                //HubTileService.FreezeHubTile(bookTile1);

                bookTile2.Message = AppStrings.NoCollectedBooksMsg;
                bookTile2.IsEnabled = false;
                //HubTileService.FreezeHubTile(bookTile2);
            }
        }

        private void RefreshHighlightTile()
        {
            if (collectionsVM.CollectedHighlights != null && collectionsVM.CollectedHighlights.Count > 0)
            {
                Random randomGen = new Random();

                int i = randomGen.Next(0, collectionsVM.CollectedHighlights.Count);
                highlightTextBlock.Text = collectionsVM.CollectedHighlights[i].Content;
            }
            else
            {
                highlightTextBlock.Text = AppStrings.NoCollectedHighlights;
            }
        }

        private void InitializeCollectionsView()
        {
            bookTile1.IsEnabled = bookTile2.IsEnabled = false;
        }

        private void collectionsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //Initially everything is disabled
            InitializeCollectionsView();

            //Load Books and Tiles
            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            if (collectionsVM.BooksCollectionRefreshNeeded)
            {
                collectionsVM.LoadCollectedBooksAsync(true, cancel.Token).ContinueWith(task =>
                {
                    cancel.Token.Register(() =>
                        {
                            throw new OperationCanceledException(cancel.Token);
                        });

                    RefreshBookTiles();
                    
                }, cancel.Token, TaskContinuationOptions.OnlyOnRanToCompletion, uiTaskScheduler);
            }
            else
            {
                //just refresh view
                RefreshBookTiles();
            }

            if (collectionsVM.HighlightsCollectionRefreshNeeded)
            {
                //Load Highlights
                List<string> ids = AppContext.CurrentUser.TryLoadCollectedHighlightsList(true);
                if (ids == null)
                {
                    highlightTextBlock.Text = AppStrings.NoCollectedHighlights;
                }
                else
                {
                    collectionsVM.LoadCollectedHighlightsAsync(ids, true, cancel.Token).ContinueWith(
                        task =>
                        {
                            RefreshHighlightTile();

                        }, cancel.Token, TaskContinuationOptions.OnlyOnRanToCompletion, uiTaskScheduler);
                }
            }
            else
            {
                RefreshHighlightTile();
            }
        }

        private void bookTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            HubTile bookTile = sender as HubTile;
            Book selectedBook = bookTile.DataContext as Book;

            //this maybe because the tile wasn't ready
            if (selectedBook == null)
                return;

            //ToDo: Do we need to synchronize access to Current.State?
            if (PhoneApplicationService.Current.State.ContainsKey("SelectedBook"))
                PhoneApplicationService.Current.State.Remove("SelectedBook");

            PhoneApplicationService.Current.State.Add("SelectedBook", selectedBook);

            NavigationService.Navigate(new Uri("/Views/BookDetailsPage.xaml", UriKind.Relative));
        }

        private void booksList_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            //workaround for jumpy / jerky listbox scrolling - don't know the reason
            this.Focus();
        }
    }
}