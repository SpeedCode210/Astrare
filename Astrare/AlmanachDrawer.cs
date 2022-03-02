using System;
using System.Collections.Generic;
using VectSharp;
using System.IO;
using System.Linq;
using Astrare.KosmorroConnection;
using Astrare.Models;
using Astrare.Translate;
using ImageMagick;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using VectSharp.SVG;
using Size = VectSharp.Size;

namespace Astrare;

public static class AlmanachDrawer
{
    private static readonly FontFamily NimbusBold = new(File.OpenRead(
        Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
        @"/Resources/NimbusSanL-Bol.ttf"));

    private static readonly FontFamily NimbusRegular =
        new(File.OpenRead(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                          @"/Resources/NimbusSanL-Reg.ttf"));


    const int Margin = 300;
    const int MarginTop = 400;
    const int HourWidth = 100;
    const int DayHeight = 12;

    public static void 
        SavePdf(GlobalData[] data, DateTime[] dates, string fileName, string title, string subtitle,
            bool dayNight, bool moonEphs, bool[,] planetsLines, bool economicMode)
    {
        SolidColourBrush background = economicMode ? new(Colours.White) : new(Colour.FromRgb((byte) 181, (byte) 223, (byte) 255));
        SolidColourBrush nightFill = economicMode ? new(Colours.White) : new(Colour.FromRgb((byte) 48, (byte) 80, (byte) 105));
        SolidColourBrush gridColor = new(Colour.FromRgb((byte) 26, (byte) 145, (byte) 90));
        SolidColourBrush moonFill = new(Colour.FromRgba(0.5f, 0.5f, 0.5f, 0.5f));


        //Création du document
        Document doc = new Document();
        doc.Pages.Add(new Page(Margin * 2 + HourWidth * 24, MarginTop * 2 + data.Length * DayHeight + 150));
        var page = doc.Pages.Last();
        page.Background = Colours.White;
        Graphics gpr = page.Graphics;
        gpr.FillRectangle(new Point(Margin, MarginTop),
            new Size(page.Width - Margin * 2, data.Length * DayHeight),
            nightFill);

        var riseDash = new LineDash(9, 9, 0);
        var setDash = new LineDash(3, 9, 0);
        var culminationDash = new LineDash(1, 1, 0);

        //Ecriture des titres
        gpr.FillText(new Point(100, 120),
            title, new Font(NimbusBold, 100), new SolidColourBrush(Colours.Chocolate), TextBaselines.Middle);

        gpr.FillText(new Point(100, 220),
            subtitle, new Font(NimbusRegular, 75), new SolidColourBrush(Colours.Teal), TextBaselines.Middle);

        //Affichage du logo

        using (var image =
               new MagickImage(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                               "/Resources/astrareLogoDark.png"))
        {
            List<byte> arr = new();
            foreach (var b in image.GetPixels())
            {
                foreach (var c in b.ToArray())
                {
                    arr.Add(c);
                }
            }

            gpr.DrawRasterImage(page.Width - 100 - 884, 5, 884.0, 300.0,
                new RasterImage(arr.ToArray(), image.Width, image.Height, PixelFormats.RGBA, true));
        }


        // Soleil

        if (dayNight)
        {
            for (int i = 0; i < data.Length; i++)
            {
                var eph = data[i].ephemerides[0].Clone(); // Ephémérides du soleil
                var visible = eph.IsVisible(eph);

                if (eph.rise_time is null)
                {
                    eph.rise_time = new DateTime(2000, 1, 1, 0, 0, 0);
                }

                if (eph.set_time is null)
                {
                    eph.set_time = new DateTime(2000, 1, 1, 23, 59, 59);
                }

                var riseTimeInt = eph.rise_time!.Value.Hour * 60 + eph.rise_time!.Value.Minute;
                var setTimeInt = eph.set_time!.Value.Hour * 60 + eph.set_time!.Value.Minute;

                if (visible)
                {
                    if (riseTimeInt < setTimeInt)
                    {
                        gpr.FillRectangle(
                            new Point(Margin + Math.Min(riseTimeInt, setTimeInt) * (HourWidth * 24) / 1440,
                                MarginTop + i * DayHeight),
                            new Size(Math.Abs(riseTimeInt - setTimeInt) * (HourWidth * 24) / 1440, DayHeight), background);
                    }
                    else
                    {
                        gpr.FillRectangle(new Point(Margin, MarginTop + i * DayHeight),
                            new Size(Math.Min(riseTimeInt, setTimeInt) * (HourWidth * 24) / 1440, DayHeight), background);

                        gpr.FillRectangle(
                            new Point(Margin + Math.Max(riseTimeInt, setTimeInt) * (HourWidth * 24) / 1440,
                                MarginTop + i * DayHeight),
                            new Size((1440 - Math.Max(riseTimeInt, setTimeInt)) * (HourWidth * 24) / 1440, DayHeight),
                            background);
                    }
                }
            }
            
        }

        //Lune

        if (moonEphs)
        {
            for (int i = 0; i < data.Length; i++)
        {
            var eph = data[i].ephemerides[1].Clone(); // Ephémérides de la lune
            var visible = eph.IsVisible(data[i].ephemerides[0]);

            if (eph.rise_time is null)
            {
                eph.rise_time = new DateTime(2000, 1, 1, 0, 0, 0);
            }

            if (eph.set_time is null)
            {
                eph.set_time = new DateTime(2000, 1, 1, 23, 59, 59);
            }

            var riseTimeInt = eph.rise_time!.Value.Hour * 60 + eph.rise_time!.Value.Minute;
            var setTimeInt = eph.set_time!.Value.Hour * 60 + eph.set_time!.Value.Minute;

            if (visible)
            {
                if (riseTimeInt < setTimeInt)
                {
                    gpr.FillRectangle(
                        new Point(Margin + Math.Min(riseTimeInt, setTimeInt) * (HourWidth * 24) / 1440,
                            MarginTop + i * DayHeight + 1),
                        new Size(Math.Abs(riseTimeInt - setTimeInt) * (HourWidth * 24) / 1440, DayHeight - 2),
                        moonFill);
                }
                else
                {
                    gpr.FillRectangle(new Point(Margin, MarginTop + i * DayHeight + 1),
                        new Size(Math.Min(riseTimeInt, setTimeInt) * (HourWidth * 24) / 1440, DayHeight - 2), moonFill);

                    gpr.FillRectangle(
                        new Point(Margin + Math.Max(riseTimeInt, setTimeInt) * (HourWidth * 24) / 1440,
                            MarginTop + i * DayHeight + 1),
                        new Size((1440 - Math.Max(riseTimeInt, setTimeInt)) * (HourWidth * 24) / 1440, DayHeight - 2),
                        moonFill);
                }
            }
        }

        for (int i = 0; i < data.Length; i++)
        {
            var eph = data[i].ephemerides[1].Clone(); // Ephémérides de la lune

            // Icone phase

            if (eph.culmination_time != null && eph.culmination_time.HasValue)
            {
                var culminationTimeInt = eph.culmination_time!.Value.Hour * 60 + eph.culmination_time!.Value.Minute;

                //Chargement de l'icone de la lune
                using var image =
                    new MagickImage(
                        Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                        "/Resources/MoonPhases/" +
                        data[i].moon_phase.phase.ToString().ToLower().Replace('_', '-') +
                        ".png");
                List<byte> arr = new();
                foreach (var b in image.GetPixels())
                {
                    foreach (var c in b.ToArray())
                    {
                        arr.Add(c);
                    }
                }

                gpr.DrawRasterImage(
                    Margin + ((culminationTimeInt * HourWidth * 24) / 1440) - 16,
                    MarginTop + i * DayHeight + DayHeight / 2 - 16,
                    32.0,
                    32.0,
                    new RasterImage(arr.ToArray(), image.Width, image.Height, PixelFormats.RGBA, true));
            }
        }
        }
        
                
        //Dessin planètes


        List<Tuple<GraphicsPath, Colour, LineDash>> elements = new();


        if (economicMode && dayNight)
        {
            //Soleil eco
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(0, data), Colours.Black, culminationDash));
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(0, data), Colours.Black, culminationDash));

        }
        
        //mercure
        if (planetsLines[0, 0])
           elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(2, data), Colours.Gray, riseDash));
        if (planetsLines[0, 1])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetCulminationPath(2, data), Colours.Gray, culminationDash));
        if (planetsLines[0, 2])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(2, data), Colours.Gray, setDash));
        //venus
        if (planetsLines[1, 0])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(3, data), economicMode ? Colours.Black : Colours.White, riseDash));
        if (planetsLines[1, 1])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetCulminationPath(3, data), economicMode ? Colours.Black : Colours.White, culminationDash));
        if (planetsLines[1, 2])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(3, data), economicMode ? Colours.Black : Colours.White, setDash));
        //mars
        if (planetsLines[2, 0])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(4, data), Colours.Red, riseDash));
        if (planetsLines[2, 1])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetCulminationPath(4, data), Colours.Red, culminationDash));
        if (planetsLines[2, 2])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(4, data), Colours.Red, setDash));
        //jupiter
        if (planetsLines[3, 0])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(5, data), Colours.Orange, riseDash));
        if (planetsLines[3, 1])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetCulminationPath(5, data), Colours.Orange, culminationDash));
        if (planetsLines[3, 2])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(5, data), Colours.Orange, setDash));
        //saturne
        if (planetsLines[4, 0])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(6, data), Colours.Teal, riseDash));
        if (planetsLines[4, 1])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetCulminationPath(6, data), Colours.Teal, culminationDash));
        if (planetsLines[4, 2])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(6, data), Colours.Teal, setDash));
        //uranus
        if (planetsLines[5, 0])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(7, data), Colours.LimeGreen, riseDash));
        if (planetsLines[5, 1])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetCulminationPath(7, data), Colours.LimeGreen, culminationDash));
        if (planetsLines[5, 2])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(7, data), Colours.LimeGreen, setDash));
        //neptune
        if (planetsLines[6, 0])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetRisePath(8, data), Colours.Purple, riseDash));
        if (planetsLines[6, 1])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetCulminationPath(8, data), Colours.Purple, culminationDash));
        if (planetsLines[6, 2])
            elements.Add(new Tuple<GraphicsPath, Colour, LineDash>(GetPlanetSetPath(8, data), Colours.Purple, setDash));
        
        foreach (var e in elements)
        {
            gpr.StrokePath(e.Item1, new SolidColourBrush(e.Item2), 4,
                LineCaps.Round, LineJoins.Bevel, e.Item3);
        }



        //Dessiner grille
        //Colonnes
        for (int i = 1; i < 24; i++)
        {
            gpr.StrokeRectangle(new Point(Margin, MarginTop), new Size(i * 100 - 1, data.Length * DayHeight),
                gridColor);

            var text = new DateTime(2000, 01, 01, i, 0, 0).Hour.ToString("00") + "h";
            gpr.FillText(
                new Point(i * 100 - 1 + Margin - gpr.MeasureText(text, new Font(NimbusBold, 25)).Width / 2,
                    MarginTop - 25), text,
                new Font(NimbusBold, 25), gridColor);
        }

        //Lignes
        for (int i = 0; i < data.Length; i += 7)
        {
            gpr.StrokeRectangle(new Point(Margin, MarginTop), new Size(page.Width - Margin * 2, i * DayHeight - 1),
                gridColor);

            var text = dates[i].Day.ToString("00") + " "
                                                   + Language.Current.Months[dates[i].Month - 1];

            gpr.FillText(
                new Point((Margin - 50) - gpr.MeasureText(text, new Font(NimbusBold, 25)).Width,
                    i * DayHeight - 1 + MarginTop), text,
                new Font(NimbusBold, 25), gridColor, TextBaselines.Middle);

            text = dates[i].Day.ToString("00") + " " +
                   Language.Current.Months[dates[i].AddDays(1).Month - 1];

            gpr.FillText(new Point((Margin + HourWidth * 24 + 25), i * DayHeight - 1 + MarginTop), text,
                new Font(NimbusBold, 25),
                gridColor, TextBaselines.Middle);
        }


        gpr.StrokeRectangle(new Point(Margin, MarginTop),
            new Size(page.Width - Margin * 2, data.Length * DayHeight),
            gridColor, 3D);


        //Dessin légende

        //Dessin tableau
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40),
            new Size(page.Width - Margin * 2, 300),
            gridColor, 5D);
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40),
            new Size((page.Width - Margin * 2) / 2, 300),
            gridColor, 5D);
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40),
            new Size((page.Width - Margin * 2) * 3 / 4, 300),
            gridColor, 5D);
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40),
            new Size((page.Width - Margin * 2) / 4, 300),
            gridColor, 5D);
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40),
            new Size((page.Width - Margin * 2) * 3 / 4, 300),
            gridColor, 5D);
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40),
            new Size((page.Width - Margin * 2) / 4, 300),
            gridColor, 5D);
        gpr.StrokeRectangle(new Point(Margin, MarginTop + data.Length * DayHeight + 40),
            new Size((page.Width - Margin * 2) / 4, 300),
            gridColor, 5D);

        //Lune
        var str = Language.Current.Translate("Moon");
        gpr.FillText(
            new Point(Margin + HourWidth * 3 - gpr.MeasureText(str, new Font(NimbusBold, 50)).Width / 2,
                MarginTop + data.Length * DayHeight + 60),
            str, new Font(NimbusBold, 50),
            gridColor);

        str = Language.Current.Translate("Culmination");
        gpr.FillText(
            new Point(Margin + HourWidth * 3 - gpr.MeasureText(str, new Font(NimbusRegular, 35)).Width / 2,
                MarginTop + data.Length * DayHeight + 130),
            str, new Font(NimbusRegular, 35),
            gridColor);

        str = Language.Current.Translate("Rise");
        gpr.FillText(new Point(Margin + 5, MarginTop + data.Length * DayHeight + 225),
            str, new Font(NimbusRegular, 35),
            gridColor);

        str = Language.Current.Translate("Set");
        gpr.FillText(
            new Point(Margin + HourWidth * 6 - gpr.MeasureText(str, new Font(NimbusRegular, 35)).Width - 15,
                MarginTop + data.Length * DayHeight + 225),
            str, new Font(NimbusRegular, 35),
            gridColor);

        gpr.FillRectangle(new Point(Margin + 30, MarginTop + data.Length * DayHeight + 180),
            new Size(HourWidth * 6 - 60, 32), new SolidColourBrush(Colours.LightGray));

        using (var image =
               new MagickImage(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                               "/Resources/MoonPhases/waning-crescent.png"))
        {
            List<byte> arr = new();
            foreach (var b in image.GetPixels())
            {
                foreach (var c in b.ToArray())
                {
                    arr.Add(c);
                }
            }

            gpr.DrawRasterImage(Margin + HourWidth * 3 - 32, MarginTop + data.Length * DayHeight + 164, 64.0, 64.0,
                new RasterImage(arr.ToArray(), image.Width, image.Height, PixelFormats.RGBA, true));
        }

        //Jour-Nuit

        gpr.FillRectangle(new Point(Margin + HourWidth * 6 + 80, MarginTop + data.Length * DayHeight + 80),
            new Size(80, 100), background);

        gpr.FillRectangle(new Point(Margin + HourWidth * 6 + 80, MarginTop + data.Length * DayHeight + 180),
            new Size(80, 100), nightFill);
        
        if(economicMode)
            gpr.FillRectangle(new Point(Margin + HourWidth * 6 + 80, MarginTop + data.Length * DayHeight + 180),
                new Size(80, 6), new SolidColourBrush(Colours.Black));

        str = Language.Current.Translate("Day");
        gpr.FillText(new Point(Margin + HourWidth * 6 + 180, MarginTop + data.Length * DayHeight + 110),
            str, new Font(NimbusRegular, 45),
            gridColor);

        str = Language.Current.Translate("Night");
        gpr.FillText(new Point(Margin + HourWidth * 6 + 180, MarginTop + data.Length * DayHeight + 210),
            str, new Font(NimbusRegular, 45),
            gridColor);

        //Planètes

        str = Language.Current.Translate("Planets");
        gpr.FillText(
            new Point(Margin + HourWidth * 15 - gpr.MeasureText(str, new Font(NimbusBold, 50)).Width / 2,
                MarginTop + data.Length * DayHeight + 60),
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
        gpr.StrokePath(path, gridColor, 4, LineCaps.Butt, LineJoins.Bevel, riseDash);
        path = new GraphicsPath();
        path.MoveTo(new Point(Margin + HourWidth * 15 + 10, MarginTop + data.Length * DayHeight + 200));
        path.LineTo(new Point(Margin + HourWidth * 18 - 20, MarginTop + data.Length * DayHeight + 200));
        gpr.StrokePath(path, gridColor, 4);
        path = new GraphicsPath();
        path.MoveTo(new Point(Margin + HourWidth * 15 + 10, MarginTop + data.Length * DayHeight + 260));
        path.LineTo(new Point(Margin + HourWidth * 18 - 20, MarginTop + data.Length * DayHeight + 260));
        gpr.StrokePath(path, gridColor, 4, LineCaps.Butt, LineJoins.Bevel, setDash);

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

        doc.Pages.Last().SaveAsSVG(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/alma.svg", SVGContextInterpreter.TextOptions.ConvertIntoPaths);

        // Read first frame of svg image
        using (var image = new MagickImage(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/alma.svg"))
        {
            // Save frame as png
            image.Write(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/alma.png");
        }
        

        var html = "<html><head><style>@page {margin: 0.5cm; }body{margin:0px;text-align:center}img{height:1100px}</style></head><body><img src=\"alma.png\"></body></html>";

        var props = new ConverterProperties();
        props.SetBaseUri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));


        HtmlConverter.ConvertToPdf(html, new PdfWriter(fileName), props);
    }

    private static int HourToMinutes(DateTime time)
    {
        return time.Hour * 60 + time.Minute;
    }


    private static GraphicsPath GetPlanetRisePath(int id, GlobalData[] data)
    {
        var path = new GraphicsPath();

        Point lastPoint = new(0, 0);
            
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i].ephemerides[id].rise_time is null)
            {
                continue;
            }

            var timeInt = data[i].ephemerides[id].rise_time!.Value.Hour * 60
                          + data[i].ephemerides[id].rise_time!.Value.Minute;

            var point = new Point(
                Margin + timeInt * HourWidth * 24 / 1440,
                MarginTop + i * DayHeight
            );

            
            if ((lastPoint.X - point.X)*(lastPoint.X - point.X) + (lastPoint.Y - point.Y)*(lastPoint.Y - point.Y) > 80000)
            {
                path.MoveTo(point);
            }
            else
            {
                path.LineTo(point);
                if (i == data.Length - 1)
                {
                    path.LineTo(new Point(
                        Margin + timeInt * HourWidth * 24 / 1440,
                        MarginTop + (i + 1) * DayHeight
                    ));
                }
            }
            
            lastPoint = point;
            
        }

        return path;
    }

    private static GraphicsPath GetPlanetCulminationPath(int id, GlobalData[] data)
    {
        var path = new GraphicsPath();

        Point lastPoint = new(0, 0);
            
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i].ephemerides[id].culmination_time is null)
            {
                continue;
            }

            var timeInt = data[i].ephemerides[id].culmination_time!.Value.Hour * 60
                          + data[i].ephemerides[id].culmination_time!.Value.Minute;

            var point = new Point(
                Margin + timeInt * HourWidth * 24 / 1440,
                MarginTop + i * DayHeight
            );
            
            if ((lastPoint.X - point.X)*(lastPoint.X - point.X) + (lastPoint.Y - point.Y)*(lastPoint.Y - point.Y) > 80000)
            {
                path.MoveTo(point);
            }
            else
            {
                path.LineTo(point);
                if (i == data.Length - 1)
                {
                    path.LineTo(new Point(
                        Margin + timeInt * HourWidth * 24 / 1440,
                        MarginTop + (i + 1) * DayHeight
                    ));
                }
            }
            
            lastPoint = point;

        }

        return path;
    }

    private static GraphicsPath GetPlanetSetPath(int id, GlobalData[] data)
    {
        var path = new GraphicsPath();

        Point lastPoint = new(0, 0);
            
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i].ephemerides[id].set_time is null)
            {
                continue;
            }

            var timeInt = data[i].ephemerides[id].set_time!.Value.Hour * 60
                          + data[i].ephemerides[id].set_time!.Value.Minute;

            var point = new Point(
                Margin + timeInt * HourWidth * 24 / 1440,
                MarginTop + i * DayHeight
            );
            
            if ((lastPoint.X - point.X)*(lastPoint.X - point.X) + (lastPoint.Y - point.Y)*(lastPoint.Y - point.Y) > 80000)
            {
                path.MoveTo(point);
            }
            else
            {
                path.LineTo(point);
                if (i == data.Length - 1)
                {
                    path.LineTo(new Point(
                        Margin + timeInt * HourWidth * 24 / 1440,
                        MarginTop + (i + 1) * DayHeight
                    ));
                }
            }
            
            lastPoint = point;

            
        }

        return path;
    }
}