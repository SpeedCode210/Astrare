using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Astrare.Models;

namespace Astrare.KosmorroConnection;

public static class KosmorroConnector
{
    public static GlobalData GetFromKosmorro(DateTime? date, decimal lat, decimal lon, int timezone)
    {
        date ??= DateTime.Now;
        var cmd = ("cd {/d} " + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + $"/kosmorro-lite && {PythonHelper.GetPythonCommand()} kosmorro-lite -lat {lat.ToString(CultureInfo.InvariantCulture).Replace(',', '.')} -lon {lon.ToString(CultureInfo.InvariantCulture).Replace(',', '.')} -d " +
            ((DateTime)date).ToString("yyyy-MM-dd") +
            " -t " + timezone);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            cmd = cmd.Replace("{/d}", "/d");
        }
        else
        {
            cmd = cmd.Replace(" {/d}", "");
        }

        Debug.WriteLine(cmd);
        var json = ShellHelper.Bash(cmd).Replace("\"object\"", "\"aster\"");
        Debug.WriteLine(json);
        try
        {
            
            var result = JsonSerializer.Deserialize<GlobalData>(json, new JsonSerializerOptions()
            {
                Converters = {
                    new JsonStringEnumConverter()
                }
            });
            
            if (result is null)
                throw new Exception("Can't get data from Kosmorro.");

            List<AsterEphemerides> toCorrect = new();

            foreach (var eph in result.ephemerides)
            {
                if (eph.rise_time is null || eph.set_time is null)
                {
                    toCorrect.Add(eph);
                }
            }
            
            if(toCorrect.Count == 0)
                return result;

            var result2 = GetFromKosmorro(date, lat, lon, timezone - 1);
            
            foreach (var eph in result2.ephemerides)
            {
                foreach (var eph2 in toCorrect)
                {
                    if (eph2.aster.identifier == eph.aster.identifier)
                    {
                        if (eph2.rise_time is null && eph.rise_time != null && eph.rise_time!.Value.AddHours(1).Day == date!.Value.Day)
                        {
                            eph2.rise_time = eph.rise_time!.Value.AddHours(1);
                        }
                        if (eph2.set_time is null && eph.set_time != null && eph.set_time!.Value.AddHours(1).Day == date!.Value.Day)
                        {
                            eph2.set_time = eph.set_time!.Value.AddHours(1);
                        }
                    }
                }
            }

            return result;

        }
        catch (JsonException)
        {
            throw new Exception("Can't parse data from Kosmorro, please see the console for more information.");
        }
    }
}