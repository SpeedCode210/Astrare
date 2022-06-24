using System;
using Astrare.Translate;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Astrare;

public partial class About : Window
{
    public About()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        this.Title = "Astrare - " + Language.Current.Translate("About");
        
        if (Settings.Current.Theme is 0)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            Logo.Source = new Bitmap(assets.Open(new Uri("avares://Astrare/astrareLogoAlt.png")));
        }
        
        ExecutingVersion.Text = "Using " + ShellHelper.Bash("cd kosmorro-lite && " + PythonHelper.GetPythonCommand() + " kosmorro-lite --version");
        
    }

}