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
            [
                VirtualKeyCode.LeftWin, VirtualKeyCode.LeftShift,
                VirtualKeyCode.Up,
            ]
        );

        Register(
            Minimize,
            [
                VirtualKeyCode.LeftWin, VirtualKeyCode.LeftShift,
                VirtualKeyCode.Down,
            ]
        );

        Register(
            SendTimeStamp,
            [
                VirtualKeyCode.LeftControl, VirtualKeyCode.LeftWin,
                VirtualKeyCode.Menu, VirtualKeyCode.KeyT,
            ]
        );
    }

    private static void SendTimeStamp()
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss");
        var keycodes = ToKeyCodes(timestamp);
        KeyboardInputGenerator.KeyPresses(keycodes);
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

    private static VirtualKeyCode[] ToKeyCodes(string input)
    {
        var keyCodes = new List<VirtualKeyCode>(input.Length);
        foreach (var c in input)
        {
            if (char.IsLetterOrDigit(c))
            {
                keyCodes.Add((VirtualKeyCode)char.ToUpper(c));
            }
            else if (c == '-')
            {
                keyCodes.Add(VirtualKeyCode.OemMinus);
            }
            else if (c == ':')
            {
                keyCodes.Add(VirtualKeyCode.Oem1);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(c), c, message: null);
            }
        }

        return keyCodes.ToArray();
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
