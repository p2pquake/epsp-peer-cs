using ModernWpf.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            ((FrameworkElement)e.Content).DataContext = ((RootViewModel)DataContext).BindingDataContext;
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

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
                Hide();
            }
        }

        // ---------------------------------------------------------------------
        // トレイアイコンの処理
        // ---------------------------------------------------------------------
        private void InitializeNotifyIcon()
        {
            icon = new NotifyIcon()
            {
                Icon = Resource.p2pquake,
                Text = "P2P地震情報 (Beta4)",
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
            // TODO: Windows 終了時などと共通化する
            throw new NotImplementedException();
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
    }
}
