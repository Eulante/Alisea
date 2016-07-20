using AliseaTorrent.Peering;
using AliseaTorrent.Tracking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Tracking
{
    public abstract class AbstractTracker
    {
        public string AnnounceUrl { get; }
        public string ScrapeUrl { get; }


        public AbstractTracker(string announceUrl)
        {
            this.AnnounceUrl = announceUrl;
        }

        public abstract Task<AnnounceResponse> RequestAnnounceAsync(AnnounceRequest Request);
        public abstract void RequestScrape(ScrapeRequest Request);

        
    }
}
