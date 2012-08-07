using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.Readmill.Api.DataContracts;
using System.Xml;

namespace Com.Readmill.Api
{
    public class ReadingSession
    {
        private bool isOpen;
        private string sessionId;
        private string accessToken;
        private string readingId;
        private ReadingsClient client;

        private DateTime lastPingTime;

        public ReadingSession(string accessToken, string readingId, ReadingsClient readingsClient)
        {
            this.client = readingsClient;
            this.accessToken = accessToken;
            this.readingId = readingId;
            this.lastPingTime = DateTime.Now;

            this.sessionId = Guid.NewGuid().ToString();
            this.isOpen = true;
        }

        public Task Ping(float progress, bool sendDuration = true, bool sendOccuredAt = true)
        {
            if (!this.isOpen)
                throw new InvalidOperationException("Session Closed.");

            Ping ping = new Ping();
            ping.SessionId = this.sessionId;

            //ToDo: Shouldn't be less than last progress?
            ping.Progress = progress;

            if (sendOccuredAt)
                ping.OccuredAt = XmlConvert.ToString(lastPingTime = DateTime.Now);

            if (sendDuration)
                ping.Duration = (DateTime.Now - lastPingTime).Seconds;

            return this.client.SendReadingPingAsync(this.accessToken, this.readingId, ping);
        }

        public Task Ping(float progress, float latitude, float longitude, bool sendDuration = true, bool sendOccuredAt = true)
        {
            if (!this.isOpen)
                throw new InvalidOperationException("Session Closed.");

            Ping ping = new Ping();
            ping.SessionId = this.sessionId;

            //ToDo: Shouldn't be less than last progress?
            ping.Progress = progress;

            ping.Latitude = latitude;
            ping.Longitude = longitude;

            if (sendOccuredAt)
                ping.OccuredAt = XmlConvert.ToString(lastPingTime = DateTime.Now);

            if (sendDuration)
                ping.Duration = (DateTime.Now - lastPingTime).Seconds;

            return this.client.SendReadingPingAsync(this.accessToken, this.readingId, ping);
        }

        public Task PostHighlightAsync(Highlight highlight)
        {
            if (!this.isOpen)
                throw new InvalidOperationException("Session Closed.");

            return client.PostReadingHighlightAsync(this.accessToken, this.readingId, highlight);
        }

        public Task PostReadingCommentAsync(string content)
        {
            if (!this.isOpen)
                throw new InvalidOperationException("Session Closed.");

            Comment comment = new Comment() { Content = content };
            return client.PostReadingCommentAsync(this.accessToken, this.readingId, comment);
        }
        
        public void Close()
        {
            this.isOpen = false;
            this.accessToken = null;
            this.sessionId = null;
            this.client = null;
        }
    }
}
