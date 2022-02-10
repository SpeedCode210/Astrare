using System;
using Astrare.Translate;

namespace Astrare.Models;

public class MoonPhase
{

    public MoonPhaseType phase { get; set; }
    public DateTime? time { get; set; }
    public MoonPhase? next { get; set; }

    public static string TypeToString(MoonPhaseType t)
    {
        switch (t)
        {
            case MoonPhaseType.NEW_MOON:
                return Language.Current.Translate("New moon");
            case MoonPhaseType.FULL_MOON:
                return Language.Current.Translate( "Full moon");
            case MoonPhaseType.LAST_QUARTER:
                return Language.Current.Translate( "Last quarter");
            case MoonPhaseType.FIRST_QUARTER:
                return Language.Current.Translate( "First quarter");
            case MoonPhaseType.WANING_GIBBOUS:
                return Language.Current.Translate( "Waning gibbous");
            case MoonPhaseType.WAXING_GIBBOUS:
                return Language.Current.Translate( "Waxing gibbous");
            case MoonPhaseType.WANING_CRESCENT:
                return Language.Current.Translate( "Waning crescent");
            case MoonPhaseType.WAXING_CRESCENT:
                return Language.Current.Translate( "Waxing crescent");
        }
        return String.Empty;
    }
}