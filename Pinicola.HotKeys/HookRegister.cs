using Dapplo.Windows.Input.Enums;
using Dapplo.Windows.Input.Keyboard;
using Dapplo.Windows.User32;
using Dapplo.Windows.User32.Enums;

namespace Pinicola.HotKeys;

public static class HookRegister
{
    public static void Run()
    {
        Register(
            Maximize,
            VirtualKeyCode.LeftWin,
            VirtualKeyCode.LeftShift,
            VirtualKeyCode.Up
        );

        Register(
            Minimize,
            VirtualKeyCode.LeftWin,
            VirtualKeyCode.LeftShift,
            VirtualKeyCode.Down
        );
    }

    private static void Register(Action action, params VirtualKeyCode[] keyCodes)
    {
        var keyCombinationHandler = new KeyCombinationHandler(keyCodes)
        {
            IgnoreInjected = false,
        };

        KeyboardHook.KeyboardEvents.Where(keyCombinationHandler).Subscribe(x =>
            {
                x.Handled = true;
                action();
            }
        );
    }

    private static void Maximize()
    {
        ShowOrHideForegroundWindow(ShowWindowCommands.Maximize);
    }

    private static void Minimize()
    {
        ShowOrHideForegroundWindow(ShowWindowCommands.Minimize);
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
