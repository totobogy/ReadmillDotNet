using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Net.NetworkInformation;


namespace PhoneApp1
{
    public static class AppContext
    {
        public static class Constants
        {
            public const string RedirectUri = "http://totobogy.thoughtbubblez.com";
            public const string AuthUri = "http://readmill.com/oauth/authorize?response_type=code&client_id=" + ClientId + "&scope=non-expiring&redirect_uri=" + RedirectUri;
            public const string TokenUri = "http://readmill.com/oauth/token?grant_type=authorization_code&client_id=" + ClientId + "&client_secret=" + ClientSecret + "&redirect_uri=" + RedirectUri + "&code=";
        }

        public const string ClientId = "3f2116709bb1f330084b9cd9f1045961";
        public const string ClientSecret = "0b8d3bdaacfa1797637bbb6791eb21dd";

        public static AccessToken AccessToken { get; set; }

        public static bool IsConnected
        {
            get
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
        }
    }
}
