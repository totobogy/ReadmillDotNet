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

namespace Com.Readmill.Api
{
    public class CommentsClient : ReadmillClientBase
    {
        Dictionary<CommentsUriTemplateType, UriTemplate> commentsUriTemplates;

        #region Url Templates used by CommentsClient

        //Uri Template Parameter Constants
        const string CommentId = "CommentId";

        //Uri Template Types
        enum CommentsUriTemplateType { SingleComment };

        #region Template Strings
        const string singleCommentTemplate = "/comments/{"
            + CommentsClient.CommentId
            + "}?client_id={"
            + ReadmillConstants.ClientId
            + "}&access_token={"
            + ReadmillConstants.AccessToken
            + "}";

        #endregion

        #endregion

        /// <summary>
        /// Instantiates a client for the Readmill/Users api
        /// </summary>
        /// <param name="clientId">Client Id of the application, assgined by Readmill when the app is registered</param>
        public CommentsClient(string clientId)
            : base(clientId)
        {

        }

        override protected void LoadTemplates()
        {
            commentsUriTemplates = new Dictionary<CommentsUriTemplateType, UriTemplate>();
            commentsUriTemplates.Add(CommentsUriTemplateType.SingleComment, new UriTemplate(singleCommentTemplate, true));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commentId"></param>
        /// <param name="accessToken">(optional) for private comments</param>
        /// <returns></returns>
        public Task<Comment> GetCommentByIdAsync(string commentId, string accessToken = null)
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(CommentsClient.CommentId, commentId);

            //Remove extraneous parameters because Readmill doesn't like empty pairs
            IDictionary<string, string> tmpParams = new Dictionary<string, string>();
            foreach (string key in parameters.Keys)
            {
                if (!string.IsNullOrEmpty(parameters[key]))
                    tmpParams.Add(key, parameters[key]);
            }
            parameters = tmpParams;

            var commentsUrl = commentsUriTemplates[CommentsUriTemplateType.SingleComment].BindByName(this.readmillBaseUri, parameters);
            return GetAsync<Comment>(commentsUrl);
        }

        public Task<string> UpdateCommentAsync(string accessToken, string commentId, Comment updatedComment)
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(CommentsClient.CommentId, commentId);

            WrappedComment wrappedUpdate = new WrappedComment();
            wrappedUpdate.Comment = updatedComment;

            var commentUrl = commentsUriTemplates[CommentsUriTemplateType.SingleComment].BindByName(this.readmillBaseUri, parameters);

            return PutAsync<WrappedComment>(wrappedUpdate, commentUrl);
        }

        public Task DeleteCommentAsync(string accessToken, string commentId)
        {
            IDictionary<string, string> parameters = GetInitializedParameterCollection();
            parameters.Add(ReadmillConstants.AccessToken, accessToken);
            parameters.Add(CommentsClient.CommentId, commentId);

            var commentUrl = commentsUriTemplates[CommentsUriTemplateType.SingleComment].BindByName(this.readmillBaseUri, parameters);
            return DeleteAsync(commentUrl);
        }        
    }
}
