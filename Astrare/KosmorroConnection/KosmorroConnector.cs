using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Astrare.Models;

namespace Astrare.KosmorroConnection;

public static class KosmorroConnector
{
    public static GlobalData GetFromKosmorro(DateTime? date, decimal lat, decimal lon, int timezone, DataGetMode mode = DataGetMode.Default)
    {
        date ??= DateTime.Now;
        var cmd = ("cd {/d} " + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + $"/kosmorro-lite && {PythonHelper.GetPythonCommand()} kosmorro-lite -lat {lat.ToString(CultureInfo.InvariantCulture).Replace(',', '.')} -lon {lon.ToString(CultureInfo.InvariantCulture).Replace(',', '.')} -d " +
            ((DateTime)date).ToString("yyyy-MM-dd") +
            " -t " + timezone);
        switch (mode)
        {
            case DataGetMode.Events:
                cmd += " -eph -co";
                break;
            case DataGetMode.Almanach:
                cmd += " -ev -co";
                break;
        }
            

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            cmd = cmd.Replace("{/d}", "/d");
        }
        else
        {
            cmd = cmd.Replace(" {/d}", "");
        }

        var json = ShellHelper.Bash(cmd).Replace("\"object\"", "\"aster\"");
        try
        {
            
            var result = JsonSerializer.Deserialize<GlobalData[]>(json, new JsonSerializerOptions()
            {
                Converters = {
                    new JsonStringEnumConverter()
                }
            });
            
            if (result is null || result.Length < 1)
                throw new Exception("Can't get data from Kosmorro.");


            if (true)
            {
                foreach (var ev in result[0].events)
                {
                    if(ev.details != null)
                        ev.details["north_hemispher"] = lat >= 0;
                }
                foreach (var eph in result[0].ephemerides)
                {
                    if (!(eph.aster.identifier is ObjectIdentifier.SUN or ObjectIdentifier.MOON))
                        continue;
                    if (eph.rise_time.GetValueOrDefault().Date.DayOfYear != date.GetValueOrDefault().DayOfYear)
                    {
                        eph.rise_time = null;
                    }
                
                    if (eph.set_time.GetValueOrDefault().Date.DayOfYear != date.GetValueOrDefault().DayOfYear)
                    {
                        eph.set_time = null;
                    }
                }
            }
            

            return result[0];

        }
        catch (JsonException)
        {
            throw new Exception("Can't parse data from Kosmorro, please see the console for more information.");
        }
    }
}

public enum DataGetMode{
    Default,
    Events,
    Almanach
}