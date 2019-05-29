using Reactive.Bindings;
using System;
namespace EPSPWPFClient.Mediator
{
    public class Show
    {
        public ReactiveProperty<bool> IsEarthquake = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> IsUserquake = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> IsTsunami = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> IsEEWTest = new ReactiveProperty<bool>();

        // {10=>1,20=>2,30=>3,40=>4,45=>"5弱",50=>"5強",55=>"6弱",60=>"6強",70=>7}
        public ReactiveProperty<int> EarthquakeSeismicScale = new ReactiveProperty<int>();

        public ReactiveProperty<int> UserquakeReliability = new ReactiveProperty<int>();

        public bool HasSound;
        public bool IsTopMost;
        public bool HasNotFocused;
        public bool IsAnotherWindow;
    }

    public class ClientConfiguration
    {
        public Show Show { get; private set; } = new Show();

        // Singleton
        public static ClientConfiguration Instance = new ClientConfiguration();

        private ClientConfiguration()
        {
        }
    }
}
