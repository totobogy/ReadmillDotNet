using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.Readmill.Api;
using Com.Readmill.Api.DataContracts;
using System.Threading.Tasks;
using System.Threading;

namespace ReadmillWrapperTester
{
    [TestClass]
    public class UnitTests
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
            options.CountValue = "5";

            client.Users.GetUserReadings(me.Id, options).ContinueWith(
                (getReadingsTask) =>
                {
                    //Validations
                    if (getReadingsTask.Result.Count != 5)
                        throw new InternalTestFailureException("Expected 3 Readings. Retrieved: " + getReadingsTask.Result.Count);
                    
                    //Add more / stronger validations
                });
        }

        [TestMethod]
        public void TestGetReadings()
        {
            ReadmillClient client = new ReadmillClient(this.clientId);

            ReadingsQueryOptions options = new ReadingsQueryOptions();
            options.CountValue = "5";

            List<Reading> readings = client.Readings.GetReadingsAsync(options).Result;

            if (readings.Count != 5)
                throw new InternalTestFailureException("Expected 5 Readings. Got: " + readings.Count);
        }

        [TestMethod]
        public void TestUpdateReading()
        {
            ReadmillClient client = new ReadmillClient(this.clientId);

            //Reading id for "Why the lucky stiff's Poignant"
            string readingId = "77987";

            //Put Test
            ReadingUpdategram testReading = new ReadingUpdategram();
            testReading.IsPrivate = false;

            try
            {
                //'Put' an Update - set reading to public
                client.Readings.UpdateReadingAsync(this.accessToken, readingId, testReading).Wait(TimeSpan.FromMinutes(1));

                //Validate
                Assert.IsFalse(client.Readings.GetReadingByIdAsync(readingId).Result.IsPrivate);
            }
            finally
            {
                //Reset this reading to 'private' - Not full proof - what if this throws?
                testReading.IsPrivate = true;
                client.Readings.UpdateReadingAsync(this.accessToken, readingId, testReading).Wait(TimeSpan.FromMinutes(1));
            }
        }

        [TestMethod]
        public void TestGetBooks()
        {
            ReadmillClient client = new ReadmillClient(this.clientId);

            BooksQueryOptions options = new BooksQueryOptions();
            options.CountValue = 5;

            List<Book> books = client.Books.GetBooksAsync(options).Result;

            if (books.Count != 5)
                throw new InternalTestFailureException("Expected 5 Books. Got: " + books.Count);
        }

        [TestMethod]
        public void TestBestMatch()
        {
            ReadmillClient client = new ReadmillClient(this.clientId);

            BookMatchOptions options = new BookMatchOptions();
            options.TitleValue = "Zen and the Art of Motorcycle Maintenance";
            options.AuthorValue = "Robert M. Pirsig";

            Book book = client.Books.GetBestMatchAsync(options).Result;

            if (!book.Title.Equals(options.TitleValue) && !book.Author.Equals(options.AuthorValue))
                throw new InternalTestFailureException("Returned bad match");
        }

        [TestMethod]
        public void TestAddBook()
        {
            //Test only on staging?
        }

        [TestMethod]
        public void TestCreatePingDeleteReading()
        {
            ReadmillClient client = new ReadmillClient(this.clientId);

            BookMatchOptions options = new BookMatchOptions();
            options.TitleValue = "Zen and the Art of Motorcycle Maintenance";
            options.AuthorValue = "Robert M. Pirsig";

            Book book = client.Books.GetBestMatchAsync(options).Result;

            client.Books.CreateBookReadingAsync(this.accessToken, book.Id, Reading.ReadingState.Open).Wait();

            //Validate and Reset
            User me = client.Users.GetOwnerAsync(this.accessToken).Result;
            List<Reading> myReadings = client.Users.GetUserReadings(me.Id, new ReadingsQueryOptions()).Result;
            foreach (Reading r in myReadings)
            {
                if (r.Book.Title.Equals(options.TitleValue) && r.State == Reading.ReadingState.Open)
                {
                    ReadingSession session = client.Readings.GetReadingSession(this.accessToken, r.Id);
                    session.Ping((float)0.1, false, false).Wait();

                    //We found the reading. Now delete it.            
                    client.Readings.DeleteReadingAsync(this.accessToken, r.Id);
                    return;
                }
            }

            //We shouldn't be here - throw
            throw new InternalTestFailureException("Validation or Deletion failed");
        }

        //Test Ping and Periods
    }
}
