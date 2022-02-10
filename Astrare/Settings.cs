using System;
using System.IO;
using System.Text.Json;

namespace Astrare;

public class Settings
{
    public decimal Latitude { get; set; } = 360;
    public decimal Longitude { get; set; } = 360;

    public string Language { get; set; } = "English";

    private static Settings? _current;

    public static Settings Current
    {
        get
        {
            if (_current != null)
                return _current;
            if (File.Exists(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/settings.json"))
            {
                var result = JsonSerializer.Deserialize<Settings>(File.ReadAllText(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/settings.json"));
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
        File.WriteAllText(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/settings.json",JsonSerializer.Serialize(this));
    }
}

public enum Languages
{
    English
}