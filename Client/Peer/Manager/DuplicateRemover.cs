using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Client.Common.Net;

namespace Client.Peer.Manager
{
    class DuplicateRemover
    {
        private static readonly int DUPLICATE_CAPACITY = 10000;
        private List<string> duplicateList;

        public DuplicateRemover()
        {
            duplicateList = new List<string>(DUPLICATE_CAPACITY);
        }

        public bool isDuplicate(Packet packet)
        {
            string data = $"{packet.Code}:{string.Join(":", packet.Data)}";

            lock (duplicateList)
            {
                if (duplicateList.Contains(data))
                {
                    return true;
                }
                else
                {
                    if (duplicateList.Count >= DUPLICATE_CAPACITY)
                    {
                        duplicateList.RemoveAt(0);
                    }

                    duplicateList.Add(data);
                    return false;
                }
            }
        }
    }
}
