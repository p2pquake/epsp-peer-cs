﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WpfClient
{
    public class StatusViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string numberOfPeers = "- ピア";
        public string NumberOfPeers
        {
            get { return numberOfPeers; }
            set
            {
                numberOfPeers = value;
                OnPropertyChanged();
            }
        }

        private string connections = "- ピア";
        public string Connections
        {
            get { return connections; }
            set
            {
                connections = value;
                OnPropertyChanged();
            }
        }

        private string portStatus = "-";
        public string PortStatus
        {
            get { return portStatus; }
            set
            {
                portStatus = value;
                OnPropertyChanged();
            }
        }

        private string keyStatus = "-";
        public string KeyStatus
        {
            get { return keyStatus; }
            set
            {
                keyStatus = value;
                OnPropertyChanged();
            }    
        }

        private string statusTitle = "接続されていません。";
        public string StatusTitle
        {
            get { return statusTitle; }
            set
            {
                statusTitle = value;
                OnPropertyChanged();
            }
        }

        private string statusDescription = "P2P地震情報を起動しました。自動的にネットワークへの接続を開始します。";
        public string StatusDescription
        {
            get { return statusDescription; }
            set
            {
                statusDescription = value;
                OnPropertyChanged();
            }
        }

        public string Version
        {
            get
            {
                var info = FileVersionInfo.GetVersionInfo(Environment.ProcessPath);
                return $"P2P地震情報 Beta{info.ProductMinorPart/10.0:0.0}(Rev{info.ProductBuildPart:00})";
            }
        }

        private string areapeerText;
        public string AreapeerText
        {
            get { return areapeerText; }
            set
            {
                areapeerText = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage bitmapImage;
        public BitmapImage BitmapImage
        {
            get { return bitmapImage; }
            set
            {
                bitmapImage = value;
                OnPropertyChanged();
            }
        }

        // See: https://docs.microsoft.com/ja-jp/dotnet/desktop/wpf/data/how-to-implement-property-change-notification
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
