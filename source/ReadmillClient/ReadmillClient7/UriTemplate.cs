using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Com.Readmill.Api
{
    /// <summary>
    /// Design Hack: 
    /// Since the UriTemplate class is not available in Windows Phone 7 environment, 
    /// this is a hacked, bare-bones implementation. Should be absolutely done away with in Win8.
    /// </summary>
    class UriTemplate
    {
        bool ignoreTrailingSlash;
        public string Template { get; private set; }
        
        private const string parameterPattern = @"\{[a-z, A-Z, _, \[, \]]*\}";
        private const string queryParameterPattern = @"[\?, &][a-z, A-Z, _, \[, \]]*=" + parameterPattern;

        public UriTemplate(string template, bool ignoreTrailingSlash)
        {
            this.Template = template;
            this.ignoreTrailingSlash = ignoreTrailingSlash;
        }

        public Uri BindByName(Uri baseUri, IDictionary<string, string> parameters)
        {
            string pathSegment = this.Template.Split(new char[] { '?' })[0];
            string querySegment = "?" + this.Template.Split(new char[] { '?' })[1];

            //Substitute all variables in Path Segment
            foreach (string variable in parameters.Keys)
            {
                pathSegment = pathSegment.Replace("{" + variable + "}", parameters[variable]);
            }

            //There should be any unsubstituted variable in path segment anymore
            if (Regex.IsMatch(pathSegment, UriTemplate.parameterPattern))
                throw new ArgumentException("One or more path segment parameter values were missing. All path segment parameters must be substituted.");

            //Query Segment
            foreach (string variable in parameters.Keys)
            {
                querySegment = querySegment.Replace("{" + variable + "}", parameters[variable]);
            }

            //remove unsubstituted query parameter "parameter=value" pairs
            foreach (Match match in Regex.Matches(querySegment, UriTemplate.queryParameterPattern))
            {
                querySegment = querySegment.Replace(match.Value, string.Empty);
            }

            //If the first query parameter was missing we ended up removing the '?' separator and have an extraneous '&'
            if (querySegment.StartsWith("&"))
                querySegment = "?" + querySegment.Substring(1);

            return new Uri(baseUri, pathSegment + querySegment);
        }
    }
}
