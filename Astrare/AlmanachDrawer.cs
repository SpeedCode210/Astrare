using System;
using System.Collections.Generic;
using VectSharp;
using System.IO;
using System.Linq;
using Astrare.KosmorroConnection;
using Astrare.Translate;
using ImageMagick;
using VectSharp.SVG;
using Size = VectSharp.Size;

namespace Astrare;

public static class AlmanachDrawer
{
    private static readonly FontFamily NimbusBold = new(File.OpenRead(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"/Resources/NimbusSanL-Bol.ttf"));

    private static readonly FontFamily NimbusRegular =
        new(File.OpenRead(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"/Resources/NimbusSanL-Reg.ttf"));

    
    const int Margin = 300;
    const int MarginTop = 400;
    const int HourWidth = 100;
    const int DayHeight = 12;

    public static void SaveSvg(GlobalData[] data, string fileName, string title, string subtitle)
    {
        SolidColourBrush background = new(Colour.FromRgb((byte) 181, (byte) 223, (byte) 255));
        SolidColourBrush nightFill = new(Colour.FromRgb((byte) 48, (byte) 80, (byte) 105));
        SolidColourBrush gridColor = new(Colour.FromRgb((byte) 26, (byte) 145, (byte) 90));
        SolidColourBrush phaseFill = new(Colour.FromRgba(1.0f, 1.0f, 1.0f, 0.5f));
        

        //Création du document
        Document doc = new Document();
        doc.Pages.Add(new Page(Margin * 2 + HourWidth * 24, MarginTop * 2 + (data.Length - 1) * DayHeight + 150));
        var page = doc.Pages.Last();
        page.Background = Colours.White;
        Graphics gpr = page.Graphics;
        gpr.FillRectangle(new Point(Margin, MarginTop), new Size(page.Width - Margin * 2, (data.Length - 1) * DayHeight),
            background);
        
        //Ecriture des titres
        gpr.FillText(new Point(100,120), 
            title, new Font(NimbusBold, 100), new SolidColourBrush(Colours.Chocolate), TextBaselines.Middle);
        
        gpr.FillText(new Point(100,220), 
            subtitle, new Font(NimbusRegular, 75), new SolidColourBrush(Colours.Teal), TextBaselines.Middle);

        //Affichage du logo
        
        using (var image =
               new MagickImage(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Resources/astrareLogoDark.png"))
        {
            List<byte> arr = new();
            foreach (var b in image.GetPixels())
            {
                foreach (var c in b.ToArray())
                {
                    arr.Add(c);
                }
            }

            gpr.DrawRasterImage(page.Width-100-884, 5, 884.0, 300.0,
                new RasterImage(arr.ToArray(), image.Width, image.Height, PixelFormats.RGBA, true));
        }

        //Lever-coucher soleil
        for (int i = 1; i < data.Length; i++)
        {
            gpr.FillRectangle(
                new Point(Margin + (HourToMinutes(data[i - 1].ephemerides[0].set_time!.Value) - 720) * 2400 / 1440,
                    MarginTop + (i - 1) * DayHeight),
                new Size(
                    (1440 - HourToMinutes(data[i].ephemerides[0].set_time!.Value) +
                     HourToMinutes(data[i - 1].ephemerides[0].rise_time!.Value)) * 2400.0 / 1440, DayHeight),
                nightFill);
        }

        //Phases lune


        //Partie gauche 12h-00h
        for (int i = 1; i < data.Length; i++)
        {
            var rise = data[i - 1].ephemerides[1].rise_time != null
                ? HourToMinutes(data[i - 1].ephemerides[1].rise_time!.Value)
                : 0;
            var set = data[i - 1].ephemerides[1].set_time != null
                ? HourToMinutes(data[i - 1].ephemerides[1].set_time!.Value)
                : 1440;

            if (rise < 720 && set < 720)
            {
                if (rise < set)
                    continue;

                set = 1440;
            }

            int start1;
            int end1;

            if (rise >= 720)
            {
                start1 = rise - 720;
                end1 = 720;
            }
            else
            {
                end1 = set - 720;
                start1 = 0;
            }

            gpr.FillRectangle(new Point(Margin + start1 * 24 * HourWidth / 1440, MarginTop + (i - 1) * DayHeight + 1),
                new Size(Math.Abs((end1 - start1) * 24 * HourWidth / 1440), 10), phaseFill);
        }

        //Partie droite 00h-12h
        for (int i = 1; i < data.Length; i++)
        {
            var rise = data[i].ephemerides[1].rise_time != null
                ? HourToMinutes(data[i].ephemerides[1].rise_time!.Value)
                : 0;
            var set = data[i].ephemerides[1].set_time != null
                ? HourToMinutes(data[i].ephemerides[1].set_time!.Value)
                : 1440;


            int start1;
            int end1;

            if (rise < set)
            {
                if (rise is >= 0 and < 720)
                {
                    start1 = rise;
                    end1 = 720;
                }
                else
                {
                    if (set > 0)
                    {
                        start1 = 0;
                        end1 = set;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            else
            {
                if (set is > 0 and < 1200)
                {
                    start1 = 0;
                    end1 = set;
                }
                else
                {
                    if (rise is >= 0 and < 720)
                    {
                        start1 = rise;
                        end1 = 720;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            gpr.FillRectangle(
                new Point(Margin + (start1 + 720) * 24 * HourWidth / 1440, MarginTop + (i - 1) * DayHeight + 1),
                new Size(Math.Abs((end1 - start1) * 24 * HourWidth / 1440), 10), phaseFill);
        }

        //Icones lune
        for (int j = 1; j < data.Length - 1; j++)
        {
            if (data[j].ephemerides[1].culmination_time is null)
                continue;

            int x, y;
            if (HourToMinutes(data[j].ephemerides[1].culmination_time!.Value) > 720)
            {
                y = MarginTop + (j * DayHeight) - ((32 - DayHeight) / 2);
                x = Margin - 16 + (HourToMinutes(data[j].ephemerides[1].culmination_time!.Value) - 720) * HourWidth *
                    24 / 1440;
            }
            else
            {
                y = MarginTop + (j - 1) * DayHeight - (32 - DayHeight) / 2;
                x = Margin - 16 + (HourToMinutes(data[j].ephemerides[1].culmination_time!.Value) + 720) * HourWidth *
                    24 / 1440;
            }


            //Affichage de l'icone de la lune
            using var image =
                new MagickImage(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Resources/MoonPhases/" +
                                data[j].moon_phase.phase.ToString().ToLower().Replace('_', '-') +
                                ".png");
            List<byte> arr = new();
            foreach (var b in image.GetPixels())
            {
                foreach (var c in b.ToArray())
                {
                    arr.Add(c);
                }
            }

            gpr.DrawRasterImage(x, y, 32.0, 32.0,
                new RasterImage(arr.ToArray(), image.Width, image.Height, PixelFormats.RGBA, true));
        }

        //Dessin planètes

        var riseDash = new LineDash(9, 9, 0);
        var setDash = new LineDash(3, 9, 0);
        var culminationDash = new LineDash(1, 1, 0);
        

        Tuple<GraphicsPath, Colour, LineDash>[] elements = new[]
        {
            //mercure
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(2, data), Colours.Gray, riseDash), 
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(2, data), Colours.Gray, setDash),
            //venus
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(3, data), Colours.White, riseDash), 
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(3, data), Colours.White, setDash),
            //mars
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(4, data), Colours.Red, riseDash), 
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetCulminationPath(4, data), Colours.Red, culminationDash), 
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(4, data), Colours.Red, setDash),
            //jupiter
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(5, data), Colours.Orange, riseDash), 
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetCulminationPath(5, data), Colours.Orange, culminationDash), 
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(5, data), Colours.Orange, setDash),
            //saturne
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(6, data), Colours.Teal, riseDash), 
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetCulminationPath(6, data), Colours.Teal, culminationDash), 
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(6, data), Colours.Teal, setDash),
            //uranus
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(7, data), Colours.LimeGreen, riseDash), 
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetCulminationPath(7, data), Colours.LimeGreen, culminationDash), 
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(7, data), Colours.LimeGreen, setDash),
            //neptune
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(8, data), Colours.Purple, riseDash), 
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetCulminationPath(8, data), Colours.Purple, culminationDash), 
            new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(8, data), Colours.Purple, setDash),
        };

        foreach (var e in elements)
        {
            gpr.StrokePath(e.Item1, new SolidColourBrush(e.Item2), 4, 
                LineCaps.Round, LineJoins.Bevel, e.Item3);
        }

        


        //Dessiner grille
        //Colonnes
        for (int i = 1; i < 24; i++)
        {
            gpr.StrokeRectangle(new Point(Margin, MarginTop), new Size(i * 100 - 1, (data.Length - 1) * DayHeight), gridColor);

            var text = new DateTime(2000, 01, 01, i, 0, 0).AddHours(-12).Hour.ToString("00") + "h";
            gpr.FillText(
                new Point(i * 100 - 1 + Margin - gpr.MeasureText(text, new Font(NimbusBold, 25)).Width / 2, MarginTop-25), text,
                new Font(NimbusBold, 25), gridColor);
        }

        //Lignes
        for (int i = 0; i < data.Length; i += 7)
        {
            gpr.StrokeRectangle(new Point(Margin, MarginTop), new Size(page.Width - Margin * 2, i * DayHeight - 1),
                gridColor);

            var text = data[i].ephemerides[0].rise_time!.Value.Day.ToString("00") + " "
                + Language.Current.Months[data[i].ephemerides[0].rise_time!.Value.Month - 1];

            gpr.FillText(
                new Point((Margin - 50) - gpr.MeasureText(text, new Font(NimbusBold, 25)).Width,
                    i * DayHeight - 1 + MarginTop), text,
                new Font(NimbusBold, 25), gridColor, TextBaselines.Middle);

            text = data[i].ephemerides[0].rise_time!.Value.AddDays(1).Day.ToString("00") + " "
                + Language.Current.Months[data[i].ephemerides[0].rise_time!.Value.AddDays(1).Month - 1];

            gpr.FillText(new Point((Margin + HourWidth * 24 + 25), i * DayHeight - 1 + MarginTop), text,
                new Font(NimbusBold, 25),
                gridColor, TextBaselines.Middle);
        }


        gpr.StrokeRectangle(new Point(Margin, MarginTop), new Size(page.Width - Margin * 2, (data.Length - 1) * DayHeight),
            gridColor, 3D);
        
        
        //Dessin légende
        
        //Dessin tableau
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40), new Size(page.Width - Margin * 2, 300),
            gridColor, 5D);
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40), new Size((page.Width - Margin * 2)/2, 300),
            gridColor, 5D);
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40), new Size((page.Width - Margin * 2)*3/4, 300),
            gridColor, 5D);
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40), new Size((page.Width - Margin * 2)/4, 300),
            gridColor, 5D);
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40), new Size((page.Width - Margin * 2)*3/4, 300),
            gridColor, 5D);
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40), new Size((page.Width - Margin * 2)/4, 300),
            gridColor, 5D);
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40), new Size((page.Width - Margin * 2)/4, 300),
            gridColor, 5D);
        
        //Lune
        var str = Language.Current.Translate("Moon");
        gpr.FillText(new Point(Margin + HourWidth * 3 - gpr.MeasureText(str, new Font(NimbusBold, 50)).Width/2, MarginTop + data.Length * DayHeight + 60),
            str, new Font(NimbusBold, 50),
            gridColor);
        
        str = Language.Current.Translate("Culmination");
        gpr.FillText(new Point(Margin + HourWidth * 3 - gpr.MeasureText(str, new Font(NimbusRegular, 35)).Width/2, MarginTop + data.Length * DayHeight + 130),
            str, new Font(NimbusRegular, 35),
            gridColor);
        
        str = Language.Current.Translate("Rise");
        gpr.FillText(new Point(Margin + 5, MarginTop + data.Length * DayHeight + 225),
            str, new Font(NimbusRegular, 35),
            gridColor);
        
        str = Language.Current.Translate("Set");
        gpr.FillText(new Point(Margin + HourWidth * 6 - gpr.MeasureText(str, new Font(NimbusRegular, 35)).Width - 15, MarginTop + data.Length * DayHeight + 225),
            str, new Font(NimbusRegular, 35),
            gridColor);
        
        gpr.FillRectangle(new Point(Margin + 30, MarginTop + data.Length * DayHeight + 180),
            new Size(HourWidth*6-60, 32), new SolidColourBrush(Colours.LightGray));
        
        using (var image =
               new MagickImage(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Resources/MoonPhases/waning-crescent.png"))
        {
            List<byte> arr = new();
            foreach (var b in image.GetPixels())
            {
                foreach (var c in b.ToArray())
                {
                    arr.Add(c);
                }
            }

            gpr.DrawRasterImage(Margin + HourWidth*3 - 32, MarginTop + data.Length * DayHeight + 164, 64.0, 64.0,
                new RasterImage(arr.ToArray(), image.Width, image.Height, PixelFormats.RGBA, true));
        }
        
        //Jour-Nuit
        
        gpr.FillRectangle(new Point(Margin + HourWidth*6 + 80, MarginTop + data.Length * DayHeight + 80),
            new Size(80, 100), background);
        
        gpr.FillRectangle(new Point(Margin + HourWidth*6 + 80, MarginTop + data.Length * DayHeight + 180),
            new Size(80, 100), nightFill);
        
        str = Language.Current.Translate("Day");
        gpr.FillText(new Point(Margin + HourWidth * 6 + 180, MarginTop + data.Length * DayHeight + 110),
            str, new Font(NimbusRegular, 45),
            gridColor);
        
        str = Language.Current.Translate("Night");
        gpr.FillText(new Point(Margin + HourWidth * 6 + 180, MarginTop + data.Length * DayHeight + 210),
            str, new Font(NimbusRegular, 45),
            gridColor);
        
        str = Language.Current.Translate("Planets");
        gpr.FillText(new Point(Margin + HourWidth * 15 - gpr.MeasureText(str, new Font(NimbusBold, 50)).Width/2, MarginTop + data.Length * DayHeight + 60),
            str, new Font(NimbusBold, 45),
            gridColor);
        
        str = Language.Current.Translate("Rise");
        gpr.FillText(new Point(Margin + HourWidth * 12 + 15, MarginTop + data.Length * DayHeight + 140),
            str, new Font(NimbusRegular, 40),
            gridColor, TextBaselines.Middle);
        
        str = Language.Current.Translate("Culmination");
        gpr.FillText(new Point(Margin + HourWidth * 12 + 15, MarginTop + data.Length * DayHeight + 200),
            str, new Font(NimbusRegular, 40),
            gridColor, TextBaselines.Middle);
        
        str = Language.Current.Translate("Set");
        gpr.FillText(new Point(Margin + HourWidth * 12 + 15, MarginTop + data.Length * DayHeight + 260),
            str, new Font(NimbusRegular, 40),
            gridColor, TextBaselines.Middle);

        var path = new GraphicsPath();
        path.MoveTo(new Point(Margin + HourWidth * 15 + 10, MarginTop + data.Length * DayHeight + 140));
        path.LineTo(new Point(Margin + HourWidth * 18 - 20, MarginTop + data.Length * DayHeight + 140));
        gpr.StrokePath(path, gridColor, 6, LineCaps.Butt, LineJoins.Bevel, riseDash);
        path = new GraphicsPath();
        path.MoveTo(new Point(Margin + HourWidth * 15 + 10, MarginTop + data.Length * DayHeight + 200));
        path.LineTo(new Point(Margin + HourWidth * 18 - 20, MarginTop + data.Length * DayHeight + 200));
        gpr.StrokePath(path, gridColor, 6);
        path = new GraphicsPath();
        path.MoveTo(new Point(Margin + HourWidth * 15 + 10, MarginTop + data.Length * DayHeight + 260));
        path.LineTo(new Point(Margin + HourWidth * 18 - 20, MarginTop + data.Length * DayHeight + 260));
        gpr.StrokePath(path, gridColor, 6, LineCaps.Butt, LineJoins.Bevel, setDash);

        str = Language.Current.Translate("Mercury");
        gpr.FillText(new Point(Margin + HourWidth * 18 + 15, MarginTop + data.Length * DayHeight + 100),
            str, new Font(NimbusRegular, 40),
            new SolidColourBrush(Colours.Gray), TextBaselines.Middle);
        
        str = Language.Current.Translate("Mars");
        gpr.FillText(new Point(Margin + HourWidth * 18 + 15, MarginTop + data.Length * DayHeight + 160),
            str, new Font(NimbusRegular, 40),
            new SolidColourBrush(Colours.Red), TextBaselines.Middle);
        
        str = Language.Current.Translate("Jupiter");
        gpr.FillText(new Point(Margin + HourWidth * 18 + 15, MarginTop + data.Length * DayHeight + 220),
            str, new Font(NimbusRegular, 40),
            new SolidColourBrush(Colours.Orange), TextBaselines.Middle);
        
        str = Language.Current.Translate("Saturn");
        gpr.FillText(new Point(Margin + HourWidth * 18 + 15, MarginTop + data.Length * DayHeight + 280),
            str, new Font(NimbusRegular, 40),
            new SolidColourBrush(Colours.Teal), TextBaselines.Middle);
        
        str = Language.Current.Translate("Venus");
        gpr.FillText(new Point(Margin + HourWidth * 21 + 15, MarginTop + data.Length * DayHeight + 130),
            str, new Font(NimbusRegular, 40),
            new SolidColourBrush(Colours.Black), TextBaselines.Middle);
        
        str = Language.Current.Translate("Uranus");
        gpr.FillText(new Point(Margin + HourWidth * 21 + 15, MarginTop + data.Length * DayHeight + 190),
            str, new Font(NimbusRegular, 40),
            new SolidColourBrush(Colours.LimeGreen), TextBaselines.Middle);
        
        str = Language.Current.Translate("Neptune");
        gpr.FillText(new Point(Margin + HourWidth * 21 + 15, MarginTop + data.Length * DayHeight + 250),
            str, new Font(NimbusRegular, 40),
            new SolidColourBrush(Colours.Purple), TextBaselines.Middle);
        
        doc.Pages.Last().SaveAsSVG(fileName);
    }

    private static int HourToMinutes(DateTime time)
    {
        return time.Hour * 60 + time.Minute;
    }


    private static GraphicsPath GetPlanetRisePath(int id, GlobalData[] data)
    {
        var path = new GraphicsPath();
        bool started = false;
        Point lastPoint = new(0, 0);
        for (int j = 1; j < data.Length - 1; j++)
        {

            if (data[j].ephemerides[id].rise_time is null)
                continue;
            if (data[j - 1].ephemerides[id].rise_time is null)
                continue;

            Point newPoint = new Point(0,0);
            
            if (HourToMinutes(data[j].ephemerides[id].rise_time!.Value) > 720)
            {
                if (!started)
                {
                    var pt = new Point(
                        Margin + (HourToMinutes(data[j - 1].ephemerides[id].rise_time!.Value) - 720) * HourWidth * 24 /
                        1440.0,
                        MarginTop + (j - 1) * DayHeight);
                    path.MoveTo(pt);
                    lastPoint = pt;
                    started = true;
                }

                newPoint = new Point(
                    Margin + (HourToMinutes(data[j].ephemerides[id].rise_time!.Value) - 720) * HourWidth * 24 / 1440.0,
                    MarginTop + j * DayHeight);
            }
            else if (HourToMinutes(data[j].ephemerides[id].rise_time!.Value) <= 720)
            {
                if (!started)
                {
                    var pt = new Point(
                        Margin + (HourToMinutes(data[j - 1].ephemerides[id].rise_time!.Value) + 720) * HourWidth * 24 /
                        1440.0,
                        MarginTop + (j - 2) * DayHeight);
                    path.MoveTo(pt);
                    lastPoint = pt;
                    started = true;
                }

                newPoint = new Point(
                    Margin + (HourToMinutes(data[j].ephemerides[id].rise_time!.Value) + 720) * HourWidth * 24 / 1440.0,
                    MarginTop + (j - 1) * DayHeight);
            }

            if (newPoint.X != 0)
            {
                if (Math.Abs(newPoint.X - lastPoint.X) < 200)
                    path.LineTo(newPoint);
                else
                {
                    path.MoveTo(newPoint);
                }
            }

            lastPoint = newPoint;
        }

        return path;
    }
    
    private static GraphicsPath GetPlanetCulminationPath(int id, GlobalData[] data)
    {
        var path = new GraphicsPath();
        bool started = false;
        Point lastPoint = new(0, 0);
        for (int j = 1; j < data.Length - 1; j++)
        {

            if (data[j].ephemerides[id].culmination_time is null)
                continue;
            if (data[j - 1].ephemerides[id].culmination_time is null)
                continue;

            Point newPoint = new Point(0,0);
            
            if (HourToMinutes(data[j].ephemerides[id].culmination_time!.Value) > 720)
            {
                if (!started)
                {
                    var pt = new Point(
                        Margin + (HourToMinutes(data[j - 1].ephemerides[id].culmination_time!.Value) - 720) * HourWidth * 24 /
                        1440.0,
                        MarginTop + (j - 1) * DayHeight);
                    path.MoveTo(pt);
                    lastPoint = pt;
                    started = true;
                }

                newPoint = new Point(
                    Margin + (HourToMinutes(data[j].ephemerides[id].culmination_time!.Value) - 720) * HourWidth * 24 / 1440.0,
                    MarginTop + j * DayHeight);
            }
            else if (HourToMinutes(data[j].ephemerides[id].culmination_time!.Value) <= 720)
            {
                if (!started)
                {
                    var pt = new Point(
                        Margin + (HourToMinutes(data[j - 1].ephemerides[id].culmination_time!.Value) + 720) * HourWidth * 24 /
                        1440.0,
                        MarginTop + (j - 2) * DayHeight);
                    path.MoveTo(pt);
                    lastPoint = pt;
                    started = true;
                }

                newPoint = new Point(
                    Margin + (HourToMinutes(data[j].ephemerides[id].culmination_time!.Value) + 720) * HourWidth * 24 / 1440.0,
                    MarginTop + (j - 1) * DayHeight);
            }

            if (newPoint.X != 0)
            {
                if (Math.Abs(newPoint.X - lastPoint.X) < 200)
                    path.LineTo(newPoint);
                else
                {
                    path.MoveTo(newPoint);
                }
            }

            lastPoint = newPoint;
        }

        return path;
    }
    
    private static GraphicsPath GetPlanetSetPath(int id, GlobalData[] data)
    {
        var path = new GraphicsPath();
        bool started = false;
        Point lastPoint = new(0, 0);
        for (int j = 1; j < data.Length - 1; j++)
        {

            if (data[j].ephemerides[id].set_time is null)
                continue;
            if (data[j - 1].ephemerides[id].set_time is null)
                continue;

            Point newPoint = new Point(0,0);
            
            if (HourToMinutes(data[j].ephemerides[id].set_time!.Value) > 720)
            {
                if (!started)
                {
                    var pt = new Point(
                        Margin + (HourToMinutes(data[j - 1].ephemerides[id].set_time!.Value) - 720) * HourWidth * 24 /
                        1440.0,
                        MarginTop + (j - 1) * DayHeight);
                    path.MoveTo(pt);
                    lastPoint = pt;
                    started = true;
                }

                newPoint = new Point(
                    Margin + (HourToMinutes(data[j].ephemerides[id].set_time!.Value) - 720) * HourWidth * 24 / 1440.0,
                    MarginTop + j * DayHeight);
            }
            else if (HourToMinutes(data[j].ephemerides[id].set_time!.Value) <= 720)
            {
                if (!started)
                {
                    var pt = new Point(
                        Margin + (HourToMinutes(data[j - 1].ephemerides[id].set_time!.Value) + 720) * HourWidth * 24 /
                        1440.0,
                        MarginTop + (j - 2) * DayHeight);
                    path.MoveTo(pt);
                    lastPoint = pt;
                    started = true;
                }

                newPoint = new Point(
                    Margin + (HourToMinutes(data[j].ephemerides[id].set_time!.Value) + 720) * HourWidth * 24 / 1440.0,
                    MarginTop + (j - 1) * DayHeight);
            }

            if (newPoint.X != 0)
            {
                if (Math.Abs(newPoint.X - lastPoint.X) < 200)
                    path.LineTo(newPoint);
                else
                {
                    path.MoveTo(newPoint);
                }
            }

            lastPoint = newPoint;
        }

        return path;
    }
}