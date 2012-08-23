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
using Com.Readmill.Api;
using Com.Readmill.Api.DataContracts;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PhoneApp1.ViewModels
{
    public class BookHighlightsViewModel : BookDetailsViewModel
    {
        private ReadmillClient client;

        public IList<Highlight> BookHighlights { get; private set; }

        public BookHighlightsViewModel(Book selectedBook): base(selectedBook)
        {
            client = new ReadmillClient(AppContext.ClientId);
        }

        public Task LoadBookHighlightsAsync()
        {
            IDictionary<decimal, Highlight> highlights = new Dictionary<decimal, Highlight>();
            
            ReadingsQueryOptions readingOptions = new ReadingsQueryOptions() { CountValue = 100 };
            RangeQueryOptions highlightOptions = new RangeQueryOptions() { CountValue = 100 };

            //Get all readings
            return Task.Factory.StartNew(() =>
            {
                List<Reading> readings = client.Books.GetBookReadingsAsync(SelectedBook.Id, readingOptions).Result;

                foreach (Reading reading in readings)
                {
                    //foreach reading, Get all Highlights
                    foreach (Highlight h in client.Readings.GetReadingHighlightsAsync(reading.Id, highlightOptions).Result)
                    {
                        //ToDo: Better heuristics? Remove duplicates?
                        if (h.Content.Length >= 20)
                            if (!highlights.ContainsKey(h.Locators.Position))
                                highlights.Add(h.Locators.Position, h);
                    }
                }

                BookHighlights = highlights.Values.ToList<Highlight>();                              
            });           
        }
    }
}
