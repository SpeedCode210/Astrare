using Avalonia;
using Avalonia.Controls;

namespace Astrare.Translate.Controls;

public class DataGridTextColumn : Avalonia.Controls.DataGridTextColumn
{

    public DataGridTextColumn()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private int _changeId;
    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != "Header")
            return;
        _changeId++;
        if (_changeId % 2 == 0)
            return;
        if(this.Header is string)
            this.Header = Language.Current.Translate((string)this.Header);
        else if (this.Header is TextBlock)
            ((TextBlock) this.Header).Text = Language.Current.Translate(((TextBlock) this.Header).Text);
    }
}