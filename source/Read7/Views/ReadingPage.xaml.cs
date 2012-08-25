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

        public ReadingPage()
        {
            InitializeComponent();

            Book book = (Book)PhoneApplicationService.Current.State["SelectedBook"];
            bookHighlightsVM = new BookHighlightsViewModel(book);
            this.DataContext = bookHighlightsVM;

            //show progress bar
            highlightsProgressBar.IsIndeterminate = true;
            highlightsProgressBar.Visibility = System.Windows.Visibility.Visible;

            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            bookHighlightsVM.LoadBookHighlightsAsync().ContinueWith(displayList =>
                {
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
                }, uiTaskScheduler);            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

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
        }

        private void Like_Click(object sender, RoutedEventArgs e)
        {
            Button likeButton = (Button)sender;
            string highlightId = (string)likeButton.Tag;

            //Can't we live with data context?
            Highlight highlight = (Highlight) likeButton.DataContext;

            if ((string)likeButton.Content == AppStrings.LikeHighlightButton)
            {
                //ToDo: Like on readmill
                AppContext.CurrentUser.CollectHighlight(highlight);

                //toggle state to 'unlike'
                likeButton.Content = AppStrings.UnlikeHighlightButton;
            }
            else
            {
                //unlike on readmill
                AppContext.CurrentUser.RemoveHighlight(highlightId);

                //toggle state to 'like'
                likeButton.Content = AppStrings.LikeHighlightButton;
            }

        }

        private void fullscreenButton_Click(object sender, EventArgs e)
        {
            ApplicationBarIconButton fullScreenButton = sender as ApplicationBarIconButton;
            if (fullScreenButton.Text == AppStrings.FullScreenButton)
            {
                TitlePanel.Visibility = System.Windows.Visibility.Collapsed;
                bookTitlePanel.Visibility = System.Windows.Visibility.Collapsed;
                fullScreenButton.Text = AppStrings.CollapseFullScreenButton;
                fullScreenButton.IconUri = new Uri("/icons/appbar.arrow.collapsed.png", UriKind.Relative);
            }
            else
            {
                TitlePanel.Visibility = System.Windows.Visibility.Visible;
                bookTitlePanel.Visibility = System.Windows.Visibility.Visible;
                fullScreenButton.Text = AppStrings.FullScreenButton;
                fullScreenButton.IconUri = new Uri("/icons/appbar.fullscreen.png", UriKind.Relative);
            }
        }

        private void likeBookButton_Click(object sender, EventArgs e)
        {
            //Mark book as interesting, show in My Books
            //Mark book as interesting, show in My Books
            ApplicationBarIconButton likeBookButton = (ApplicationBarIconButton)sender;
            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            if (likeBookButton.Text == AppStrings.LikeBookButton)
            {
                bookHighlightsVM.LikeBookAsync().ContinueWith(
                        task =>
                        {
                            if (task.IsFaulted)
                            {
                                MessageBox.Show(AppStrings.CouldNotLikeBook);
                                task.Exception.Handle((ex) =>
                                {
                                    return true;
                                });
                            }
                            else
                            {
                                likeBookButton.Text = AppStrings.UnlikeBookButton;
                                likeBookButton.IconUri = new Uri("/icons/appbar.heart.png", UriKind.Relative);
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
                    }, uiTaskScheduler);
            }
        }

        //ToDo: This is probably too expensive if we have too many highlights?
        private void LikeButton_Loaded(object sender, RoutedEventArgs e)
        {
            Button likeButton = (Button)sender;
            string highlightId = (string)likeButton.Tag;

            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            AppContext.CurrentUser.HasHighlight(highlightId).ContinueWith(
                has =>
                {
                    if (has.Result)
                        likeButton.Content = AppStrings.UnlikeHighlightButton;
                    else
                        likeButton.Content = AppStrings.LikeHighlightButton;
                }, uiTaskScheduler);
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Try Saving Liked Highlights
            AppContext.CurrentUser.TrySaveCollectedHighlightsLocally();

            //Save Book? - Later
        }

        private void highlightsListBox_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            //workaround for jumpy listbox bug - not sure why it works!
            this.Focus();
        }

    }
}