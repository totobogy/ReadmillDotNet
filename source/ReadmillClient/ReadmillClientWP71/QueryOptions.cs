﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.Readmill.Api.DataContracts;

namespace Com.Readmill.Api
{
    public class ReadingsQueryOptions: RangeQueryOptions
    {        
        //Order
        /*private string orderValueInternal;
        public string OrderValueInternal { get { return orderValueInternal; } }

        public enum OrderOptions { TouchedAt_Desc, CreatedAt_Desc };
        /// <summary>
        /// The sort order of the collection. Valid options are touched_at (descending), created_at (descending) (optional)
        /// The default sort order is descending on created_at
        /// </summary>
        private OrderOptions orderValue;
        public OrderOptions OrderValue
        {
            get
            {
                return orderValue;
            }

            set
            {
                orderValue = value;
                switch (value)
                {
                    case OrderOptions.CreatedAt_Desc: orderValueInternal = "created_at"; break;
                    case OrderOptions.TouchedAt_Desc: orderValueInternal = "touched_at"; break;
                }
            }
        }
        public const string Order = "order";*/

        /// <summary>
        /// Ways to filter the set. Valid options are followings (optional but requires access_token when used)
        /// </summary>
        public string FilterValue { get; set; }
        public const string Filter = "filter";

        /// <summary>
        /// The first count of highlights to be included. (optional)
        /// If only highlights_count[from] is included when filtering on highlights_count, highlights_count[to] is assumed to be higher than the highest highlights_count available
        /// </summary>
        public string HighlightsCountFromValue { get; set; }
        public const string HighlightsCountFrom = "highlightscount[from]";

        /// <summary>
        /// The last count of highlights to be included. (optional)
        /// </summary>
        public string HighlightsCountToValue { get; set; }
        public const string HighlightsCountTo = "highlightscount[to]";

        /// <summary>
        /// Comma-separated list of interesting, reading, finished, abandoned. (optional)
        /// </summary>
        public string StatusValue { get; set; }
        public const string Status = "status";
    }

    public class BooksQueryOptions
    {
        public const string SearchString = "search";
        public string SearchStringValue { get; set; }

        public const string Count = "count";
        public Nullable<int> CountValue { get; set; }
    }

    public class BookMatchOptions
    {
        /// <summary>
        /// Either BookMatchOptions.ISBNValue OR BOTH BookMatchOptions.TitleValue and BookMatchOptions.AuthorValue are required for a search
        /// </summary>
        public string TitleValue { get; set; }
        public const string Title = "title";

        /// <summary>
        /// Either BookMatchOptions.ISBNValue OR BOTH BookMatchOptions.TitleValue and BookMatchOptions.AuthorValue are required for a search
        /// </summary>
        public string AuthorValue { get; set; }
        public const string Author = "author";

        /// <summary>
        /// Either BookMatchOptions.ISBNValue OR BOTH BookMatchOptions.TitleValue and BookMatchOptions.AuthorValue are required for a search
        /// </summary>
        public string ISBNValue { get; set; }
        public const string ISBN = "ISBN";
    }

    public class RangeQueryOptions
    {
        /// <summary>
        /// The first date to be included. (optional, range query)
        /// If only from is included with a range query, to is assumed to be higher than the highest id available.
        /// </summary>
        public string FromValue { get; set; }
        public const string From = "from";

        /// <summary>
        /// The last date, not inclusive. (optional, range query)
        /// </summary>
        public string ToValue { get; set; }
        public const string To = "to";

        /// <summary>
        /// The number of results to return, defaults to 20, limited to 100. (optional)
        /// </summary>
        public Nullable<int> CountValue { get; set; }
        public const string Count = "count";

        /// <summary>
        /// A timestamp key eg "highlighted_at". (required when making a range query)
        /// </summary>
        public string OrderByValue { get; set; }
        public const string OrderBy = "order";
    }
}
