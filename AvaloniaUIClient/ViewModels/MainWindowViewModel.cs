using System.Collections.Generic;

namespace AvaloniaUIClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            _activeViewModel = new InformationViewModel();
        }

        public MainWindowViewModel(List<ViewModelBase> viewModels)
        {
            _activeViewModel = viewModels[0];
        }

        private ViewModelBase _activeViewModel;
        public ViewModelBase ActiveViewModel
        {
            get { return _activeViewModel; }
            set
            {
                _activeViewModel = value;
                OnPropertyChanged("ActiveViewModel");
            }
        }
    }
}
