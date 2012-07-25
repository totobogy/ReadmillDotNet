using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using Com.Readmill.Api.DataContracts;
using System.Collections.Specialized;

namespace Com.Readmill.Api
{
    //ToDo: USer Async? 
    //ToDo: Data types - not everything string
    //ToDo: Clean up DC names
    //ToDo: Refactor base class for client?
    public class UserClient : ReadmillClient
    {
        HttpClient httpClient;
        Dictionary<UserUriTemplateType, UriTemplate> userUriTemplates;

        #region Url Templates used by UserClient

        //Uri Template Parameter Constants
        const string UserId = "UserId";

        //Uri Template Types
        enum UserUriTemplateType { Owner, Users, UserReadings };


        #region Template Strings
        const string ownerTemplate = "/me?client_id={"
            + ReadmillConstants.ClientId
            + "}&access_token={"
            + ReadmillConstants.AccessToken
            + "}";

        const string usersTemplate = "/users/{"
            + UserClient.UserId
            + "}?client_id={"
            + ReadmillConstants.ClientId
            + "}";

        const string userReadingsTemplate = "/users/{"
            + UserClient.UserId
            + "}/readings?client_id={"
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
        public UserClient(string clientId)
        {
            this.ClientId = clientId;
            httpClient = new HttpClient();

            //Populate Templates
            CreateTemplates();
        }

        private void CreateTemplates()
        {
            userUriTemplates = new Dictionary<UserUriTemplateType, UriTemplate>();
            userUriTemplates.Add(UserUriTemplateType.Owner, new UriTemplate(ownerTemplate, true));
            userUriTemplates.Add(UserUriTemplateType.Users, new UriTemplate(usersTemplate, true));
            userUriTemplates.Add(UserUriTemplateType.UserReadings, new UriTemplate(userReadingsTemplate, true));
        }

        private User GetUserByUrl(Uri userUrl)
        {
            User user = null;

            httpClient.GetAsync(userUrl).ContinueWith(
                (requestTask) =>
                {
                    // Get HTTP response from completed task. 
                    HttpResponseMessage response = requestTask.Result;

                    // Check that response was successful or throw exception 
                    response.EnsureSuccessStatusCode();

                    //Read as stream
                    response.Content.ReadAsStreamAsync().ContinueWith(
                        (readTask) =>
                        {
                            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(User));
                            user = (User)ser.ReadObject(readTask.Result);
                        }).Wait();
                }).Wait();

            return user;
        }

        /// <summary>
        /// Retrieves the representation of the user corresponding to the authentication token.
        /// </summary>
        /// <param name="accessToken">authentication token</param>
        /// <returns>representation of the user corresponding to the authentication token</returns>
        public User GetOwner(string accessToken)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add(ReadmillConstants.ClientId, this.ClientId);
            parameters.Add(ReadmillConstants.AccessToken, accessToken);

            Uri uri = userUriTemplates[UserUriTemplateType.Owner].BindByName(new Uri(this.readmillBaseUrl), parameters);

            return GetUserByUrl(uri);
        }

        /// <summary>
        /// Retrieves the representation of a single user by id
        /// </summary>
        /// <param name="userId">The Readmill user-id of the user you want to retrieve.</param>
        /// <returns></returns>
        public User GetUserById(string userId)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add(ReadmillConstants.ClientId, this.ClientId);
            parameters.Add(UserClient.UserId, userId);

            return GetUserByUrl(userUriTemplates[UserUriTemplateType.Users].BindByName(new Uri(this.readmillBaseUrl), parameters));
        }

        /// <summary>
        /// Retrieves a list of readings corresponding to the user with the specified ID.
        /// </summary>
        /// <param name="userId">Readmill user-id of the user whose readings you want to retrieve</param>
        /// <param name="options">Query options for retrieving the readings</param>
        /// <returns></returns>
        public List<Reading> GetUserReadings(string userId, ReadingsQueryOptions options)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add(ReadmillConstants.ClientId, this.ClientId);
            parameters.Add(UserClient.UserId, userId);
            
            //ToDo: Write an extension method on UriTemplate class to ignore null params from final uri
            
            if(!string.IsNullOrEmpty(options.FromValue))
                parameters.Add(ReadingsQueryOptions.From, options.FromValue);

            if (!string.IsNullOrEmpty(options.ToValue))
                parameters.Add(ReadingsQueryOptions.To, options.ToValue);

            if (!string.IsNullOrEmpty(options.CountValue))
                parameters.Add(ReadingsQueryOptions.Count, options.CountValue);

            if (!string.IsNullOrEmpty(options.OrderValueInternal))
                parameters.Add(ReadingsQueryOptions.Order, options.OrderValueInternal);

            if (!string.IsNullOrEmpty(options.HighlightsCountFromValue))
                parameters.Add(ReadingsQueryOptions.HighlightsCountFrom, options.HighlightsCountFromValue);

            if (!string.IsNullOrEmpty(options.HighlightsCountToValue))
                parameters.Add(ReadingsQueryOptions.HighlightsCountTo, options.HighlightsCountToValue);

            if (!string.IsNullOrEmpty(options.StatusValue))
            parameters.Add(ReadingsQueryOptions.Status, options.StatusValue);

            var readingsUrl = userUriTemplates[UserUriTemplateType.UserReadings].BindByName(new Uri(this.readmillBaseUrl), parameters);
            return GetReadingsByUrl(readingsUrl);

        }

        /// <summary>
        /// Retrieves a list of readings from a fully-formed url
        /// </summary>
        /// <param name="readingsUrl">Fully formed url (including authentication information)</param>
        /// <returns></returns>
        public List<Reading> GetReadingsByUrl(Uri readingsUrl)
        {
            List<Reading> readings = new List<Reading>();

            httpClient.GetAsync(readingsUrl).ContinueWith(
                (requestTask) =>
                {
                    // Get HTTP response from completed task. 
                    HttpResponseMessage response = requestTask.Result;

                    // Check that response was successful or throw exception 
                    response.EnsureSuccessStatusCode();

                    //Read as stream
                    response.Content.ReadAsStreamAsync().ContinueWith(
                        (readTask) =>
                        {
                            DataContractJsonSerializer ser = new DataContractJsonSerializer(readings.GetType());
                            readings = (List<Reading>)ser.ReadObject(readTask.Result);
                        }).Wait();
                }).Wait();

            return readings;

        }
    }
}
