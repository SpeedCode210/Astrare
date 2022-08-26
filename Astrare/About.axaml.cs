using System;
using System.IO;
using System.Runtime.InteropServices;
using Astrare.Translate;
using Avalonia;
using Avalonia.Controls;
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
        Title = "Astrare - " + Language.Current.Translate("About");
        
        if (Settings.Current.Theme is 0)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            Logo.Source = new Bitmap(assets!.Open(new Uri("avares://Astrare/astrareLogoAlt.png")));
        }

        var cmd = "cd {/d} " +
                  Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                  "/kosmorro-lite && " + PythonHelper.GetPythonCommand() +
                  " kosmorro-lite --version";
        
        cmd = cmd.Replace("{/d}", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/d" : "");

        ExecutingVersion.Text = "Using " + ShellHelper.Bash(cmd);
        
    }

}