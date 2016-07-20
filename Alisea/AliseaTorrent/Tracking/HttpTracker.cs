using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AliseaTorrent.Tracking.Data;
using System.Net;
using Windows.Foundation;
using Windows.Web.Http;
using AliseaTorrent.Bencode;
using System.Diagnostics;
using Windows.Storage.Streams;
using AliseaTorrent.Peering;

namespace AliseaTorrent.Tracking
{
    public class HttpTracker : AbstractTracker
    {


        public HttpTracker(string announceUrl) : base(announceUrl) { }

        
        public override async Task<AnnounceResponse> RequestAnnounceAsync(AnnounceRequest Request)
        {
            string strRequest = AnnounceUrl + BuildAnnounceParam(Request);

            Uri requestUri = new Uri(strRequest);

            AnnounceResponse announceResponse = null;

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(requestUri).AsTask();

            try
            {
                if (response.IsSuccessStatusCode)
                {
                    IBuffer responseBuffer = await response.Content.ReadAsBufferAsync();
                    DataReader reader = DataReader.FromBuffer(responseBuffer);
                    Byte[] responseBytes = new Byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(responseBytes);

                    announceResponse = AnnounceResponse.BuildFromHttpTrackerResponse(responseBytes);
                }

            }
            catch (Exception e)
            {
                Debug.Write(e);
            }

            return announceResponse;
         }



        public override void RequestScrape(ScrapeRequest Request)
        {
            throw new NotImplementedException();
        }






        private string BuildAnnounceParam(AnnounceRequest Request)
        {
            StringBuilder strParam = new StringBuilder("?info_hash=%");

            strParam.Append(BitConverter.ToString(Request.InfoHash).Replace('-', '%'));
            strParam.Append("&peer_id=%" + BitConverter.ToString(Request.PeerId).Replace('-','%'));
            strParam.Append("&port=" + Request.Port);
            strParam.Append("&uploaded=" + Request.Uploaded);
            strParam.Append("&downloaded=" + Request.Downloaded);
            strParam.Append("&left=" + Request.Left);
            //strParam.Append("&compact=" + Request.Compact);
            //strParam.Append("&no_peer_id=" + Request.NoPeerId);
            strParam.Append("&event=" + Request.Event);

            if(Request.Ip != null)
                strParam.Append("&ip=" + Request.Ip);

            if(Request.Numwant > 0)
                strParam.Append("&numwant=" + Request.Numwant);

            if(Request.Key != null)
                strParam.Append("&key=" + Request.Key);

            if (Request.TrackerId != null)
                strParam.Append("&trackerid=" + Request.TrackerId);

            return strParam.ToString();
        }



      

    }

}
