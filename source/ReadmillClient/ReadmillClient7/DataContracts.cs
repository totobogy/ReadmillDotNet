using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace Com.Readmill.Api.DataContracts
{
    [DataContract]
    public class User
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "username")]
        public string UserName { get; set; }

        [DataMember(Name = "firstname")]
        public string FirstName { get; set; }

        [DataMember(Name = "lastname")]
        public string LastName { get; set; }

        [DataMember(Name = "fullname")]
        public string FullName { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "city")]
        public string City { get; set; }

        [DataMember(Name = "created_at")]
        public string CreatedAt { get; set; }

        [DataMember(Name = "website")]
        public string Website { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "uri")]
        public string Uri { get; set; }

        [DataMember(Name = "permalink_url")]
        public string PermalinkUrl { get; set; }

        [DataMember(Name = "books_interesting")]
        public int BooksInteresting { get; set; }

        [DataMember(Name = "books_open")]
        public int BooksOpen { get; set; }

        [DataMember(Name = "books_finished")]
        public int BooksFinished { get; set; }

        [DataMember(Name = "books_abandoned")]
        public int BooksAbandoned { get; set; }

        [DataMember(Name = "readings")]
        public string Readings { get; set; }

        [DataMember(Name = "avatar_url")]
        public string AvatarUrl { get; set; }

        [DataMember(Name = "followers")]
        public int Followers { get; set; }

        [DataMember(Name = "followings")]
        public int Followings { get; set; }
    }

    [DataContract(Name = "asset")]
    public class BookAsset
    {
        [DataMember(Name = "vendor")]
        public string Vendor { get; set; }

        [DataMember(Name = "uri")]
        public string Uri { get; set; }

        [DataMember(Name = "acquisition_type")]
        public string AcquisitionType { get; set; }
    }

    [DataContract]
    public class Book
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "author")]
        public string Author { get; set; }

        [DataMember(Name = "isbn")]
        public string ISBN { get; set; }

        [DataMember(Name = "story")]
        public string Story { get; set; }

        [DataMember(Name = "published_at")]
        public string PublishedAt { get; set; }

        [DataMember(Name = "language")]
        public string Language { get; set; }

        [DataMember(Name = "permalink")]
        public string PermaLink { get; set; }

        [DataMember(Name = "permalink_url")]
        public string PermalinkUrl { get; set; }

        [DataMember(Name = "uri")]
        public string Uri { get; set; }

        [DataMember(Name = "cover_url")]
        public string CoverUrl { get; set; }

        [DataMember(Name = "assets")]
        public BookAsset[] Assets { get; set; }
    }

    [DataContract]
    public class Reading
    {
        [DataMember(Name="id", EmitDefaultValue=false)]
        public string Id { get; set; }

        public enum ReadingState { Interesting = 1, Open, Finished, Abandoned };
        [DataMember(Name = "state", EmitDefaultValue = false)]
        public ReadingState State { get; set; }

        [DataMember(Name = "private")]
        public bool IsPrivate { get; set; }

        [DataMember(Name = "recommended", EmitDefaultValue = false)]
        public bool Recommended { get; set; }

        [DataMember(Name = "closing_remark", EmitDefaultValue = false)]
        public string ClosingRemark { get; set; }

        //date
        [DataMember(Name = "touched_at", EmitDefaultValue = false)]
        public string TouchedAt { get; set; }

        //date
        [DataMember(Name = "finished_at", EmitDefaultValue = false)]
        public string FinishedAt { get; set; }

        //date
        [DataMember(Name = "abandoned_at", EmitDefaultValue = false)]
        public string AbandonedAt { get; set; }

        //date
        [DataMember(Name = "created_at", EmitDefaultValue = false)]
        public string CreatedAt { get; set; }

        [DataMember(Name = "duration", EmitDefaultValue = false)]
        public Nullable<float> Duration { get; set; }

        [DataMember(Name = "progress", EmitDefaultValue = false)]
        public Nullable<decimal> Progress { get; set; }

        [DataMember(Name = "estimated_time_left", EmitDefaultValue = false)]
        public Nullable<decimal> EstimatedTimeLeft { get; set; }

        [DataMember(Name = "average_period_time", EmitDefaultValue = false)]
        public Nullable<decimal> AveragePeriodTime { get; set; }

        [DataMember(Name = "book", EmitDefaultValue = false)]
        public Book Book { get; set; }

        [DataMember(Name = "user", EmitDefaultValue = false)]
        public User User { get; set; }

        [DataMember(Name = "permalink_url", EmitDefaultValue = false)]
        public string PermalinkUrl { get; set; }

        [DataMember(Name = "uri", EmitDefaultValue = false)]
        public string Uri { get; set; }

        [DataMember(Name = "periods", EmitDefaultValue = false)]
        public string Periods { get; set; }

        [DataMember(Name = "locations", EmitDefaultValue = false)]
        public string Locations { get; set; }

        [DataMember(Name = "highlights", EmitDefaultValue = false)]
        public string Highlights { get; set; }

        [DataMember(Name = "comments", EmitDefaultValue = false)]
        public string Comments { get; set; }

        [DataMember(Name = "comments_count", EmitDefaultValue = false)]
        public int CommentsCount { get; set; }
    }

    /// <summary>
    /// This class exists due to a bug in Readmill API where the field is named differently for PUT requests
    /// </summary>
    [DataContract]
    public class ReadingUpdategram : Reading
    {

        [DataMember(Name = "is_private")]
        public bool IsPrivate { get; set; }
    }

    [DataContract]
    public class XPaths
    {
        [DataMember(Name = "start", IsRequired = true)]
        public string StartTag { get; set; }

        [DataMember(Name="end", IsRequired = true)]
        public string EndTag { get; set; }
    }

    [DataContract]
    public class Locators
    {
        [DataMember(Name = "position")]
        public Decimal Position { get; set; }

        [DataMember(Name = "pre")]
        public string Pre { get; set; }

        [DataMember(Name = "mid")]
        public string Mid { get; set; }

        [DataMember(Name = "post")]
        public string Post { get; set; }

        [DataMember(Name="xpath")]
        public XPaths XPath { get; set; }

        [DataMember(Name = "file_id")]
        public string FileId { get; set; }
    }

    [DataContract]
    public class Highlight
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(Name = "reading_id", EmitDefaultValue = false)]
        public string ReadingId { get; set; }

        [DataMember(Name="position")]
        public decimal Position { get; set; }

        [DataMember(Name="content")]
        public string Content { get; set; }

        //Date
        [DataMember(Name = "highlighted_at", EmitDefaultValue = false)]
        public string HighlightedAt { get; set; }

        [DataMember(Name = "uri", EmitDefaultValue = false)]
        public string Uri { get; set; }

        [DataMember(Name = "permalink", EmitDefaultValue = false)]
        public string Permalink { get; set; }

        [DataMember(Name = "permalink_url", EmitDefaultValue = false)]
        public string PermalinkUrl { get; set; }

        [DataMember(Name = "locators", EmitDefaultValue = false)]
        public Locators Locators { get; set; }

        [DataMember(Name = "user", EmitDefaultValue = false)]
        public User User { get; set; }

        [DataMember(Name = "comments", EmitDefaultValue = false)]
        public string Comments { get; set; }

        [DataMember(Name = "comments_count", EmitDefaultValue = false)]
        public int CommentsCount { get; set; }
    }

    [DataContract]
    public class Ping
    {
        /// <summary>
        /// A unique identifier that groups pings together for a reading session.
        /// </summary>
        [DataMember(Name="identifier", IsRequired = true)]
        public string SessionId { get; set; }

        /// <summary>
        ///  Progress given as a float between 0.0 and 1.0.
        /// </summary>
        [DataMember(Name="progress", IsRequired = true)]
        public float Progress { get; set; }

        /// <summary>
        ///  Time since last ping, in seconds. (Optional)
        /// </summary>
        [DataMember(Name="duration", EmitDefaultValue=false)]
        public int Duration { get; set; }

        /// <summary>
        /// The time the ping was made, as an RFC3339 formatted string. (Optional)
        /// </summary>
        [DataMember(Name="occured_at", EmitDefaultValue=false)]
        public string OccuredAt { get; set; }

        /// <summary>
        /// The latitude of the location at the time of the ping as a float. (Optional)
        /// </summary>
        [DataMember(Name="lat", EmitDefaultValue=false)]
        public float Latitude { get; set; }

        /// <summary>
        /// The longitude of the location at the time of the ping as a float. (Optional)
        /// </summary>
        [DataMember(Name="lng", EmitDefaultValue=false)]
        public float Longitude { get; set; }
    }

    [DataContract]
    public class Comment
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(Name = "user", EmitDefaultValue = false)]
        public User User { get; set; }

        [DataMember(Name = "reading_id", EmitDefaultValue = false)]
        public string ReadingId { get; set; }

        [DataMember(Name = "highlight_id", EmitDefaultValue = false)]
        public string HighlightId { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }

        //Date?
        [DataMember(Name = "posted_at", EmitDefaultValue = false)]
        public string PostedAt { get; set; }

        [DataMember(Name = "uri", EmitDefaultValue = false)]
        public string Uri { get; set; }
    }
    

    //Non-public DataContracts (e.g. wrappers needed for Post / Put)

    /// <summary>
    /// Needed for Post requests - Shouldn't be needed after API V2 with the change for flat structures
    /// </summary>
    [DataContract]
    class ReadingPost
    {
        [DataMember(Name = "reading")]
        public Reading Reading { get; set; }
    }

    /// <summary>
    /// Needed for Put requests - Shouldn't be needed after API V2 with the change for flat structures
    /// </summary>
    [DataContract]
    class ReadingUpdate
    {
        [DataMember(Name = "reading")]
        public ReadingUpdategram ReadingUpdategram { get; set; }

    }

    //Wrapper for Ping
    [DataContract]
    class WrappedPing
    {
        [DataMember(Name="ping")]
        public Ping Ping { get; set; }
    }

    [DataContract]
    class WrappedHighlight
    {
        [DataMember (Name="highlight")]
        public Highlight Highlight { get; set; }
    }

    [DataContract]
    class WrappedComment
    {
        [DataMember(Name="comment")]
        public Comment Comment { get; set; }
    }

}
