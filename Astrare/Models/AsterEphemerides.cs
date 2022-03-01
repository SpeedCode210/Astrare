using System;
using System.Text.Json;

namespace Astrare.Models;

public class AsterEphemerides
{
    public Object aster { get; set; }
    public DateTime? rise_time { get; set; }
    public DateTime? culmination_time { get; set; }
    public DateTime? set_time { get; set; }


    private const double Epsilon = 23.4364122222222222;

    public bool IsVisible(AsterEphemerides sun)
    {
        if (rise_time != null || rise_time != null)
            return true;
        double date = DateTime.Today.DayOfYear;
        if (culmination_time != null)
            date = culmination_time.Value.DayOfYear;
        else if (rise_time != null)
            date = rise_time!.Value.DayOfYear;
        else if (set_time != null)
            date = set_time!.Value.DayOfYear;

        double a;
        if (aster.identifier == ObjectIdentifier.SUN)
        {
            a =  date * 360.0/365.0 < 355 ? (date+10)* 360.0/365.0 : (date - 355)* 360.0/365.0;

            a %= 360;

            switch (a)
            {
                case 0:
                    a += Math.Pow(10, -10);
                    break;
                case 180:
                    a -= Math.Pow(10, -10);
                    break;
            }
        }
        else
        {
            if (sun.culmination_time is null || culmination_time is null)
                return false;

            double sunTime = sun.culmination_time!.Value.Hour * 60 + sun.culmination_time!.Value.Minute;
            double asterTime = culmination_time!.Value.Hour * 60 + culmination_time!.Value.Minute;
            
            a =  date * 360.0/365.0 < 355 ? (date+10)* 360.0/365.0 : (date - 355)* 360.0/365.0;

            if (asterTime > a)
                a += (asterTime - sunTime) * 360 / 1440;
            else if (asterTime < a)
                a -= (sunTime - asterTime) * 360 / 1440;

            a %= 360;

            switch (a)
            {
                case 0:
                    a += Math.Pow(10, -10);
                    break;
                case 180:
                    a -= Math.Pow(10, -10);
                    break;
            }
        }
            
        var (b,b2) = IsEclipticAsterVisible((double)Settings.Current.Latitude, a);

        return b;
    }
    
    (bool, bool) IsEclipticAsterVisible(double latitude, double alpha)
    {
        if (latitude < 0)
        {
            latitude = -latitude;
            alpha += 180;
        }
        var h = GetMaxDeclinaisonRepresentation(latitude);
        var d = GetEclipticAsterDeclinaisonRepresentation(alpha);

        return (d < h, Math.Abs(d-h) < 0.005);
    }

    double GetEclipticAsterDeclinaisonRepresentation(double alpha)
    {
        var (sinE, cosE) = Math.SinCos(ConvertToRadians(Epsilon));

        var s = (cosE * cosE) / (2 * Math.Pow(1 + sinE, 2));
        var m = 0.5 - s;
        var r = 0.5 + s;

        var alphaRad = ConvertToRadians(alpha);

        var d =
            (r * Math.Sin(Math.PI - alphaRad - Math.Asin(Math.Sin(alphaRad) * m / r)))
            /
            (Math.Sin(alphaRad));
    
        return d;
    }

    double GetMaxDeclinaisonRepresentation(double latitude)
    {
        var (sinE, cosE) = Math.SinCos(ConvertToRadians(Epsilon));
        var h = (Math.Tan(ConvertToRadians((180 - latitude) / 2)) * cosE) / (sinE + 1);

        return h;
    }

    double ConvertToRadians(double angle)
    {
        return (Math.PI / 180) * angle;
    }
    
    
}

public static class EphsExtension
{
    public static AsterEphemerides Clone(this AsterEphemerides e)
    {
        return new()
        {
            aster = e.aster,
            culmination_time = e.culmination_time,
            rise_time = e.rise_time,
            set_time = e.set_time
        };
    }

    public static AsterEphemerides[] CloneElements(this AsterEphemerides[] e)
    {
        var r = e.Clone() as AsterEphemerides[];
        for (int i = 0; i < r.Length; i++)
        {
            r[i] = r[i].Clone();
        }

        return r;
    }
}