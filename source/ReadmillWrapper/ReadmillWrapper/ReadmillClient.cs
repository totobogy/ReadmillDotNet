using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.Serialization.Json;

namespace Com.Readmill.Api
{
    public abstract class ReadmillClient
    {
        protected string readmillBaseUrl = ReadmillConstants.ReadmillBaseUrl;
        protected string ClientId { get; set; }
        protected string AccessToken { get; set; }

        HttpClient httpClient;

        public ReadmillClient()
        {
            httpClient = new HttpClient();
        }

        protected Task<T> GetByUrlAsync<T>(Uri url)
        {
            Task<Task<T>> task = httpClient.GetAsync(url).ContinueWith(
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

        //protected abstract void 
    }
}
