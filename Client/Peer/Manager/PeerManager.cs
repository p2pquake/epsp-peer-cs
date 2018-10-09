﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Client.Common.General;
using Client.Common.Net;
using Client.Peer.General;
using PKCSPeerCrypto;

namespace Client.Peer.Manager
{
    class PeerManager
    {
        private List<Peer> peerList;
        private DuplicateRemover duplicateRemover;
        private NetworkInquiryManager networkInquiryManager;
        private Timer echoTimer;

        public event EventHandler<EventArgs> ConnectionsChanged;
        public event EventHandler<EPSPQuakeEventArgs> OnEarthquake;
        public event EventHandler<EPSPTsunamiEventArgs> OnTsunami;
        public event EventHandler<EPSPAreapeersEventArgs> OnAreapeers;
        public event EventHandler<EPSPEEWTestEventArgs> OnEEWTest;
        public event EventHandler<EPSPUserquakeEventArgs> OnUserquake;

#if RAISE_RAW_DATA_EVENT
        public event EventHandler<EPSPRawDataEventArgs> OnData;      
#endif

        public Func<int> PeerId { get; set; }
        public Func<DateTime> ProtocolTime { get; set; }
        public Func<int> PeerCount { get; set; }

        internal int Connections { get { return peerList.Count; } }


        public PeerManager()
        {
            peerList = new List<Peer>();
            duplicateRemover = new DuplicateRemover();
            networkInquiryManager = new Manager.NetworkInquiryManager();
            echoTimer = new Timer(EchoTimer_Tick);
            echoTimer.Change(0, 300000);
        }

        public void AddFromSocket(Socket socket)
        {
            CRLFSocket crlfSocket = new CRLFSocket(socket);
            Peer peer = new Peer(this, crlfSocket);
            peer.Closed += new EventHandler(Peer_Closed);
            peer.ReadLine += new EventHandler<ReadLineEventArgs>(Peer_ReadLine);
            peer.PeerId += () => { return PeerId(); };

            IPEndPoint remoteEndPoint = crlfSocket.RemoteEndPoint;
            peer.PeerData = new PeerData(remoteEndPoint.Address.ToString(), remoteEndPoint.Port, -1);

            if (peerList.Any(e => e.PeerData.Address == peer.PeerData.Address))
            {
                peer.Disconnect();
                return;
            }

            peerList.Add(peer);
            ConnectionsChanged(this, EventArgs.Empty);

            peer.BeginReceive();

            // 初期通信はAcceptした側から開始する
            Packet packet = new Packet();
            packet.Code = 614;
            packet.Hop = 1;
            packet.Data = new string[] { Const.PROTOCOL_VERSION, Const.SOFTWARE_NAME, Const.SOFTWARE_VERSION };
            peer.Send(packet);
        }
             
        public bool Connect(PeerData peerData)
        {
            Peer peer = new Peer(this);
            peer.Closed += new EventHandler(Peer_Closed);
            peer.ReadLine += new EventHandler<ReadLineEventArgs>(Peer_ReadLine);
            peer.PeerId += () => { return PeerId(); };
            
            bool result = peer.Connect(peerData);
            if (result)
            {
                peerList.Add(peer);
                ConnectionsChanged(this, EventArgs.Empty);
            }

            return result;
        }
        
        internal void Send(Packet packet)
        {
            Send(packet, null);
        }

        internal void Send(Packet packet, Peer exceptPeer)
        {
            var list = new List<Peer>(peerList);
            foreach (Peer peer in list)
            {
                if (peer != exceptPeer)
                {
                    peer.Send(packet);
                }
            }
        }

        void Peer_ReadLine(object sender, ReadLineEventArgs e)
        {
            if (duplicateRemover.isDuplicate(e.packet))
            {
                return;
            }

            // ホップ数による破棄。VB6版P2P地震情報と同等
            if (e.packet.Hop >= PeerCount() / 20 + 5)
            {
                return;
            }

            RaiseDataEvent(e.packet);

            e.packet.Hop++;

            if (ProcessNetworkInquiry(sender, e))
            {
                return;
            }

            Send(e.packet, (Peer)sender);
        }

        private void RaiseDataEvent(Packet packet)
        {
            // オプションにより有効とする
#if RAISE_RAW_DATA_EVENT
            EPSPRawDataEventArgs raw = new EPSPRawDataEventArgs();
            raw.Packet = packet.ToPacketString();
            OnData(this, raw);  
#endif
            
            RaiseEarthquakeEvent(packet);
            RaiseTsunamiEvent(packet);
            RaiseAreapeersEvent(packet);
            RaiseEEWTestEvent(packet);
            RaiseUserquakeEvent(packet);
        }

        private void RaiseEarthquakeEvent(Packet packet)
        {
            if (packet.Code != Code.EARTHQUAKE)
            {
                return;
            }

            if (packet.Data == null || packet.Data.Length < 4)
            {
                return;
            }

            EPSPQuakeEventArgs e = new EPSPQuakeEventArgs() { ReceivedAt = ProtocolTime() };
            Verifier.VerifyResult result = Verifier.VerifyServerData(packet.Data[2] + packet.Data[3], packet.Data[1], packet.Data[0], ProtocolTime());
            e.IsExpired = result.isExpired;
            e.IsInvalidSignature = !result.isValidSignature;

            // 地震概要の解析
            string[] abstracts = packet.Data[2].Split(',');
            if (abstracts.Length != 11)
            {
                return;
            }

            e.OccuredTime = abstracts[0];
            e.Scale = abstracts[1];
            e.TsunamiType = (DomesticTsunamiType)int.Parse(abstracts[2]);
            e.InformationType = (QuakeInformationType)int.Parse(abstracts[3]);
            e.Destination = abstracts[4];
            e.Depth = abstracts[5];
            e.Magnitude = abstracts[6];
            e.IsCorrection = abstracts[7] == "1";
            e.Latitude = abstracts[8];
            e.Longitude = abstracts[9];
            e.IssueFrom = abstracts[10];

            // 震度観測点の解析
            e.PointList = new List<QuakeObservationPoint>();
            string[] details = packet.Data[3].Split(',');
            string prefecture = null;
            string scale = null;
            foreach (string detail in details)
            {
                if (detail.Length <= 0)
                {
                    continue;
                }

                if (detail[0] == '-')
                {
                    prefecture = detail.Substring(1);
                    continue;
                }
                if (detail[0] == '+')
                {
                    scale = detail.Substring(1);
                    continue;
                }
                if (detail[0] != '*')
                {
                    continue;
                }

                QuakeObservationPoint point = new QuakeObservationPoint();
                point.Prefecture = prefecture;
                point.Scale = scale;
                point.Name = detail.Substring(1);

                e.PointList.Add(point);
            }

            OnEarthquake(this, e);
        }

        private void RaiseTsunamiEvent(Packet packet)
        {
            if (packet.Code != Code.TSUNAMI)
            {
                return;
            }

            if (packet.Data == null || packet.Data.Length < 3)
            {
                return;
            }

            string[] datas = packet.Data[2].Split(',');
            EPSPTsunamiEventArgs e = new EPSPTsunamiEventArgs() { ReceivedAt = ProtocolTime() };
            Verifier.VerifyResult result = Verifier.VerifyServerData(packet.Data[2], packet.Data[1], packet.Data[0], ProtocolTime());
            e.IsExpired = result.isExpired;
            e.IsInvalidSignature = !result.isValidSignature;

            if (datas[0] == "解除")
            {
                e.IsCancelled = true;
                OnTsunami(this, e);
                return;
            }

            e.RegionList = new List<TsunamiForecastRegion>();
            TsunamiCategory category = TsunamiCategory.Unknown;
            foreach (string data in datas)
            {
                if (data.Length <= 0)
                {
                    continue;
                }

                if (data[0] == '-')
                {
                    if (data.EndsWith("津波注意報")) { category = TsunamiCategory.Advisory; }
                    if (data.EndsWith("津波警報")) { category = TsunamiCategory.Warning; }
                    if (data.EndsWith("大津波警報")) { category = TsunamiCategory.MajorWarning; }
                    continue;
                }

                if (data[0] != '*' && data[0] != '+')
                {
                    continue;
                }

                TsunamiForecastRegion item = new TsunamiForecastRegion();
                item.Category = category;
                item.Region = data.Substring(1);
                if (data[0] == '*') { item.IsImmediately = true; }

                e.RegionList.Add(item);
            }

            OnTsunami(this, e);
        }

        private void RaiseAreapeersEvent(Packet packet)
        {
            if (packet.Code != Code.AREAPEERS)
            {
                return;
            }

            if (packet.Data == null || packet.Data.Length < 3)
            {
                return;
            }

            string[] datas = packet.Data[2].Split(';');
            var dataPattern = new Regex("^[^,]*,\\d+$");

            if (!datas.All(data => dataPattern.IsMatch(data)))
            {
                return;
            }

            EPSPAreapeersEventArgs e = new EPSPAreapeersEventArgs() { ReceivedAt = ProtocolTime() };
            Verifier.VerifyResult result = Verifier.VerifyServerData(packet.Data[2], packet.Data[1], packet.Data[0], ProtocolTime());
            e.IsExpired = result.isExpired;
            e.IsInvalidSignature = !result.isValidSignature;

            e.AreaPeerDictionary = datas.ToDictionary(data => data.Split(',')[0], data => int.Parse(data.Split(',')[1]));
            OnAreapeers(this, e);
        }

        private void RaiseEEWTestEvent(Packet packet)
        {
            if (packet.Code != Code.AREAPEERS)
            {
                return;
            }

            if (packet.Data == null || packet.Data.Length < 3)
            {
                return;
            }

            string[] datas = packet.Data[2].Split(';');
            bool isEEW = datas.Any(item => item == "950,0");
            bool isEEWTest = datas.Any(item => item == "951,0");

            if (!isEEW && !isEEWTest)
            {
                return;
            }
            
            EPSPEEWTestEventArgs e = new EPSPEEWTestEventArgs() { ReceivedAt = ProtocolTime() };
            Verifier.VerifyResult result = Verifier.VerifyServerData(packet.Data[2], packet.Data[1], packet.Data[0], ProtocolTime());
            e.IsExpired = result.isExpired;
            e.IsInvalidSignature = !result.isValidSignature;
            e.IsTest = isEEWTest;

            OnEEWTest(this, e);
        }

        private void RaiseUserquakeEvent(Packet packet)
        {
            if (packet.Code != Code.USERQUAKE)
            {
                return;
            }

            if (packet.Data == null || packet.Data.Length < 6)
            {
                return;
            }

            string[] data = packet.Data[5].Split(',');
            if (data.Length < 2)
            {
                return;
            }

            EPSPUserquakeEventArgs e = new EPSPUserquakeEventArgs() { ReceivedAt = ProtocolTime() };
            Verifier.VerifyResult result = Verifier.VerifyUserquake(packet.Data[5], packet.Data[1], packet.Data[0], packet.Data[2], packet.Data[3], packet.Data[4], ProtocolTime());
            e.IsExpired = result.isExpired;
            e.IsInvalidSignature = !result.isValidSignature;

            e.PublicKey = packet.Data[2];
            e.AreaCode = data[1];
            OnUserquake(this, e);
        }

        private bool ProcessNetworkInquiry(object sender, ReadLineEventArgs e)
        {
            // 調査エコー
            if (e.packet.Code == 615)
            {
                if (e.packet.Data == null || e.packet.Data.Length < 2)
                {
                    return true;
                }

                Send(e.packet, (Peer)sender);

                // リプライ送信
                var packet = new Packet();
                packet.Code = 635;
                packet.Hop = 1;
                packet.Data = new string[]
                {
                    e.packet.Data[0],
                    e.packet.Data[1],
                    PeerId().ToString(),
                    string.Join(",", peerList.Select(peer => peer.PeerData.PeerId)),
                    (e.packet.Hop - 1).ToString()
                };
                ((Peer)sender).Send(packet);
                
                return true;
            }

            // 調査エコーリプライ
            if (e.packet.Code == 635)
            {
                if (e.packet.Data == null || e.packet.Data.Length < 5)
                {
                    return true;
                }

                var peer = networkInquiryManager.FindPeer(e.packet.Data[0], e.packet.Data[1]);
                if (peer == null || !peer.IsConnected)
                {
                    Send(e.packet, (Peer)sender);
                } else
                {
                    peer.Send(e.packet);
                }
                return true;
            }

            return false;
        }

        void Peer_Closed(object sender, EventArgs e)
        {
            peerList.Remove((Peer)sender);
            ConnectionsChanged(this, EventArgs.Empty);
        }

        internal void DisconnectAll()
        {
            foreach (Peer peer in peerList)
            {
                peer.Disconnect();
            }
            peerList.Clear();
            ConnectionsChanged(this, EventArgs.Empty);
        }
    
        private void EchoTimer_Tick(object state)
        {
            Packet packet = new Packet();
            packet.Code = Code.PEER_PING;
            packet.Hop = 1;
            Send(packet);
        }
    }
}
