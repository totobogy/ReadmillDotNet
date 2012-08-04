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

namespace Com.Readmill.Api
{
    public abstract class ReadmillClientBase
    {
        protected Uri readmillBaseUri =  new Uri(ReadmillConstants.ReadmillBaseUrl);
        protected string ClientId { get; set; }

        //HttpClient httpClient;

        public ReadmillClientBase(string clientId)
        {
            this.ClientId = clientId;
            //httpClient = new HttpClient();
            LoadTemplates();
        }


        protected Task PutAsync<T>(T readmillObject, Uri readmillUri)
        {
            TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>();

            WebClient client = new WebClient();

            client.OpenWriteCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    tcs.SetException(args.Error);
                else if (args.Cancelled)
                    tcs.SetCanceled();
                else tcs.SetResult(args.Result);
            };

            client.Headers["Content-Type"] = "application/json";
            client.OpenWriteAsync(readmillUri, "PUT");

            return tcs.Task.ContinueWith(
                        (writeTask) =>
                        {
                            using (writeTask.Result)
                            {
                                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                                ser.WriteObject(writeTask.Result, readmillObject);
                            }
                        });
            
        }


        protected Task PostAsync<T>(T readmillObject, Uri readmillUri)
        {
            TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>();

            WebClient client = new WebClient();

            client.OpenWriteCompleted += (sender, args) =>
            {
                if (args.Error != null)
                    tcs.SetException(args.Error);
                else if (args.Cancelled)
                    tcs.SetCanceled();
                else tcs.SetResult(args.Result);
            };

            client.Headers["Content-Type"] = "application/json";
            client.OpenWriteAsync(readmillUri);

            //Is this any useful? The write is still happening in the callback?
            return tcs.Task.ContinueWith(
                        (writeTask) =>
                        {
                            using (writeTask.Result)
                            {
                                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                                ser.WriteObject(writeTask.Result, readmillObject);
                            }
                        });
                        
        }


        public Task<T> GetAsync<T>(Uri readmillUri)
        {
            TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>();

            WebClient client = new WebClient();

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
                            using (readTask.Result)
                            {
                                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                                T obj = (T)ser.ReadObject(readTask.Result);
                                return obj;
                            }
                        });

        }


        protected Task DeleteAsync(Uri readmillUri)
        {
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(readmillUri);
            req.Method = "DELETE";

            Task<WebResponse> t = Task<WebResponse>.Factory.FromAsync(req.BeginGetResponse, req.EndGetResponse, null);

            return t.ContinueWith(
                (deleteTask) =>
                {
                    return;
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
