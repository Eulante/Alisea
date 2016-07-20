using AliseaTorrent.Tracking;
using System.Collections.Generic;

namespace AliseaTorrent
{
    interface ITrackerFactory
    {

        AbstractTracker CreateAbstractTracker(string trackerUrl);

        List<AbstractTracker> createTrackersList(List<string> trackers);
    }
}
