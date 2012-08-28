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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Runtime.Serialization.Json;
using System.IO.IsolatedStorage;
using System.IO;
using System.Threading.Tasks;
using Com.Readmill.Api.DataContracts;

namespace PhoneApp1
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            RootFrame.Navigating += new NavigatingCancelEventHandler(RootFrame_Navigating);

        }

        void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            //Check Network Connection, if not connected, route to landing page
            //except if we are exiting
            if (!AppContext.IsConnected 
                && 
                !(e.Uri.ToString().Contains("/Views/ErrorLandingPage.xaml") 
                    || e.Uri.ToString().Contains("app")))
            {
                MessageBox.Show(
                    AppStrings.NotConnectedMsg,
                    AppStrings.NotConnectedMsgTitle,
                    MessageBoxButton.OK);

                e.Cancel = true;
                RootFrame.Dispatcher.BeginInvoke(delegate
                {
                    RootFrame.Navigate(new Uri("/Views/ErrorLandingPage.xaml", UriKind.Relative));
                });
            }
            else
            {
                if (!e.Uri.ToString().Contains("/Views/Home.xaml"))
                    return;

                //If there's a locally stored token
                if (TryRetrieveStoredAccessToken())
                {
                    //which is valid?? How to check
                    return;
                }

                e.Cancel = true;
                RootFrame.Dispatcher.BeginInvoke(delegate
                {
                    RootFrame.Navigate(new Uri("/Views/LogInPage.xaml", UriKind.Relative));
                });
            }
        }

        private bool TryRetrieveStoredAccessToken()
        {
            //Right Now we only support non-expiring tokens, but user can still revoke access
            
            //If this was already called before and we have a token set
            if (AppContext.AccessToken != null)
                return true;

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AccessToken));
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    using (var stream = new
                                        IsolatedStorageFileStream("token.ser",
                                                                    FileMode.Open,
                                                                    FileAccess.Read,
                                                                    store))
                    {
                        AppContext.AccessToken = (AccessToken)ser.ReadObject(stream);
                        return true;
                    }
                }
                catch (IsolatedStorageException ex)
                {
                    //no-op: we'll ask for authorization when page loads
                    return false;
                }
            }
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            //Do we need to get the token again?
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            //Try to save collections, in case we are terminated
            AppContext.CurrentUser.TrySaveCollectedHighlightsLocally();

            //Anything else?
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            //Try to save collections, in case we are terminated
            AppContext.CurrentUser.TrySaveCollectedHighlightsLocally();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            //We have already communicated fatal error to the user and the app will be exited
            //No further action needed
            if (AppContext.ErrorScreenShown)
            {
                e.Handled = true;
                return;
            }

            //should we handle Cancellations silently here?
            if (e.ExceptionObject is TaskCanceledException)
                e.Handled = true;
            else if (e.ExceptionObject is OperationCanceledException)
                e.Handled = true;

            else if (e.ExceptionObject is AggregateException)
            {
                AggregateException ex = (e.ExceptionObject as AggregateException).Flatten();
                ex.Handle(err =>
                    {
                        //should we handle Cancellations silently here?
                        if (err is TaskCanceledException)
                            return e.Handled = true;

                        //should we handle Cancellations silently here?
                        if (err is OperationCanceledException)
                            return e.Handled = true;

                        if (err is WebException)
                        {
                            WebException webEx = err as WebException;

                            //should we handle Cancellations silently here?
                            if (webEx.Status == WebExceptionStatus.RequestCanceled)
                                return e.Handled = true;

                            if (webEx.Status == WebExceptionStatus.ProtocolError
                                || webEx.Status == WebExceptionStatus.UnknownError)
                            {
                                if (webEx.Response.SupportsHeaders)
                                {
                                    string status = webEx.Response.Headers["status"];

                                    if (status.Contains("401"))
                                    {
                                        //Unauthorized
                                        //what if the error is due to missing credentials?
                                        if (!AppContext.TokenRefreshing())
                                        {
                                            RootFrame.Dispatcher.BeginInvoke(
                                                delegate
                                                {
                                                    MessageBoxResult result =
                                                        MessageBox.Show(
                                                        AppStrings.AuthFailed,
                                                        AppStrings.AuthFailedTitle,
                                                        MessageBoxButton.OK);

                                                    RootFrame.Navigate(new Uri("/Views/LogInPage.xaml", UriKind.Relative));
                                                });
                                        }
                                        return e.Handled = true;
                                    }
                                    else
                                        return false;
                                }
                                else
                                    return false;
                            }
                            
                            else
                                return false;
                        }

                        else
                        {
                            //Unknown Exception
                            //ToDo: Send Error Report?
                            MessageBoxResult result = MessageBox.Show(
                                AppStrings.UnknownException,
                                AppStrings.UnknownExceptionTitle,
                                MessageBoxButton.OK);

                            RootFrame.Navigate(new Uri("/Views/ErrorLandingPage.xaml", UriKind.Relative));
                            AppContext.ErrorScreenShown = true;
                            return e.Handled = true;
                        }
                    });
            }
            else
            {
                //Unknown Exception
                //ToDo: Send Error Report?
                MessageBoxResult result = MessageBox.Show(
                    AppStrings.UnknownException,
                    AppStrings.UnknownExceptionTitle,
                    MessageBoxButton.OK);

                RootFrame.Navigate(new Uri("/Views/ErrorLandingPage.xaml", UriKind.Relative));
                AppContext.ErrorScreenShown = true;
                e.Handled = true;
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                //System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}