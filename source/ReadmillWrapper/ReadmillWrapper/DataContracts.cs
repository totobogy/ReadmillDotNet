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
        [DataMember(Name="id")]
        public string Id;

        [DataMember(Name = "state")]
        public string State;

        [DataMember(Name = "private")]
        public bool isPrivate;

        [DataMember(Name = "recommended")]
        public bool Recommended;

        [DataMember(Name = "closing_remark")]
        public string ClosingRemark;

        //date
        [DataMember(Name = "touched_at")]
        public string TouchedAt;

        //date
        [DataMember(Name = "finished_at")]
        public string FinishedAt;

        //date
        [DataMember(Name = "abandoned_at")]
        public string AbandonedAt;

        //date
        [DataMember(Name = "created_at")]
        public string CreatedAt;

        [DataMember(Name = "duration")]
        public Nullable<float> Duration;

        [DataMember(Name = "progress")]
        public Nullable<decimal> Progress;

        [DataMember(Name = "estimated_time_left")]
        public Nullable<decimal> EstimatedTimeLeft;

        [DataMember(Name = "average_period_time")]
        public Nullable<decimal> AveragePeriodTime;

        [DataMember(Name = "book")]
        public Book Book;

        [DataMember(Name = "user")]
        public User User;

        [DataMember(Name = "permalink_url")]
        public string PermalinkUrl;

        [DataMember(Name = "uri")]
        public string Uri;

        [DataMember(Name = "periods")]
        public string Periods;

        [DataMember(Name = "locations")]
        public string Locations;

        [DataMember(Name = "highlights")]
        public string Highlights;

        [DataMember(Name = "comments")]
        public string Comments;

        [DataMember(Name = "comments_count")]
        public int CommentsCount;
    }
    
}
