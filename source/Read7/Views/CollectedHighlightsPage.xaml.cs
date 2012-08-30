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
using Microsoft.Phone.Shell;
using PhoneApp1.ViewModels;
using Com.Readmill.Api.DataContracts;
using System.Threading.Tasks;
using System.Threading;
using Com.Readmill.Api;

namespace PhoneApp1.Views
{
    public partial class CollectedHighlightsPage : PhoneApplicationPage
    {
        private ReadmillClient client;
        private bool viewModelsInvalidated;
        private CancellationTokenSource cancel;

        private List<Highlight> collectedHighlights;
        public CollectedHighlightsPage()
        {
            InitializeComponent();

            viewModelsInvalidated = true;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            //If we are navigating away, lets cancel any pending tasks since they 
            //can't be revived in a deterministic fashion.
            try
            {
                if (cancel != null && !cancel.IsCancellationRequested)
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

            if (viewModelsInvalidated)
            {
                client = new ReadmillClient(AppContext.ClientId);

                if (State.ContainsKey("CollectedHighlights"))
                    collectedHighlights = State["CollectedHighlights"] as List<Highlight>;
                else if (PhoneApplicationService.Current.State.ContainsKey("CollectedHighlights"))
                    collectedHighlights = PhoneApplicationService.Current.State["CollectedHighlights"] as List<Highlight>;

                if (collectedHighlights == null)
                    throw new InvalidOperationException(AppStrings.HighlightsCollectionNull);

                collectedHighlightsList.ItemsSource = collectedHighlights;

                viewModelsInvalidated = false;
            }

            cancel = new CancellationTokenSource();
        }

        private void collectedHighlightsList_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            this.Focus();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.Opacity = 0;
        }

        private void fullscreenButton_Click(object sender, EventArgs e)
        {
            ApplicationBarIconButton fullScreenButton = sender as ApplicationBarIconButton;
            if (fullScreenButton.Text == AppStrings.FullScreenButton)
            {
                TitlePanel.Visibility = System.Windows.Visibility.Collapsed;
                ApplicationBar.Mode = ApplicationBarMode.Minimized;
                fullScreenButton.Text = AppStrings.CollapseFullScreenButton;
                fullScreenButton.IconUri = new Uri("/icons/appbar.arrow.collapsed.png", UriKind.Relative);
            }
            else
            {
                TitlePanel.Visibility = System.Windows.Visibility.Visible;
                ApplicationBar.Mode = ApplicationBarMode.Default;
                fullScreenButton.Text = AppStrings.FullScreenButton;
                fullScreenButton.IconUri = new Uri("/icons/appbar.fullscreen.png", UriKind.Relative);
            }
        }

        private void Like_Click(object sender, RoutedEventArgs e)
        {
            HighlightsClient client = this.client.Highlights;

            Button likeButton = (Button)sender;
            string highlightId = (string)likeButton.Tag;

            //diable untill we are ready to process input again
            likeButton.IsEnabled = false;

            Highlight highlight = (Highlight)likeButton.DataContext;

            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            if ((string)likeButton.Content == AppStrings.LikeHighlightButton)
            {
                client.PostAsync(AppContext.BuildReadmillLikeUri(highlightId)).ContinueWith(
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
                client.DeleteAsync(AppContext.BuildReadmillLikeUri(highlightId)).ContinueWith(
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

        private void TextBlock_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ReadingsClient client = this.client.Readings;

            TextBlock highlightText = (TextBlock)sender;
            Highlight highlight = highlightText.DataContext as Highlight;

            TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            client.GetReadingByIdAsync(
                highlight.ReadingId,
                AppContext.AccessToken.Token, cancel.Token).ContinueWith(
                task =>
                {
                    Book book = task.Result.Book;

                    if (PhoneApplicationService.Current.State.ContainsKey("SelectedBook"))
                        PhoneApplicationService.Current.State.Remove("SelectedBook");

                    PhoneApplicationService.Current.State.Add("SelectedBook", book);

                    NavigationService.Navigate(new Uri("/Views/ReadingPage.xaml", UriKind.Relative));

                }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, uiScheduler);

        }
    }
}