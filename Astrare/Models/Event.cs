using System;
using System.Collections.Generic;
using System.Text.Json;
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
            /*case EventTypes.SEASON_CHANGE:
                break;
            case EventTypes.LUNAR_ECLIPSE:
                break;*/
            default:
                return Language.Current.Translate("Unknown event");
        }
    }
}