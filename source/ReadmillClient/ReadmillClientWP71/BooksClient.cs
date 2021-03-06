﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Net.Http;
using System.Runtime.Serialization.Json;
using Com.Readmill.Api.DataContracts;
using System.Collections.Specialized;
using Com.Readmill.Api;
using System.Threading.Tasks;
using System.Security;
using System.Threading;

namespace Com.Readmill.Api
{
    public class BooksClient : ReadmillClientBase
    {
        Dictionary<BooksUriTemplateType, UriTemplate> booksUriTemplates;

        #region Url Templates used by BooksClient

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

        const string bookReadingsTemplate = "/books/{"
            + BooksClient.BookId
            + "}/readings?client_id={"
            + ReadmillConstants.ClientId
            + "}&access_token={"
            + ReadmillConstants.AccessToken
            + "}&from={"
            + ReadingsQueryOptions.From
            + "}&to={"
            + ReadingsQueryOptions.To
            + "}&count={"
            + ReadingsQueryOptions.Count
            + "}&order={"
            + ReadingsQueryOptions.OrderBy
            + "}&filter={"
            + ReadingsQueryOptions.Filter
            + "}&highlights_count[from]={"
            + ReadingsQueryOptions.HighlightsCountFrom
            + "}&highlights_count[to]={"
            + ReadingsQueryOptions.HighlightsCountTo
            + "}&status={"
            + ReadingsQueryOptions.Status
            + "}";

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
            booksUriTemplates.Add(BooksUriTemplateType.BookReadings, new UriTemplate(bookReadingsTemplate, true));
        }

        /// <summary>
        /// Retrieves a list of books.
        /// </summary>
        /// <param name="options">Query options for retrieving the books</param>
        /// <returns></returns>
        public Task<List<Book>> GetBooksAsync(BooksQueryOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            IDictionary<string,string> parameters = GetInitializedParameterCollection();

            if (options != null)
            {
                parameters.Add(BooksQueryOptions.Count, options.CountValue.ToString());
                parameters.Add(BooksQueryOptions.SearchString, options.SearchStringValue);
            }

            //Remove extraneous parameters because Readmill doesn't like empty pairs
            IDictionary<string, string> tmpParams = new Dictionary<string, string>();
            foreach (string key in parameters.Keys)
            {
                if (!string.IsNullOrEmpty(parameters[key]))
                    tmpParams.Add(key, parameters[key]);
            }
            parameters = tmpParams;

            var booksUrl = booksUriTemplates[BooksUriTemplateType.Books].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<List<Book>>(booksUrl, cancellationToken);
        }

        public Task<Book> GetBookByIdAsync(string bookId, CancellationToken cancellationToken = default(CancellationToken))
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();
            parameters.Add(BooksClient.BookId, bookId);

            var booksUrl = booksUriTemplates[BooksUriTemplateType.SingleBook].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<Book>(booksUrl, cancellationToken);
        }

        public Task<Book> GetBestMatchAsync(BookMatchOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();

            //if (options != null)
            //{
                parameters.Add(BookMatchOptions.ISBN, options.ISBNValue);
                parameters.Add(BookMatchOptions.Title, options.TitleValue);
                parameters.Add(BookMatchOptions.Author, options.AuthorValue);
            //}

            //Remove extraneous parameters because Readmill doesn't like empty pairs
            IDictionary<string, string> tmpParams = new Dictionary<string, string>();
            foreach (string key in parameters.Keys)
            {
                if (!string.IsNullOrEmpty(parameters[key]))
                    tmpParams.Add(key, parameters[key]);
            }
            parameters = tmpParams;

            var booksUrl = booksUriTemplates[BooksUriTemplateType.BooksMatch].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<Book>(booksUrl, cancellationToken);

        }

        /// <summary>
        /// Get readings associated with the specified book
        /// </summary>
        /// <param name="bookId">Readmill Id of the book for which readings need to be retrieved</param>
        /// <param name="?"></param>
        /// <returns></returns>
        public Task<List<Reading>> GetBookReadingsAsync(
            string bookId, 
            ReadingsQueryOptions options = null, 
            string accessToken = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();
            parameters.Add(BooksClient.BookId, bookId);
            parameters.Add(ReadmillConstants.AccessToken, accessToken);

            if (options != null)
            {
                parameters.Add(ReadingsQueryOptions.From, options.FromValue);
                parameters.Add(ReadingsQueryOptions.To, options.ToValue);
                parameters.Add(ReadingsQueryOptions.Count, options.CountValue.ToString());
                parameters.Add(ReadingsQueryOptions.OrderBy, options.OrderByValue);
                parameters.Add(ReadingsQueryOptions.HighlightsCountFrom, options.HighlightsCountFromValue);
                parameters.Add(ReadingsQueryOptions.HighlightsCountTo, options.HighlightsCountToValue);
                parameters.Add(ReadingsQueryOptions.Status, options.StatusValue);
            }

            //Remove extraneous parameters because Readmill doesn't like empty pairs
            IDictionary<string, string> tmpParams = new Dictionary<string, string>();
            foreach (string key in parameters.Keys)
            {
                if (!string.IsNullOrEmpty(parameters[key]))
                    tmpParams.Add(key, parameters[key]);
            }
            parameters = tmpParams;

            var bookReadingsUrl = booksUriTemplates[BooksUriTemplateType.BookReadings].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<List<Reading>>(bookReadingsUrl, cancellationToken);
        }

        public Task<string> PostBookAsync(string accessToken, Book newBook)
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);

            var booksUrl = booksUriTemplates[BooksUriTemplateType.Books].BindByName(this.readmillBaseUri, parameters);

            return PostAsync<Book>(newBook, booksUrl);
        }

        public Task<string> PostBookReadingAsync(string accessToken, string bookId, Reading.ReadingState state, bool isPrivate = false, string closingRemark = null)
        {
            ReadingPost newReading= new ReadingPost();
            
            //newReading.AccessToken = accessToken;

            newReading.Reading = new Reading();
            newReading.Reading.State = state;
            newReading.Reading.IsPrivate = isPrivate;
            newReading.Reading.ClosingRemark = closingRemark;

            IDictionary<string, string> parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(BooksClient.BookId, bookId);

            var bookReadingUrl = booksUriTemplates[BooksUriTemplateType.BookReadings].BindByName(this.readmillBaseUri, parameters);
            return PostAsync<ReadingPost>(newReading, bookReadingUrl);

        }

    }
}
