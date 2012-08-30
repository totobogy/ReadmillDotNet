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
         * First time exp - loading text etc.
         * Flipped text on tiles
         * Refresh (after timeout or otherwise)??
         * All highlights pages
         * Latest Highlights
         * 
         * unlike, tap to book
         * list refreshes
         * empty lists - disable 'all'
         */

        BookListViewModel bookListVM;

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

            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back
                && !e.Uri.OriginalString.Contains("Error"))
            {
                //State["BooksViewModel"] = bookListVM;
                //State["CollectionsViewModel"] = collectionsVM;
            }

            //If we are navigating away, lets cancel any pending tasks since they 
            //can't be revived in a deterministic fashion.
            try
            {
                if (cancel!=null && !cancel.IsCancellationRequested)
                    cancel.Cancel();
            }
            catch (OperationCanceledException ex)
            {
                //no op
            }
            catch (AggregateException ex)
            {
                ex.Flatten().Handle(err =>
                {
                    return (err is OperationCanceledException || err is TaskCanceledException);
                });
            }
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

            if (viewModelsInvalidated)
            {
                //if (State.ContainsKey("BooksViewModel"))
                    //bookListVM = State["BooksViewModel"] as BookListViewModel;
                //else
                    bookListVM = new BookListViewModel();

                booksPanoramaItem.DataContext = bookListVM;

                //if (State.ContainsKey("CollectionsViewModel"))
                    //collectionsVM = State["CollectionsViewModel"] as CollectionsViewModel;
                //else
                    collectionsVM = new CollectionsViewModel();

                collectionsPanoramaItem.DataContext = collectionsVM;

                viewModelsInvalidated = false;
            }

            cancel = new CancellationTokenSource();

            //Load UI
            LoadBooksPanoramaItem();

            LoadCollectionsPanoramaItem();

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
                booksList.ItemsSource = null;
                booksList.InvalidateArrange();

                //show progress bar - move to VM?
                booksProgressBar.IsIndeterminate = true;
                booksProgressBar.Visibility = System.Windows.Visibility.Visible;

                booksPanoramaItem.Header = AppStrings.RecentlyReadPageTitle;

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
                    ex.Flatten().Handle(err =>
                    {
                        return (err is OperationCanceledException || err is TaskCanceledException);
                    });
                }
            }
        }

        /// <summary>
        /// Any heavy loading is done asynchronously
        /// </summary>
        private void LoadBooksPanoramaItem()
        {
            if (bookListVM.ListState == BookListViewModel.State.Unloaded)
            {
                //bar interactions till the list has actually loaded
                booksList.IsEnabled = false;

                //Show progress bar
                booksProgressBar.IsIndeterminate = true;
                booksProgressBar.Visibility = System.Windows.Visibility.Visible;

                TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                bookListVM.LoadRecentlyReadBooksAsync(cancel.Token).ContinueWith(displayList =>
                {
                    //signal the event - we don't want to initiate a timeout cancellation now
                    //bookListLoadTimeoutHandle.Set();

                    //if faulted, shd we not continue? or keep showing progress bar?

                    if (!displayList.IsCanceled && !displayList.IsFaulted)
                    {
                        //hide progress-bar
                        if (booksProgressBar.Visibility == System.Windows.Visibility.Visible)
                            booksProgressBar.Visibility = System.Windows.Visibility.Collapsed;

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

        private void booksList_Loaded(object sender, RoutedEventArgs e)
        {
            
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

                //show progress bar
                booksProgressBar.IsIndeterminate = true;
                booksProgressBar.Visibility = System.Windows.Visibility.Visible;

                string searchString = searchBox.Text;
                searchBox.Text = string.Empty;

                //set focus on an element other than the search box or list
                //not on list because when that is invalidated, focus returns to searchbox
                this.Focus();

                bookListVM.SearchBooksAsync(searchString, cancel.Token).ContinueWith(task =>
                {
                    //hide progress-bar
                    if (booksProgressBar.Visibility == System.Windows.Visibility.Visible)
                        booksProgressBar.Visibility = System.Windows.Visibility.Collapsed;

                    booksList.ItemsSource = bookListVM.BookList;

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

            ShowControlsIfCollectionsReady();
        }

        private void RefreshHighlightTile()
        {
            if (collectionsVM.CollectedHighlights != null && collectionsVM.CollectedHighlights.Count > 0)
            {
                Random randomGen = new Random();

                int i = randomGen.Next(0, collectionsVM.CollectedHighlights.Count);
                highlightTextBlock.Text = collectionsVM.CollectedHighlights[i].Content;
                highlightedBy.Text = "highlighted by: " + collectionsVM.CollectedHighlights[i].User.FullName;
                highlightedBy.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                highlightTextBlock.Text = AppStrings.NoCollectedHighlights;
                highlightedBy.Visibility = System.Windows.Visibility.Collapsed;
            }

            ShowControlsIfCollectionsReady();
        }

        private void HideCollectionsControls()
        {
            //book tiles
            bookTile1.IsEnabled = bookTile2.IsEnabled = false;
            bookTile1.Visibility = bookTile2.Visibility = System.Windows.Visibility.Collapsed;
            
            //highlight tile
            //highlightTile.Visibility = System.Windows.Visibility.Collapsed;
            highlightBorder.Visibility = System.Windows.Visibility.Collapsed;
            
            //links
            allBooks.Visibility = allHighlights.Visibility = System.Windows.Visibility.Collapsed;

            //show progress-bar
            collectionsProgressBar.IsIndeterminate = true;
            collectionsProgressBar.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Displays Collections Controls if the CollectionsVM indicates that both books
        /// and highlight collections are ready to be displayed
        /// </summary>
        /// <returns>true if controls were displayed, false otherwise</returns>
        private bool ShowControlsIfCollectionsReady()
        {
            if (collectionsVM.CollectionsReady)
            {
                //book tiles
                bookTile1.IsEnabled = bookTile2.IsEnabled = true;
                bookTile1.Visibility = bookTile2.Visibility = System.Windows.Visibility.Visible;

                //highlight tile
                //highlightTile.Visibility = System.Windows.Visibility.Visible;
                highlightBorder.Visibility = System.Windows.Visibility.Visible;

                //links
                allBooks.Visibility = allHighlights.Visibility = System.Windows.Visibility.Visible;

                //show progress-bar
                collectionsProgressBar.Visibility = System.Windows.Visibility.Collapsed;

                return true;
            }
            else
                return false;
        }

        private void collectionsGrid_Loaded(object sender, RoutedEventArgs e)
        {
                     
        }

        /// <summary>
        /// Any heavy loading is done asynchronously
        /// </summary>
        private void LoadCollectionsPanoramaItem()
        {
            //Initially everything is disabled
            HideCollectionsControls();

            LoadDisplayBooksCollection();

            LoadDisplayHighlightsCollection();   
        }

        /// <summary>
        /// Any heavy loading is done asynchronously
        /// </summary>
        private void LoadDisplayBooksCollection()
        {
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

                }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, uiTaskScheduler);
            }
            else
            {
                //just refresh view
                RefreshBookTiles();
            }
        }

        /// <summary>
        /// Any heavy loading is done asynchronously
        /// </summary>
        private void LoadDisplayHighlightsCollection()
        {
            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            if (collectionsVM.HighlightsCollectionRefreshNeeded)
            {
                //Load Highlights
                List<string> ids = AppContext.CurrentUser.TryLoadCollectedHighlightsList(true);
                if (ids == null)
                {
                    //This means the use has no highlights saved
                    highlightTextBlock.Text = AppStrings.NoCollectedHighlights;
                    highlightedBy.Visibility = System.Windows.Visibility.Collapsed;

                    //Is it safe to do this here? Probably, because the user doesn't have
                    //any highlights saved, we won't need to get anything from web. 
                    //but overall this whole model seems broken!
                    collectionsVM.HighlightsCollectionRefreshNeeded = false;


                    ShowControlsIfCollectionsReady();
                }
                else
                {
                    collectionsVM.LoadCollectedHighlightsAsync(ids, true, cancel.Token).ContinueWith(
                        task =>
                        {
                            RefreshHighlightTile();

                        }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, uiTaskScheduler);
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

            if (PhoneApplicationService.Current.State.ContainsKey("SelectedBook"))
                PhoneApplicationService.Current.State.Remove("SelectedBook");

            PhoneApplicationService.Current.State.Add("SelectedBook", selectedBook);

            //The book objects obtained through reading don't have a 'story' :S
            //This is a workaround to avoid making another webservice call to get the book.
            //Coming to think of it, it might be better for UX - they already have the book, afterall!
            NavigationService.Navigate(new Uri("/Views/ReadingPage.xaml", UriKind.Relative));

            //NavigationService.Navigate(new Uri("/Views/BookDetailsPage.xaml", UriKind.Relative));
            
        }

        private void booksList_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            //workaround for jumpy / jerky listbox scrolling - don't know the reason
            this.Focus();
        }

        private void allBooks_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (collectionsVM.CollectedBooks != null)
            {
                PhoneApplicationService.Current.State["CollectedBooks"] = collectionsVM.CollectedBooks;
                NavigationService.Navigate(new Uri("/Views/CollectedBooksPage.xaml", UriKind.Relative));
            }
        }

        private void allHighlights_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (collectionsVM.CollectedHighlights != null)
            {
                PhoneApplicationService.Current.State["CollectedHighlights"] = collectionsVM.CollectedHighlights;
                NavigationService.Navigate(new Uri("/Views/CollectedHighlightsPage.xaml", UriKind.Relative));
            }
        }
    }
}