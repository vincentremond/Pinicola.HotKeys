using System.Windows;

namespace Pinicola.HotKeys;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        HookRegister.Run();
        base.OnStartup(e);
    }
}
