using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net.Http;
using System.Json;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Collections.Specialized;

using Com.Readmill.Api.DataContracts;

namespace Com.Readmill.Api
{        
    class ReadmillClientFactory
    {

        //public ReadmillClientFactory(string clientId)
        //{
            //this.ClientId = clientId;
        //}

        public UserClient GetUserClient(string clientId)
        {
            return new UserClient(clientId);
        }
   
    }
    
    class Program
    {        
        static void Main(string[] args)
        {
            string clientId = "3f2116709bb1f330084b9cd9f1045961";
            string accessToken = "cda8ddc466d23baa1cea25da55517fb5";

            UserClient client = new UserClient(clientId);
            User me =  client.GetOwner(accessToken);
            Console.WriteLine(me.FullName);

            ReadingsQueryOptions options = new ReadingsQueryOptions();
            
            //ToDo: should int not string - user facing should be natural data types, convert internally  to strings
            options.CountValue = "2";

            List<Reading> myReadings = client.GetUserReadings(me.Id, options); 
                
            Console.ReadLine();           
        }
    }
}
