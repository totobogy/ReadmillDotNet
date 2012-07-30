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
    public class ReadingsClient : ReadmillClientBase
    {
        Dictionary<ReadingsUriTemplateType, UriTemplate> readingsUriTemplates;

        #region Url Templates used by UsersClient

        //Uri Template Parameter Constants
        const string ReadingId = "ReadingId";

        //Uri Template Types
        enum ReadingsUriTemplateType { PublicReadings, SingleReading };


        #region Template Strings
        const string singleReadingTemplate = "/readings/{"
            + ReadingsClient.ReadingId
            + "}?client_id={"
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
            + ReadingsQueryOptions.Order
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
                /readings/#{id}/pings, POST
                /readings/#{id}/periods, GET
                /readings/#{id}/locations, GET
                /readings/#{id}/highlights, GET, POST
                /readings/#{id}/comments, GET, POST
             * 
             */

            readingsUriTemplates = new Dictionary<ReadingsUriTemplateType, UriTemplate>();
            readingsUriTemplates.Add(ReadingsUriTemplateType.PublicReadings, new UriTemplate(publicReadingsTemplate, true));
            readingsUriTemplates.Add(ReadingsUriTemplateType.SingleReading, new UriTemplate(singleReadingTemplate, true));
        }

        /// <summary>
        /// Retrieves a list of readings.
        /// </summary>
        /// <param name="options">Query options for retrieving the readings</param>
        /// <returns></returns>
        public Task<List<Reading>> GetReadings(ReadingsQueryOptions options)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();

            parameters.Add(ReadingsQueryOptions.From, options.FromValue);
            parameters.Add(ReadingsQueryOptions.To, options.ToValue);
            parameters.Add(ReadingsQueryOptions.Count, options.CountValue);
            parameters.Add(ReadingsQueryOptions.Order, options.OrderValueInternal);
            parameters.Add(ReadingsQueryOptions.HighlightsCountFrom, options.HighlightsCountFromValue);
            parameters.Add(ReadingsQueryOptions.HighlightsCountTo, options.HighlightsCountToValue);
            parameters.Add(ReadingsQueryOptions.Status, options.StatusValue);

            //Remove extraneous parameters because Readmill doesn't like empty pairs
            foreach (string key in parameters.AllKeys)
            {
                if (string.IsNullOrEmpty(parameters[key]))
                    parameters.Remove(key);
            }

            var readingsUrl = readingsUriTemplates[ReadingsUriTemplateType.PublicReadings].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<List<Reading>>(readingsUrl);
        }


        public Task<Reading> GetReadingById(string readingId)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadingsClient.ReadingId, readingId);

            var readingsUrl = readingsUriTemplates[ReadingsUriTemplateType.SingleReading].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<Reading>(readingsUrl);
        }


        public Task UpdateReading(string accessToken, string readingId, ReadingUpdate updatedReading)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(ReadingsClient.ReadingId, readingId);

            var readingUrl = readingsUriTemplates[ReadingsUriTemplateType.SingleReading].BindByName(this.readmillBaseUri, parameters);

            return PutAsync<ReadingUpdate>(updatedReading, readingUrl);
        }

        public Task DeleteReading(string acessToken, string readingId)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, acessToken);
            parameters.Add(ReadingsClient.ReadingId, readingId);

            var readingUrl = readingsUriTemplates[ReadingsUriTemplateType.SingleReading].BindByName(this.readmillBaseUri, parameters);
            return DeleteAsync(readingUrl);
        }
    }
}
