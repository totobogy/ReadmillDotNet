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
using Com.Readmill.Api.DataContracts;
using Com.Readmill.Api;
using System.Threading.Tasks;
using PhoneApp1.ViewModels;


namespace PhoneApp1
{
    public partial class BookDetailsPage : PhoneApplicationPage
    {
        BookDetailsViewModel bookDetailsVM;
        public BookDetailsPage()
        {
            InitializeComponent();

            Book book = (Book)PhoneApplicationService.Current.State["SelectedBook"];
            bookDetailsVM = new BookDetailsViewModel(book);

            this.DataContext = bookDetailsVM;

            //var gl = GestureService.GetGestureListener(this);
            //gl.Flick += new EventHandler<Microsoft.Phone.Controls.FlickGestureEventArgs>(gl_Flick);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);            
        }

        /*private void gl_Flick(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            if (e.OriginalSource == bookStory)
                return;

            NavigationService.Navigate(new Uri("/ReadingPage.xaml", UriKind.Relative));

        }*/

        private void likeBook_Click(object sender, EventArgs e)
        {
            //Mark book as interesting, show in My Books
            ApplicationBarIconButton likeBookButton = (ApplicationBarIconButton)sender;
            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            if (likeBookButton.Text == "like")
            {
                bookDetailsVM.LikeBookAsync().ContinueWith(
                    task =>
                    {
                        likeBookButton.Text = "unlike";
                        likeBookButton.IconUri = new Uri("/icons/favs.png", UriKind.Relative);
                    }, uiTaskScheduler);
                
            }
            else
            {
                bookDetailsVM.UnlikeBookAsync().ContinueWith(
                    task =>
                    {
                        likeBookButton.Text = "like";
                        likeBookButton.IconUri = new Uri("/icons/addTofavs.png", UriKind.Relative);
                    }, uiTaskScheduler);
            }
        }

        private void roveHighlights_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ReadingPage.xaml", UriKind.Relative));
        }
    }
}