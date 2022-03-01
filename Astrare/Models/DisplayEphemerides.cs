using System;
using Astrare.Translate;
using Avalonia.Media.Imaging;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global


namespace Astrare.Models;

public class DisplayEphemerides
{
    
    
    public Bitmap Icon { get;}
    public string IconPath { get;}
    public string Name { get;}
    public string rise_time { get; }
    public string culmination_time { get; }
    public string set_time { get; }
    
    public DisplayEphemerides(AsterEphemerides a)
    {
        rise_time = a.rise_time != null ? a.rise_time.Value.ToString("HH:mm") : "-";
        culmination_time = a.culmination_time != null ? ((DateTime)a.culmination_time).ToString("HH:mm") : "-";
        set_time = a.set_time != null ? a.set_time.Value.ToString("HH:mm") : "-";
        Name = Language.Current.Translate(a.aster.identifier.ToString().ToLower().FirstCharToUpper());
        IconPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + $"/Resources/{a.aster.identifier.ToString().ToLower()}.png";
        Icon = new Bitmap(IconPath);
    }
}