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
using Com.Readmill.Api.DataContracts;
using System.Threading.Tasks;
using Com.Readmill.Api;

namespace PhoneApp1.ViewModels
{
    public class BookDetailsViewModel
    {
        private ReadmillClient client;

        //used to represnt this user's reading of this book
        private Reading bookReading;

        public Book SelectedBook { get; private set; }

        public string DisplayTitle
        {
            get
            {
                return SelectedBook.Title + "\nby " + SelectedBook.Author;
            }
        }

        public string CoverUrl
        {
            get
            {
                return SelectedBook.CoverUrl;
            }
        }

        public string Story
        {
            get
            {
                return SelectedBook.Story;
            }
        }

        public BookDetailsViewModel(Book selectedBook)
        {
            this.client = new ReadmillClient(AppContext.ClientId);
            this.SelectedBook = selectedBook;

            //find out if a reading exists for this book

        }

        public Task LikeBookAsync()
        {
            //if reading already exists, update?
            return
                client.Books.PostBookReadingAsync
                (AppContext.AccessToken.Token,
                  SelectedBook.Id,
                    Reading.ReadingState.Interesting).ContinueWith(task =>
                        {
                            string readingLink = task.Result;
                            bookReading = client.Readings.GetFromPermalinkAsync<Reading>(readingLink, AppContext.AccessToken.Token).Result;
                        });
        }

        public Task UnlikeBookAsync()
        {
            if (bookReading == null)
                throw new InvalidOperationException("Reading not set for this book for this user");
            return
                client.Readings.DeleteReadingAsync(AppContext.AccessToken.Token, bookReading.Id);
        }

        // Is liked?
    }
}
