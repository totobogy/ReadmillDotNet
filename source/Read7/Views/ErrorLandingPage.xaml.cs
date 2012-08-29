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

namespace PhoneApp1.Views
{
    public partial class ErrorLandingPage : PhoneApplicationPage
    {
        public ErrorLandingPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ErrorLandingPage_Loaded);
        }

        void ErrorLandingPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.Opacity = 0.0;
            //SystemTray.IsVisible = false;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Clear back-stack. We only want to be able to exit from here.
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();
        }
    }
}