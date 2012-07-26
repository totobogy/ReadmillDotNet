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
                   
        }
    }
}
