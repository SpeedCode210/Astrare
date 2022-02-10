using System;

namespace Astrare.Models;

public class AsterEphemerides
{
    public Object aster { get; set; }
    public DateTime? rise_time { get; set; }
    public DateTime? culmination_time { get; set; }
    public DateTime? set_time { get; set; }
}