using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Astrare.Translate;

public class Language : Attribute
{
    public string Name { get; protected set; } = "";

    protected Dictionary<string, string> Translations = new();

    public string[] Months { get; protected set; } 
    

    public virtual string Translate(string source, params string[] args)
    {
        foreach (var trsl in Translations)
        {
            if (source.Contains(trsl.Key))
            {
                source = source.Replace(trsl.Key, trsl.Value);
            }
        }

        for (int i =0; i < args.Length; i++)
        {
            source = source.Replace("{" + i + "}",  args[i]);
        }

        return source;
    }

    public virtual string Translate(DateTime date, DateTranslateMode mode)
    {
        switch (mode)
        {
            case DateTranslateMode.Date:
                return $"{Months[date.Month-1]} {date.Day}, {date.Year}";
            case DateTranslateMode.Hour:
                return $"{date.Hour}:{date.Minute}";
            case DateTranslateMode.DateHour:
                return Translate(date, DateTranslateMode.Date) + " at " + Translate(date, DateTranslateMode.Hour);
        }

        return "";
    }

    public enum DateTranslateMode
    {
        Date,
        Hour,
        DateHour
    }

    public static Language Current;

    public static Language[] Languages;

    static Language()
    {
        Languages = Array.Empty<Language>();
        Current = new English();

        string current = Settings.Current.Language;

        List<Language> result = new();
        
        var types = Assembly.GetEntryAssembly()?.GetTypes().Where(t => t.IsDefined(typeof(Language)));

        if (types is null)
            return;
        foreach (var t in types)
        {
            var a = (Language?) Activator.CreateInstance(t);
            if (a is not null)
            {
                result.Add(a);
                if (a.Name == current)
                    Current = a;
            }
        }

        Languages = result.ToArray();



    }

    public override string ToString()
    {
        return Name;
    }
}