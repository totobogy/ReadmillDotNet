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
using Microsoft.Phone.Shell;
using Com.Readmill.Api.DataContracts;

namespace PhoneApp1.Views
{
    public partial class CollectedBooksPage : PhoneApplicationPage
    {
        private bool viewModelsInvalidated;
        private CollectionsViewModel collectionsVM;

        private List<Book> collectedBooks;
        public CollectedBooksPage()
        {
            InitializeComponent();

            viewModelsInvalidated = true;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (viewModelsInvalidated)
            {
                if (State.ContainsKey("CollectedBooks"))
                    collectedBooks = State["CollectedBooks"] as List<Book>;
                else if (PhoneApplicationService.Current.State.ContainsKey("CollectedBooks"))
                    collectedBooks = PhoneApplicationService.Current.State["CollectedBooks"] as List<Book>;

                if (collectedBooks == null)
                    throw new InvalidOperationException(AppStrings.BooksCollectionNull);

                //handle empty collection?

                collectedBooksList.ItemsSource = collectedBooks;

                viewModelsInvalidated = false;
            }
        }

        private void collectedBooksList_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Book selectedBook = (Book)collectedBooksList.SelectedItem;

            if (PhoneApplicationService.Current.State.ContainsKey("SelectedBook"))
                PhoneApplicationService.Current.State.Remove("SelectedBook");

            PhoneApplicationService.Current.State.Add("SelectedBook", selectedBook);

            //The book objects obtained through reading don't have a 'story' :S
            //This is a workaround to avoid making another webservice call to get the book.
            //Coming to think of it, it might be better for UX - they already have the book, afterall!
            NavigationService.Navigate(new Uri("/Views/ReadingPage.xaml", UriKind.Relative));
        }

        private void collectedBooksList_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            this.Focus();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.Opacity = 0;
        }
    }
}