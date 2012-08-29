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
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;
using System.Threading;


namespace PhoneApp1
{
    public static class AppContext
    {
        public static bool ErrorScreenShown { get; set; }

        public static class Constants
        {
            public const string RedirectUri = "http://about.me/totobogy";
            public const string AuthUri = "http://m.readmill.com/oauth/authorize?response_type=code&client_id=" + ClientId + "&scope=non-expiring&redirect_uri=" + RedirectUri;
            public const string TokenUri = "http://readmill.com/oauth/token?grant_type=authorization_code&client_id=" + ClientId + "&client_secret=" + ClientSecret + "&redirect_uri=" + RedirectUri + "&code=";
        }

        public const string ClientId = "3f2116709bb1f330084b9cd9f1045961";
        public const string ClientSecret = "0b8d3bdaacfa1797637bbb6791eb21dd";

        public static AccessToken AccessToken { get; set; }

        /// <summary>
        /// used to indicate that the current access token has been invalidated
        /// and is being refreshed. The first actor which finds that the token has been invalidated (e.g. gets
        ///  a 401) must set this to 'true', refresh the token and set it to false again
        /// (so that multiple actors don't try to refresh it)
        /// ToDo: Use a locking device which doesn't cause others to wait/block?
        /// </summary>
        private static bool tokenRefreshing;

        /// <summary>
        /// This method is intended to be called by actors who discover that the token has been 
        /// invalidated and needs to be refreshed. Only on actor needs to refresh the token so
        /// the first actor who calls is indicated that no one is refreshing the token yet.
        /// </summary>
        /// <returns></returns>
        public static bool TokenRefreshing()
        {
            lock (typeof(AppContext))
            {
                if (!tokenRefreshing)
                {
                    //No one is refreshing the token yet. Tell the caller so that it can refresh.
                    
                    tokenRefreshing = true;
                    
                    //invalidate current token
                    AppContext.AccessToken = null;
                    
                    return false;
                }
                else
                {
                    //Token is already being refreshed. The caller can ignore and move on.
                    return true;
                }
            }
        }

        /// <summary>
        /// Reset the token refreshing flag to indicate that no actor is now in the process of
        /// refreshing the token.
        /// </summary>
        public static void ResetTokenRefreshing()
        {
            tokenRefreshing = false;
        }


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

    /*
     * Singleton. Represents the Authenticated User.
     * Used to optimize on web serivce calls and provide single access point
     * for current user's data
     */
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
        private IDictionary<string, Reading> bookReadingHash;

        public List<Book> CollectedBooks
        {
            get
            {
                return collectedBooks.Values.ToList();
            }
        }

        public List<Highlight> CollectedHighlights
        {
            get
            {
                return collectedHighlights.Values.ToList();
            }
        }

        public Task<User> GetMeAsync(bool forceRefresh = false, CancellationToken cancelToken = default(CancellationToken))
        {
            if (forceRefresh || me == null)
            {
                return client.Users.GetOwnerAsync(AppContext.AccessToken.Token).ContinueWith(
                    task =>
                    {
                        return me = task.Result;
                    });
            }
            else
            {
                return Task.Factory.StartNew(() =>
                {
                    return me;
                }, cancelToken);
            }
        }

        public Task<List<Book>> LoadCollectedBooksAsync(bool forceRefresh = false, CancellationToken cancelToken = default(CancellationToken))
        {
            if (forceRefresh || collectedBooks == null)
            {
                collectedBooks = new Dictionary<string, Book>();
                bookReadingHash = new Dictionary<string, Reading>();

                Task<List<Reading>> r = GetMeAsync().ContinueWith(me =>
                {
                    return
                        client.Users.GetUserReadings(me.Result.Id, accessToken: AppContext.AccessToken.Token, cancellationToken:cancelToken);
                }, cancelToken).Unwrap();

                return r.ContinueWith(t =>
                {
                    foreach (Reading reading in t.Result)
                    {
                        bookReadingHash.Add(reading.Book.Id, reading);
                        collectedBooks.Add(reading.Book.Id, reading.Book);
                    }

                    return collectedBooks.Values.ToList();
                }, cancelToken);
            }
            else
            {
                return Task.Factory.StartNew(() =>
                {
                    return collectedBooks.Values.ToList();
                }, cancelToken);
            }

        }

        //ToDo: right now the model is to store list of collected highlights locally
        //so this call loads the locally stored list of id's
        //this should not be needed with API V2 anymore since there's an api to get user highlights
        public List<string> TryLoadCollectedHighlightsList(bool forceRefresh = false)
        {
            if (collectedHighlights == null || forceRefresh)
            {
                //Load highlights from the saved file
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        using (var stream = new
                                            IsolatedStorageFileStream("CollectedHighlights.xml",
                                                                        FileMode.Open,
                                                                        FileAccess.Read,
                                                                        store))
                        {
                            XmlSerializer ser = new XmlSerializer(typeof(List<string>));
                            return (List<string>)ser.Deserialize(stream);
                        }
                    }
                    catch (IsolatedStorageException ex)
                    {
                        //no-op
                        return null;
                    }
                }
            }
            else
            {
                return collectedHighlights.Keys.ToList();
            }
        }

        //ToDo: right now the model is to store list of collected highlights locally
        //this call saves the current highlights list locally
        //this should not be needed with API V2 anymore since there's an api to get user highlights
        public bool TrySaveCollectedHighlightsLocally()
        {
            //Load highlights from the saved file
            if (collectedHighlights != null && collectedHighlights.Count > 0)
            {
                try
                {
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (var stream = new
                            IsolatedStorageFileStream("CollectedHighlights.xml",
                                                       FileMode.Create,
                                                       FileAccess.Write,
                                                       store))
                        {
                            XmlSerializer ser = new XmlSerializer(typeof(List<string>));
                            ser.Serialize(stream, collectedHighlights.Keys.ToList());
                            return true;
                        }
                    }
                }
                catch (IsolatedStorageException ex)
                {
                    return false;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Must only be called after the actual operation on Readmill has succeeded.
        /// </summary>
        /// <param name="highlight"></param>
        public void TryCollectHighlight(Highlight highlight)
        {
            lock (this)
            {
                if (collectedHighlights == null)
                    collectedHighlights = new Dictionary<string, Highlight>();

                if(!collectedHighlights.ContainsKey(highlight.Id))
                    collectedHighlights.Add(highlight.Id, highlight);
            }
        }

        /// <summary>
        /// Must only be called after the actual operation on Readmill has succeeded
        /// </summary>
        /// <param name="id"></param>
        public void TryRemoveHighlight(string id)
        {
            if (collectedHighlights == null)
                return;//throw new InvalidOperationException("User's highlights not initialized.");

            else
                lock (this)
                {
                    if (collectedHighlights.ContainsKey(id))
                        collectedHighlights.Remove(id);
                }
        }


        /// <summary>
        /// Must only be called after the actual operation on Readmill has succeeded.
        /// This is only an optimization to avoid extra web service call
        /// </summary>
        /// <param name="book"></param>
        public void AddBookToLocalCollection(Book book)
        {
            lock (this)
            {
                //should never happen
                if (collectedBooks == null)
                    throw new InvalidOperationException("Book collection must be already initialized.");

                if (!collectedBooks.ContainsKey(book.Id))
                    collectedBooks.Add(book.Id, book);
            }
        }

        /// <summary>
        /// Must only be called after the actual operation on Readmill has succeeded.
        /// This is only an optimization to avoid extra web service call
        /// </summary>
        /// <param name="book"></param>
        public void RemoveBookFromLocalCollection(string bookId)
        {
            if (collectedBooks == null)
                return;//throw new InvalidOperationException("User's highlights not initialized.");

            else
                lock (this)
                {
                    if (collectedBooks.ContainsKey(bookId))
                        collectedBooks.Remove(bookId);
                }
        }


        //ToDo: right now the model is to store list of collected highlights locally
        //so this call expects a list of highlight id's to get
        //this should not be needed with API V2 anymore since there's an api to get user highlights
        public Task<List<Highlight>> LoadCollectedHighlightsAsync(
            List<string> highlightIds, 
            bool forceRefresh = false,
            CancellationToken cancelToken = default(CancellationToken))
        {
            if (collectedHighlights == null || forceRefresh)
            {
                collectedHighlights = new Dictionary<string, Highlight>();

                if (highlightIds == null)
                    throw new ArgumentNullException("highlightIds");

                return Task.Factory.StartNew(() =>
                {
                    cancelToken.Register(() =>
                        {
                            throw new OperationCanceledException(cancelToken);
                        });

                    foreach (string id in highlightIds)
                    {
                        Highlight h = client.Highlights.GetHighlightByIdAsync(
                                        id,
                                        AppContext.AccessToken.Token,
                                        cancelToken).Result;
                        lock (collectedHighlights)
                        {
                            collectedHighlights.Add(h.Id, h);
                        }
                    }

                    return collectedHighlights.Values.ToList();
                }, cancelToken, TaskCreationOptions.None, TaskScheduler.Default);
            }
            else
            {
                return Task.Factory.StartNew(() =>
                    {
                        return collectedHighlights.Values.ToList();
                    }, cancelToken);
            }
        }

        //ToDo: Save showcase items?

        public Task<bool> HasBook(string bookId, bool forceRefresh = false, CancellationToken cancelToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(bookId))
                throw new ArgumentNullException("highlightId");

            if (collectedBooks == null || forceRefresh)
            {
                return LoadCollectedBooksAsync(forceRefresh, cancelToken).ContinueWith(task =>
                    {
                        return collectedBooks.ContainsKey(bookId);
                    }, cancelToken);
            }
            else
            {
                return Task.Factory.StartNew(() =>
                    {
                        return collectedBooks.ContainsKey(bookId);
                    }, cancelToken);
            }
        }


        public Task<bool> HasHighlight(string highlightId, bool forceRefresh = false, CancellationToken cancelToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(highlightId))
                throw new ArgumentNullException("highlightId");

            if (collectedHighlights == null || forceRefresh)
            {
                List<string> ids = TryLoadCollectedHighlightsList(forceRefresh);
                if (ids == null)
                {
                    //This means there are no locally stored ids yet. 
                    //We need to return false.
                    return Task.Factory.StartNew(() =>
                    {
                        return false;
                    }, cancelToken);
                }
                else
                {
                    return LoadCollectedHighlightsAsync(ids, forceRefresh, cancelToken).ContinueWith(task =>
                    {
                        return collectedHighlights.ContainsKey(highlightId);
                    }, cancelToken);
                }
            }
            else
            {
                return Task.Factory.StartNew(() =>
                {
                    return collectedHighlights.ContainsKey(highlightId);
                }, cancelToken);
            }
        }

        public Reading GetReadingForBook(string bookId)
        {
            if (bookId == null)
                throw new ArgumentNullException("bookId can not be null");

            if (bookReadingHash == null)
                throw new InvalidOperationException("Data has not been loaded.");

            return bookReadingHash[bookId];
        }

        //force refresh?

    }
}
