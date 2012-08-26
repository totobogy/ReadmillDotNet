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
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Threading;

namespace PhoneApp1
{
    public partial class LogInPage : PhoneApplicationPage
    {
        public LogInPage()
        {
            InitializeComponent();
            readmillBrowser.Navigate(new Uri(AppContext.Constants.AuthUri));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Remove login page(s) from back-stack
            while (NavigationService.CanGoBack)
                NavigationService.RemoveBackEntry();
        }

        private void readmillBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            Uri redirectUri = e.Uri;
            if (redirectUri.AbsoluteUri.StartsWith(AppContext.Constants.RedirectUri))
            {
                //ToDo: Handle "Deny"
                if (redirectUri.Query.Contains("error=user_denied"))
                {
                    e.Cancel = true;
                    this.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show(
                                AppStrings.MustAuthorize, 
                                AppStrings.MustAuthorizeTitle,
                                MessageBoxButton.OK);

                            readmillBrowser.Navigate(new Uri(AppContext.Constants.AuthUri));
                        });

                    return;
                }

                string authCode = redirectUri.Query.Split(new string[] { "?code=" }, StringSplitOptions.RemoveEmptyEntries)[0];
                string tokenUri = AppContext.Constants.TokenUri + authCode;

                HttpWebRequest req = WebRequest.CreateHttp(tokenUri);
                req.Method = "POST";

                TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

                req.BeginGetResponse(
                    (result) =>
                    {
                        using (HttpWebResponse response = (HttpWebResponse)req.EndGetResponse(result))
                        {
                            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AccessToken));
                            AppContext.AccessToken = (AccessToken)ser.ReadObject(response.GetResponseStream());

                            //Save token for next time
                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                using (var stream = new IsolatedStorageFileStream("token.ser",
                                                                                   FileMode.Create,
                                                                                   FileAccess.Write,
                                                                                   store))
                                {
                                    ser.WriteObject(stream, AppContext.AccessToken);
                                }
                            }

                            AppContext.ResetTokenRefreshing();
                        }

                        Task.Factory.StartNew(() =>
                        {
                            e.Cancel = true;

                            NavigationService.Navigate(new Uri("/Views/Home.xaml?from_login=true", UriKind.Relative));
                        
                        }, CancellationToken.None, TaskCreationOptions.None, uiTaskScheduler);

                    }, null);
            }
        }
    }

    [DataContract]
    public class AccessToken
    {
        [DataMember(Name = "access_token")]
        public string Token { get; set; }

        [DataMember(Name = "expires_in")]
        public string ExpiresIn { get; set; }

        [DataMember(Name = "scope")]
        public string Scope { get; set; }

        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }
    }
}