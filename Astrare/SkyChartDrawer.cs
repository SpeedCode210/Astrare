using System;
using VectSharp;
using System.IO;
using System.Linq;
using System.Text.Json;
using Astrare.Models;
using ImageMagick;
using VectSharp.SVG;

namespace Astrare;

public static class SkyChartDrawer
{
    private static readonly FontFamily NimbusRegular =
        new(File.OpenRead(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                          @"/Resources/NimbusSanL-Reg.ttf"));

    public static void SaveSVG(AsterPosition[] positions,DateTime date, TimeSpan hour, int utcPlusX, bool dark = false, bool png = true)
    {
        var background = dark ? Colour.FromRgb(16, 12, 12) : Colours.White;
        var foreground = dark ? Colours.White : Colours.Black;

        var epsilon = 23.45;
// Jour de l'année
        var d = date.DayOfYear;
// Longitude et Latitude
        double longitude = (double) Settings.Current.Longitude;
        double latitude = (double) Settings.Current.Latitude;
// UTC + u
        var u = utcPlusX;
// Heure civile
        var h = hour.TotalMinutes / 60.0;
// Rayon horizon
        var rh = 1050;

        bool nord = false;
        if (latitude >= 0)
        {
            nord = true;
        }
        else
        {
            latitude = -latitude;
        }

// Rayon cercle tropique
        var rt =
            rh
            * (2 + 2 * Math.Sin(epsilon * Math.PI / 180.0))
            / (
                Math.Cos(epsilon * Math.PI / 180)
                * (Math.Tan((180.0 - latitude) / 2.0 * Math.PI / 180.0) + Math.Tan((latitude / 2.0) * Math.PI / 180.0))
            );


// Rayon cercle équateur
        var r = (Math.Cos(epsilon * Math.PI / 180.0) * rt) / (1 + Math.Sin(epsilon * Math.PI / 180.0));


// Équation du temps
        var M = 357.5291 + 0.98560028 * d;
        var C = 1.9148 * Math.Sin((M) * Math.PI / 180.0) + 0.02 * Math.Sin((2 * M) * Math.PI / 180.0) +
                0.0003 * Math.Sin((3 * M) * Math.PI / 180.0);
        var L = 280.4665 + 0.98564736 * d + C;
        var R = -2.46569 * Math.Sin((2 * L) * Math.PI / 180.0)
                + 0.053 * Math.Sin((4 * L) * Math.PI / 180.0)
                - 0.0014 * Math.Sin((6 * L) * Math.PI / 180.0);

        var deltaTime = C + R;

// Asc.droite soleil
        var alphaSun = (d + 10) * 360.0 / 365.0 - 270;

        var doc = new Document();
        var page = new Page(2400, 2400);
        doc.Pages.Add(page);
        page.Background = background;
        var gpr = page.Graphics;


        var json = File.ReadAllText(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                                    "/Resources/data.json");
        var data = JsonSerializer.Deserialize<StarData[][]>(json);
        //Etoiles
        foreach (var line in data)
        {
            if (!nord)
            {
                line[0].dec = -line[0].dec;
                line[1].dec = -line[1].dec;
            }

            draw_line(line[0].ra, line[0].dec, line[1].ra, line[1].dec);
            draw_star(line[0].ra, line[0].dec, line[0].magnitude);
            draw_star(line[1].ra, line[1].dec, line[1].magnitude);
        }
        //Astres kosmorro
        foreach(var p in positions)
            if(p.aster is ObjectIdentifier.SUN)
                draw_star(p.right_ascension, p.declinaison, -7, Colours.Yellow);
            else if(p.aster is ObjectIdentifier.MOON)
                draw_star(p.right_ascension, p.declinaison, -5, Colours.Gray);
            else if(p.aster is ObjectIdentifier.VENUS)
                draw_star(p.right_ascension, p.declinaison, -4, Colours.Beige);
            else if(p.aster is ObjectIdentifier.MERCURY)
                draw_star(p.right_ascension, p.declinaison, -0.5, Colours.Gray);
            else if(p.aster is ObjectIdentifier.MARS)
                draw_star(p.right_ascension, p.declinaison, -1.5, Colours.Red);
            else if(p.aster is ObjectIdentifier.JUPITER)
                draw_star(p.right_ascension, p.declinaison, -2, Colours.Orange);
            else if(p.aster is ObjectIdentifier.SATURN)
                draw_star(p.right_ascension, p.declinaison, -1.5, Colours.LimeGreen);

//Masque horizon
        var path = new GraphicsPath();
        path.Arc(new Point(1200, 1200), rh, 0, Math.PI / 2);
        path.LineTo(new Point(1200, 2400));
        path.LineTo(new Point(2400, 2400));
        path.LineTo(new Point(2400, 1200));
        gpr.FillPath(path, new SolidColourBrush(background));
        path = new GraphicsPath();
        path.Arc(new Point(1200, 1200), rh, Math.PI / 2, Math.PI);
        path.LineTo(new Point(0, 1200));
        path.LineTo(new Point(0, 2400));
        path.LineTo(new Point(1200, 2400));
        gpr.FillPath(path, new SolidColourBrush(background));
        path = new GraphicsPath();
        path.Arc(new Point(1200, 1200), rh, Math.PI, Math.PI * 3 / 2);
        path.LineTo(new Point(1200, 0));
        path.LineTo(new Point(0, 0));
        path.LineTo(new Point(0, 1200));
        gpr.FillPath(path, new SolidColourBrush(background));
        path = new GraphicsPath();
        path.Arc(new Point(1200, 1200), rh, Math.PI * 3 / 2, Math.PI * 2);
        path.LineTo(new Point(2400, 1200));
        path.LineTo(new Point(2400, 0));
        path.LineTo(new Point(1200, 0));
        gpr.FillPath(path, new SolidColourBrush(background));
// Cerle horizon
        path = new GraphicsPath();
        path.Arc(new Point(1200, 1200), rh, 0, Math.PI * 2);
        gpr.StrokePath(path, new SolidColourBrush(foreground), 5D);


// Zenith
        var yZ = -(
                     (
                         Math.Cos((epsilon) * Math.PI / 180)
                         * rt
                         * (Math.Tan(((180.0 - latitude) / 2) * Math.PI / 180) -
                            Math.Tan((latitude / 2.0) * Math.PI / 180))
                     )
                     / (2 + 2 * Math.Sin((epsilon) * Math.PI / 180))
                 )
                 + r * Math.Tan(((90.0 - latitude) / 2) * Math.PI / 180);


        path = new GraphicsPath();
        path.Arc(new Point(1200,
            2400 - (yZ + 1200)
        ), 10, 0, Math.PI * 2);
//Ouest
        var ouest = new Point(-Math.Sqrt(
                Math.Pow(rh, 2)
                - Math.Pow(
                    (
                        Math.Cos(Math.PI / 180 * (epsilon))
                        * rt
                        * (Math.Tan(Math.PI / 180 * ((180.0 - latitude) / 2)) -
                           Math.Tan(Math.PI / 180 * (latitude / 2.0)))
                    )
                    / (2 + 2 * Math.Sin(Math.PI / 180 * (epsilon)))
                    , 2)
            ) + 1200,
            (
                Math.Cos(Math.PI / 180 * (epsilon))
                * rt
                * (Math.Tan(Math.PI / 180 * ((180.0 - latitude) / 2)) - Math.Tan(Math.PI / 180 * (latitude / 2.0)))
            )
            / (2 + 2 * Math.Sin(Math.PI / 180 * (epsilon))) + 1200
        );

        path.MoveTo(new Point(ouest.X - 15, ouest.Y + 15));
        path.LineTo(new Point(ouest.X + 15, ouest.Y - 15));
        path.MoveTo(new Point(2400 - ouest.X + 15, ouest.Y + 15));
        path.LineTo(new Point(2400 - ouest.X - 15, ouest.Y - 15));
        gpr.StrokePath(path, new SolidColourBrush(foreground), 5D);
        var font = new Font(NimbusRegular, 60);
        if (nord)
        {
            gpr.FillText(new Point(1200 - gpr.MeasureText("S", font).Width / 2, 60 + (1100 - rh)), "S", font,
                new SolidColourBrush(foreground), TextBaselines.Middle);
            gpr.FillText(new Point(1200 - gpr.MeasureText("N", font).Width / 2, 2340 - (1100 - rh)), "N", font,
                new SolidColourBrush(foreground), TextBaselines.Middle);
            gpr.FillText(new Point(ouest.X - 45 - gpr.MeasureText("O", font).Width / 2, ouest.Y + 45), "O", font,
                new SolidColourBrush(foreground), TextBaselines.Middle);
            gpr.FillText(new Point(2400 - ouest.X + 45 - gpr.MeasureText("E", font).Width / 2, ouest.Y + 45), "E", font,
                new SolidColourBrush(foreground), TextBaselines.Middle);
        }
        else
        {
            gpr.FillText(new Point(1200 - gpr.MeasureText("N", font).Width / 2, 60 + (1100 - rh)), "N", font,
                new SolidColourBrush(foreground), TextBaselines.Middle);
            gpr.FillText(new Point(1200 - gpr.MeasureText("S", font).Width / 2, 2340 - (1100 - rh)), "S", font,
                new SolidColourBrush(foreground), TextBaselines.Middle);
            gpr.FillText(new Point(ouest.X - 45 - gpr.MeasureText("E", font).Width / 2, ouest.Y + 45), "E", font,
                new SolidColourBrush(foreground), TextBaselines.Middle);
            gpr.FillText(new Point(2400 - ouest.X + 45 - gpr.MeasureText("O", font).Width / 2, ouest.Y + 45), "O", font,
                new SolidColourBrush(foreground), TextBaselines.Middle);
        }


        void draw_star(double alphaAster, double deltaAster, double magnitude, Colour? colour = null)
        {
            if (colour is null)
                colour = foreground;
            
            var l = (r * Math.Cos(deltaAster * Math.PI / 180)) / (1 + Math.Sin(deltaAster * Math.PI / 180));
            var theta = (
                alphaAster
                - alphaSun
                - 270
                + 4 * longitude
                - h * 360.0 / 24
                - u * 360.0 / 24
                - deltaTime
            );
            // Coordonnées cartésiennes (rectangulaires)
            var x = l * Math.Cos(theta * Math.PI / 180);
            var y = l * Math.Sin(theta * Math.PI / 180) - (
                Math.Cos(epsilon * Math.PI / 180)
                * rt
                * (Math.Tan((180.0 - latitude) / 2 * Math.PI / 180) - Math.Tan(latitude / 2.0 * Math.PI / 180))
            ) / (2 + 2 * Math.Sin(epsilon * Math.PI / 180));
            if (x * x + y * y <= rh * rh)
            {
                if (nord)
                {
                    x = 1200 - x;
                }
                else
                {
                    x = 1200 + x;
                }

                path = new GraphicsPath();
                path.Arc(new Point(x, 1200 - y), 9 * Math.Pow(1.212, -magnitude), 0, Math.PI * 2);
                gpr.FillPath(path, new SolidColourBrush(colour!.Value));
                if(!dark && colour!.Value.ToCSSString(true) != foreground.ToCSSString(true))
                    gpr.StrokePath(path, foreground,1.5);
            }
        }

        void draw_line(double alphaAster, double deltaAster, double alphaAster2, double deltaAster2)
        {
            var l = (r * Math.Cos(deltaAster * Math.PI / 180)) / (1 + Math.Sin(deltaAster * Math.PI / 180));
            var theta = (
                alphaAster
                - alphaSun
                - 270
                + 4 * longitude
                - h * 360.0 / 24
                - u * 360.0 / 24
                - deltaTime
            );
            // Coordonnées cartésiennes (rectangulaires)
            var x = l * Math.Cos(theta * Math.PI / 180);
            var y = l * Math.Sin(theta * Math.PI / 180) - (
                Math.Cos(epsilon * Math.PI / 180)
                * rt
                * (Math.Tan((180.0 - latitude) / 2 * Math.PI / 180) - Math.Tan(latitude / 2.0 * Math.PI / 180))
            ) / (2 + 2 * Math.Sin(epsilon * Math.PI / 180));

            var l2 = (r * Math.Cos(deltaAster2 * Math.PI / 180)) / (1 + Math.Sin(deltaAster2 * Math.PI / 180));
            var theta2 = (
                alphaAster2
                - alphaSun
                - 270
                + 4 * longitude
                - h * 360.0 / 24
                - u * 360.0 / 24
                - deltaTime
            );
            // Coordonnées cartésiennes (rectangulaires)
            var x2 = l2 * Math.Cos(theta2 * Math.PI / 180);
            var y2 = l2 * Math.Sin(theta2 * Math.PI / 180) - (
                Math.Cos(epsilon * Math.PI / 180)
                * rt
                * (Math.Tan((180.0 - latitude) / 2 * Math.PI / 180) - Math.Tan(latitude / 2.0 * Math.PI / 180))
            ) / (2 + 2 * Math.Sin(epsilon * Math.PI / 180));

            if (x * x + y * y <= rh * rh || x2 * x2 + y2 * y2 <= rh * rh)
            {
                if (nord)
                {
                    x = 1200 - x;
                    x2 = 1200 - x2;
                }
                else
                {
                    x = 1200 + x;
                    x2 = 1200 + x2;
                }

                path = new GraphicsPath();
                path.MoveTo(x, 1200 - y);
                path.LineTo(x2, 1200 - y2);
                gpr.StrokePath(path, new SolidColourBrush(foreground), 2D);
            }
        }

        doc.Pages.Last()
            .SaveAsSVG(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/skychart.svg",
                SVGContextInterpreter.TextOptions.ConvertIntoPaths);
        
        if (png)
            using (var image =
                   new MagickImage(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                                   "/skychart.svg"))
            {
                image.Write(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                            "/skychart.png");
            }
    }
}