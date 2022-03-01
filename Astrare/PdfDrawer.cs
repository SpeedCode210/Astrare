using System.IO;
using Astrare.KosmorroConnection;
using Astrare.Models;
using Astrare.Translate;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Layout.Font;

namespace Astrare;

public static class PdfDrawer
{
    public static void DrawToDocument(GlobalData data, string fileName, int utc)
    {
        var html = File.ReadAllText(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Resources/template.html")
            .Replace("{text-1}", Language.Current.Translate("Overview of your sky"))
            .Replace("{text-2}", Language.Current.Translate(data.ephemerides[0].rise_time!.Value, Language.DateTranslateMode.Date))
            .Replace("{text-3}", Language.Current.Translate("Events :"))
            .Replace("{text-6}", Language.Current.Translate("Don't forget to check the weather forecast before you go out with your equipment."))
            .Replace("{text-7}", data.moon_phase.phase.ToString().ToLower().Replace("_","-"))
            .Replace("{text-8}", MoonPhase.TypeToString(data.moon_phase.phase))
            .Replace("{text-9}", Language.Current.Translate("Moon phase :"))
            .Replace("{text-5}", Language.Current.Translate("This document summarizes the ephemerides and the events of {0}."+
                                                            " It aims to help you to prepare your observation session. All the hours are given in {1}", 
                Language.Current.Translate(data.ephemerides[0].rise_time!.Value, Language.DateTranslateMode.Date), "UTC" + (utc >= 0 ? "+" : "-") + utc)
             + Language.Current.Translate(" - Latitude : " + Settings.Current.Latitude + "° / Longitude : " + Settings.Current.Longitude + ""))
            ;

        if (data.events.Length > 0)
        {
            if (data.events[0].objects.Length > 0 )
            {
                html = html.Replace("{text-10}",  data.events[0].objects[0].identifier.ToString().ToLower());
                html = html.Replace("<!--2", "").Replace("2-->", "");
            }
            
            if (data.events[0].objects.Length > 1 )
            {
                html = html.Replace("{text-11}",  data.events[0].objects[1].identifier.ToString().ToLower());
                html = html.Replace("<!--3", "").Replace("3-->", "");

            }

        }
        else
        {
            html = html.Replace("{text-10}", "empty").Replace("{text-11}", "empty");
        }
        
        string events = "";

        foreach (var ev in data.events)
        {
            events += ev.starts_at.ToString("[HH:mm]") + "&nbsp;";
            events += ev + "<br>";
        }

        if (events == "")
        {
            events = "<br>";
        }
        else
        {
            html = html.Replace("<!--1", "").Replace("1-->", "");
            html = html.Replace("<!--4", "").Replace("4-->", "");
        }

        html = html.Replace("{text-4}", events);


        GraphDrawer.SaveSvg(data.ephemerides, Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/ephs.svg");
        HtmlConverter.ConvertToPdf(html, new PdfWriter(fileName));
    }
}