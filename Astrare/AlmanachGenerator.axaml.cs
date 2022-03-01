using System;
using System.Collections.Generic;
using System.Linq;
using Astrare.Models;
using Astrare.Translate;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Markup.Xaml;
using iText.Commons.Utils;
using Microsoft.VisualBasic;

namespace Astrare;

public partial class AlmanachGenerator : Window
{
    
    public AlmanachGenerator()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }
    

    public AlmanachGenerator(Action callback)
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        Title = "Astrare - " + Language.Current.Translate("Year graphic almanach");

        Generate.Click += (_, _) => callback.Invoke();
    }
    
    public DateTime GetYearDate()
    {
        return new((int)Year.Value, 1, 1);
    }

    public bool GetIfMoonEphemerides()
    {
        return (bool)MoonEphs.IsChecked!;
    }
    
    public bool GetIfDayNight()
    {
        return (bool)DayNight.IsChecked!;
    }
    
    public bool GetIfEconomic()
    {
        return (bool)EcoMode.IsChecked!;
    }

    public bool[,] PlanetsLines()
    {
        return new [,]
        {
            { RiseMercury.IsChecked!.Value, CulminationMercury.IsChecked!.Value, SetMercury.IsChecked!.Value },
            { RiseVenus.IsChecked!.Value, CulminationVenus.IsChecked!.Value, SetVenus.IsChecked!.Value },
            { RiseMars.IsChecked!.Value, CulminationMars.IsChecked!.Value, SetMars.IsChecked!.Value },
            { RiseJupiter.IsChecked!.Value, CulminationJupiter.IsChecked!.Value, SetJupiter.IsChecked!.Value },
            { RiseSaturn.IsChecked!.Value, CulminationSaturn.IsChecked!.Value, SetSaturn.IsChecked!.Value },
            { RiseUranus.IsChecked!.Value, CulminationUranus.IsChecked!.Value, SetUranus.IsChecked!.Value },
            { RiseNeptune.IsChecked!.Value, CulminationNeptune.IsChecked!.Value, SetNeptune.IsChecked!.Value },
        };
    }

}