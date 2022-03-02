using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Astrare;

public static class PythonHelper
{
    private static string? _currentPython = "TEST";

    public static string? GetPythonCommand()
    {
        if (_currentPython != "TEST")
            return _currentPython;
        var tests = new [] {"python", "python3", "python3.9","python3.8","python3.7","python37","python38","python39","python310","python3.10"};
        foreach (var test in tests)
        {
            var regex = new Regex("Python 3\\.[\\d]+\\.[\\d]+");

            //On essaye d'obtenir la version de python installée
            var rslt = ShellHelper.Bash(test + " --version");
            var commandResult = regex.Match(rslt);
            
            //On vérifie si version >= Python 3.7
            if (commandResult.Success && commandResult.Length >= 1 && int.Parse(commandResult.Value.Split(".")[1]) >= 7)
            {
                _currentPython = test;
                return test;
            }
        }

        _currentPython = null;
        return null;
    }
}