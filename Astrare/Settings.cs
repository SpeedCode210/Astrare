using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Astrare;

public class Settings
{
    public decimal Latitude { get; set; } = 360;
    public decimal Longitude { get; set; } = 360;
    
    public int Theme { get; set; } = 1;

    public string Language { get; set; } = "English";

    private static Settings? _current;

    public static Settings Current
    {
        get
        {
            if (_current != null)
                return _current;
            string filename;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                filename = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                           + "\\Astrare\\settings.json";
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                                          + "\\Astrare");
            }
            else
            {
                filename = Environment.GetEnvironmentVariable("HOME") + "/.astrare_settings";

            }
            if (File.Exists(filename))
            {
                var result = JsonSerializer.Deserialize<Settings>(File.ReadAllText(filename));
                if (result != null)
                {
                    _current = result;
                    return result;
                }
            }
            _current = new();
            return _current;
        }
    }

    public void Save()
    {
        string filename;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            filename = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                       + "\\Astrare\\settings.json";
        }
        else
        {
            filename = Environment.GetEnvironmentVariable("HOME") + "/.astrare_settings";

        }
        File.WriteAllText(filename,JsonSerializer.Serialize(this));
    }
}
