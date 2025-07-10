using Dapplo.Windows.Input.Enums;
using Dapplo.Windows.Input.Keyboard;
using Dapplo.Windows.User32;
using Dapplo.Windows.User32.Enums;
using TextCopy;

namespace Pinicola.HotKeys;

public static class HookRegister
{
    public static void Run()
    {
        Register(
            Noop,
            [
                VirtualKeyCode.Capital,
            ]
        );

        Register(
            Maximize,
            [
                VirtualKeyCode.Shift,
                VirtualKeyCode.LeftWin,
                VirtualKeyCode.Up,
            ]
        );

        Register(
            Minimize,
            [
                VirtualKeyCode.Shift,
                VirtualKeyCode.LeftWin,
                VirtualKeyCode.Down,
            ]
        );


        Register(
            SetTimeStampToClipBoard,
            [
                VirtualKeyCode.LeftControl, VirtualKeyCode.LeftWin,
                VirtualKeyCode.Menu, VirtualKeyCode.KeyT,
            ]
        );
        Register(
            SetDayStampToClipBoard,
            [
                VirtualKeyCode.LeftControl, VirtualKeyCode.LeftWin,
                VirtualKeyCode.Menu, VirtualKeyCode.KeyD,
            ]
        );
    }

    private static void Noop()
    {
        // This is a no-op, used to register the hotkey without any action.
        // It can be useful for debugging or testing purposes.
    }

    private static void SetTimeStampToClipBoard()
    {
        var timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd--HH-mm-ss");
        ClipboardService.SetText(timestamp);
    }

    private static void SetDayStampToClipBoard()
    {
        var timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd");
        ClipboardService.SetText(timestamp);
    }

    private static void Register(Action action, VirtualKeyCode[] keyCodes)
    {
        var keyCombinationHandler = new KeyCombinationHandler(keyCodes)
        {
            IgnoreInjected = false,
        };

        KeyboardHook.KeyboardEvents.Where(keyCombinationHandler)
            .Subscribe(x =>
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
