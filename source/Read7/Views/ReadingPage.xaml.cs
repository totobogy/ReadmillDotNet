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
        }

        /*private void gl_Flick(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).Parent != ContentPanel
                && ((FrameworkElement)e.OriginalSource).Parent != TitlePanel)
                return;

            NavigationService.Navigate(new Uri("/MyReadingsPage.xaml", UriKind.Relative));

        }*/

        private void Like_Click(object sender, RoutedEventArgs e)
        {
            Button likeButton = (Button)sender;
            string highlightId = (string)likeButton.Tag;

            if (likeButton.Content == AppStrings.LikeHighlightButton)
            {
                //Like on readmill

                //toggle state to 'unlike'
                likeButton.Content = AppStrings.UnlikeHighlightButton;
            }
            else
            {
                //unlike on readmill

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
            ApplicationBarIconButton likeBookButton = (ApplicationBarIconButton)sender;
            if (likeBookButton.Text == AppStrings.LikeBookButton)
            {
                likeBookButton.Text = AppStrings.UnlikeBookButton;
                likeBookButton.IconUri = new Uri("/icons/favs.png", UriKind.Relative);
            }
            else
            {
                likeBookButton.Text = AppStrings.LikeBookButton;
                likeBookButton.IconUri = new Uri("/icons/addTofavs.png", UriKind.Relative);
            }
        }
    }
}