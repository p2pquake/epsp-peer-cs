namespace AvaloniaUIClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        public MainWindowViewModel()
        {
            _mediator = new(this);
            _activeViewModel = new InformationViewModel();
        }

        public MainWindowViewModel(InformationViewModel informationViewModel)
        {
            _mediator = new(this);
            InformationViewModel = informationViewModel;
            _activeViewModel = informationViewModel;
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
        public InformationViewModel InformationViewModel
        {
            get; private set;
        }

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
    }
}
