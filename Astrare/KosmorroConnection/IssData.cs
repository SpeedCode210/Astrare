using System;
using Astrare.Translate;

namespace Astrare.KosmorroConnection;

public class IssData
{
    public IssTimeData rise { get; set; }
    public IssTimeData culmination { get; set; }
    public IssTimeData set { get; set; }

    public string DisplayRiseTime => rise.time.ToString("HH:mm:ss");
    public string DisplayCulminationTime => culmination.time.ToString("HH:mm:ss");
    public string DisplaySetTime => set.time.ToString("HH:mm:ss");

    public bool occulted { get; set; }

    public string VisibilityString
    {
        get
        {
            if (occulted)
                return Language.Current.Translate("Occulted");
            if (culmination.time < MainWindow.CurrentData.ephemerides[0].set_time.Value &&
                culmination.time > MainWindow.CurrentData.ephemerides[0].rise_time.Value)
                return Language.Current.Translate("Daylight");
            return Language.Current.Translate("Visible");
        }
    }
}

public class IssTimeData
{
    public double ra { get; set; }
    public double dec { get; set; }
    public DateTime time { get; set; }
}