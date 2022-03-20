using System;
using System.IO;
using System.Runtime.InteropServices;
using Astrare.KosmorroConnection;
using Astrare.Translate;
using iText.Html2pdf;
using iText.Kernel.Pdf;

namespace Astrare;

public static class SkychartPdfDrawer
{
    public static void DrawToDocument(GlobalData data, string fileName, int utc, TimeSpan time)
    {
        var html = File.ReadAllText(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                                    "/Resources/skychart_template.html")
            .Replace("{text-1}", Language.Current.Translate("Skychart"))
            .Replace("{text-2}",
                Language.Current.Translate(data.ephemerides[0].rise_time!.Value, Language.DateTranslateMode.Date))
            .Replace("{text-6}",
                Language.Current.Translate(
                    "Don't forget to check the weather forecast before you go out with your equipment."))
            .Replace("{text-5}", Language.Current.Translate(
                "This document summarizes stars, constellations and other asters that are visible in the sky "
                + "on {0} at {1}, at latitude {2} and longitude {3}" +
                ". t aims to help you to prepare your observation session.",
                Language.Current.Translate(data.ephemerides[0].rise_time!.Value, Language.DateTranslateMode.Date),
                time.Hours.ToString("00") + ':' + time.Minutes.ToString("00") + " UTC" + (utc >= 0 ? "+" : "-") + utc
                , Settings.Current.Latitude + "°", Settings.Current.Longitude+ "°"))
            ;

        
        var props = new ConverterProperties();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            props.SetBaseUri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

        HtmlConverter.ConvertToPdf(html, new PdfWriter(fileName), props);

    }
}
