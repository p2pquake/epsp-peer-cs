using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfClient
{
    public class LicenseViewModel
    {
        public ObservableCollection<LicenseModel> Licenses { get; init; }

        public LicenseViewModel()
        {
            var licenseItems = License.ResourceManager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true).OfType<DictionaryEntry>().Where(e => e.Value is string).OrderBy(e => e.Key).Select(e => new LicenseModel() { Name = (string)e.Key, Text = (string)e.Value });
            Licenses = new ObservableCollection<LicenseModel>(licenseItems);
        }
    }

    public class LicenseModel
    {
        public string Name { get; init; }
        public string Text { get; init; }
    }
}
