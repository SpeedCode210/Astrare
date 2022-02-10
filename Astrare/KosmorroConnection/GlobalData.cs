using Astrare.Models;

namespace Astrare.KosmorroConnection;

public class GlobalData
{
    public AsterEphemerides[] ephemerides { get; set; }
    public MoonPhase moon_phase { get; set; }
    public Event[] events { get; set; }
}