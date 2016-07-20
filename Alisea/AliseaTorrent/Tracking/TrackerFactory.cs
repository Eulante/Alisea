using AliseaTorrent.Tracking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AliseaTorrent.Metadata;

namespace AliseaTorrent.Tracking
{
    public class TrackerFactory : ITrackerFactory
    {
        public AbstractTracker CreateAbstractTracker(string trackerUrl)
        {
            AbstractTracker tracker = null;

            if (trackerUrl.StartsWith("http"))
                tracker = new HttpTracker(trackerUrl);
            else
                tracker = new UdpTracker(trackerUrl);

            return tracker;
        }


        // Given an Announce List from a torrent, the method will create a list of possible trackers, either 
        // Udp then Http, depending on their type;
        public List<AbstractTracker> createTrackersList(List<string> trackers)
        {
            List<AbstractTracker> trackersList = new List<AbstractTracker>();

            foreach (string s in trackers)
                trackersList.Add(CreateAbstractTracker(s));

            return trackersList;
        }


    }
}
