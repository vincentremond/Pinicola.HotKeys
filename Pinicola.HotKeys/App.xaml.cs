using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Dapplo.Windows.User32;
using Dapplo.Windows.User32.Enums;
using NHotkey;
using NHotkey.Wpf;

namespace Pinicola.HotKeys;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Register(ModifierKeys.Alt | ModifierKeys.Shift | ModifierKeys.Control, Key.Down, Minimize);
        Register(ModifierKeys.Alt | ModifierKeys.Shift | ModifierKeys.Control, Key.Up, Maximize);
        base.OnStartup(e);
    }

    private void Register(ModifierKeys modifiers, Key key, EventHandler<HotkeyEventArgs> eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? callerName = null)
    {
        HotkeyManager.Current.AddOrReplace(
            $"Pinicola.HotKeys.{callerName}",
            key,
            modifiers,
            eventHandler
        );
    }

    private void Maximize(object? sender, HotkeyEventArgs e)
    {
        ShowOrHideForegroundWindow(ShowWindowCommands.Maximize);
        e.Handled = true; // Prevent further processing of this hotkey
    }

    private void Minimize(object? sender, HotkeyEventArgs e)
    {
        ShowOrHideForegroundWindow(ShowWindowCommands.Minimize);
        e.Handled = true; // Prevent further processing of this hotkey
    }

    private static void ShowOrHideForegroundWindow(ShowWindowCommands command)
    {
        var foregroundWindowHandle = User32Api.GetForegroundWindow();
        if (foregroundWindowHandle == IntPtr.Zero)
        {
            return;
        }

        User32Api.ShowWindow(foregroundWindowHandle, command);
    }
}
