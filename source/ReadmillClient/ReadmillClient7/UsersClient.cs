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


namespace Com.Readmill.Api
{
    //ToDo: Data types - not everything string

    public class UsersClient : ReadmillClientBase
    {
        Dictionary<UsersUriTemplateType, UriTemplate> userUriTemplates;

        #region Url Templates used by UsersClient

        //Uri Template Parameter Constants
        const string UserId = "UserId";

        //Uri Template Types
        enum UsersUriTemplateType { Owner, Users, UserReadings };


        #region Template Strings
        const string ownerTemplate = "/me?client_id={"
            + ReadmillConstants.ClientId
            + "}&access_token={"
            + ReadmillConstants.AccessToken
            + "}";

        const string usersTemplate = "/users/{"
            + UsersClient.UserId
            + "}?client_id={"
            + ReadmillConstants.ClientId
            + "}";

        const string userReadingsTemplate = "/users/{"
            + UsersClient.UserId
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
        public UsersClient(string clientId): base(clientId)
        {
      
        }

        override protected void LoadTemplates()
        {
            userUriTemplates = new Dictionary<UsersUriTemplateType, UriTemplate>();
            userUriTemplates.Add(UsersUriTemplateType.Owner, new UriTemplate(ownerTemplate, true));
            userUriTemplates.Add(UsersUriTemplateType.Users, new UriTemplate(usersTemplate, true));
            userUriTemplates.Add(UsersUriTemplateType.UserReadings, new UriTemplate(userReadingsTemplate, true));
        }        

        /// <summary>
        /// Retrieves the representation of the user corresponding to the authentication token.
        /// </summary>
        /// <param name="accessToken">authentication token</param>
        /// <returns>representation of the user corresponding to the authentication token</returns>
        public Task<User> GetOwnerAsync(string accessToken)
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();

            parameters.Add(ReadmillConstants.AccessToken, accessToken);

            Uri uri = userUriTemplates[UsersUriTemplateType.Owner].BindByName(this.readmillBaseUri, parameters);

            return GetAsync<User>(uri);
        }

        /// <summary>
        /// Retrieves the representation of a single user by id
        /// </summary>
        /// <param name="userId">The Readmill user-id of the user you want to retrieve.</param>
        /// <returns></returns>
        public Task<User> GetUserByIdAsync(string userId)
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();

            parameters.Add(UsersClient.UserId, userId);

            return GetAsync<User>(userUriTemplates[UsersUriTemplateType.Users].BindByName(this.readmillBaseUri, parameters));
        }

        /// <summary>
        /// Retrieves a list of readings corresponding to the user with the specified ID.
        /// </summary>
        /// <param name="userId">Readmill user-id of the user whose readings you want to retrieve</param>
        /// <param name="options">Query options for retrieving the readings</param>
        /// <returns></returns>
        public Task<List<Reading>> GetUserReadings(string userId, ReadingsQueryOptions options)
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();

            parameters.Add(UsersClient.UserId, userId);

            parameters.Add(ReadingsQueryOptions.From, options.FromValue);
            parameters.Add(ReadingsQueryOptions.To, options.ToValue);
            parameters.Add(ReadingsQueryOptions.Count, options.CountValue.ToString());
            parameters.Add(ReadingsQueryOptions.Order, options.OrderValueInternal);
            parameters.Add(ReadingsQueryOptions.HighlightsCountFrom, options.HighlightsCountFromValue);
            parameters.Add(ReadingsQueryOptions.HighlightsCountTo, options.HighlightsCountToValue);
            parameters.Add(ReadingsQueryOptions.Status, options.StatusValue);

            //Remove extraneous parameters because Readmill doesn't like empty pairs
            IDictionary<string, string> tmpParams = new Dictionary<string, string>();
            foreach (string key in parameters.Keys)
            {
                if (!string.IsNullOrEmpty(parameters[key]))
                    tmpParams.Add(key, parameters[key]);
            }
            parameters = tmpParams;

            var readingsUrl = userUriTemplates[UsersUriTemplateType.UserReadings].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<List<Reading>>(readingsUrl);
        }

    }
}
