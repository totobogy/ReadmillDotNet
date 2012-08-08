﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections.Specialized;
using System.Json;
using System.Net;

namespace Com.Readmill.Api
{
    public abstract class ReadmillClientBase
    {
        protected Uri readmillBaseUri =  new Uri(ReadmillConstants.ReadmillBaseUrl);
        protected string ClientId { get; set; }

        HttpClient httpClient;

        public ReadmillClientBase(string clientId)
        {
            this.ClientId = clientId;
            httpClient = new HttpClient();
            LoadTemplates();
        }

        protected Task<string> PutAsync<T>(T readmillObject, Uri readmillUri)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(readmillUri);
            req.Method = "PUT";
            req.ContentType = "application/json";

            //should this also be 'Task-ified'
            using (Stream stream = req.EndGetRequestStream(req.BeginGetRequestStream(null, null)))
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

            /*DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));

            //ToDo: Figure out a way to use stream safely
            MemoryStream m = new MemoryStream();
            ser.WriteObject(m, readmillObject);
            m.Position = 0;

            StreamContent content = new StreamContent(m);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            return httpClient.PutAsync(readmillUri, content).ContinueWith(
                (requestTask) =>
                {
                    try
                    {
                        requestTask.Result.EnsureSuccessStatusCode();
                        return requestTask.Result.Headers.Location;
                    }
                    finally
                    {
                        m.Dispose();
                    }
                });*/
        }

        protected Task<string> PostAsync<T>(T readmillObject, Uri readmillUri)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(readmillUri);
            req.Method = "POST";
            req.ContentType = "application/json";

            //should this also be 'Task-ified'
            using (Stream stream = req.EndGetRequestStream(req.BeginGetRequestStream(null, null)))
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
        }

        public Task<T> GetAsync<T>(Uri readmillUri)
        {
            Task<Task<T>> task = httpClient.GetAsync(readmillUri).ContinueWith(
                (requestTask) =>
                {
                    // Get HTTP response from completed task. 
                    HttpResponseMessage response = requestTask.Result;

                    // Check that response was successful or throw exception 
                    response.EnsureSuccessStatusCode();

                    //Read as stream
                    return (response.Content.ReadAsStreamAsync().ContinueWith(
                        (readTask) =>
                        {
                            using (readTask.Result)
                            {
                                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                                return (T)ser.ReadObject(readTask.Result);
                            }
                        }));
                });

            return task.Unwrap();

        }

        protected Task DeleteAsync(Uri readmillUri)
        {
            return httpClient.DeleteAsync(readmillUri).ContinueWith(
                (deleteTask) =>
                {
                    deleteTask.Result.EnsureSuccessStatusCode();
                });
        }

        protected NameValueCollection GetInitializedParameterCollection()
        {
            NameValueCollection uriParameterCollection = new NameValueCollection();
            uriParameterCollection.Add(ReadmillConstants.ClientId, this.ClientId);

            return uriParameterCollection;
        }

        /// <summary>
        /// Derived classes are expected to use this method to populate appropriate Uri Templates
        /// </summary>
        protected abstract void LoadTemplates();
    }
}
