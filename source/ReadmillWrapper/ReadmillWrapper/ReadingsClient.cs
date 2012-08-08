using System;
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
using System.Xml;

namespace Com.Readmill.Api
{
    public class ReadingsClient : ReadmillClientBase
    {
        Dictionary<ReadingsUriTemplateType, UriTemplate> readingsUriTemplates;

        #region Url Templates used by ReadingsClient

        //Uri Template Parameter Constants
        const string ReadingId = "ReadingId";

        //Uri Template Types
        enum ReadingsUriTemplateType { PublicReadings, SingleReading, ReadingPing, ReadingPeriods, ReadingHighlights, ReadingComments };


        #region Template Strings
        const string singleReadingTemplate = "/readings/{"
            + ReadingsClient.ReadingId
            + "}?client_id={"
            + ReadmillConstants.ClientId
            + "}&access_token={"
            + ReadmillConstants.AccessToken
            + "}";

        const string readingPingTemplate = "/readings/{"
            + ReadingsClient.ReadingId
            + "}/pings?client_id={"
            + ReadmillConstants.ClientId
            + "}&access_token={"
            + ReadmillConstants.AccessToken
            + "}";

        const string readingHighlightsTemplate = "/readings/{"
            + ReadingsClient.ReadingId
            + "}/highlights?client_id={"
            + ReadmillConstants.ClientId
            + "}&access_token={"
            + ReadmillConstants.AccessToken
            + "}&from={"
            + ReadingsQueryOptions.From
            + "}&to={"
            + ReadingsQueryOptions.To
            + "}&count={"
            + ReadingsQueryOptions.Count
            + "}";

        const string readingCommentsTemplate = "/readings/{"
            + ReadingsClient.ReadingId
            + "}/comments?client_id={"
            + ReadmillConstants.ClientId
            + "}&access_token={"
            + ReadmillConstants.AccessToken
            + "}&from={"
            + ReadingsQueryOptions.From
            + "}&to={"
            + ReadingsQueryOptions.To
            + "}&count={"
            + ReadingsQueryOptions.Count
            + "}";

        const string readingPeriodsTemplate = "/readings/{"
            + ReadingsClient.ReadingId
            + "}/periods?client_id={"
            + ReadmillConstants.ClientId
            + "}&access_token={"
            + ReadmillConstants.AccessToken
            + "}";

        const string publicReadingsTemplate = "/readings?client_id={"
            + ReadmillConstants.ClientId
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
        public ReadingsClient(string clientId)
            : base(clientId)
        {

        }

        override protected void LoadTemplates()
        {
            /*  ToDo:
             * ---------------------------------
                /readings/#{id}/periods, GET
                /readings/#{id}/locations, GET
             * 
             */
            readingsUriTemplates = new Dictionary<ReadingsUriTemplateType, UriTemplate>();
            readingsUriTemplates.Add(ReadingsUriTemplateType.PublicReadings, new UriTemplate(publicReadingsTemplate, true));
            readingsUriTemplates.Add(ReadingsUriTemplateType.SingleReading, new UriTemplate(singleReadingTemplate, true));
            readingsUriTemplates.Add(ReadingsUriTemplateType.ReadingPing, new UriTemplate(readingPingTemplate, true));
            readingsUriTemplates.Add(ReadingsUriTemplateType.ReadingHighlights, new UriTemplate(readingHighlightsTemplate, true));
            readingsUriTemplates.Add(ReadingsUriTemplateType.ReadingComments, new UriTemplate(readingCommentsTemplate, true));
        }

        /// <summary>
        /// Retrieves a list of readings.
        /// </summary>
        /// <param name="options">Query options for retrieving the readings</param>
        /// <returns></returns>
        public Task<List<Reading>> GetReadingsAsync(ReadingsQueryOptions options)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();

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
            foreach (string key in parameters.AllKeys)
            {
                if (string.IsNullOrEmpty(parameters[key]))
                    parameters.Remove(key);
            }

            var readingsUrl = readingsUriTemplates[ReadingsUriTemplateType.PublicReadings].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<List<Reading>>(readingsUrl);
        }

        public Task<Reading> GetReadingByIdAsync(string readingId)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadingsClient.ReadingId, readingId);

            var readingsUrl = readingsUriTemplates[ReadingsUriTemplateType.SingleReading].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<Reading>(readingsUrl);
        }

        /// <returns>permalink to the updated resource</returns>
        public Task<string> UpdateReadingAsync(string accessToken, string readingId, ReadingUpdategram updatedReading)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(ReadingsClient.ReadingId, readingId);

            ReadingUpdate wrappedUpdate = new ReadingUpdate();
            wrappedUpdate.ReadingUpdategram = updatedReading;

            var readingUrl = readingsUriTemplates[ReadingsUriTemplateType.SingleReading].BindByName(this.readmillBaseUri, parameters);

            return PutAsync<ReadingUpdate>(wrappedUpdate, readingUrl);
        }

        public Task DeleteReadingAsync(string accessToken, string readingId)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(ReadingsClient.ReadingId, readingId);

            var readingUrl = readingsUriTemplates[ReadingsUriTemplateType.SingleReading].BindByName(this.readmillBaseUri, parameters);
            return DeleteAsync(readingUrl);
        }
        
        public Task<List<Highlight>> GetReadingHighlightsAsync(string readingId, RangeQueryOptions options)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadingsClient.ReadingId, readingId);

            if (options != null)
            {
                parameters.Add(RangeQueryOptions.From, options.FromValue);
                parameters.Add(RangeQueryOptions.To, options.ToValue);
                parameters.Add(RangeQueryOptions.Count, options.CountValue.ToString());
            }

            //Remove extraneous parameters because Readmill doesn't like empty pairs
            foreach (string key in parameters.AllKeys)
            {
                if (string.IsNullOrEmpty(parameters[key]))
                    parameters.Remove(key);
            }

            var highlightsUrl = readingsUriTemplates[ReadingsUriTemplateType.ReadingHighlights].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<List<Highlight>>(highlightsUrl);
            
        }

        /// <returns>permalink to the created resource</returns>
        public Task<string> PostReadingHighlightAsync(string accessToken, string readingId, Highlight highlight)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(ReadingsClient.ReadingId, readingId);

            //Wrap in InternalHighlight
            var wrappedHighlight = new WrappedHighlight() { Highlight = highlight};

            var highlightUrl = readingsUriTemplates[ReadingsUriTemplateType.ReadingHighlights].BindByName(this.readmillBaseUri, parameters);
            return PostAsync<WrappedHighlight>(wrappedHighlight, highlightUrl);
        }

        public Task<List<Comment>> GetReadingCommentsAsync(string readingId, RangeQueryOptions options)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadingsClient.ReadingId, readingId);

            if (options != null)
            {
                parameters.Add(RangeQueryOptions.From, options.FromValue);
                parameters.Add(RangeQueryOptions.To, options.ToValue);
                parameters.Add(RangeQueryOptions.Count, options.CountValue.ToString());
            }

            //Remove extraneous parameters because Readmill doesn't like empty pairs
            foreach (string key in parameters.AllKeys)
            {
                if (string.IsNullOrEmpty(parameters[key]))
                    parameters.Remove(key);
            }

            var commentsUrl = readingsUriTemplates[ReadingsUriTemplateType.ReadingComments].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<List<Comment>>(commentsUrl);
        }

        /// <returns>permalink to the created resource</returns>
        public Task<string> PostReadingCommentAsync(string accessToken, string readingId, Comment comment)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(ReadingsClient.ReadingId, readingId);

            //Wrap in InternalHighlight
            var wrappedComment = new WrappedComment() { Comment = comment };

            var commentUrl = readingsUriTemplates[ReadingsUriTemplateType.ReadingComments].BindByName(this.readmillBaseUri, parameters);
            return PostAsync<WrappedComment>(wrappedComment, commentUrl);
        }

        public ReadingSession GetReadingSession(string accessToken, string readingId)
        {
            return new ReadingSession(accessToken, readingId, this);
        }

        /// <returns>permalink to the created resource</returns>
        public Task<string> PostReadingPingAsync(string accessToken, string readingId, Ping ping)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();

            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(ReadingsClient.ReadingId, readingId);

            var pingUrl = readingsUriTemplates[ReadingsUriTemplateType.ReadingPing].BindByName(this.readmillBaseUri, parameters);

            var wrappedPing = new WrappedPing();
            wrappedPing.Ping = ping;

            return PostAsync<WrappedPing>(wrappedPing, pingUrl);
        }

    }
}
