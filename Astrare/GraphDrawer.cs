using System;
using System.Collections.Generic;
using VectSharp;
using System.IO;
using System.Linq;
using Astrare.Models;
using Avalonia.Controls;
using ImageMagick;
using VectSharp.Canvas;
using VectSharp.SVG;
using Size = VectSharp.Size;

namespace Astrare;

public static class GraphDrawer
{
    private static readonly FontFamily NimbusBold = new(File.OpenRead(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"/Resources/NimbusSanL-Bol.ttf"));

    private static readonly FontFamily NimbusRegular =
        new(File.OpenRead(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"/Resources/NimbusSanL-Reg.ttf"));

    public static Canvas GetCanvas(AsterEphemerides[] eph)
    {
        eph = eph.CloneElements();
        //Création du document
        Document doc = new Document();
        doc.Pages.Add(new Page(600, 1000));
        Graphics gpr = doc.Pages.Last().Graphics;
        for (int i = 0; i < eph.Length; i++)
        {
            var visible = eph[i].IsVisible(eph[0]);
            var writeSetHour = true;
            var writeRiseHour = true;
            //Mettre des temps fictifs si nécessaire
            if (eph[i].rise_time is null)
            {
                eph[i].rise_time = new DateTime(2000, 1, 1, 0, 0, 0);
                writeRiseHour = false;
            }

            if (eph[i].set_time is null)
            {
                eph[i].set_time = new DateTime(2000, 1, 1, 23, 59, 59);
                writeSetHour = false;
            }

            var display = new DisplayEphemerides(eph[i]);

            //Affichage de l'icone de la planète
            using (var image = new MagickImage(display.IconPath))
            {
                List<byte> arr = new();
                foreach (var b in image.GetPixels())
                {
                    foreach (var c in b.ToArray())
                    {
                        arr.Add(c);
                    }
                }

                gpr.DrawRasterImage(75, 20 + (i * 100), 50.0, 50.0,
                    new RasterImage(arr.ToArray(), image.Width, image.Height, PixelFormats.RGBA, true));
            }

            //Ecriture du nom de la planète
            gpr.FillText(new Point(150, 30 + (i * 100)), display.Name, new Font(NimbusBold, 15),
                new SolidColourBrush(Colours.White), TextBaselines.Middle);
            

            var riseTimeInt = eph[i].rise_time!.Value.Hour * 60 + eph[i].rise_time!.Value.Minute;
            var setTimeInt = eph[i].set_time!.Value.Hour * 60 + eph[i].set_time!.Value.Minute;
            var culminationTimeInt = eph[i].culmination_time != null
                ? eph[i].culmination_time!.Value.Hour * 60 + eph[i].culmination_time!.Value.Minute
                : 0;

            var minTime =
                (riseTimeInt > setTimeInt ? eph[i].set_time!.Value : eph[i].rise_time!.Value).ToString("HH:mm");
            var maxTime =
                (riseTimeInt < setTimeInt ? eph[i].set_time!.Value : eph[i].rise_time!.Value).ToString("HH:mm");
            var middleTime = eph[i].culmination_time != null ? eph[i].culmination_time!.Value.ToString("HH:mm") : "";

            var yellowBrush = new LinearGradientBrush(new Point(0, 0), new Point(350, 5),
                new GradientStop[] {new(Colour.FromRgb(232, 166, 20), 0), new(Colour.FromRgb(245, 255, 0), 1)});
            var grayBrush = new SolidColourBrush(Colours.Gray);
            
            //ecriture de l'heure de culmination si disponible
            if (middleTime != "")
                gpr.FillText(
                    new Point(
                        150 + culminationTimeInt * 350 / 1440 -
                        gpr.MeasureText(middleTime, new Font(NimbusRegular, 13.0)).Width / 2, 65 + (i * 100)),
                    middleTime, new Font(NimbusRegular, 13), new SolidColourBrush(Colours.White), TextBaselines.Middle);
            
            
            //Dessin de la ligne d'arrière plan
            gpr.StrokeRectangle(new Point(150, 45 + (i * 100)), new Size(350, 0),
                grayBrush, 4D, LineCaps.Round, LineJoins.Round);
            
            if(!visible)
            {
                //Si disponible, repérer le point de culmination
                if (middleTime != "")
                    gpr.FillRectangle(new Point(149 + culminationTimeInt * 350 / 1440, 40 + i * 100), new Size(2, 10),
                        new SolidColourBrush(Colours.Brown));
                continue;
            }


            //Dessin du remplissage
            if (riseTimeInt < setTimeInt)
                gpr.StrokeRectangle(new Point(150 + Math.Min(riseTimeInt, setTimeInt) * 350 / 1440, 45 + i * 100),
                    new Size(Math.Abs(riseTimeInt - setTimeInt) * 350.0 / 1440.0, 0),
                    yellowBrush, 10D, LineCaps.Round, LineJoins.Round);
            else
            {
                gpr.StrokeRectangle(new Point(150, 45 + i * 100),
                    new Size(Math.Min(riseTimeInt, setTimeInt) * 350.0 / 1440, 0),
                    yellowBrush, 10D, LineCaps.Round, LineJoins.Round);
                gpr.StrokeRectangle(new Point(150 + Math.Max(riseTimeInt, setTimeInt) * 350 / 1440, 45 + i * 100),
                    new Size(350 - Math.Max(riseTimeInt, setTimeInt) * 350 / 1440, 0),
                    yellowBrush, 10D, LineCaps.Round, LineJoins.Round);
            }
            

            //Ecriture de la borne inférieure
            if (writeRiseHour)
                gpr.FillText(
                    new Point(
                        150 + Math.Min(riseTimeInt, setTimeInt) * 350 / 1440 -
                        gpr.MeasureText(minTime, new Font(NimbusRegular, 13.0)).Width / 2, 65 + (i * 100)),
                    minTime, new Font(NimbusRegular, 13), new SolidColourBrush(Colours.White), TextBaselines.Middle);

            //Ecriture de la borne supérieure
            if (writeSetHour)
                gpr.FillText(
                    new Point(
                        150 + Math.Max(riseTimeInt, setTimeInt) * 350 / 1440 -
                        gpr.MeasureText(maxTime, new Font(NimbusRegular, 13.0)).Width / 2, 65 + (i * 100)),
                    maxTime, new Font(NimbusRegular, 13), new SolidColourBrush(Colours.White), TextBaselines.Middle);


            //Si disponible, repérer le point de culmination
            if (middleTime != "")
                gpr.FillRectangle(new Point(149 + culminationTimeInt * 350 / 1440, 40 + i * 100), new Size(2, 10),
                    new SolidColourBrush(Colours.Brown));

        }

        Canvas result = doc.Pages.Last().PaintToCanvas();
        return result;
    }

    public static void SaveSvg(AsterEphemerides[] eph, string fileName)
    {
        eph = eph.CloneElements();
        //Création du document
        Document doc = new Document();
        doc.Pages.Add(new Page(800, 800));
        doc.Pages.Last().Background = Colours.White;
        Graphics gpr = doc.Pages.Last().Graphics;
        for (int i = 0; i < eph.Length - 1; i++)
        {
            var visible = eph[i].IsVisible(eph[0]);
            var writeSetHour = true;
            var writeRiseHour = true;
            //Mettre des temps fictifs si nécessaire
            if (eph[i].rise_time is null)
            {
                eph[i].rise_time = new DateTime(2000, 1, 1, 0, 0, 0);
                writeRiseHour = false;
            }

            if (eph[i].set_time is null)
            {
                eph[i].set_time = new DateTime(2000, 1, 1, 23, 59, 59);
                writeSetHour = false;
            }

            var display = new DisplayEphemerides(eph[i]);

            //Affichage de l'icone de la planète
            using (var image = new MagickImage(display.IconPath))
            {
                List<byte> arr = new();
                foreach (var b in image.GetPixels())
                {
                    foreach (var c in b.ToArray())
                    {
                        arr.Add(c);
                    }
                }

                gpr.DrawRasterImage(0, 20 + (i * 100), 50.0, 50.0,
                    new RasterImage(arr.ToArray(), image.Width, image.Height, PixelFormats.RGBA, true));
            }


            var riseTimeInt = eph[i].rise_time!.Value.Hour * 60 + eph[i].rise_time!.Value.Minute;
            var setTimeInt = eph[i].set_time!.Value.Hour * 60 + eph[i].set_time!.Value.Minute;
            var culminationTimeInt = eph[i].culmination_time != null
                ? eph[i].culmination_time!.Value.Hour * 60 + eph[i].culmination_time!.Value.Minute
                : 0;

            var minTime =
                (riseTimeInt > setTimeInt ? eph[i].set_time!.Value : eph[i].rise_time!.Value).ToString("HH:mm");
            var maxTime =
                (riseTimeInt < setTimeInt ? eph[i].set_time!.Value : eph[i].rise_time!.Value).ToString("HH:mm");
            var middleTime = eph[i].culmination_time != null ? eph[i].culmination_time!.Value.ToString("HH:mm") : "";

            var yellowBrush = new SolidColourBrush(Colours.Orange);
            var grayBrush = new SolidColourBrush(Colours.Gray);




            //Dessin de la ligne d'arrière plan
            gpr.StrokeRectangle(new Point(75, 45 + (i * 100)), new Size(700, 0),
                grayBrush, 4D, LineCaps.Round, LineJoins.Round);
            
            //ecriture de l'heure de culmination si disponible
            if (middleTime != "")
                gpr.FillText(
                    new Point(
                        75 + culminationTimeInt * 700 / 1440 -
                        gpr.MeasureText(middleTime, new Font(NimbusRegular, 15.0)).Width / 2, 65 + (i * 100)),
                    middleTime, new Font(NimbusRegular, 15), new SolidColourBrush(Colours.Black), TextBaselines.Middle);

            //Ecriture du nom de la planète
            gpr.FillText(new Point(75, 30 + (i * 100)), display.Name, new Font(NimbusBold, 18),
                new SolidColourBrush(Colours.Black), TextBaselines.Middle);
            
            if (!visible)
            {
                //Si disponible, repérer le point de culmination
                if (middleTime != "")
                    gpr.FillRectangle(new Point(74 + culminationTimeInt * 700 / 1440, 40 + i * 100), new Size(2, 10),
                        new SolidColourBrush(Colours.Brown));
                continue;
            }

            //Dessin du remplissage
            if (riseTimeInt < setTimeInt)
                gpr.StrokeRectangle(new Point(75 + Math.Min(riseTimeInt, setTimeInt) * 700 / 1440, 45 + i * 100),
                    new Size(Math.Abs(riseTimeInt - setTimeInt) * 700.0 / 1440.0, 0),
                    yellowBrush, 10D, LineCaps.Round, LineJoins.Round);
            else
            {
                gpr.StrokeRectangle(new Point(75, 45 + i * 100),
                    new Size(Math.Min(riseTimeInt, setTimeInt) * 700.0 / 1440, 0),
                    yellowBrush, 10D, LineCaps.Round, LineJoins.Round);
                gpr.StrokeRectangle(new Point(75 + Math.Max(riseTimeInt, setTimeInt) * 700 / 1440, 45 + i * 100),
                    new Size(700 - Math.Max(riseTimeInt, setTimeInt) * 700 / 1440, 0),
                    yellowBrush, 10D, LineCaps.Round, LineJoins.Round);
            }

            //Si disponible, repérer le point de culmination
            if (middleTime != "")
                gpr.FillRectangle(new Point(74 + culminationTimeInt * 700 / 1440, 40 + i * 100), new Size(2, 10),
                    new SolidColourBrush(Colours.Brown));

            //Ecriture de la borne inférieure
            if (writeRiseHour)
                gpr.FillText(
                    new Point(
                        75 + Math.Min(riseTimeInt, setTimeInt) * 700 / 1440 -
                        gpr.MeasureText(minTime, new Font(NimbusRegular, 15.0)).Width / 2, 65 + (i * 100)),
                    minTime, new Font(NimbusRegular, 15), new SolidColourBrush(Colours.Black), TextBaselines.Middle);

            //Ecriture de la borne supérieure
            if (writeSetHour)
                gpr.FillText(
                    new Point(
                        75 + Math.Max(riseTimeInt, setTimeInt) * 700 / 1440 -
                        gpr.MeasureText(maxTime, new Font(NimbusRegular, 15.0)).Width / 2, 65 + (i * 100)),
                    maxTime, new Font(NimbusRegular, 15), new SolidColourBrush(Colours.Black), TextBaselines.Middle);
            
        }

        doc.Pages.Last().SaveAsSVG(fileName, SVGContextInterpreter.TextOptions.ConvertIntoPaths);
    }
}