using EPSPWPFClient.Mediator;
using Reactive.Bindings;
using System;
using System.Collections.Generic;

namespace EPSPWPFClient.ViewModel
{
    public class ConfigViewModel
    {
        private ClientConfiguration configuration;

        public ReactiveProperty<bool> IsEarthquake { get; private set; }
        public ReactiveProperty<bool> IsUserquake { get; private set; }
        public ReactiveProperty<bool> IsTsunami { get; private set; }
        public ReactiveProperty<bool> IsEEWTest { get; private set; }

        public ReactiveProperty<int> EarthquakeSeismicScale { get; private set; }
        public ReactiveProperty<int> UserquakeReliability { get; private set; }

        public ConfigViewModel()
        {
            configuration = ClientConfiguration.Instance;

            IsEarthquake = configuration.Show.IsEarthquake;
            IsUserquake = configuration.Show.IsUserquake;
            IsTsunami = configuration.Show.IsTsunami;
            IsEEWTest = configuration.Show.IsEEWTest;

            EarthquakeSeismicScale = ReactiveProperty.FromObject(configuration.Show.EarthquakeSeismicScale,
                x => x.Value,
                convert: x => Scale2Slider(x),
                convertBack: x => Slider2Scale(x));
            UserquakeReliability = configuration.Show.UserquakeReliability;
        }

        private int Scale2Slider(int scale)
        {
            IDictionary<int, int> scale2Slider = new Dictionary<int, int>
            {
                { 10, 1 }, { 20, 2 }, { 30, 3 }, { 40, 4 },
                { 45, 5 }, { 50, 6 }, { 55, 7 }, { 60, 8 }, { 70, 9 }
            };

            if (scale2Slider.ContainsKey(scale))
            {
                return scale2Slider[scale];
            }
            return 1;
        }

        private int Slider2Scale(int slider)
        {
            IDictionary<int, int> slider2Scale = new Dictionary<int, int>
            {
                { 1, 10 }, { 2, 20 }, { 3, 30 }, { 4, 40 },
                { 5, 45 }, { 6, 50 }, { 7, 55 }, { 8, 60 }, { 9, 70 }
            };

            if (slider2Scale.ContainsKey(slider))
            {
                return slider2Scale[slider];
            }
            return 10;
        }
    }
}
