using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using Com.Readmill.Api.DataContracts;
using System.Collections.Specialized;
using Com.Readmill.Api;
using System.Threading.Tasks;
using System.Security;

namespace Com.Readmill.Api
{
    public class BooksClient : ReadmillClientBase
    {
        Dictionary<BooksUriTemplateType, UriTemplate> booksUriTemplates;

        #region Url Templates used by ReadingsClient

        //Uri Template Parameter Constants
        const string BookId = "BookId";
        const string SearchQuery = "SearchQuery";

        //Uri Template Types
        enum BooksUriTemplateType { Books, BooksMatch, SingleBook, BookReadings };


        #region Template Strings
        const string booksTemplate = "/books/?client_id={"
            + ReadmillConstants.ClientId
            + "}&access_token={"
            + ReadmillConstants.AccessToken
            + "}&q={"
            + BooksQueryOptions.SearchString
            + "}&count={"
            + BooksQueryOptions.Count
            + "}";

        const string booksMatchTemplate = "/books/match?client_id={"
            + ReadmillConstants.ClientId
            + "}&access_token={"
            + ReadmillConstants.AccessToken
            + "}&q[title]={"
            + BookMatchOptions.Title
            + "}&q[author]={"
            + BookMatchOptions.Author
            + "}&q[isbn]={"
            + BookMatchOptions.ISBN
            + "}";

        const string singleBookTemplate = "/books/{"
            + BooksClient.BookId
            + "}?client_id={"
            + ReadmillConstants.ClientId
            + "}";

        //bookReadingsTemplate

        #endregion

        #endregion

        /// <summary>
        /// Instantiates a client for the Readmill/Users api
        /// </summary>
        /// <param name="clientId">Client Id of the application, assgined by Readmill when the app is registered</param>
        public BooksClient(string clientId)
            : base(clientId)
        {

        }

        override protected void LoadTemplates()
        {
            booksUriTemplates = new Dictionary<BooksUriTemplateType, UriTemplate>();
            booksUriTemplates.Add(BooksUriTemplateType.Books, new UriTemplate(booksTemplate, true));
            booksUriTemplates.Add(BooksUriTemplateType.BooksMatch, new UriTemplate(booksMatchTemplate, true));
            booksUriTemplates.Add(BooksUriTemplateType.SingleBook, new UriTemplate(singleBookTemplate, true));
            //bookReadingTemplate
        }

        /// <summary>
        /// Retrieves a list of readings.
        /// </summary>
        /// <param name="options">Query options for retrieving the readings</param>
        /// <returns></returns>
        public Task<List<Book>> GetBooksAsync(BooksQueryOptions options)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();

            parameters.Add(BooksQueryOptions.Count, options.CountValue.ToString());
            parameters.Add(BooksQueryOptions.SearchString, options.SearchStringValue);

            //Remove extraneous parameters because Readmill doesn't like empty pairs
            foreach (string key in parameters.AllKeys)
            {
                if (string.IsNullOrEmpty(parameters[key]))
                    parameters.Remove(key);
            }

            var booksUrl = booksUriTemplates[BooksUriTemplateType.Books].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<List<Book>>(booksUrl);
        }

        public Task<Book> GetBookByIdAsync(string bookId)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(BooksClient.BookId, bookId);

            var booksUrl = booksUriTemplates[BooksUriTemplateType.SingleBook].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<Book>(booksUrl);
        }

        public Task<Book> GetBestMatchAsync(BookMatchOptions options)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();

            parameters.Add(BookMatchOptions.ISBN, options.ISBNValue);
            parameters.Add(BookMatchOptions.Title, options.TitleValue);
            parameters.Add(BookMatchOptions.Author, options.AuthorValue);

            //Remove extraneous parameters because Readmill doesn't like empty pairs
            foreach (string key in parameters.AllKeys)
            {
                if (string.IsNullOrEmpty(parameters[key]))
                    parameters.Remove(key);
            }

            var booksUrl = booksUriTemplates[BooksUriTemplateType.BooksMatch].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<Book>(booksUrl);

        }

        public Task AddBookAsync(string accessToken, Book newBook)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);

            var booksUrl = booksUriTemplates[BooksUriTemplateType.Books].BindByName(this.readmillBaseUri, parameters);

            return PostAsync<Book>(newBook, booksUrl);
        }

        /*
         * #Tags for highlights - useful for quotable quotes like apps
         * Social api / better Users api: get followers / following, search for people by name
         * More powerful search / metadata for books - categories / genres etc?
         * Get latest highlights, especially from my social graph
         * Basically the idea is to let people discover what to read?
         */
    }
}
