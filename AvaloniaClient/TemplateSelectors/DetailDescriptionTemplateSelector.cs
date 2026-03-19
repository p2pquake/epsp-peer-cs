using System.Collections.Generic;

using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

using AvaloniaClient.EPSPDataView;

namespace AvaloniaClient.TemplateSelectors;

public class DetailDescriptionTemplateSelector : IDataTemplate
{
    [Content]
    public Dictionary<string, IDataTemplate> Templates { get; } = new();

    public Control? Build(object? param)
    {
        var key = GetKey(param);
        if (key != null && Templates.TryGetValue(key, out var template))
            return template.Build(param);
        return null;
    }

    public bool Match(object? data) => data is DetailItemView;

    private static string? GetKey(object? item)
    {
        if (item is not DetailItemView view) return null;
        return view.TextStyle switch
        {
            TextStyles.Title => "DetailHeaderItem",
            TextStyles.Name => "DetailNameItem",
            TextStyles.Prefecture => "DetailPrefectureItem",
            TextStyles.Scale => "DetailScaleItem",
            TextStyles.Eruption => "DetailEruptionItem",
            TextStyles.FreeFormComment => "DetailFreeFormCommentItem",
            TextStyles.Section => "DetailSectionItem",
            TextStyles.MajorWarning => "DetailMajorWarningItem",
            TextStyles.Warning => "DetailWarningItem",
            TextStyles.Advisory => "DetailAdvisoryItem",
            _ => null,
        };
    }
}
