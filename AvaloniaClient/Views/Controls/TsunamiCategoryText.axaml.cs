using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaClient.Views.Controls;

public partial class TsunamiCategoryText : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<TsunamiCategoryText, string>(nameof(Text), "");

    public static readonly StyledProperty<string> BackgroundColorProperty =
        AvaloniaProperty.Register<TsunamiCategoryText, string>(nameof(BackgroundColor), "White");

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string BackgroundColor
    {
        get => GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public TsunamiCategoryText()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextProperty || change.Property == BackgroundColorProperty)
        {
            UpdateVisuals();
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        var text = Text;
        ShadowText.Text = text;
        Outline1.Text = text; Outline2.Text = text; Outline3.Text = text; Outline4.Text = text;
        Outline5.Text = text; Outline6.Text = text; Outline7.Text = text; Outline8.Text = text;
        MainText.Text = text;

        if (Color.TryParse(BackgroundColor, out var color))
        {
            RootGrid.Background = new SolidColorBrush(color);
        }
    }
}
