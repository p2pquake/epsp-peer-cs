using System;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaUIClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        public MainWindowViewModel()
        {
            _mediator = new(this);
            InformationViewModel = new InformationViewModel();
            ConfigurationViewModel = new ConfigurationViewModel();
            _activeViewModel = InformationViewModel;
            
            // 「揺れた！」コマンドの初期化
            SendUserquakeCommand = new RelayCommand(SendUserquake, CanSendUserquake);
        }

        public MainWindowViewModel(InformationViewModel informationViewModel)
        {
            _mediator = new(this);
            InformationViewModel = informationViewModel;
            ConfigurationViewModel = new ConfigurationViewModel();
            _activeViewModel = informationViewModel;
            
            // 「揺れた！」コマンドの初期化
            SendUserquakeCommand = new RelayCommand(SendUserquake, CanSendUserquake);
        }

        private readonly Mediator.Mediator _mediator;
        public Mediator.Mediator Mediator
        {
            get { return _mediator; }
        }

        private ViewModelBase _activeViewModel;
        public ViewModelBase ActiveViewModel
        {
            get { return _activeViewModel; }
            set
            {
                _activeViewModel = value;
                OnPropertyChanged(nameof(ActiveViewModel));
            }
        }

        // -----------------------------------------------------------
        // 各ビューの表示
        // -----------------------------------------------------------
        public InformationViewModel InformationViewModel { get; private set; }
        public ConfigurationViewModel ConfigurationViewModel { get; private set; }

        // -----------------------------------------------------------
        // トップレベルの表示
        // -----------------------------------------------------------
        private string _status = "未接続";
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        private string _portStatus = "ポート: -";
        public string PortStatus
        {
            get { return _portStatus; }
            set
            {
                _portStatus = value;
                OnPropertyChanged(nameof(PortStatus));
            }
        }

        // -----------------------------------------------------------
        // 「揺れた！」機能
        // -----------------------------------------------------------
        public RelayCommand SendUserquakeCommand { get; }

        private void SendUserquake()
        {
            try
            {
                bool success = _mediator.MediatorContext.SendUserquake();
                if (success)
                {
                    // 成功時のフィードバックを表示
                    OnUserquakeSent?.Invoke("地震感知情報を発信しました。");
                }
                else
                {
                    // 失敗時のフィードバックを表示
                    OnUserquakeSent?.Invoke("地震感知情報の発信に失敗しました。\n接続状態や設定を確認してください。");
                }
            }
            catch (Exception ex)
            {
                OnUserquakeSent?.Invoke($"地震感知情報の発信でエラーが発生しました。\n{ex.Message}");
            }
        }

        private bool CanSendUserquake()
        {
            // 接続状態と設定に基づいて送信可能かどうかを判定
            var mediatorContext = _mediator.MediatorContext;
            
            // 基本的な接続状態チェック
            if (mediatorContext.ReadonlyState.GetType().Name != "ConnectedState")
                return false;
                
            // 地域設定チェック  
            var config = ConfigurationManager.LoadConfiguration();
            if (config.AreaCode == 900) // 地域未設定
                return false;
                
            return true;
        }

        /// <summary>
        /// 「揺れた！」送信結果の通知イベント
        /// </summary>
        public event Action<string>? OnUserquakeSent;
    }
}
