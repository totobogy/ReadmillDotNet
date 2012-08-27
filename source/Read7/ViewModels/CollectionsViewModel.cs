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
using Com.Readmill.Api;
using System.ComponentModel;
using System.Collections.Generic;
using Com.Readmill.Api.DataContracts;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Threading;

namespace PhoneApp1.ViewModels
{
    public class CollectionsViewModel
    {
        private ReadmillClient client;

        public List<Book> CollectedBooks { get; private set; }
        public List<Highlight> CollectedHighlights { get; private set; }

        public bool BooksCollectionRefreshNeeded { get; set; }
        public bool HighlightsCollectionRefreshNeeded { get; set; }

        public CollectionsViewModel()
        {
            client = new ReadmillClient(AppContext.ClientId);

            CollectedBooks = new List<Book>();
            CollectedHighlights = new List<Highlight>();

            //Is this the only place a refresh is needed? Use INotification
            BooksCollectionRefreshNeeded = true;
            HighlightsCollectionRefreshNeeded = true;
        }

        public Task LoadCollectedBooksAsync(bool forceRefresh, CancellationToken cancelToken = default(CancellationToken))
        { 
            return 
                AppContext.CurrentUser.LoadCollectedBooksAsync(forceRefresh, cancelToken).ContinueWith(
                loadTask =>
                {
                    CollectedBooks = loadTask.Result;
                    BooksCollectionRefreshNeeded = false;
                }, cancelToken);
        }

        public Task LoadCollectedHighlightsAsync(List<string> highlightIds, bool forceRefresh, CancellationToken cancelToken = default(CancellationToken))
        {
            return
                AppContext.CurrentUser.LoadCollectedHighlightsAsync(highlightIds, forceRefresh, cancelToken).ContinueWith(
                loadTask =>
                {
                    CollectedHighlights = loadTask.Result;
                    HighlightsCollectionRefreshNeeded = false;
                }, cancelToken);
        }
    }
}
