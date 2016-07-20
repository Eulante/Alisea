using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Standard
{
    public class ProtocolNames
    {
        public struct MetaData
        {
            public const string Info = "info";
            public const string Announce = "announce";
            public const string AnnounceList = "announce-list";
            public const string CreationDate = "creation date";
            public const string CreatedBy = "created by";
            public const string Comment = "comment";
            public const string Encoding = "encoding";

        }

        public struct MetaFiles
        {
            public const string PieceLength = "piece length";
            public const string Pieces = "pieces";
            public const string Privat = "private";
            public const string Name = "name";
            public const string Length = "length";
            public const string Md5sum = "md5sum";
            public const string Files = "files";
        }

        public struct MetaFilesDetail
        {
            public const string Length = "length";
            public const string Md5sum = "md5sum";
            public const string Path = "path";
        }


        public class Tracking
        {
            public struct Request
            {
                public const string EventEmpty = "empty";
                public const string EventStarted = "started";
                public const string EventStopped = "stopped";
                public const string EventCompleted = "completed";
            }
        }
    }
}
