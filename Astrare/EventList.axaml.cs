using System;
using System.Collections.Generic;
using System.Linq;
using Astrare.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Markup.Xaml;
using iText.Commons.Utils;
using Microsoft.VisualBasic;

namespace Astrare;

public partial class EventList : Window
{
    
    private Dictionary<CheckBox, EventTypes> _events = new();

    public EventList()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }
    

    public EventList(bool showMonths, string title, Action callback)
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        Title = "Astrare - " + title;
        Month.Items = new []{1,2,3,4,5,6,7,8,9,10,11,12};
        Month.SelectedIndex = 0;
        Year.Value = DateTime.Now.Year;

        _events = new Dictionary<CheckBox, EventTypes>()
        {
            {Apogees, EventTypes.APOGEE},
            {Perigee, EventTypes.PERIGEE},
            {Oppositions, EventTypes.OPPOSITION},
            {Conjunctions, EventTypes.CONJUNCTION},
            {Occultations, EventTypes.OCCULTATION},
            {MaxElong, EventTypes.MAXIMAL_ELONGATION},
            {Seasons, EventTypes.SEASON_CHANGE},
            {MoonEclipses, EventTypes.LUNAR_ECLIPSE},
            {MoonPhases, EventTypes.MOON_PHASE},
        };

        if (!showMonths) Month.IsVisible = false;
        
        Generate.Click += (_, _) => callback.Invoke();
    }

    public EventTypes[] GetEvents()
    {
        return (from keyVar in _events where keyVar.Key.IsChecked != null && (bool) keyVar.Key.IsChecked select keyVar.Value).ToArray();
    }

    public DateTime GetMonthAndYear()
    {
        return new((int)Year.Value, (int)Month.SelectedItem!, 1);
    }

    public bool ShowPlanetIcons()
    {
        return (bool)PlanetIcons.IsChecked!;
    }

    public double GetFontSize()
    {
        return TextSize.Value;
    }

}