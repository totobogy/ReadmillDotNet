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
using PhoneApp1.ViewModels;

namespace PhoneApp1
{
    public partial class ReadingPage : PhoneApplicationPage
    {
        BookHighlightsViewModel bookHighlightsVM;

        CancellationTokenSource cancelLoadHighlights;

        private bool viewModelInvalidated;

        public ReadingPage()
        {
            InitializeComponent();

            viewModelInvalidated = true;                        
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Book book = (Book)PhoneApplicationService.Current.State["SelectedBook"];
            bookHighlightsVM = new BookHighlightsViewModel(book);
            this.DataContext = bookHighlightsVM;

            //only enable app-bar when highlights have loaded to avoid problems
            ApplicationBar.IsVisible = false;

            cancelLoadHighlights = new CancellationTokenSource();

            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            bookHighlightsVM.IsCollectedAsync().ContinueWith(
                has =>
                {
                    ApplicationBarIconButton button = this.ApplicationBar.Buttons[0] as ApplicationBarIconButton;

                    if (has.Result)
                    {
                        button.Text = AppStrings.UnlikeBookButton;
                        button.IconUri = new Uri("/icons/appbar.heart.png", UriKind.Relative);
                    }
                    else
                    {
                        button.Text = AppStrings.LikeBookButton;
                        button.IconUri = new Uri("/icons/appbar.heart.outline.png", UriKind.Relative);
                    }
                }, uiTaskScheduler);

            SystemTray.IsVisible = false;
        }

        private void Like_Click(object sender, RoutedEventArgs e)
        {
            Button likeButton = (Button)sender;
            string highlightId = (string)likeButton.Tag;

            //diable untill we are ready to process input again
            likeButton.IsEnabled = false;

            Highlight highlight = (Highlight) likeButton.DataContext;

            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            if ((string)likeButton.Content == AppStrings.LikeHighlightButton)
            {               
                bookHighlightsVM.LikeHighlightAsync(highlightId).ContinueWith(
                    task =>
                    {
                        if (task.IsFaulted)
                        {
                            MessageBox.Show(
                                AppStrings.LikeReadmillHighlightFailed,
                                AppStrings.LikeFailedTitle, MessageBoxButton.OK);

                            throw task.Exception;
                        }

                        //should allow this to throw?
                        AppContext.CurrentUser.TryCollectHighlight(highlight);

                        //toggle state to 'unlike'
                        likeButton.Content = AppStrings.UnlikeHighlightButton;
                        likeButton.IsEnabled = true;

                    }, CancellationToken.None, TaskContinuationOptions.None, uiTaskScheduler);
                
            }
            else
            {               
                //unlike on readmill first -to be consistent with likeBook
                bookHighlightsVM.UnlikeHighlightAsync(highlightId).ContinueWith(
                    task =>
                    {
                        if (task.IsFaulted)
                        {
                            MessageBox.Show(
                            AppStrings.UnlikeReadmillHighlightFailed,
                            AppStrings.UnlikeFailedTitle, MessageBoxButton.OK);

                            throw task.Exception;
                        }

                        //remove locally, if present in collection? or should allow to throw
                        AppContext.CurrentUser.TryRemoveHighlight(highlightId);

                        //toggle state to 'like'
                        likeButton.Content = AppStrings.LikeHighlightButton;
                        likeButton.IsEnabled = true;

                    }, CancellationToken.None, TaskContinuationOptions.None, uiTaskScheduler);

            }

        }

        private void fullscreenButton_Click(object sender, EventArgs e)
        {
            ApplicationBarIconButton fullScreenButton = sender as ApplicationBarIconButton;
            if (fullScreenButton.Text == AppStrings.FullScreenButton)
            {
                TitlePanel.Visibility = System.Windows.Visibility.Collapsed;
                bookTitlePanel.Visibility = System.Windows.Visibility.Collapsed;
                ApplicationBar.Mode = ApplicationBarMode.Minimized;
                fullScreenButton.Text = AppStrings.CollapseFullScreenButton;
                fullScreenButton.IconUri = new Uri("/icons/appbar.arrow.collapsed.png", UriKind.Relative);
            }
            else
            {
                TitlePanel.Visibility = System.Windows.Visibility.Visible;
                bookTitlePanel.Visibility = System.Windows.Visibility.Visible;
                ApplicationBar.Mode = ApplicationBarMode.Default;
                fullScreenButton.Text = AppStrings.FullScreenButton;
                fullScreenButton.IconUri = new Uri("/icons/appbar.fullscreen.png", UriKind.Relative);
            }
        }

        private void likeBookButton_Click(object sender, EventArgs e)
        {
            //Mark book as interesting, show in My Books
            ApplicationBarIconButton likeBookButton = (ApplicationBarIconButton)sender;

            //disable unless we have processed the operation
            likeBookButton.IsEnabled = false;

            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            if (likeBookButton.Text == AppStrings.LikeBookButton)
            {
                bookHighlightsVM.LikeBookAsync().ContinueWith(
                        task =>
                        {
                            if (task.IsFaulted)
                            {
                                MessageBox.Show(
                                    AppStrings.CouldNotLikeBook,
                                    AppStrings.LikeFailedTitle,
                                    MessageBoxButton.OK);

                                /*task.Exception.Handle((ex) =>
                                {
                                    return true;
                                });*/
                            }
                            else
                            {
                                likeBookButton.Text = AppStrings.UnlikeBookButton;
                                likeBookButton.IconUri = new Uri("/icons/appbar.heart.png", UriKind.Relative);
                                likeBookButton.IsEnabled = true;
                            }
                        }, uiTaskScheduler);
            }
            else
            {
                bookHighlightsVM.UnlikeBookAsync().ContinueWith(
                    task =>
                    {
                        likeBookButton.Text = AppStrings.LikeBookButton;
                        likeBookButton.IconUri = new Uri("/icons/appbar.heart.outline.png", UriKind.Relative);
                        likeBookButton.IsEnabled = true;

                    }, uiTaskScheduler);
            }
        }

        //ToDo: This is probably too expensive if we have too many highlights?
        private void LikeButton_Loaded(object sender, RoutedEventArgs e)
        {
            Button likeButton = (Button)sender;
            string highlightId = (string)likeButton.Tag;

            //disable untill we are ready to process input again
            likeButton.IsEnabled = false;

            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            AppContext.CurrentUser.HasHighlight(highlightId).ContinueWith(
                has =>
                {
                    if (has.Result)
                    {
                        likeButton.Content = AppStrings.UnlikeHighlightButton;
                        likeButton.IsEnabled = true;
                    }
                    else
                    {
                        likeButton.Content = AppStrings.LikeHighlightButton;
                        likeButton.IsEnabled = true;
                    }

                }, uiTaskScheduler);
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Try Saving Liked Highlights
            AppContext.CurrentUser.TrySaveCollectedHighlightsLocally();

            //Cancel Pending Tasks
            try
            {
                cancelLoadHighlights.Cancel();
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

            //Save Book? - Later
        }

        private void highlightsListBox_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            //workaround for jumpy listbox bug - not sure why it works!
            this.Focus();
        }

        private void highlightsListBox_Loaded(object sender, RoutedEventArgs e)
        {
            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            //show progress bar
            highlightsProgressBar.IsIndeterminate = true;
            highlightsProgressBar.Visibility = System.Windows.Visibility.Visible;

            bookHighlightsVM.LoadBookHighlightsAsync(cancelLoadHighlights.Token).ContinueWith(displayList =>
            {
                //Is this the right thing to do?
                cancelLoadHighlights.Token.Register(() =>
                {
                    throw new OperationCanceledException(cancelLoadHighlights.Token);
                }, true);

                //hide progress bar
                highlightsProgressBar.Visibility = System.Windows.Visibility.Collapsed;

                if (bookHighlightsVM.BookHighlights.Count <= 0)
                {
                    highlightsListBox.Items.Add(new ListBoxItem()
                    {
                        Content = new TextBlock()
                        {
                            TextWrapping = System.Windows.TextWrapping.Wrap,
                            FontSize = 24,
                            Padding = new Thickness(10, 30, 10, 10),
                            Text = AppStrings.NoHighlights
                        }
                    });
                }
                else
                {
                    //ToDo: sorting should be in VM
                    highlightsListBox.ItemsSource = from highlight in bookHighlightsVM.BookHighlights
                                                    orderby highlight.Position
                                                    select highlight;
                }

                //Show App-bar now 
                ApplicationBar.IsVisible = true;

            }, cancelLoadHighlights.Token, TaskContinuationOptions.NotOnCanceled, uiTaskScheduler);            
        }
    }
}