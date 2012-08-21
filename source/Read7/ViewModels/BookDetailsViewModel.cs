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

namespace PhoneApp1.ViewModels
{
    public class BookDetailsViewModel
    {
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
            this.SelectedBook = selectedBook;
        }
    }
}
