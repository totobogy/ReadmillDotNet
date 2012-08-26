using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections.Specialized;
//using System.Json;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading;

namespace Com.Readmill.Api
{
    public abstract class ReadmillClientBase
    {
        protected Uri readmillBaseUri =  new Uri(ReadmillConstants.ReadmillBaseUrl);
        protected string ClientId { get; set; }

        public ReadmillClientBase(string clientId)
        {
            this.ClientId = clientId;
            LoadTemplates();
        }


        protected Task<string> PutAsync<T>(T readmillObject, Uri readmillUri)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(readmillUri);
            req.Method = "PUT";
            req.ContentType = "application/json";

            Task<Stream> s = Task<Stream>.Factory.FromAsync(req.BeginGetRequestStream, req.EndGetRequestStream, null);
            return s.ContinueWith(
                streamTask =>
                {
                    using (Stream stream = streamTask.Result)
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                        ser.WriteObject(stream, readmillObject);
                    }

                    Task<WebResponse> t = Task<WebResponse>.Factory.FromAsync(req.BeginGetResponse, req.EndGetResponse, null);

                    return t.ContinueWith(
                        (responseTask) =>
                        {
                            using (responseTask.Result)
                            {
                                return responseTask.Result.Headers["Location"];
                            }
                        });
                }).Unwrap();            
        }

        public Task PostAsync(Uri readmillUri)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(readmillUri);
            req.Method = "POST";
            //req.ContentType = "application/json";

            Task<WebResponse> t = Task<WebResponse>.Factory.FromAsync(req.BeginGetResponse, req.EndGetResponse, null);

            return t.ContinueWith(
                (responseTask) =>
                {
                    if (responseTask.IsFaulted)
                    {
                        responseTask.Exception.Flatten().Handle(
                            ex =>
                            {
                                //We don't really want to mark as handled - only add extra info.
                                if(ex is WebException 
                                    && (((WebException)ex).Status == WebExceptionStatus.ProtocolError)
                                    || (((WebException)ex).Status == WebExceptionStatus.UnknownError))
                                {
                                    try
                                    {
                                        using (HttpWebResponse resp = responseTask.Result as HttpWebResponse)
                                        {
                                            ex.Data.Add("HttpStatusCode", resp.StatusCode);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        //no op
                                    }
                                }
                                return false;
                            });
                    }

                    using (responseTask.Result)
                    {
                        return;
                    }
                });

            /*cancellationToken.Register(() =>
                {
                    req.Abort();
                    throw new OperationCanceledException(cancellationToken);
                });*/
        }

        protected Task<string> PostAsync<T>(T readmillObject, Uri readmillUri)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(readmillUri);
            req.Method = "POST";
            req.ContentType = "application/json";

            /*cancellationToken.Register(() =>
                {
                    req.Abort();
                    throw new OperationCanceledException(cancellationToken);
                });*/

            Task<Stream> s = Task<Stream>.Factory.FromAsync(req.BeginGetRequestStream, req.EndGetRequestStream, null);
            return s.ContinueWith(
                streamTask =>
                {
                    using (Stream stream = streamTask.Result)
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                        ser.WriteObject(stream, readmillObject);
                    }

                    Task<WebResponse> t = Task<WebResponse>.Factory.FromAsync(req.BeginGetResponse, req.EndGetResponse, null);

                    return t.ContinueWith(
                        (responseTask) =>
                        {
                            using (responseTask.Result)
                            {
                                return responseTask.Result.Headers["Location"];
                            }
                        });                
                }).Unwrap();                     
        }

        protected Task<T> GetAsync<T>(Uri readmillUri, CancellationToken cancellationToken = default(CancellationToken))
        {
            TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>();

            WebClient client = new WebClient();

            cancellationToken.Register(() =>
            {
                client.CancelAsync();
            });

            client.OpenReadCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    tcs.SetException(args.Error);
                else if (args.Cancelled)
                    tcs.SetCanceled();
                else tcs.SetResult(args.Result);
            };

            client.OpenReadAsync(readmillUri);

            return tcs.Task.ContinueWith(
                        (readTask) =>
                        {
                            cancellationToken.Register(() =>
                            {
                                throw new OperationCanceledException(cancellationToken);
                            });

                            using (readTask.Result)
                            {
                                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                                T obj = (T)ser.ReadObject(readTask.Result);
                                return obj;
                            }
                        }, cancellationToken);

        }

        /// <summary>
        /// Helper method to Get a resource based on permalink uri (e.g. returned by a previous Post call)
        /// </summary>
        /// <typeparam name="T">Readmill Resource type</typeparam>
        /// <param name="permalink">Permalink of the resource</param>
        /// <param name="accessToken">Needed of the resource is private</param>
        /// <returns></returns>
        public Task<T> GetFromPermalinkAsync<T>(string permalink, string accessToken = null)
        {
            string readmillUri = permalink + "?client_id=" + this.ClientId;

            if (accessToken != null)
                readmillUri = readmillUri + "&access_token=" + accessToken;

            return GetAsync<T>(new Uri(readmillUri));
        }


        public Task DeleteAsync(Uri readmillUri)
        {
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(readmillUri);
            req.Method = "DELETE";

            Task<WebResponse> t = Task<WebResponse>.Factory.FromAsync(req.BeginGetResponse, req.EndGetResponse, null);

            return t.ContinueWith(
                (deleteTask) =>
                {
                    using (deleteTask.Result)
                    {
                        return;
                    }
                });
        }


        protected IDictionary<string,string> GetInitializedParameterCollection()
        {
            IDictionary<string, string> uriParameterCollection = new Dictionary<string, string>();
            uriParameterCollection.Add(ReadmillConstants.ClientId, this.ClientId);

            return uriParameterCollection;
        }

        /// <summary>
        /// Derived classes are expected to use this method to populate appropriate Uri Templates
        /// </summary>
        protected abstract void LoadTemplates();
    }
}
