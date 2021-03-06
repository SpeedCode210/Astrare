using System.Runtime.InteropServices;
using System.IO;
using Astrare.Models;
using iText.Html2pdf;
using iText.Kernel.Pdf;

namespace Astrare;

public static class EventsDrawer
{
    public static void DrawToDocument(string title, string subtitle, Event[] events, string fileName, bool showIcons, double fontSize)
    {
        var html =
            "<html><head><style>@font-face { font-family: 'NimbusSanLReg'; src: url('{path}/Resources/NimbusSanL-Reg.ttf') format('truetype');  } body{color:#400000;font-family: NimbusSanLReg!important;font-size:"+fontSize+"px;}h1,h2{text-align:center;color:black;}div{display: inline;color:#001547}img {vertical-align: top;height: 0.8em;width: 0.8em}.logo {width: 168px;height: 56px;}</style></head><body><center><img src=\"{path}/Resources/astrareLogoDark.png\" class=\"logo\"></center><h1>" + title+"</h1><h2>"+subtitle+"</h2>";

        foreach (var ev in events)
        {
            string toAdd = "";
            toAdd += "<div>" + ev.starts_at.ToString("[dd/MM/yyyy HH:mm]") + "&nbsp;&nbsp;</div>";
            var text = ev.ToString();

            if (showIcons)
            {
                foreach (var planet in ev.objects)
                {
                    text = text.Replace(planet.ToString(), $"<img src=\"{{path}}/Resources/{planet.identifier.ToString().ToLower()}.png\"/> "+ planet);
                }

                switch (ev.EventType)
                {
                    case EventTypes.MOON_PHASE:
                        text = "<img src=\"{path}/Resources/MoonPhases/"+(ev.details["phase"] as MoonPhase).phase.ToString().ToLower().Replace('_','-')+".png\"/> " + text;
                        break;
                    case EventTypes.SEASON_CHANGE:
                        text = "<img src=\"{path}/Resources/earth.png\"/> " + text;
                        break;
                    case EventTypes.LUNAR_ECLIPSE:
                        text = "<img src=\"{path}/Resources/moon.png\"/> " + text;
                        break;
                }
            }
            
            toAdd += text + "<br>";
            
            html += toAdd;
        }

        html += "</body></html>";

        html = html.Replace("{path}", MainWindow.TmpPath);

        HtmlConverter.ConvertToPdf(html, new PdfWriter(fileName));
    }
    
    
    public static void DrawToTextFile(string title, string subtitle, Event[] events, string fileName)
    {
        var result = $" -- {title} -- \n {subtitle}\n";

        foreach (var ev in events)
        {
            string toAdd = "";
            toAdd += ev.starts_at.ToString("[dd/MM/yyyy HH:mm]");
            var text = ev.ToString();

            toAdd += text + "\n";
            
            result += toAdd;
        }

        
        File.WriteAllText(fileName, result);
    }
}