using AliseaTorrent.Bencode;
using AliseaTorrent.Standard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Metadata
{
    public class TorrentMetaBuilder : ITorrentMetaBuilder
    {

        private TorrentMetaData metadata = null;


        public TorrentMetaData GetTorrentMetaData(string metaFileUrl)
        {
            metadata = new TorrentMetaData();
            BencodeTorrent bencodeTorrent = new BencodeTorrent(metaFileUrl);

            ExtractData(bencodeTorrent);
            return metadata;

        }




        public TorrentMetaData GetTorrentMetaData(byte[] metadataByteArray)
        {
            metadata = new TorrentMetaData();
            BencodeTorrent bencodeTorrent = new BencodeTorrent(metadataByteArray);

            ExtractData(bencodeTorrent);
            return metadata;
        }




        private void ExtractData(BencodeTorrent bencodeTorrent)
        {

            BencodeDictionary dataDictionary = (BencodeDictionary)bencodeTorrent.DictionaryInfo;

#if DEBUG
            List<BencodeElement> ll = dataDictionary.Elements;
            if (ll == null)
                Debug.Write("\nNULL LISTA NULL\n");
#endif



            foreach (BencodeElement element in dataDictionary.Elements)
            {
                switch (element.Key)
                {
                    case ProtocolNames.MetaData.Announce:
                        BencodeByteString announce = (BencodeByteString)element.Value;
                        metadata.Announce = Encoding.UTF8.GetString(announce.ByteStringValue);
                        break;
                    case ProtocolNames.MetaData.AnnounceList:
                        BencodeList bencodedAnnounceList = (BencodeList)element.Value;
                        metadata.AnnounceList = ReadAnnounceList(bencodedAnnounceList);
                        break;
                    case ProtocolNames.MetaData.Comment:
                        BencodeByteString comment = (BencodeByteString)element.Value;
                        metadata.Comment = Encoding.UTF8.GetString(comment.ByteStringValue);
                        break;
                    case ProtocolNames.MetaData.CreatedBy:
                        BencodeByteString createdby = (BencodeByteString)element.Value;
                        metadata.CreatedBy = Encoding.UTF8.GetString(createdby.ByteStringValue);
                        break;
                    case ProtocolNames.MetaData.CreationDate:
                        BencodeLong creationDate = (BencodeLong)element.Value;
                        metadata.CreationDate = (int)creationDate.LongValue;
                        break;
                    case ProtocolNames.MetaData.Encoding:
                        BencodeByteString encoding = (BencodeByteString)element.Value;
                        metadata.Encoding = Encoding.UTF8.GetString(encoding.ByteStringValue);
                        break;
                    case ProtocolNames.MetaData.Info:
                        BencodeDictionary infoDictionary = (BencodeDictionary)element.Value;
                        metadata.InfoBytes = infoDictionary.ByteData;
                        metadata.MetaInfo = ReadFilesInfo(infoDictionary);
                        break;
                }
            }
            
        }


        private TorrentFilesInfo ReadFilesInfo(BencodeDictionary bencodedInfo)
        {
            TorrentFilesInfo filesInfo = new TorrentFilesInfo();

            foreach (BencodeElement element in bencodedInfo.Elements)
            {
                switch (element.Key)
                {
                    case ProtocolNames.MetaFiles.PieceLength:
                        BencodeLong pieceLength = (BencodeLong)element.Value;
                        filesInfo.PieceLength = (int)pieceLength.LongValue;
                        break;
                    case ProtocolNames.MetaFiles.Pieces:
                        BencodeByteString pieces = (BencodeByteString)element.Value;
                        filesInfo.Pieces = pieces.ByteStringValue;
                        break;
                    case ProtocolNames.MetaFiles.Privat:
                        BencodeLong privat = (BencodeLong)element.Value;
                        filesInfo.Private = (int)privat.LongValue;
                        break;
                    case ProtocolNames.MetaFiles.Name:
                        BencodeByteString name = (BencodeByteString)element.Value;
                        filesInfo.Name = Encoding.UTF8.GetString(name.ByteStringValue);
                        break;
                    case ProtocolNames.MetaFiles.Md5sum:
                        BencodeByteString md5sum = (BencodeByteString)element.Value;
                        filesInfo.md5Sum = Encoding.UTF8.GetString(md5sum.ByteStringValue);
                        break;
                    case ProtocolNames.MetaFiles.Length:
                        BencodeLong length = (BencodeLong)element.Value;
                        filesInfo.Length = (int)length.LongValue;
                        break;
                    case ProtocolNames.MetaFiles.Files:
                        BencodeList bencodedDetail = (BencodeList)element.Value;
                        filesInfo.FilesDetails = ReadFilesDetail(bencodedDetail);
                        break;


                }
            }

            return filesInfo;
        }

        private List<TorrentFilesDetails> ReadFilesDetail(BencodeList listofdictionary)
        {
            List<TorrentFilesDetails> filesDetailList = new List<TorrentFilesDetails>();

            foreach(BencodeItem dictionaryItem in listofdictionary.Items)
            {
                BencodeDictionary detailDictionary = (BencodeDictionary)dictionaryItem;
                TorrentFilesDetails fileDetails = new TorrentFilesDetails();

                foreach (BencodeElement element in detailDictionary.Elements)
                {
                    switch (element.Key)
                    {
                        case ProtocolNames.MetaFilesDetail.Path:
                            BencodeList pathList = (BencodeList)element.Value;
                            fileDetails.Path = ReadFilePathList(pathList);
                            break;
                        case ProtocolNames.MetaFilesDetail.Length:
                            BencodeLong length = (BencodeLong)element.Value;
                            fileDetails.Length = (int)length.LongValue;
                            break;
                        case ProtocolNames.MetaFilesDetail.Md5sum:
                            BencodeByteString md5sum = (BencodeByteString)element.Value;
                            fileDetails.md5Sum = Encoding.UTF8.GetString(md5sum.ByteStringValue);
                            break;

                    }
                }

                filesDetailList.Add(fileDetails);
            }

            return filesDetailList;
        }


        private List<string> ReadAnnounceList(BencodeList listoflistofstring)
        {
            List<string> announceList = new List<string>();

            foreach (BencodeItem item in listoflistofstring.Items)
            {
                BencodeList listofstring = (BencodeList)item;
                BencodeByteString announceByte = (BencodeByteString)listofstring.Items[0];
                String announce = announceByte.ToString();
                if (announce.StartsWith("\""))
                    announce = announce.Substring(1);
                if (announce.EndsWith("\""))
                    announce = announce.Substring(0, announce.Length - 1);
                announceList.Add(announce);
            }

            return announceList;
        }

        private List<string> ReadFilePathList(BencodeList bencodedPathList)
        {
            List<string> pathList = new List<string>();

            foreach(BencodeItem item in bencodedPathList.Items)
            {
                BencodeByteString path = (BencodeByteString)item;
                pathList.Add(Encoding.UTF8.GetString(path.ByteStringValue));
            }

            return pathList;
        }


    }
}
