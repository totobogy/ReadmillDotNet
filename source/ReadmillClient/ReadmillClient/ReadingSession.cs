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

        public Task PingAsync(double progress, bool sendDuration = true, bool sendOccuredAt = true)
        {
            if (!this.isOpen)
                throw new InvalidOperationException("Session Closed.");

            Ping ping = new Ping();
            ping.SessionId = this.sessionId;
            ping.Progress = progress;

            DateTime current = DateTime.Now;
            if (sendDuration)
                ping.Duration = (current - lastPingTime).Seconds;

            if (sendOccuredAt)
                ping.OccuredAt = XmlConvert.ToString(lastPingTime = current);

            return this.client.PostReadingPingAsync(this.accessToken, this.readingId, ping);
        }

        public Task PingAsync(double progress, double latitude, double longitude, bool sendDuration = true, bool sendOccuredAt = true)
        {
            if (!this.isOpen)
                throw new InvalidOperationException("Session Closed.");

            Ping ping = new Ping();
            ping.SessionId = this.sessionId;

            ping.Progress = progress;

            ping.Latitude = latitude;
            ping.Longitude = longitude;

            DateTime current = DateTime.Now;
            if (sendDuration)
                ping.Duration = (current - lastPingTime).Seconds;

            if (sendOccuredAt)
                ping.OccuredAt = XmlConvert.ToString(lastPingTime = current);

            return this.client.PostReadingPingAsync(this.accessToken, this.readingId, ping);
        }

        public Task<string> PostHighlightAsync(Highlight highlight)
        {
            if (!this.isOpen)
                throw new InvalidOperationException("Session Closed.");

            return client.PostReadingHighlightAsync(this.accessToken, this.readingId, highlight);
        }

        public Task<string> PostReadingCommentAsync(string content)
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
