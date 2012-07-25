using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Readmill.Api
{
    public class ReadmillClient
    {
        protected string readmillBaseUrl = ReadmillConstants.ReadmillBaseUrl;
        protected string ClientId { get; set; }
        protected string AccessToken { get; set; }
    }
}
