using System;
namespace EPSPWPFClient.Mediator
{
    public struct Show
    {
        public bool IsEarthquake;
        public bool IsUserquake;
        public bool IsTsunami;
        public bool IsEEWTest;

        // {10=>1,20=>2,30=>3,40=>4,45=>"5弱",50=>"5強",55=>"6弱",60=>"6強",70=>7}
        public int EarthquakeSeismicScale;

        public int UserquakeReliability;

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
