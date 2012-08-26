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

namespace PhoneApp1.Views
{
    public partial class ErrorLandingPage : PhoneApplicationPage
    {
        public ErrorLandingPage()
        {
            InitializeComponent();
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