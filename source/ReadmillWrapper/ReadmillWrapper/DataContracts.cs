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
        public string Id;

        [DataMember(Name = "username")]
        public string UserName;

        [DataMember(Name = "firstname")]
        public string FirstName;

        [DataMember(Name = "lastname")]
        public string LastName;

        [DataMember(Name = "fullname")]
        public string FullName;

        [DataMember(Name = "country")]
        public string Country;

        [DataMember(Name = "city")]
        public string City;

        [DataMember(Name = "created_at")]
        public string CreatedAt;

        [DataMember(Name = "website")]
        public string Website;

        [DataMember(Name = "description")]
        public string Description;

        [DataMember(Name = "uri")]
        public string Uri;

        [DataMember(Name = "permalink_url")]
        public string PermalinkUrl;

        [DataMember(Name = "books_interesting")]
        public int BooksInteresting;

        [DataMember(Name = "books_open")]
        public int BooksOpen;

        [DataMember(Name = "books_finished")]
        public int BooksFinished;

        [DataMember(Name = "books_abandoned")]
        public int BooksAbandoned;

        [DataMember(Name = "readings")]
        public string Readings;

        [DataMember(Name = "avatar_url")]
        public string AvatarUrl;

        [DataMember(Name = "followers")]
        public int Followers;

        [DataMember(Name = "followings")]
        public int Followings;
    }

    [DataContract(Name = "asset")]
    public class BookAsset
    {
        [DataMember(Name = "vendor")]
        public string Vendor;

        [DataMember(Name = "uri")]
        public string Uri;

        [DataMember(Name = "acquisition_type")]
        public string AcquisitionType;
    }

    [DataContract]
    public class Book
    {
        [DataMember(Name = "id")]
        public string Id;

        [DataMember(Name = "title")]
        public string Title;

        [DataMember(Name = "author")]
        public string Author;

        [DataMember(Name = "isbn")]
        public string ISBN;

        [DataMember(Name = "story")]
        public string Story;

        [DataMember(Name = "published_at")]
        public string PublishedAt;

        [DataMember(Name = "language")]
        public string Language;

        [DataMember(Name = "permalink")]
        public string PermaLink;

        [DataMember(Name = "permalink_url")]
        public string PermalinkUrl;

        [DataMember(Name = "uri")]
        public string Uri;

        [DataMember(Name = "cover_url")]
        public string CoverUrl;

        [DataMember(Name = "assets")]
        public BookAsset[] Assets;
    }

    [DataContract]
    public class Reading
    {
        [DataMember(Name="id", EmitDefaultValue=false)]
        public string Id;

        public enum ReadingState { Interesting = 1, Open, Finished, Abandoned };
        [DataMember(Name = "state", EmitDefaultValue = false)]
        public ReadingState State;

        [DataMember(Name = "private")]
        public bool IsPrivate;

        [DataMember(Name = "recommended", EmitDefaultValue = false)]
        public bool Recommended;

        [DataMember(Name = "closing_remark", EmitDefaultValue = false)]
        public string ClosingRemark;

        //date
        [DataMember(Name = "touched_at", EmitDefaultValue = false)]
        public string TouchedAt;

        //date
        [DataMember(Name = "finished_at", EmitDefaultValue = false)]
        public string FinishedAt;

        //date
        [DataMember(Name = "abandoned_at", EmitDefaultValue = false)]
        public string AbandonedAt;

        //date
        [DataMember(Name = "created_at", EmitDefaultValue = false)]
        public string CreatedAt;

        [DataMember(Name = "duration", EmitDefaultValue = false)]
        public Nullable<float> Duration;

        [DataMember(Name = "progress", EmitDefaultValue = false)]
        public Nullable<decimal> Progress;

        [DataMember(Name = "estimated_time_left", EmitDefaultValue = false)]
        public Nullable<decimal> EstimatedTimeLeft;

        [DataMember(Name = "average_period_time", EmitDefaultValue = false)]
        public Nullable<decimal> AveragePeriodTime;

        [DataMember(Name = "book", EmitDefaultValue = false)]
        public Book Book;

        [DataMember(Name = "user", EmitDefaultValue = false)]
        public User User;

        [DataMember(Name = "permalink_url", EmitDefaultValue = false)]
        public string PermalinkUrl;

        [DataMember(Name = "uri", EmitDefaultValue = false)]
        public string Uri;

        [DataMember(Name = "periods", EmitDefaultValue = false)]
        public string Periods;

        [DataMember(Name = "locations", EmitDefaultValue = false)]
        public string Locations;

        [DataMember(Name = "highlights", EmitDefaultValue = false)]
        public string Highlights;

        [DataMember(Name = "comments", EmitDefaultValue = false)]
        public string Comments;

        [DataMember(Name = "comments_count", EmitDefaultValue = false)]
        public int CommentsCount;
    }

    /// <summary>
    /// This class exists due to a bug in Readmill API where the field is named differently for PUT requests
    /// </summary>
    [DataContract]
    public class ReadingUpdategram : Reading
    {

        [DataMember(Name = "is_private")]
        public bool IsPrivate;
    }

    [DataContract]
    public class XPaths
    {
        [DataMember(Name = "start", IsRequired = true)]
        public string StartTag;

        [DataMember(Name="end", IsRequired = true)]
        public string EndTag;
    }

    [DataContract]
    public class Locators
    {
        [DataMember(Name = "position")]
        public Decimal Position;

        [DataMember(Name = "pre")]
        public string Pre;

        [DataMember(Name = "mid")]
        public string Mid;

        [DataMember(Name = "post")]
        public string Post;

        [DataMember(Name="xpath")]
        public XPaths XPath;

        [DataMember(Name = "file_id")]
        public string FileId;
    }

    [DataContract]
    public class Highlight
    {
        [DataMember(Name = "id")]
        public string Id;

        [DataMember(Name="reading_id")]
        public string ReadingId;

        [DataMember(Name="position")]
        public decimal Position;

        [DataMember(Name="content")]
        public string Content;

        [DataMember(Name="highlighted_at")]
        public string HighlightedAt;

        [DataMember(Name="uri")]
        public string Uri;

        [DataMember(Name="permalink")]
        public string Permalink;

        [DataMember(Name="permalink_url")]
        public string PermalinkUrl;

        [DataMember(Name="locators")]
        public Locators Locators;

        [DataMember(Name="user")]
        public User User;

        [DataMember(Name="comments")]
        public string Comments;

        [DataMember(Name="comments_count")]
        public int CommentsCount;
    }

    [DataContract]
    public class Ping
    {
        /// <summary>
        /// A unique identifier that groups pings together for a reading session.
        /// </summary>
        [DataMember(Name="identifier", IsRequired = true)]
        public string SessionId;

        /// <summary>
        ///  Progress given as a float between 0.0 and 1.0.
        /// </summary>
        [DataMember(Name="progress", IsRequired = true)]
        public float Progress;

        /// <summary>
        ///  Time since last ping, in seconds. (Optional)
        /// </summary>
        [DataMember(Name="duration", EmitDefaultValue=false)]
        public int Duration;

        /// <summary>
        /// The time the ping was made, as an RFC3339 formatted string. (Optional)
        /// </summary>
        [DataMember(Name="occured_at", EmitDefaultValue=false)]
        public string OccuredAt;

        /// <summary>
        /// The latitude of the location at the time of the ping as a float. (Optional)
        /// </summary>
        [DataMember(Name="lat", EmitDefaultValue=false)]
        public float Latitude;

        /// <summary>
        /// The longitude of the location at the time of the ping as a float. (Optional)
        /// </summary>
        [DataMember(Name="lng", EmitDefaultValue=false)]
        public float Longitude;
    }
    

    //Non-public DataContracts (e.g. wrappers needed for Post / Put)

    /// <summary>
    /// Needed for Post requests - Shouldn't be needed after API V2 with the change for flat structures
    /// </summary>
    [DataContract]
    class ReadingPost
    {
        [DataMember(Name = "reading")]
        public Reading Reading;
    }

    /// <summary>
    /// Needed for Put requests - Shouldn't be needed after API V2 with the change for flat structures
    /// </summary>
    [DataContract]
    class ReadingUpdate
    {
        [DataMember(Name = "reading")]
        public ReadingUpdategram ReadingUpdategram;

    }

    //Wrapper for Ping
    [DataContract]
    class ReadingPing
    {
        [DataMember(Name="ping")]
        public Ping Ping;
    }

}
