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
    //ToDo: Improve Errors / Error parsing - html /css
    
    public class ReadmillClient
    {
        public string ClientId { get; set; }
        
        UsersClient userClient;
        ReadingsClient readingsClient;
        
        public ReadmillClient(string clientId)
        {
            this.ClientId = clientId;
        }

        public UsersClient Users
        {
            get
            {
                if (this.userClient == null)
                    userClient = new UsersClient(this.ClientId);
                
                return userClient;
            }
        }

        public ReadingsClient Readings
        {
            get
            {
                if (this.readingsClient == null)
                    readingsClient = new ReadingsClient(this.ClientId);

                return readingsClient;
            }

        }


   
    }
    
}
