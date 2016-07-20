using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Peering
{
    public class Peer : IEquatable<Peer>
    {
        public string Id { get; private set; } = null;

        public string Address { get; private set; }  = null;

        public int Port { get; private set; }

        // this client is choking the peer
        public bool am_choking = true;

        // this client is interested in the peer
        public bool am_interested = false;

        // peer is choking this client
        public bool peer_choking = true;

        // peer is interested in this client
        public bool peer_interested = false;



        public Peer(string id, string address, int port)
        {
            this.Id = id;
            this.Address = address;
            this.Port = port;
        }

        public Peer(string address, int port) : this("NoPeerId",address, port) { }     


        public bool Equals(Peer other)
        {
            if (this.Address.Equals(other.Address) && this.Port == other.Port)
                return true;
            return false;
        }

        

    }
}
