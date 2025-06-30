using System.Windows;

namespace Pinicola.HotKeys;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        KillOtherInstances();
        
        HookRegister.Run();
        base.OnStartup(e);
    }

    private void KillOtherInstances()
    {
        var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        var processes = System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName);
        
        foreach (var process in processes)
        {
            if (process.Id != currentProcess.Id)
            {
                process.Kill();
            }
        }
    }
}
