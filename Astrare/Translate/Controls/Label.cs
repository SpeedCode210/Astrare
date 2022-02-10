using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace Astrare.Translate.Controls;

public class Label : Avalonia.Controls.Label, IStyleable
{
    
    Type IStyleable.StyleKey => typeof(Avalonia.Controls.Label);
    
    public Label()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private int _changeId;
    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != "Content")
            return;
        _changeId++;
        if (_changeId % 2 == 0)
            return;
        if(this.Content is string)
            this.Content = Language.Current.Translate((string)this.Content);
        else if (this.Content is TextBlock)
            ((TextBlock) this.Content).Text = Language.Current.Translate(((TextBlock) this.Content).Text);
    }
}