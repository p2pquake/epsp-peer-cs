using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Peer.Manager
{
    class InquiryData
    {
        public string peerId;
        public string uniqueValue;
        public Peer peer;
    }

    class NetworkInquiryManager
    {
        private static readonly int CAPACITY = 100;
        private List<InquiryData> inquiryList;

        public NetworkInquiryManager()
        {
            inquiryList = new List<InquiryData>(CAPACITY);
        }

        public Peer FindPeer(string peerId, string uniqueValue)
        {
            var result = inquiryList.Where(e => (e.peerId == peerId && e.uniqueValue == uniqueValue));
            if (result.Count() > 0)
            {
                return result.First().peer;
            }
            return null;
        }

        public void Add(string peerId, string uniqueValue, Peer peer)
        {
            var inquiryData = new InquiryData() { peerId = peerId, uniqueValue = uniqueValue, peer = peer };
            if (inquiryList.Count >= CAPACITY)
            {
                inquiryList.RemoveAt(0);
            }
            inquiryList.Add(inquiryData);
        }
    }
}
