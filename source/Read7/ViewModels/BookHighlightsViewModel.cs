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
using Com.Readmill.Api.DataContracts;

namespace PhoneApp1.ViewModels
{
    public class BookHighlightsViewModel : BookDetailsViewModel
    {
        private ReadmillClient client;

        public BookHighlightsViewModel(Book selectedBook): base(selectedBook)
        {
            client = new ReadmillClient(AppConstants.ClientId);
        }
    }
}
