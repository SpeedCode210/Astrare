using System;
using Avalonia;
using Avalonia.Styling;

namespace Astrare.Translate.Controls;

public class MenuItem : Avalonia.Controls.MenuItem, IStyleable
{
    
    Type IStyleable.StyleKey => typeof(Avalonia.Controls.MenuItem);
    
    public MenuItem()
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
        this.Header = Language.Current.Translate((string)this.Header);
    }
}