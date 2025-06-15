using AvaloniaUIClient.ViewModels.Information;

using Client.App.Userquake;
using Client.Peer;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AvaloniaUIClient.ViewModels
{
    public class DesignInformationViewModel : InformationViewModel
    {
        private ObservableCollection<EventArgs> _histories = new()
        {
            new EPSPQuakeEventArgs(){
                InformationType = QuakeInformationType.ScalePrompt,
                Scale = "4",
                OccuredTime = "1日2時3分",
            },
            new EPSPQuakeEventArgs(){
                InformationType = QuakeInformationType.Detail,
                Scale = "4",
                Destination = "千葉県北西部",
                OccuredTime = "1日2時3分",
            },
            new UserquakeEvaluateEventArgs(){
                StartedAt = DateTime.Now.AddSeconds(-30),
                UpdatedAt = DateTime.Now,
                Count = 10,
                Confidence = 0.98052,
                AreaConfidences = new Dictionary<string, IUserquakeEvaluationArea>() {
                    { "250", new UserquakeEvaluationArea(){ AreaCode = "250", Confidence = 0.9, Count = 8 } },
                    { "241", new UserquakeEvaluationArea(){ AreaCode = "241", Confidence = 0.4, Count = 2 } },
                },
            },
        };
        public new ObservableCollection<EventArgs> Histories
        {
            get { return _histories; }
        }

        private ViewModelBase? _activeViewModel = new EmptyViewModel();
        public new ViewModelBase? ActiveViewModel
        {
            get { return _activeViewModel; }
            //set
            //{
            //    _activeViewModel = value;
            //    OnPropertyChanged(nameof(ActiveViewModel));
            //}
        }
    }
}
