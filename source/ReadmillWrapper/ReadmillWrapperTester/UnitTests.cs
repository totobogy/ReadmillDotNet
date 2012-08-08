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
            options.CountValue = 5;

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
            options.CountValue = 5;

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

        [TestMethod]
        public void TestReadingActions()
        {
            ReadmillClient client = new ReadmillClient(this.clientId);

            BookMatchOptions options = new BookMatchOptions();
            options.TitleValue = "Zen and the Art of Motorcycle Maintenance";
            options.AuthorValue = "Robert M. Pirsig";

            Book book = client.Books.GetBestMatchAsync(options).Result;

            string permalink = client.Books.PostBookReadingAsync(this.accessToken, book.Id, Reading.ReadingState.Open).Result;

            
            Reading r = client.Readings.GetAsync<Reading>(new Uri(permalink+"?client_id="+this.clientId)).Result;
            try
            {
                ReadingSession session = client.Readings.GetReadingSession(this.accessToken, r.Id);
                session.PingAsync((float)0.1).Wait();
                //Thread.Sleep(5000);
                session.PingAsync((float)0.12).Wait();

                Highlight testHighlight = new Highlight() { Content = "When one person suffers from a delusion, it is called insanity. When many people suffer from a delusion it is called a Religion." };
                session.PostHighlightAsync(testHighlight).Wait();

                string content = @"It’s a fascinating tale of a journey, both physically and metaphysically,
                                       into the world of quality and values";
                session.PostReadingCommentAsync(content).Wait();

                session.Close();
            }
            finally
            {
                client.Readings.DeleteReadingAsync(this.accessToken, r.Id);
            }
        }

        [TestMethod]
        public void TestGetHighlights()
        {
            ReadmillClient client = new ReadmillClient(this.clientId);
            //List<Highlight> highlights = new List<Highlight>();

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
                //Foreach reading, Get all Highlights
                RangeQueryOptions highlightOptions = new RangeQueryOptions() { CountValue = 100 };

                foreach (Highlight h in client.Readings.GetReadingHighlightsAsync(reading.Id, highlightOptions).Result)
                {
                    if(!highlights.ContainsKey(h.Locators.Position))
                        highlights.Add(h.Locators.Position, h);
                }
            }

            //Sort all highlights by position - how accurate?
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

        //Test PingAsync and Periods
    }
}
