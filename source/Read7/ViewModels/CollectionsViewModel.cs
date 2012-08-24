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

namespace PhoneApp1.ViewModels
{
    public class CollectionsViewModel
    {
        private ReadmillClient client;

        public List<Book> CollectedBooks { get; private set; }
        public List<Highlight> CollectedHighlights { get; private set; }

        public CollectionsViewModel()
        {
            client = new ReadmillClient(AppContext.ClientId);

            CollectedBooks = new List<Book>();
            CollectedHighlights = new List<Highlight>();

            //LoadCollectedBooksAsync();

            //readableBooks = new Dictionary<string, Book>();
            //ListState = State.Unloaded;
        }

        public Task LoadCollectedBooksAsync()
        { 
            return 
                AppContext.CurrentUser.LoadCollectedBooksAsync().ContinueWith(loadTask =>
                {
                    CollectedBooks = loadTask.Result;
                });
        }

        public List<string> TryLoadCollectedHighlightsList()
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

        public Task LoadCollectedHighlightsAsync(List<string> highlightIds)
        {
            if (highlightIds != null)
            {
                return Task.Factory.StartNew(() =>
                    {
                        foreach (string id in highlightIds)
                        {
                            Highlight h = 
                                client.Highlights.GetHighlightByIdAsync(id, AppContext.AccessToken.Token).Result;

                            CollectedHighlights.Add(h);
                        }
                    });
            }
            else
                return null;
        }
    }
}
