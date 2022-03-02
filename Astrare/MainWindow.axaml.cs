using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using Astrare.KosmorroConnection;
using Astrare.Models;
using Astrare.Translate;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Astrare;

[SuppressMessage("ReSharper", "UnusedParameter.Local")]
public partial class MainWindow : Window
{
    private GlobalData? _currentData;

    private DateTime _currentDate = DateTime.Now;

    private bool _languageChanged;

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif


        Longitude.Value = (double) Settings.Current.Longitude;
        Latitude.Value = (double) Settings.Current.Latitude;
        TimeZonePicker.Value = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Hours;
        Latitude.ValueChanged += Latitude_OnValueChanged;
        Longitude.ValueChanged += Latitude_OnValueChanged;
        LanguagePicker.Items = Language.Languages;
        for (int i = 0; i < Language.Languages.Length; i++)
        {
            if (Equals(Language.Languages[i], Language.Current))
            {
                LanguagePicker.SelectedIndex = i;
                break;
            }
        }

        LanguagePicker.SelectionChanged += LanguagePicker_OnSelectionChanged;
        CheckKosmorro(() =>
        {
            CalculsDatePicker.SelectedDate = DateTimeOffset.Now;
            ReloadData();
        });
    }

    private void ReloadData()
    {
        if (_languageChanged)
        {
            var mw = new MainWindow();
            mw.Show();
            this.Close();
            return;
        }

        // ReSharper disable once AsyncVoidLambda
        new Thread(async () =>
        {
            //Récupération des coordonnées du client si indéfinies
            if (Settings.Current.Latitude == 360 || Settings.Current.Longitude == 360)
            {
                Latitude.ValueChanged -= Latitude_OnValueChanged;
                Longitude.ValueChanged -= Latitude_OnValueChanged;
                var ipInfo = JsonSerializer.Deserialize<IPAPIClient.Result>(
                    await (await new HttpClient().GetAsync("http://ip-api.com/json/")).Content.ReadAsStringAsync());
                if (ipInfo?.Latitude != null && ipInfo.Longitude != null)
                {
                    Settings.Current.Latitude = (decimal) ipInfo.Latitude;
                    Settings.Current.Longitude = (decimal) ipInfo.Longitude;
                    Settings.Current.Save();
                }
                else
                {
                    Settings.Current.Latitude = Settings.Current.Longitude = 0;
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Latitude.Value = (double) Settings.Current.Latitude;
                    Longitude.Value = (double) Settings.Current.Longitude;
                    Latitude.ValueChanged += Latitude_OnValueChanged;
                    Longitude.ValueChanged += Latitude_OnValueChanged;
                });
            }

            //tentative de récupérer les données de Kosmorro
            try
            {
                _currentData =
                    KosmorroConnector.GetFromKosmorro(_currentDate, Settings.Current.Latitude,
                        Settings.Current.Longitude, (int) TimeZonePicker.Value);
            }
            catch (Exception e)
            {
                ShowMessage("Unhandled exception", e.Message);
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                //Remplissage du tableau
                List<DisplayEphemerides> data = new();
                foreach (var e in _currentData.ephemerides)
                    data.Add(new(e));

                EphemeridsGrid.Items = data;

                //Génération du graphique
                GraphViewer.Content = GraphDrawer.GetCanvas(_currentData.ephemerides);


                //Affichage des phases lunaires
                ActualMoonImage.Source = new Bitmap(
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                    "/Resources/MoonPhases/" +
                    _currentData.moon_phase.phase.ToString().ToLower()
                        .Replace("_", "-") +
                    ".png");
                ActualMoonName.Content = MoonPhase.TypeToString(_currentData.moon_phase.phase);
                if (_currentData.moon_phase.time != null)
                {
                    ActualMoonDate.Content =
                        "Start : " + ((DateTime) _currentData.moon_phase.time).ToString("dd/MM/yyyy HH:mm");
                }
                else
                {
                    ActualMoonDate.Content = "";
                }

                if (_currentData.moon_phase.next != null)
                {
                    NextMoonImage.Source = new Bitmap(
                        System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                        "/Resources/MoonPhases/" +
                        _currentData.moon_phase.next.phase.ToString().ToLower()
                            .Replace("_", "-") +
                        ".png");
                    NextMoonName.Content = MoonPhase.TypeToString(_currentData.moon_phase.next.phase);

                    if (_currentData.moon_phase.next.time != null)
                    {
                        NextMoonDate.Content =
                            "Start : " + ((DateTime) _currentData.moon_phase.next.time).ToString("dd/MM/yyyy HH:mm");
                    }
                    else
                    {
                        NextMoonDate.Content = "";
                    }
                }
                else
                {
                    NextMoonImage.Source =
                        new Bitmap(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly()
                            .Location) + "/Resources/MoonPhases/unknown.png");
                    NextMoonName.Content = "Unknown";
                    NextMoonDate.Content = "";
                }

                //Affichage des évènements
                List<DisplayEvent> eventsDisplay = new();
                foreach (var e in _currentData.events)
                    eventsDisplay.Add(new(e));

                EventsGrid.Items = eventsDisplay;
            });
        }).Start();
    }

    private void CheckKosmorro(Action callback)
    {
        if (PythonHelper.GetPythonCommand() is null)
        {
            ShowMessage("Python 3 is not installed",
                "The program didn't find a compatible version of Python 3 installed in your computer, please install Python 3.7 or later");
            return;
        }


        var regex = new Regex("Kosmorro Lite [\\d]+\\.[\\d]+\\.?[\\d]*");
        //On essaye d'obtenir la version de kosmorro installée

        var cmd = "cd {/d} " +
                  System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                  "/kosmorro-lite && " + PythonHelper.GetPythonCommand() +
                  " kosmorro-lite -v";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            cmd = cmd.Replace("{/d}", "/d");
        }
        else
        {
            cmd = cmd.Replace(" {/d}", "");
        }


        var commandResult =
            regex.Match(
                ShellHelper.Bash(cmd));

        //Pas de version de kosmorro installée
        if (!commandResult.Success || commandResult.Length < 1)
        {
            ShowMessage("Kosmorro is not installed", "Kosmorro is being installed, please wait until it finishes.");
            var th = new Thread(() =>
            {
                ShellHelper.Bash(PythonHelper.GetPythonCommand() + " -m pip install kosmorrolib==1.0.5");
                ShellHelper.Bash(PythonHelper.GetPythonCommand() + " -m pip install python-dateutil");

                commandResult = regex.Match(ShellHelper.Bash(cmd));
                if (!commandResult.Success || commandResult.Length < 1)
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ShowMessage("Kosmorro is not installed", "Please install it manually.");
                    });
                }
                else
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        MessageView.IsEnabled = MessageView.IsVisible = false;
                        callback.Invoke();
                    });
                }
            });
            th.Start();
        }
        else
        {
            ShellHelper.Bash(PythonHelper.GetPythonCommand() + " -m pip install kosmorrolib==1.0.5");
            callback.Invoke();
        }
    }

    private void ShowMessage(string title, string description)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            MessageView.IsEnabled = MessageView.IsVisible = true;
            MessageTitle.Text = Language.Current.Translate(title);
            MessageText.Text = Language.Current.Translate(description);
        });
    }

    private void CalculsDatePicker_OnSelectedDateChanged(object? sender, DatePickerSelectedValueChangedEventArgs e)
    {
        _currentDate = new(CalculsDatePicker.SelectedDate!.Value.Year, CalculsDatePicker.SelectedDate.Value.Month,
            CalculsDatePicker.SelectedDate.Value.Day);
    }

    private void Latitude_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        Settings.Current.Latitude = (decimal) Latitude.Value;
        Settings.Current.Longitude = (decimal) Longitude.Value;
        Settings.Current.Save();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        ReloadData();
    }


    private void Quit_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }


    private void About_OnClick(object? sender, RoutedEventArgs e)
    {
        var about = new About();
        about.Show();
    }

    private async void Save()
    {
        if (_currentData is null)
            return;

        var saveFileBox = new SaveFileDialog
        {
            Title = Language.Current.Translate("Save Document As...")
        };
        var filters = new List<FileDialogFilter>();
        var filter = new FileDialogFilter();
        var extension = new List<string> {"pdf"};
        filter.Extensions = extension;
        filter.Name = "Document Files";
        filters.Add(filter);
        saveFileBox.Filters = filters;

        saveFileBox.DefaultExtension = "pdf";

        var fileName = await saveFileBox.ShowAsync(this);

        if (fileName is null)
            return;

        if (!fileName.EndsWith(".pdf"))
            fileName += ".pdf";
        try
        {
            PdfDrawer.DrawToDocument(_currentData, fileName, (int) TimeZonePicker.Value);
            OpenFile(fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void SavePDF_OnClick(object? sender, RoutedEventArgs e)
    {
        Save();
    }

    private void LanguagePicker_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Language.Current = Language.Languages[LanguagePicker.SelectedIndex];
        Settings.Current.Language = Language.Current.Name;
        Settings.Current.Save();
        _languageChanged = true;
    }

    private void YearEvents_OnClick(object? sender, RoutedEventArgs e)
    {
        EventList window = null;
        window = new EventList(false, Language.Current.Translate("Year events"), () => { Execute(); });
        window.Show();

        async void Execute()
        {
            if (window is null)
                return;

            var acceptedEvents = window.GetEvents();
            var showPlanets = window.ShowPlanetIcons();
            var date = window.GetMonthAndYear();
            var fontSize = window.GetFontSize();
            window.Close();

            var saveFileBox = new SaveFileDialog
            {
                Title = Language.Current.Translate("Save Document As..."),
                DefaultExtension = "pdf",
                Filters = new List<FileDialogFilter>()
                {
                    new()
                    {
                        Extensions = new List<string> {"pdf"},
                        Name = "PDF Document"
                    },
                    new()
                    {
                        Extensions = new List<string> {"txt"},
                        Name = "TXT File"
                    },
                }
            };


            var fileName = await saveFileBox.ShowAsync(this);

            if (fileName is null)
                return;


            ShowMessage("Calculating events 0%", "Please wait until the process finishes");
            new Thread(() =>
            {
                var dates = GetDatesOfYear(date.Year);
                var events = new List<Event>();
                for (int i = 0; i < dates.Length; i++)
                {
                    try
                    {
                        var data =
                            KosmorroConnector.GetFromKosmorro(dates[i], Settings.Current.Latitude,
                                Settings.Current.Longitude, (int) TimeZonePicker.Value);

                        foreach (var ev in data.events)
                            if (acceptedEvents.Contains(ev.EventType))
                                events.Add(ev);

                        if (acceptedEvents.Contains(EventTypes.MOON_PHASE) && data.moon_phase.time is not null &&
                            data.moon_phase.time!.Value.Date == dates[i])
                            events.Add(new()
                            {
                                details = new()
                                {
                                    {"phase", data.moon_phase}
                                },
                                EventType = EventTypes.MOON_PHASE,
                                starts_at = data.moon_phase.time!.Value,
                                objects = Array.Empty<Models.Object>()
                            });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    var i1 = i;
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ShowMessage($"Calculating events {100 * i1 / dates.Length}%",
                            "Please wait until the process finishes");
                    });
                }

                if (fileName.EndsWith("pdf"))
                {
                    EventsDrawer.DrawToDocument(
                        Language.Current.Translate("Astronomical events for {0}", _currentDate.ToString("yyyy")),
                        Language.Current.Translate(
                            $"Latitude : {Settings.Current.Latitude}° | Longitude : {Settings.Current.Longitude}° | UTC" +
                            ((int) TimeZonePicker.Value >= 0 ? "+" : "-") + (int) TimeZonePicker.Value),
                        events.ToArray(),
                        fileName, showPlanets, fontSize);
                }
                else if (fileName.EndsWith("txt"))
                {
                    EventsDrawer.DrawToTextFile(
                        Language.Current.Translate("Astronomical events for {0}", _currentDate.ToString("yyyy")),
                        Language.Current.Translate(
                            $"Latitude : {Settings.Current.Latitude}° | Longitude : {Settings.Current.Longitude}° | UTC" +
                            ((int) TimeZonePicker.Value >= 0 ? "+" : "-") + (int) TimeZonePicker.Value),
                        events.ToArray(),
                        fileName);
                }

                Dispatcher.UIThread.InvokeAsync(() => { MessageView.IsEnabled = MessageView.IsVisible = false; });
                OpenFile(fileName);
            }).Start();
        }
    }

    private void MonthEvents_OnClick(object? sender, RoutedEventArgs e)
    {
        EventList window = null;
        window = new EventList(true, Language.Current.Translate("Month events"), () => { Execute(); });
        window.Show();

        async void Execute()
        {
            if (window is null)
                return;

            var acceptedEvents = window.GetEvents();
            var showPlanets = window.ShowPlanetIcons();
            var date = window.GetMonthAndYear();
            var fontSize = window.GetFontSize();
            window.Close();

            var saveFileBox = new SaveFileDialog
            {
                Title = Language.Current.Translate("Save Document As..."),
                DefaultExtension = "pdf",
                Filters = new List<FileDialogFilter>()
                {
                    new()
                    {
                        Extensions = new List<string> {"pdf"},
                        Name = "PDF Document"
                    },
                    new()
                    {
                        Extensions = new List<string> {"txt"},
                        Name = "TXT File"
                    },
                }
            };

            var fileName = await saveFileBox.ShowAsync(this);

            if (fileName is null)
                return;


            ShowMessage("Calculating events 0%", "Please wait until the process finishes");
            new Thread(() =>
            {
                var dates = GetDatesOfMonth(date.Year, date.Month);
                var events = new List<Event>();

                for (int i = 0; i < dates.Length; i++)
                {
                    try
                    {
                        var data =
                            KosmorroConnector.GetFromKosmorro(dates[i], Settings.Current.Latitude,
                                Settings.Current.Longitude, (int) TimeZonePicker.Value);
                        foreach (var ev in data.events)
                            if (acceptedEvents.Contains(ev.EventType))
                                events.Add(ev);
                        if (acceptedEvents.Contains(EventTypes.MOON_PHASE) && data.moon_phase.time is not null &&
                            data.moon_phase.time!.Value.Date == dates[i])
                            events.Add(new()
                            {
                                details = new()
                                {
                                    {"phase", data.moon_phase}
                                },
                                EventType = EventTypes.MOON_PHASE,
                                starts_at = data.moon_phase.time!.Value,
                                objects = Array.Empty<Models.Object>()
                            });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    var i1 = i;
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ShowMessage($"Calculating events {100 * i1 / dates.Length}%",
                            "Please wait until the process finishes");
                    });
                }

                if (fileName.EndsWith("pdf"))
                {
                    EventsDrawer.DrawToDocument(
                        Language.Current.Translate("Astronomical events for {0}",
                            Language.Current.Months[_currentDate.Month - 1] + " " + _currentDate.ToString("yyyy")),
                        Language.Current.Translate(
                            $"Latitude : {Settings.Current.Latitude}° | Longitude : {Settings.Current.Longitude}° | UTC" +
                            ((int) TimeZonePicker.Value >= 0 ? "+" : "-") + (int) TimeZonePicker.Value),
                        events.OrderBy(evnt => evnt.starts_at.ToBinary()).ToArray(), fileName, showPlanets, fontSize);
                }
                else if (fileName.EndsWith("txt"))
                {
                    EventsDrawer.DrawToTextFile(
                        Language.Current.Translate("Astronomical events for {0}",
                            Language.Current.Months[_currentDate.Month - 1] + " " + _currentDate.ToString("yyyy")),
                        Language.Current.Translate(
                            $"Latitude : {Settings.Current.Latitude}° | Longitude : {Settings.Current.Longitude}° | UTC" +
                            ((int) TimeZonePicker.Value >= 0 ? "+" : "-") + (int) TimeZonePicker.Value),
                        events.OrderBy(evnt => evnt.starts_at.ToBinary()).ToArray(), fileName);
                }

                Dispatcher.UIThread.InvokeAsync(() => { MessageView.IsEnabled = MessageView.IsVisible = false; });
                OpenFile(fileName);
            }).Start();
        }
    }

    private void YearAlmanach_OnClick(object? sender, RoutedEventArgs e)
    {
        AlmanachGenerator window = null;
        window = new AlmanachGenerator(() => { Execute(); });
        window.Show();

        async void Execute()
        {
            var date = window.GetYearDate();
            var moonEphs = window.GetIfMoonEphemerides();
            var ecoMode = window.GetIfEconomic();
            var dayNight = window.GetIfDayNight();
            var planetsLines = window.PlanetsLines();
            
            window.Close();
            
            var saveFileBox = new SaveFileDialog
            {
                Title = Language.Current.Translate("Save Document As...")
            };
            var filters = new List<FileDialogFilter>();
            var filter = new FileDialogFilter();
            var extension = new List<string> {"pdf"};
            filter.Extensions = extension;
            filter.Name = "PDF Document";
            filters.Add(filter);
            saveFileBox.Filters = filters;

            saveFileBox.DefaultExtension = "pdf";

            var fileName = await saveFileBox.ShowAsync(this);

            if (fileName is null)
                return;


            ShowMessage("Calculating events 0%", "Please wait until the process finishes");
            new Thread(() =>
            {
                var dates = GetDatesOfYear(date.Year);
                var datas = new List<GlobalData>();

                for (int i = 0; i < dates.Length; i++)
                {
                    try
                    {
                        var data =
                            KosmorroConnector.GetFromKosmorro(dates[i], Settings.Current.Latitude,
                                Settings.Current.Longitude, (int) TimeZonePicker.Value);
                        datas.Add(data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    var i1 = i;
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ShowMessage($"Calculating events {100 * i1 / dates.Length}%",
                            "Please wait until the process finishes");
                    });
                }


                AlmanachDrawer.SavePdf(datas.ToArray(), dates, fileName,
                    Language.Current.Translate("Graphic almanach - " + _currentDate.Year),
                    Language.Current.Translate(
                        $"Latitude : {Settings.Current.Latitude.ToString("F1")}° | Longitude : {Settings.Current.Longitude.ToString("F1")}° | UTC" +
                        ((int) TimeZonePicker.Value >= 0 ? "+" : "-") + (int) TimeZonePicker.Value),
                    dayNight, moonEphs, planetsLines, ecoMode);

                Dispatcher.UIThread.InvokeAsync(() => { MessageView.IsEnabled = MessageView.IsVisible = false; });
                OpenFile(fileName);
            }).Start();
        }
    }

    private void OpenFile(string fileName)
    {
        new Thread(() =>
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    ShellHelper.Bash("explorer " + fileName);
                }
                else if (OperatingSystem.IsLinux())
                {
                    ShellHelper.Bash($"xdg-open {fileName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }).Start();
    }

    private static DateTime[] GetDatesOfYear(int year, bool addFirstAndLast = false)
    {
        var dates = new List<DateTime>();

        if (addFirstAndLast)
            dates.Add(new DateTime(year, 1, 1).AddDays(-1));
        for (var date = new DateTime(year, 1, 1); date.Year == year; date = date.AddDays(1))
        {
            dates.Add(date);
        }

        if (addFirstAndLast)
            dates.Add(new DateTime(year + 1, 1, 1));

        return dates.ToArray();
    }

    private static DateTime[] GetDatesOfMonth(int year, int month)
    {
        var dates = new List<DateTime>();

        for (var date = new DateTime(year, month, 1); date.Month == month; date = date.AddDays(1))
        {
            dates.Add(date);
        }

        return dates.ToArray();
    }
}