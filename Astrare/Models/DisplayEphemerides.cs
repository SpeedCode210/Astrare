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
    public string RiseTime { get; }
    public string CulminationTime { get; }
    public string SetTime { get; }
    
    public DisplayEphemerides(AsterEphemerides a)
    {
        RiseTime = a.rise_time != null ? a.rise_time.Value.ToString("HH:mm") : "-";
        CulminationTime = a.culmination_time != null ? ((DateTime)a.culmination_time).ToString("HH:mm") : "-";
        SetTime = a.set_time != null ? a.set_time.Value.ToString("HH:mm") : "-";
        Name = Language.Current.Translate(a.aster.identifier.ToString().ToLower().FirstCharToUpper());
        IconPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + $"/Resources/{a.aster.identifier.ToString().ToLower()}.png";
        Icon = new Bitmap(IconPath);
    }
}