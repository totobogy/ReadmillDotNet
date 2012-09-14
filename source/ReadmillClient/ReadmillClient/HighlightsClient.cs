using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using Com.Readmill.Api.DataContracts;
using System.Collections.Specialized;
using Com.Readmill.Api;
using System.Threading.Tasks;
using System.Security;
using System.Xml;
using System.Threading;

namespace Com.Readmill.Api
{
    public class HighlightsClient : ReadmillClientBase
    {
        Dictionary<HighlightsUriTemplateType, UriTemplate> highlightsUriTemplates;

        #region Url Templates used by HighlightsClient

        //Uri Template Parameter Constants
        const string HighlightId = "HighlightId";

        //Uri Template Types
        enum HighlightsUriTemplateType { PublicHighlights, SingleHighlight, HighlightComments };

        #region Template Strings
        const string singleHighlightTemplate = "/highlights/{"
            + HighlightsClient.HighlightId
            + "}?client_id={"
            + ReadmillConstants.ClientId
            + "}&access_token={"
            + ReadmillConstants.AccessToken
            + "}";

        const string highlightCommentsTemplate = "/highlights/{"
            + HighlightsClient.HighlightId
            + "}/comments?client_id={"
            + ReadmillConstants.ClientId
            + "}&access_token={"
            + ReadmillConstants.AccessToken
            + "}&from={"
            + RangeQueryOptions.From
            + "}&to={"
            + RangeQueryOptions.To
            + "}&count={"
            + RangeQueryOptions.Count
            + "}&order={"
            + RangeQueryOptions.OrderBy
            + "}";

        const string publicHighlightsTemplate = "/highlights?client_id={"
            + ReadmillConstants.ClientId
            + "}&from={"
            + RangeQueryOptions.From
            + "}&to={"
            + RangeQueryOptions.To
            + "}&count={"
            + RangeQueryOptions.Count
            + "}&order={"
            + RangeQueryOptions.OrderBy
            + "}";

        #endregion

        #endregion

        /// <summary>
        /// Instantiates a client for the Readmill/Users api
        /// </summary>
        /// <param name="clientId">Client Id of the application, assgined by Readmill when the app is registered</param>
        public HighlightsClient(string clientId)
            : base(clientId)
        {

        }

        override protected void LoadTemplates()
        {
            highlightsUriTemplates = new Dictionary<HighlightsUriTemplateType, UriTemplate>();
            highlightsUriTemplates.Add(HighlightsUriTemplateType.PublicHighlights, new UriTemplate(publicHighlightsTemplate, true));
            highlightsUriTemplates.Add(HighlightsUriTemplateType.SingleHighlight, new UriTemplate(singleHighlightTemplate, true));
            highlightsUriTemplates.Add(HighlightsUriTemplateType.HighlightComments, new UriTemplate(highlightCommentsTemplate, true));
        }

        /// <summary>
        /// Retrieves a list of highlights.
        /// </summary>
        /// <param name="options">Query options for retrieving the highlights (optional)</param>
        /// <returns></returns>
        public Task<List<Highlight>> GetHighlightsAsync(RangeQueryOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();

            if (options != null)
            {
                parameters.Add(RangeQueryOptions.From, options.FromValue);
                parameters.Add(RangeQueryOptions.To, options.ToValue);
                parameters.Add(RangeQueryOptions.Count, options.CountValue.ToString());
                parameters.Add(RangeQueryOptions.OrderBy, options.OrderByValue);
            }

            //Remove extraneous parameters because Readmill doesn't like empty pairs
            IDictionary<string, string> tmpParams = new Dictionary<string, string>();
            foreach (string key in parameters.Keys)
            {
                if (!string.IsNullOrEmpty(parameters[key]))
                    tmpParams.Add(key, parameters[key]);
            }
            parameters = tmpParams;

            var highlightsUrl = highlightsUriTemplates[HighlightsUriTemplateType.PublicHighlights].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<List<Highlight>>(highlightsUrl, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="highlightId"></param>
        /// <param name="accessToken">(optional) for private highlights</param>
        /// <returns></returns>
        public Task<Highlight> GetHighlightByIdAsync(string highlightId, string accessToken = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(HighlightsClient.HighlightId, highlightId);

            //Remove extraneous parameters because Readmill doesn't like empty pairs
            IDictionary<string, string> tmpParams = new Dictionary<string, string>();
            foreach (string key in parameters.Keys)
            {
                if (!string.IsNullOrEmpty(parameters[key]))
                    tmpParams.Add(key, parameters[key]);
            }
            parameters = tmpParams;

            var highlightsUrl = highlightsUriTemplates[HighlightsUriTemplateType.SingleHighlight].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<Highlight>(highlightsUrl, cancellationToken);
        }

        public Task<string> UpdateHighlightAsync(string accessToken, string highlightId, Highlight updatedHighlight)
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(HighlightsClient.HighlightId, highlightId);

            WrappedHighlight wrappedUpdate = new WrappedHighlight();
            wrappedUpdate.Highlight = updatedHighlight;

            var highlightUrl = highlightsUriTemplates[HighlightsUriTemplateType.SingleHighlight].BindByName(this.readmillBaseUri, parameters);

            return PutAsync<WrappedHighlight>(wrappedUpdate, highlightUrl);
        }

        public Task DeleteHighlightAsync(string accessToken, string highlightId)
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(HighlightsClient.HighlightId, highlightId);

            var highlightUrl = highlightsUriTemplates[HighlightsUriTemplateType.SingleHighlight].BindByName(this.readmillBaseUri, parameters);
            return DeleteAsync(highlightUrl);
        }
     
        public Task<List<Comment>> GetHighlightCommentsAsync(string highlightId, RangeQueryOptions options = null, string accessToken = null)
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();
            parameters.Add(HighlightsClient.HighlightId, highlightId);
            parameters.Add(ReadmillConstants.AccessToken, accessToken);

            if (options != null)
            {
                parameters.Add(RangeQueryOptions.From, options.FromValue);
                parameters.Add(RangeQueryOptions.To, options.ToValue);
                parameters.Add(RangeQueryOptions.Count, options.CountValue.ToString());
                parameters.Add(RangeQueryOptions.OrderBy, options.OrderByValue);
            }

            //Remove extraneous parameters because Readmill doesn't like empty pairs
            IDictionary<string, string> tmpParams = new Dictionary<string, string>();
            foreach (string key in parameters.Keys)
            {
                if (!string.IsNullOrEmpty(parameters[key]))
                    tmpParams.Add(key, parameters[key]);
            }
            parameters = tmpParams;

            var commentsUrl = highlightsUriTemplates[HighlightsUriTemplateType.HighlightComments].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<List<Comment>>(commentsUrl);
        }

        public Task<string> PostHighlightCommentAsync(string accessToken, string highlightId, Comment comment)
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(HighlightsClient.HighlightId, highlightId);

            //Wrap in InternalHighlight
            var wrappedComment = new WrappedComment() { Comment = comment };

            var commentUrl = highlightsUriTemplates[HighlightsUriTemplateType.HighlightComments].BindByName(this.readmillBaseUri, parameters);
            return PostAsync<WrappedComment>(wrappedComment, commentUrl);
        }

    }
}
