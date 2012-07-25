using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.Readmill.Api;
using Com.Readmill.Api.DataContracts;

namespace ReadmillWrapperTester
{
    [TestClass]
    public class UsersUnitTests
    {
        //Client ID:3f2116709bb1f330084b9cd9f1045961
        //Client Secret:0b8d3bdaacfa1797637bbb6791eb21dd
        //Auth code=c34ab5591a8e5a715fc698b4c6a7fe12
        //{"access_token":"cda8ddc466d23baa1cea25da55517fb5","expires_in":3155673599,"scope":"non-expiring"}

        string clientId = "3f2116709bb1f330084b9cd9f1045961";
        string accessToken = "cda8ddc466d23baa1cea25da55517fb5";

        [TestMethod]
        public void TestGetOwner()
        {
            UserClient client = new UserClient(this.clientId);
            User me = client.GetOwner(this.accessToken);

            //Validations
            if (!(me.FullName == "Tushar Malhotra"))
                throw new InternalTestFailureException("FullName not validated.");

            //ToDo: Add more / stronger validations      
        }

        [TestMethod]
        public void TestGetUserReadings()
        {
            UserClient client = new UserClient(this.clientId);
            User me = client.GetOwner(this.accessToken);
            
            ReadingsQueryOptions options = new ReadingsQueryOptions();
            options.CountValue = "3";

            List<Reading> myReadings = client.GetUserReadings(me.Id, options);

            //Validations
            if (myReadings.Count != 3)
                throw new InternalTestFailureException("Expected 3 Readings. Retrieved: " + myReadings.Count);

            //Add more / stronger validations?
        }
    }
}
