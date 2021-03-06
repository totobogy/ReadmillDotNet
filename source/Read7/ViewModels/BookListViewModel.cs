﻿using System;
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
using System.Threading;
using System.Runtime.Serialization;

namespace PhoneApp1.ViewModels
{
    public class BookListViewModel : INotifyPropertyChanged
    {
        private ReadmillClient client;
        private IDictionary<string, Book> readableBooks;

        public ICollection<Book> BookList { get; private set; }

        private string bookListTitle = "just read";
        public String BookListTitle 
        {
            get
            {
                return bookListTitle;
            }
            private set
            {
                if (value != bookListTitle)
                {
                    bookListTitle = value;
                    NotifyPropertyChanged("BookListTitle");
                }
            } 
        }

        public enum State { Disconnected, Unloaded, RecentlyRead, SearchResult};
        private State listState;
        public State ListState
        {
            get
            {
                return listState;
            }

            private set
            {
                if (value != listState)
                {
                    listState = value;
                    switch (value)
                    {
                        case State.RecentlyRead:
                            BookListTitle = "just read";
                            break;
                        case State.SearchResult:
                            BookListTitle = "search";
                            break;
                        default:
                            BookListTitle = "just read";
                            break;
                    }
                }
            }
        }

        public BookListViewModel()
        {
            Initialize();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            Initialize();
        }  

        private void Initialize()
        {
            client = new ReadmillClient(AppContext.ClientId);
            readableBooks = new Dictionary<string, Book>();
            ListState = State.Unloaded;
        }

        public Task LoadRecentlyReadBooksAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            //BooksQueryOptions booksOptions = new BooksQueryOptions() { CountValue = 25 };
            ReadingsQueryOptions readingsOptions = new ReadingsQueryOptions() { CountValue = 50 };

            Task<List<Reading>> readingsTask = client.Readings.GetReadingsAsync(readingsOptions, cancelToken);

            return readingsTask.ContinueWith(task =>
            {
                foreach (Reading r in task.Result)
                {
                    if (!readableBooks.ContainsKey(r.Book.Id))
                        readableBooks.Add(r.Book.Id, r.Book);
                }

                BookList = readableBooks.Values;
                ListState = State.RecentlyRead;
            });
        }

        public Task SearchBooksAsync(string searchString, CancellationToken cancelToken = default(CancellationToken))
        {
            BooksQueryOptions booksOptions = new BooksQueryOptions() { SearchStringValue = searchString, CountValue = 100 };
            
            Task<List<Book>> booksTask = client.Books.GetBooksAsync(booksOptions, cancelToken);
            
            return booksTask.ContinueWith(task =>
            {
                BookList = task.Result;
                ListState = State.SearchResult;
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                if (Deployment.Current.Dispatcher.CheckAccess())
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    });
                }
            }
        }
    }
}
