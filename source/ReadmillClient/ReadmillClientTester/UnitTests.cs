using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.Readmill.Api;
using Com.Readmill.Api.DataContracts;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;

namespace ReadmillWrapperTester
{
    [TestClass]
    public class UnitTests
    {
        //Provide your Client-Id. This is just a dummy.
        string clientId = "3f211blahblahblah";

        //Provide a valid the oauth access Token (e.g. a non-expiring token)
        string accessToken = "cda8blahblahblah";

        [TestMethod]
        public void TestGetOwner()
        {
            //Replace this with your Full Name
            string myFullName = "Tushar Malhotra";

            ReadmillClient client = new ReadmillClient(this.clientId);

            client.Users.GetOwnerAsync(this.accessToken).ContinueWith(
                (getUserTask) =>
                {
                    if (!(getUserTask.Result.FullName == myFullName))
                        throw new InternalTestFailureException("Expected fullName does not match Retrieved: " + getUserTask.Result.FullName);

                    //ToDo: Add more / stronger validations     
                });         
        }

        [TestMethod]
        public void TestGetUserReadings()
        {
            //Modify this variable according to your own data before running
            int expectedReadingCount = 6;

            ReadmillClient client = new ReadmillClient(this.clientId);

            User me = client.Users.GetOwnerAsync(this.accessToken).Result;   

            client.Users.GetUserReadings(me.Id, accessToken:accessToken).ContinueWith(
                (getReadingsTask) =>
                {
                    //Validations
                    if (getReadingsTask.Result.Count != expectedReadingCount)
                        throw new InternalTestFailureException("Expected" + expectedReadingCount + "readings. Retrieved: " + getReadingsTask.Result.Count);
                    
                    //Add more / stronger validations
                });
        }

        [TestMethod]
        public void TestGetReadings()
        {
            ReadmillClient client = new ReadmillClient(this.clientId);

            ReadingsQueryOptions options = new ReadingsQueryOptions() { CountValue = 30 };

            List<Reading> readings = client.Readings.GetReadingsAsync(options).Result;

            foreach (Reading r in readings)
            {
                //Periods
                List<Period> sessions = client.Readings.GetReadingPeriodsAsync(r.Id).Result;
                Console.WriteLine("Reading {0} has {1} sessions.", r.Id, sessions.Count);

                //Locaions
                List<Location> locations = client.Readings.GetReadingLocationsAsync(r.Id).Result;
                Console.WriteLine("Reading {0} has {1} locations.", r.Id, locations.Count);
            }

            if (readings.Count != 30)
                throw new InternalTestFailureException("Expected 30 Readings. Got: " + readings.Count);
        }

        [TestMethod]
        public void TestUpdateReading()
        {
            /*
             * Uncomment the below code and modify it for one of your own readings suitably
             * e.g. The simplest way is to create a private reading and set readingId to its id
             */

            /*ReadmillClient client = new ReadmillClient(this.clientId);

            //Reading-id for my reading od "Why the lucky stiff's Poignant Guide to Ruby"
            string readingId = "77987";

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
            }*/
        }

        [TestMethod]
        public void TestGetBooks()
        {
            ReadmillClient client = new ReadmillClient(this.clientId);

            BooksQueryOptions options = new BooksQueryOptions();
            options.CountValue = 50;

            List<Book> books = client.Books.GetBooksAsync(options).Result;

            foreach (Book book in books)
                Console.WriteLine(book.Title + " by " + book.Author);

            if (books.Count != 50)
                throw new InternalTestFailureException("Expected 5 Books. Got: " + books.Count);
        }

        [TestMethod]
        public void TestGetBestMatch()
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

        /// <summary>
        /// Less of a test, more of a sample
        /// </summary>
        [TestMethod]
        public void TestReadingActions()
        {
            ReadmillClient client = new ReadmillClient(this.clientId);

            BookMatchOptions options = new BookMatchOptions();
            options.TitleValue = "Zen and the Art of Motorcycle Maintenance";
            options.AuthorValue = "Robert M. Pirsig";

            Book book = client.Books.GetBestMatchAsync(options).Result;

            //Create a new reading and retrieve it using the permalink
            string permalink = client.Books.PostBookReadingAsync(this.accessToken, book.Id, Reading.ReadingState.Open).Result;
            Reading r = client.Readings.GetFromPermalinkAsync<Reading>(permalink).Result;

            try
            {
                //Create a new Reading Session
                ReadingSession session = client.Readings.GetReadingSession(this.accessToken, r.Id);
                session.PingAsync(0.1, 52.53826, 13.41268).Wait();
                Thread.Sleep(5000);
                session.PingAsync(0.12, 52.53826, 13.41268).Wait();

                Highlight testHighlight = new Highlight() { Content = "When one person suffers from a delusion, it is called insanity. When many people suffer from a delusion it is called a Religion." };
                session.PostHighlightAsync(testHighlight).Wait();

                string content = @"It’s a fascinating tale of a journey, both physically and metaphysically,
                                       into the world of quality and values";
                session.PostReadingCommentAsync(content).Wait();

                session.Close();
            }
            finally
            {
                //Comment this line or set a breakpoint here if you want to check the Reading on Readmill
                client.Readings.DeleteReadingAsync(this.accessToken, r.Id);
            }
        }

        /// <summary>
        /// Less of a test, more of a sample
        /// </summary>
        [TestMethod]
        public void TestGetHighlights()
        {
            ReadmillClient client = new ReadmillClient(this.clientId);

            SortedList<decimal, Highlight> highlights = new SortedList<decimal, Highlight>();

            //Get a book
            BookMatchOptions options = new BookMatchOptions();
            options.AuthorValue = "Chad Fowler";//"Edward Rutherfurd";
            options.TitleValue = "The Passionate Programmer";//"New York: The Novel";

            Book book = client.Books.GetBestMatchAsync(options).Result;

            ReadingsQueryOptions readingOptions = new ReadingsQueryOptions();
            //Get all readings
            List<Reading> readings = client.Books.GetBookReadingsAsync(book.Id, readingOptions).Result;
            
            foreach (Reading reading in readings)
            {
                //Foreach reading, Get all (100 latest) Highlights
                RangeQueryOptions highlightOptions = new RangeQueryOptions() { CountValue = 100 };

                foreach (Highlight h in client.Readings.GetReadingHighlightsAsync(reading.Id, highlightOptions).Result)
                {
                    if(!highlights.ContainsKey(h.Locators.Position))
                        highlights.Add(h.Locators.Position, h);
                }
            }

            //Now do something with the sorted list of highlights. E.g. Print a summary of the book
        }

        [TestMethod]
        public void TestRangeQuery()
        {
            ReadmillClient client = new ReadmillClient(this.clientId);

            ReadingsQueryOptions options = new ReadingsQueryOptions()
                                           {
                                               FromValue = XmlConvert.ToString(new DateTime(2012, 7, 1)),
                                               ToValue = XmlConvert.ToString(new DateTime(2012, 7, 31)),
                                               OrderByValue = "created_at"
                                           };

            List<Reading> jul2012Readings = client.Readings.GetReadingsAsync(options).Result;
            foreach (Reading reading in jul2012Readings)
            {
                //There shouldn't be any reading not created in July 2012
                Assert.IsTrue(DateTime.Parse(reading.CreatedAt) < new DateTime(2012, 8, 7));
                Assert.IsTrue(DateTime.Parse(reading.CreatedAt) > new DateTime(2012, 6, 30));
            }
        }
    }
}
