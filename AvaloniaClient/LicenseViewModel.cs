using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace AvaloniaClient;

public class LicenseViewModel
{
    public ObservableCollection<LicenseModel> Licenses { get; init; }

    public LicenseViewModel()
    {
        var resourceSet = License.ResourceManager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
        var licenseItems = (resourceSet?.OfType<DictionaryEntry>() ?? []).Where(e => e.Value is string).OrderBy(e => e.Key).Select(e => new LicenseModel() { Name = (string)e.Key!, Text = (string)e.Value! });
        Licenses = new ObservableCollection<LicenseModel>(licenseItems);
    }
}

public class LicenseModel
{
    public required string Name { get; init; }
    public required string Text { get; init; }
}
