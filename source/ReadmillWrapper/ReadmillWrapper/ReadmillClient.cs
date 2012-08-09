using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Collections.Specialized;

using Com.Readmill.Api.DataContracts;

namespace Com.Readmill.Api
{
    //ToDo: Improve Errors / Error parsing - html /css
    
    public class ReadmillClient
    {
        public string ClientId { get; set; }
                
        public ReadmillClient(string clientId)
        {
            this.ClientId = clientId;
        }

        UsersClient userClient;
        public UsersClient Users
        {
            get
            {
                if (this.userClient == null)
                    userClient = new UsersClient(this.ClientId);
                
                return userClient;
            }
        }

        ReadingsClient readingsClient;
        public ReadingsClient Readings
        {
            get
            {
                if (this.readingsClient == null)
                    readingsClient = new ReadingsClient(this.ClientId);

                return readingsClient;
            }

        }

        BooksClient booksClient;
        public BooksClient Books
        {
            get
            {
                if (this.booksClient == null)
                    booksClient = new BooksClient(this.ClientId);

                return booksClient;
            }
        }

        HighlightsClient highlightsClient;
        public HighlightsClient Highlights
        {
            get
            {
                if (this.highlightsClient == null)
                    highlightsClient = new HighlightsClient(this.ClientId);

                return highlightsClient;
            }
        }

        CommentsClient commentsClient;
        public CommentsClient Comments
        {
            get
            {
                if (this.commentsClient == null)
                    commentsClient = new CommentsClient(this.ClientId);

                return commentsClient;
            }
        }

    }
    
}
