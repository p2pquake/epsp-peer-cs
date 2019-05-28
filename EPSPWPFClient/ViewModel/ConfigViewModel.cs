using EPSPWPFClient.Mediator;
using Reactive.Bindings;
using System;
namespace EPSPWPFClient.ViewModel
{
    public class ConfigViewModel
    {
        private ClientConfiguration configuration;

        public ReactiveProperty<bool> IsEarthquake { get; private set; }

        public ConfigViewModel()
        {
            configuration = ClientConfiguration.Instance;

            IsEarthquake = configuration.Show.IsEarthquake;
        }
    }
}
