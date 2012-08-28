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
using System.Threading;

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

        public Task<bool> IsCollectedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return
                AppContext.CurrentUser.HasBook(SelectedBook.Id, cancelToken: cancellationToken);
        }

        public Task LikeBookAsync()
        {
            //if reading already exists, update?
                return IsCollectedAsync().ContinueWith(isCollected =>
                    {
                        if (!isCollected.Result)
                        {
                                string readingLink =
                                    client.Books.PostBookReadingAsync(
                                    AppContext.AccessToken.Token,
                                    SelectedBook.Id,
                                    Reading.ReadingState.Interesting).Result;

                                AppContext.CurrentUser.AddBookToLocalCollection(SelectedBook);

                                bookReading = client.Readings.GetFromPermalinkAsync<Reading>(
                                    readingLink,
                                    AppContext.AccessToken.Token).Result;
                        }
                    });
        }

        public Task UnlikeBookAsync()
        {
            if (bookReading == null)
            {
                //this could happen if the book was already liked from before
                return IsCollectedAsync().ContinueWith(isCollected =>
                    {
                        if (isCollected.Result)
                        {
                            bookReading = AppContext.CurrentUser.GetReadingForBook(SelectedBook.Id);

                            client.Readings.DeleteReadingAsync(
                                AppContext.AccessToken.Token,
                                bookReading.Id).Wait();

                            AppContext.CurrentUser.RemoveBookFromLocalCollection(SelectedBook.Id);
                        }
                        else
                        {
                            throw new InvalidOperationException(AppStrings.NoReadingEx);
                        }
                    });
            }
            else
            {
                return
                    client.Readings.DeleteReadingAsync(
                    AppContext.AccessToken.Token,
                    bookReading.Id).ContinueWith(task =>
                        {
                            if(task.IsCompleted)
                                AppContext.CurrentUser.RemoveBookFromLocalCollection(SelectedBook.Id);
                        });
            }
        }
    }
}
