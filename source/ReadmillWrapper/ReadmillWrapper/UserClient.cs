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

namespace Com.Readmill.Api
{
    //ToDo: Data types - not everything string
    //ToDo: Refactor base class for client?
    //ToDo: Need Design / Redesign the entry point interface for API

    public class UserClient : ReadmillClient
    {
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

        /// <summary>
        /// Retrieves the representation of the user corresponding to the authentication token.
        /// </summary>
        /// <param name="accessToken">authentication token</param>
        /// <returns>representation of the user corresponding to the authentication token</returns>
        public Task<User> GetOwnerAsync(string accessToken)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add(ReadmillConstants.ClientId, this.ClientId);
            parameters.Add(ReadmillConstants.AccessToken, accessToken);

            Uri uri = userUriTemplates[UserUriTemplateType.Owner].BindByName(new Uri(this.readmillBaseUrl), parameters);

            return GetByUrlAsync<User>(uri);
        }

        /// <summary>
        /// Retrieves the representation of a single user by id
        /// </summary>
        /// <param name="userId">The Readmill user-id of the user you want to retrieve.</param>
        /// <returns></returns>
        public Task<User> GetUserByIdAsync(string userId)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add(ReadmillConstants.ClientId, this.ClientId);
            parameters.Add(UserClient.UserId, userId);

            return GetByUrlAsync<User>(userUriTemplates[UserUriTemplateType.Users].BindByName(new Uri(this.readmillBaseUrl), parameters));
        }

        /// <summary>
        /// Retrieves a list of readings corresponding to the user with the specified ID.
        /// </summary>
        /// <param name="userId">Readmill user-id of the user whose readings you want to retrieve</param>
        /// <param name="options">Query options for retrieving the readings</param>
        /// <returns></returns>
        public Task<List<Reading>> GetUserReadings(string userId, ReadingsQueryOptions options)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add(ReadmillConstants.ClientId, this.ClientId);
            parameters.Add(UserClient.UserId, userId);
            
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

            var readingsUrl = userUriTemplates[UserUriTemplateType.UserReadings].BindByName(new Uri(this.readmillBaseUrl), parameters);
            return GetByUrlAsync<List<Reading>>(readingsUrl);
        }

    }
}
