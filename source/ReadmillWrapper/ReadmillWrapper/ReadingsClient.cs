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
        enum ReadingsUriTemplateType { PublicReadings, SingleReading, ReadingPing, ReadingPeriods };


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
                /readings/#{id}/periods, GET
                /readings/#{id}/locations, GET
                /readings/#{id}/highlights, GET, POST
                /readings/#{id}/comments, GET, POST
             * 
             */
            readingsUriTemplates = new Dictionary<ReadingsUriTemplateType, UriTemplate>();
            readingsUriTemplates.Add(ReadingsUriTemplateType.PublicReadings, new UriTemplate(publicReadingsTemplate, true));
            readingsUriTemplates.Add(ReadingsUriTemplateType.SingleReading, new UriTemplate(singleReadingTemplate, true));
            readingsUriTemplates.Add(ReadingsUriTemplateType.ReadingPing, new UriTemplate(readingPingTemplate, true));
        }

        /// <summary>
        /// Retrieves a list of readings.
        /// </summary>
        /// <param name="options">Query options for retrieving the readings</param>
        /// <returns></returns>
        public Task<List<Reading>> GetReadingsAsync(ReadingsQueryOptions options)
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

        public Task<Reading> GetReadingByIdAsync(string readingId)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadingsClient.ReadingId, readingId);

            var readingsUrl = readingsUriTemplates[ReadingsUriTemplateType.SingleReading].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<Reading>(readingsUrl);
        }

        public Task UpdateReadingAsync(string accessToken, string readingId, ReadingUpdategram updatedReading)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(ReadingsClient.ReadingId, readingId);

            ReadingUpdate wrappedUpdate = new ReadingUpdate();
            wrappedUpdate.ReadingUpdategram = updatedReading;

            var readingUrl = readingsUriTemplates[ReadingsUriTemplateType.SingleReading].BindByName(this.readmillBaseUri, parameters);

            return PutAsync<ReadingUpdate>(wrappedUpdate, readingUrl);
        }

        public Task DeleteReadingAsync(string acessToken, string readingId)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, acessToken);
            parameters.Add(ReadingsClient.ReadingId, readingId);

            var readingUrl = readingsUriTemplates[ReadingsUriTemplateType.SingleReading].BindByName(this.readmillBaseUri, parameters);
            return DeleteAsync(readingUrl);
        }


        public ReadingSession GetReadingSession(string accessToken, string readingId)
        {
            return new ReadingSession(accessToken, readingId, this);
        }

        public Task SendReadingPingAsync(string accessToken, string readingId, Ping ping)
        {
            NameValueCollection parameters = GetInitializedParameterCollection();

            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(ReadingsClient.ReadingId, readingId);

            var pingUrl = readingsUriTemplates[ReadingsUriTemplateType.ReadingPing].BindByName(this.readmillBaseUri, parameters);

            var wrappedPing = new ReadingPing();
            wrappedPing.Ping = ping;

            return PostAsync<ReadingPing>(wrappedPing, pingUrl);
        }

    }


    public class ReadingSession
    {
        private string sessionId;
        private string accessToken;
        private string readingId;
        private ReadingsClient client;

        private DateTime lastPingTime;

        public ReadingSession(string accessToken, string readingId, ReadingsClient readingsClient)
        {
            this.client = readingsClient;
            this.accessToken = accessToken;
            this.readingId = readingId;

            this.sessionId = Guid.NewGuid().ToString();
        }

        public Task Ping(float progress, bool sendDuration = true, bool sendOccuredAt = true)
        {
            Ping ping = new Ping();
            ping.SessionId = this.sessionId;

            //ToDo: Shouldn't be less than last progress?
            ping.Progress = progress;

            if (sendOccuredAt)
                ping.OccuredAt = XmlConvert.ToString(lastPingTime = DateTime.Now);

            if (sendDuration)
                ping.Duration = (DateTime.Now - lastPingTime).Seconds;

            return this.client.SendReadingPingAsync(this.accessToken, this.readingId, ping);
        }

        public Task Ping(float progress, float latitude, float longitude, bool sendDuration = true, bool sendOccuredAt = true)
        {
            Ping ping = new Ping();
            ping.SessionId = this.sessionId;

            //ToDo: Shouldn't be less than last progress?
            ping.Progress = progress;

            ping.Latitude = latitude;
            ping.Longitude = longitude;

            if (sendOccuredAt)
                ping.OccuredAt = XmlConvert.ToString(lastPingTime = DateTime.Now);

            if (sendDuration)
                ping.Duration = (DateTime.Now - lastPingTime).Seconds;

            return this.client.SendReadingPingAsync(this.accessToken, this.readingId, ping);
        }

        public void Close()
        {

        }
    }
}
