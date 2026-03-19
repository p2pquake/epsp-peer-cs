namespace AvaloniaClient.EPSPDataView;

public enum TextStyles
{
    Title,
    Name,
    Prefecture,
    Scale,
    Eruption,
    FreeFormComment,
    Section,
    MajorWarning,
    Warning,
    Advisory,
}

public class DetailItemView
{
    public string Text { get; init; }
    public TextStyles TextStyle { get; init; }
    public string ScaleIconPath { get; init; }

    public DetailItemView(string text, TextStyles style, int scale = -1)
    {
        Text = text;
        TextStyle = style;
        ScaleIconPath = $"avares://P2PQuake/Resources/Scales/scale_{scale}.png";
    }
}
