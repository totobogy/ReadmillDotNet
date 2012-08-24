using System;
using System.Linq;
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
using Com.Readmill.Api.DataContracts;
using System.Threading.Tasks;
using System.Collections.Generic;
using Com.Readmill.Api;


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
        
        public static AuthenticatedUser CurrentUser
        {
            get
            {
                return AuthenticatedUser.CurrentUser;
            }
        }

    }

    public sealed class AuthenticatedUser
    {
        private ReadmillClient client;

        private static readonly AuthenticatedUser user = new AuthenticatedUser();
        private AuthenticatedUser()
        {
            client = new ReadmillClient(AppContext.ClientId);
            
            //collectedHighlights = new Dictionary<string, Highlight>();
        }

        public static AuthenticatedUser CurrentUser
        {
            get
            {
                return user;
            }
        }

        private User me;
        private IDictionary<string, Book> collectedBooks;
        private IDictionary<string, Highlight> collectedHighlights;

        public Task<User> GetMe(bool forceRefresh = false)
        {
            if (forceRefresh || me == null)
            {
                return client.Users.GetOwnerAsync(AppContext.AccessToken.Token);
            }
            else
            {
                return Task.Factory.StartNew(() =>
                {
                    return me;
                });
            }
        }

        public Task<List<Book>> LoadCollectedBooksAsync(bool forceRefresh = false)
        {
            if (forceRefresh || collectedBooks == null)
            {
                collectedBooks = new Dictionary<string, Book>();

                Task<List<Reading>> r = GetMe().ContinueWith(me =>
                {
                    return
                        client.Users.GetUserReadings(me.Result.Id, accessToken: AppContext.AccessToken.Token);
                }).Unwrap();

                return r.ContinueWith(t =>
                {
                    foreach (Reading reading in t.Result)
                    {
                        collectedBooks.Add(reading.Book.Id, reading.Book);
                    }

                    return collectedBooks.Values.ToList();
                });
            }
            else
            {
                return Task.Factory.StartNew(() =>
                {
                    return collectedBooks.Values.ToList();
                });
            }

        }


        //Save CollectedHighlights
        //Save CollectedBooks (2)
        //Load from disk   

    }
}
