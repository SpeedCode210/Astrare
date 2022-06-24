using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace Astrare;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void ReloadTheme()
    {
        IStyle[] themes = new[]
        {
            new FluentTheme(new Uri("resm:Styles?assembly=Astrare"))
            {
                Mode = FluentThemeMode.Light
            },
            new FluentTheme(new Uri("resm:Styles?assembly=Astrare"))
            {
                Mode = FluentThemeMode.Dark
            }
        };

        Styles[0] = themes[Settings.Current.Theme];

        Styles[1] = new StyleInclude(new Uri("resm:Styles?assembly=Astrare"))
        {
            Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
        };
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ReloadTheme();
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}