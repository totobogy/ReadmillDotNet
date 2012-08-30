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

namespace PhoneApp1.Views
{
    public partial class CollectedHighlightsPage : PhoneApplicationPage
    {
        private bool viewModelsInvalidated;
        private CollectionsViewModel collectionsVM;

        private List<Highlight> collectedHighlights;
        public CollectedHighlightsPage()
        {
            InitializeComponent();

            viewModelsInvalidated = true;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (viewModelsInvalidated)
            {
                if (State.ContainsKey("CollectedHighlights"))
                    collectedHighlights = State["CollectedHighlights"] as List<Highlight>;
                else if (PhoneApplicationService.Current.State.ContainsKey("CollectedHighlights"))
                    collectedHighlights = PhoneApplicationService.Current.State["CollectedHighlights"] as List<Highlight>;

                if (collectedHighlights == null)
                    throw new InvalidOperationException(AppStrings.HighlightsCollectionNull);

                //handle empty collection?

                collectedHighlightsList.ItemsSource = collectedHighlights;

                viewModelsInvalidated = false;
            }
        }


        private void collectedHighlightsList_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            this.Focus();
        }


        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.Opacity = 0;
        }
    }
}