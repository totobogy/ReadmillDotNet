using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

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

        [DataMember(Name = "private", EmitDefaultValue = false)]
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


    /// <summary>
    /// Needed for Put/Post requests
    /// </summary>
    [DataContract]
    public class ReadingUpdate
    {
        [DataMember(Name = "reading")]
        public ReadingUpdategram ReadingUpdategram;
    }
    
}
