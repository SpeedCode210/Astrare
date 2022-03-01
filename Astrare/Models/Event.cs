using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Astrare.Translate;

namespace Astrare.Models;

public class Event
{
    public EventTypes EventType { get; set; }
    public Object[] objects { get; set; }
    public DateTime starts_at { get; set; }
    public DateTime? ends_at { get; set; }
    public Dictionary<string, object>? details { get; set; }

    public override string ToString()
    {
        switch (EventType)
        {
            case EventTypes.APOGEE:
                return Language.Current.Translate("{0} is at its apogee ({1} Km)", objects[0].ToString(), Math.Round(((JsonElement)details["distance_km"]).GetDouble()).ToString());
            case EventTypes.PERIGEE:
                return Language.Current.Translate("{0} is at its perigee ({1} Km)", objects[0].ToString(), Math.Round(((JsonElement)details["distance_km"]).GetDouble()).ToString());
            case EventTypes.OPPOSITION:
                return Language.Current.Translate("{0} is in opposition", objects[0].ToString());
            case EventTypes.CONJUNCTION:
                return Language.Current.Translate("{0} and {1} are in conjunction", objects[0].ToString(), objects[1].ToString());            
            case EventTypes.OCCULTATION:
                return Language.Current.Translate("{0} occults {1}", objects[0].ToString(), objects[1].ToString());
            case EventTypes.MAXIMAL_ELONGATION:
                return Language.Current.Translate("Elongation of {0} is maximal ({1}°)", objects[0].ToString(), Math.Round(((JsonElement)details["deg"]).GetDouble()).ToString());
            case EventTypes.MOON_PHASE:
                try
                {
                    return MoonPhase.TypeToString(((MoonPhase) details["phase"]).phase);
                }
                catch
                {
                    return MoonPhase.TypeToString((MoonPhaseType)((JsonElement) details["phase"]).GetProperty("phase").GetInt32());
                }
            //TODO : Intégrer ça
            case EventTypes.SEASON_CHANGE:
                switch (Enum.Parse<SeasonType>(((JsonElement)details["season"]).GetString()))
                {
                    case SeasonType.JUNE_SOLSTICE:
                        return Language.Current.Translate((bool)details["north_hemispher"] ? "Summer solstice" : "Winter solstice");
                    case SeasonType.MARCH_EQUINOX:
                        return Language.Current.Translate((bool)details["north_hemispher"] ? "Spring equinox" : "Autumn equinox");
                    case SeasonType.DECEMBER_SOLSTICE:
                        return Language.Current.Translate(!(bool)details["north_hemispher"] ? "Summer solstice" : "Winter solstice");
                    case SeasonType.SEPTEMBER_EQUINOX:
                        return Language.Current.Translate(!(bool)details["north_hemispher"] ? "Spring equinox" : "Autumn equinox");
                    default:
                        return "Unknown event";
                }
            case EventTypes.LUNAR_ECLIPSE:
                switch (Enum.Parse<LunarEclipseType>(((JsonElement)details["type"]).GetString()))
                {
                    case LunarEclipseType.TOTAL:
                        return Language.Current.Translate("Total lunar eclipse - Maximum : {0}",
                            ((JsonElement) details["maximum"]).GetDateTime().ToString("hh:mm"));
                    case LunarEclipseType.PARTIAL:
                        return Language.Current.Translate("Partial lunar eclipse - Maximum : {0}",
                            ((JsonElement) details["maximum"]).GetDateTime().ToString("hh:mm"));
                    case LunarEclipseType.PENUMBRAL:
                        return Language.Current.Translate("Penumbral lunar eclipse - Maximum : {0}",
                            ((JsonElement) details["maximum"]).GetDateTime().ToString("hh:mm"));
                    default:
                        return "Unknown event";
                }
                break;
            default:
                return Language.Current.Translate("Unknown event");
        }
    }
}