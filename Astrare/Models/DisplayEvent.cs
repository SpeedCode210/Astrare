using Astrare.Translate;
using Avalonia.Media.Imaging;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global


namespace Astrare.Models;

public class DisplayEvent
{
    
    
    public Bitmap Icon { get;}
    public Bitmap Icon2 { get;}
    public string IconPath { get;}
    public string IconPath2 { get;}
    public string Name { get;}
    public string Description { get; }

    public string Start { get; } = "-";

    public string End { get; } = "-";
    
    public DisplayEvent(Event e)
    {
        Description = e.ToString();
        Name = Language.Current.Translate(e.EventType.ToString().ToLower().Replace("_"," ").FirstCharToUpper());
        Start = e.starts_at.ToString("HH:mm");
        if (e.ends_at != null)
            End = e.ends_at!.Value.ToString("HH:mm");
        if (e.objects.Length < 1)
            return;
        IconPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + $"/Resources/{e.objects[0].identifier.ToString().ToLower()}.png";
        Icon = new Bitmap(IconPath);
        
        if (e.objects.Length < 2)
            return;
        IconPath2 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + $"/Resources/{e.objects[1].identifier.ToString().ToLower()}.png";
        Icon2 = new Bitmap(IconPath2);
    }
}