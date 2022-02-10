using Astrare.Translate;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Astrare;

public partial class About : Window
{
    public About()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        this.Title = Language.Current.Translate("About");
        
        ExecutingVersion.Text = ShellHelper.Bash("cd kosmorro-lite && " + PythonHelper.GetPythonCommand() + " kosmorro-lite --version");
        
    }

}