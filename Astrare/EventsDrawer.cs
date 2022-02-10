using System;
using Astrare.Models;
using iText.Html2pdf;
using iText.Kernel.Pdf;

namespace Astrare;

public static class EventsDrawer
{
    public static void DrawToDocument(string title, string subtitle, Event[] events, string fileName)
    {
        var html =
            "<html><head><style>body{color:#400000}h1,h2{text-align:center;color:black;}div{display: inline;color:#001547}img {vertical-align: top;height: 0.8em;width: 0.8em}.logo {width: 168px;height: 56px;}</style></head><body><center><img src=\"Resources/astrareLogoDark.png\" class=\"logo\"></center><h1>" + title+"</h1><h2>"+subtitle+"</h2>";

        foreach (var ev in events)
        {
            string toAdd = "";
            toAdd += "<div>" + ev.starts_at.ToString("[dd/MM/yyyy HH:mm]") + "&nbsp;&nbsp;</div>";
            var text = ev.ToString();
            foreach (var planet in ev.objects)
            {
                text = text.Replace(planet.ToString(), $"<img src=\"Resources/{planet.identifier.ToString().ToLower()}.png\"/> "+ planet);
            }

            if (ev.EventType is EventTypes.MOON_PHASE)
                text = "<img src=\"Resources/moon.png\"/> " + text;
            toAdd += text + "<br>";
            
            html += toAdd;
        }

        html += "</body></html>";
        
        ConverterProperties properties = new ConverterProperties();
        properties.SetBaseUri(AppDomain.CurrentDomain.BaseDirectory);
        HtmlConverter.ConvertToPdf(html, new PdfWriter(fileName));
    }
}