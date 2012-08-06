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


namespace PhoneApp1
{
    public partial class BookDetailsPage : PhoneApplicationPage
    {
        public BookDetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Book book = (Book) PhoneApplicationService.Current.State["SelectedBook"];

            ContentPanel.DataContext = book;            

            var gl = GestureService.GetGestureListener(this);
            gl.Flick +=new EventHandler<Microsoft.Phone.Controls.FlickGestureEventArgs>(gl_Flick);

        }

        private void gl_Flick(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            if (e.OriginalSource == bookStory)
                return;

            NavigationService.Navigate(new Uri("/ReadingPage.xaml", UriKind.Relative));

        }
    }
}