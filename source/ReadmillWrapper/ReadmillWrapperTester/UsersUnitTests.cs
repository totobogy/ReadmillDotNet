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
            ReadmillClient client = new ReadmillClient(this.clientId);

            client.Users.GetOwnerAsync(this.accessToken).ContinueWith(
                (getUserTask) =>
                {
                    if (!(getUserTask.Result.FullName == "Tushar Malhotra"))
                        throw new InternalTestFailureException("Expected FullName: Tushar Malhotra. Retrieved: " + getUserTask.Result.FullName);

                    //ToDo: Add more / stronger validations     
                });         
        }

        [TestMethod]
        public void TestGetUserReadings()
        {
            ReadmillClient client = new ReadmillClient(this.clientId);

            User me = client.Users.GetOwnerAsync(this.accessToken).Result;   

            ReadingsQueryOptions options = new ReadingsQueryOptions();
            options.CountValue = "3";

            client.Users.GetUserReadings(me.Id, options).ContinueWith(
                (getReadingsTask) =>
                {
                    //Validations
                    if (getReadingsTask.Result.Count != 3)
                        throw new InternalTestFailureException("Expected 3 Readings. Retrieved: " + getReadingsTask.Result.Count);
                    
                    //Add more / stronger validations
                });
            
        }
    }
}
