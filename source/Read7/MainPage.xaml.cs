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

namespace PhoneApp1
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }


        private void booksList_Loaded(object sender, RoutedEventArgs e)
        {
            ReadmillClient client = new ReadmillClient("3f2116709bb1f330084b9cd9f1045961");
            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            BooksQueryOptions options = new BooksQueryOptions();
            options.CountValue = 100;
            //options.SearchStringValue = "zen";
            client.Books.GetBooksAsync(options).ContinueWith(
                (booksTask) =>
                {
                    foreach (Book book in booksTask.Result)
                    {
                        TextBlock bookItem = new TextBlock();
                        bookItem.TextWrapping = TextWrapping.Wrap;
                        bookItem.Padding = new Thickness(6);
                        bookItem.Text = book.Title;

                        PhoneApplicationPage page = new PhoneApplicationPage();
                        //page.
           
                        ListBoxItem item = new ListBoxItem();

                        /*SolidColorBrush brush = new SolidColorBrush();
                        brush.Color = Color.FromArgb(50, 250, 0, 0);
                        item.Background = brush;*/

                        item.Content = bookItem;
                        booksList.Items.Add(item);
                    }

                }, uiTaskScheduler);
        }
    }
}