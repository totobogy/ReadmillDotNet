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
using System.Threading.Tasks;
using Com.Readmill.Api.DataContracts;

namespace PhoneApp1
{
    public partial class MyReadingsPage : PhoneApplicationPage
    {
        public MyReadingsPage()
        {
            InitializeComponent();


            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            IDictionary<string, Book> readableBooks = new Dictionary<string, Book>();

            ReadmillClient client = new ReadmillClient(AppContext.ClientId);

            Task<User> task = client.Users.GetOwnerAsync(AppContext.AccessToken.Token);
            task.ContinueWith(getMe =>
                {
                    client.Users.GetUserReadings(getMe.Result.Id,
                                                    accessToken: AppContext.AccessToken.Token).ContinueWith(
                                                    getReadings =>
                                                    {
                                                        //myReadingsList.Items.RemoveAt(0);
                                                        //myReadingsList.ItemsSource = getReadings.Result;

                                                    }, uiTaskScheduler);

                });

        }
    }
}