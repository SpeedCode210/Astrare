using System;
using System.Linq;

namespace Astrare.Translate;

[Language]
public class French : Language
{
    public French()
    {
        Name = "Français";
        Months = new[]
        {
            "Janvier",
            "Février",
            "Mars",
            "Avril",
            "Mai",
            "Juin",
            "Juillet",
            "Août",
            "Septembre",
            "Octobre",
            "Novembre",
            "Décembre",
        }; 
        Translations = new()
        {
            {"File", "Fichier"},
            {"Year events", "Événements de l'année"},
            {"Month events", "Événements du mois"},
            {"Moon phase", "Phase de la lune"},
            {"Month graphic almanach", "Almanach graphique mensuel"},
            {"Year graphic almanach", "Almanach graphique annuel"},
            {"Graphic almanach", "Almanach graphique"},
            {"Astronomical events for {0}", "Événements astronomiques de {0}"},
            {"Object", "Objet"},
            {"Day", "Jour"},
            {"Night", "Nuit"},
            {"Planets", "Planètes"},
            {"Sun", "Soleil"},
            {"Moon", "Lune"},
            {"Mercury", "Mercure"},
            {"Venus", "Venus"},
            {"Earth", "Terre"},
            {"Jupiter", "Jupiter"},
            {"Saturn", "Saturne"},
            {"Uranus", "Uranus"},
            {"Neptune", "Neptune"},
            {"Pluto", "Pluton"},
            {"Rise", "Lever"},
            {"Culmination", "Culmination"},
            {"Set", "Coucher"},
            {"Actual phase", "Phase actuelle"},
            {"Next phase", "Phase suivante"},
            {"Start", "Début"},
            {"End", "Fin"},
            {"Type", "Type"},
            {"Description", "Description"},
            {"Extra", "Extra"},
            {"About", "A propos"},
            {"Save PDF", "Enregistrer PDF"},
            {"Quit", "Quitter"},
            {"Date of ephemerides", "Date des éphémérides"},
            {"Latitude", "Latitude"},
            {"Longitude", "Longitude"},
            {"Language", "Langue"},
            {"Reload", "Actualiser"},
            {"New moon", "Nouvelle lune"},
            {"Full moon", "Pleine lune"},
            {"Last quarter", "Dernier quartier"},
            {"First quarter", "Premier quartier"},
            {"Waning gibbous", "Gibbeuse décroissante"},
            {"Waxing gibbous", "Gibbeuse croissante"},
            {"Waning crescent", "Dernier croissant"},
            {"Waxing crescent", "Premier croissant"},
            {"Table", "Tableau"},
            {"Events", "Événements"},
            {"Graphs", "Graphiques"},
            {"Opposition", "Opposition"},
            {"Conjunction", "Conjonction"},
            {"Occultation", "Occultation"},
            {"Maximal elongation", "Élongation maximale"},
            {"Perigee", "Périgée"},
            {"Apogee", "Apogée"},
            {"Season change", "Saison"},
            {"Lunar eclipse", "Éclipse lunaire"},
            {"Unknown event", "Événement inconnu"},
            {"Overview of your sky", "Résumé de votre ciel"},
            {"Please install it manually.", "Merci de l'installer manuellement."},
            {"{0} is at its apogee ({1} Km)", "{0} est à son apogée ({1} Km)"},
            {"{0} is at its perigee ({1} Km)", "{0} est à son périgée ({1} Km)"},
            {"{0} is in opposition", "{0} est en opposition"},
            {"{0} and {1} are in conjunction", "{0} et {1} sont en conjonction"},
            {"{0} occults {1}", "{0} occulte {1}"},
            {"Elongation of {0} is maximal ({1}°)", "L'élongation de {0} est maximale ({1}°)"},
            {"Unsupported OS", "Système incompatible"},
            {"Kosmorro doesn't support Windows officialy yet, please run the program in Linux or MacOS.",
                "Kosmorro ne supporte pas encore officiellement Windows, merci d'exécuter le programme sous MacOS ou Linux."},
            {"Unhandled exception", "Erreur innatendue"},
            {"Python 3 is not installed", "Python 3 n'est pas installé"},
            {"Kosmorro is not installed", "Kosmorro n'est pas installé"},
            {"Kosmorro is being installed, please wait until it finishes.", "Kosmorro est en cours d'installation, veuillez patienter."},
            {"Incompatible Python 3 version", "version de Python 3 incompatible"},
            {"The program didn't find a compatible version of Python 3 installed in your computer, please install Python 3.7 or later", 
                "Le programme n'a pas trouvé de version de Python 3 compatible, merci d'installer Python 3.7 ou plus"},
            {"The program didn't find a compatible version of Python 3 installed in your computer, please update to Python 3.7 or later", 
                "Le programme n'a pas trouvé de version de Python 3 compatible, merci de mettre à jour vers Python 3.7 ou plus"},
            {"Calculating events", "Calcul en cours"},
            {"Please wait until the process finishes", "Merci d'attendre la fin du processus"},
            {"Save Document As", "Enregistrer sous"},
            {"This document summarizes the ephemerides and the events of {0}."+
             " It aims to help you to prepare your observation session. All the hours are given in {1}",
                "Ce document synthétise les éphémérides et les événements prévus pour le {0}" +
                ". Son but est de vous aider à préparer votre soirée d'observation. "+
                "Toutes les heures sont données en {1}"
            },
            {
                "Don't forget to check the weather forecast before you go out with your equipment.",
                "N’oubliez pas de vous assurer que les conditions météo sont favorables avant de sortir votre matériel d’observation."
            }
        };
        Translations = Translations.OrderByDescending(k => k.Key.Length).ToDictionary(obj => obj.Key, obj => obj.Value);
    }
    
    public override string Translate(DateTime date, DateTranslateMode mode)
    {
        switch (mode)
        {
            case DateTranslateMode.Date:
                return $"{date.Day} {Months[date.Month-1]} {date.Year}";
            case DateTranslateMode.Hour:
                return $"{date.Hour}h{date.Minute}";
            case DateTranslateMode.DateHour:
                return Translate(date, DateTranslateMode.Date) + " à " + Translate(date, DateTranslateMode.Hour);
        }

        return "";
    }
}