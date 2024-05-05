﻿using ModernWpf.Controls;

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Navigation;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event EventHandler OnExit = (s, e) => { };
        public event EventHandler OnUserquake = (s, e) => { };
        private Configuration configuration;
        private NotifyIcon icon;
        private ContextMenuStrip menu;

        public MainWindow()
        {
            InitializeComponent();
            InitializeNotifyIcon();

            configuration = ConfigurationManager.Configuration;
        }

        private void ShowWindow()
        {
            Show();
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            Activate();
        }

        private void SendUserquake()
        {
            // FIXME: 送信時のフィードバックをどうするか考えてない
            OnUserquake(this, EventArgs.Empty);
        }

        // ---------------------------------------------------------------------
        // ウィンドウ上の処理
        // ---------------------------------------------------------------------
        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var viewModel = (RootViewModel)DataContext;
            var item = (NavigationViewItem)args.SelectedItem;

            viewModel.BindingDataContext = item.Tag switch
            {
                "Information" => viewModel.InformationViewModel,
                "Setting" => viewModel.SettingViewModel,
                "Status" => viewModel.StatusViewModel,
                _ => throw new NotImplementedException(),
            };
            viewModel.PageFileName = $"Pages/{item.Tag}.xaml";
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            var frame = (System.Windows.Controls.Frame)sender;
            while (frame.CanGoBack)
            {
                frame.RemoveBackEntry();
            }
            ((FrameworkElement)e.Content).DataContext = ((RootViewModel)DataContext).BindingDataContext;
        }

        private void UpdateHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("アップデートしますか？", Title, System.Windows.MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) { return; }

            Task.Run(() =>
            {
                Updater.Run();
            });
        }

        private void ShakeButton_Click(object sender, RoutedEventArgs e)
        {
            SendUserquake();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        // ---------------------------------------------------------------------
        // トレイアイコンの処理
        // ---------------------------------------------------------------------
        public void HideNotifyIcon()
        {
            icon.Visible = false;
        }

        private void InitializeNotifyIcon()
        {
            icon = new NotifyIcon()
            {
                Icon = Resource.p2pquake,
                Text = "P2P地震情報 Beta3.5",
                Visible = true,
            };
            icon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;

            menu = new ContextMenuStrip();
            menu.Items.Add("表示", null, MenuShowItem_Click);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("「揺れた！」発信", null, MenuShakeItem_Click);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("終了", null, MenuExitItem_Click);

            icon.ContextMenuStrip = menu;
        }

        private void MenuExitItem_Click(object sender, EventArgs e)
        {
            OnExit(this, EventArgs.Empty);
        }

        private void MenuShakeItem_Click(object sender, EventArgs e)
        {
            SendUserquake();
        }

        private void MenuShowItem_Click(object sender, EventArgs e)
        {
            ShowWindow();
        }

        private void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            /* Note: MouseButtons
             *  Left は Primary, Right は Secondary の意味
             *  （主副を入れ替えると Left は左ではなくなる）
             */
            if (e.Button == MouseButtons.Left)
            {
                ShowWindow();
            }
            if (e.Button == MouseButtons.Right && configuration.SendIfRightDoubleClick)
            {
                SendUserquake();
            }
            if (e.Button == MouseButtons.Middle && configuration.SendIfMiddleDoubleClick)
            {
                SendUserquake();
            }
        }

        private void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.New) { e.Cancel = true; }
        }
    }
}
