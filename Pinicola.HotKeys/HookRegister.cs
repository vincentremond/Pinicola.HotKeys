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
            Noop,
            [
                VirtualKeyCode.Capital,
            ],
            [
                VirtualKeyCode.Shift,
                VirtualKeyCode.Capital,
            ]
        );

        Register(
            Maximize,
            [
                VirtualKeyCode.Shift,
                VirtualKeyCode.LeftWin,
                VirtualKeyCode.Up,
            ],
            [
                VirtualKeyCode.Multiply,
            ]
        );

        Register(
            Minimize,
            [
                VirtualKeyCode.Shift,
                VirtualKeyCode.LeftWin,
                VirtualKeyCode.Down,
            ],
            [
                VirtualKeyCode.Divide,
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
        Register(
            YoutubeNext,
            [
                VirtualKeyCode.LeftControl, VirtualKeyCode.LeftWin,
                VirtualKeyCode.Menu, VirtualKeyCode.KeyN,
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
        KeyPresses(DateTimeOffset.Now.ToString("yyyy-MM-dd--HH-mm-ss"));
    }

    private static void SetDayStampToClipBoard()
    {
        KeyPresses(DateTimeOffset.Now.ToString("yyyy-MM-dd"));
    }

    private static void KeyPresses(string str)
    {
        var keyPresses = ToKeyCode(str);
        KeyboardInputGenerator.KeyPresses(keyPresses);
    }

    private static VirtualKeyCode[] ToKeyCode(string str)
    {
        var virtualKeyCodes = str.Select(ToKeyCode).ToArray();
        return virtualKeyCodes;
    }

    private static VirtualKeyCode ToKeyCode(char c)
    {
        return c switch
        {
            '-' => VirtualKeyCode.OemMinus,
            '0' => VirtualKeyCode.Key0,
            '1' => VirtualKeyCode.Key1,
            '2' => VirtualKeyCode.Key2,
            '3' => VirtualKeyCode.Key3,
            '4' => VirtualKeyCode.Key4,
            '5' => VirtualKeyCode.Key5,
            '6' => VirtualKeyCode.Key6,
            '7' => VirtualKeyCode.Key7,
            '8' => VirtualKeyCode.Key8,
            '9' => VirtualKeyCode.Key9,

            _ => throw new ArgumentOutOfRangeException(nameof(c), c, message: null),
        };
    }

    private static void Register(Action action, params VirtualKeyCode[][] keyCombinations)
    {
        foreach (var keyCombination in keyCombinations)
        {
            var keyCombinationHandler = new KeyCombinationHandler(keyCombination)
            {
                IgnoreInjected = false,
            };

            KeyboardHook.KeyboardEvents
                .Where(keyCombinationHandler)
                .Subscribe(x =>
                    {
                        x.Handled = true;
                        Task.Run(async () =>
                            {
                                while (keyCombinationHandler.HasKeysPressed)
                                {
                                    await Task.Delay(millisecondsDelay: 50);
                                }

                                action();
                            }
                        );
                    }
                );
        }
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

        var nextWindow = command == ShowWindowCommands.Minimize
            ? User32Api.GetWindow(foregroundWindowHandle, GetWindowCommands.GW_HWNDNEXT)
            : IntPtr.Zero;

        User32Api.ShowWindow(foregroundWindowHandle, command);

        if (nextWindow != IntPtr.Zero)
        {
            User32Api.SetForegroundWindow(nextWindow);
        }
    }

    private static void YoutubeNext()
    {
        // Save current foreground window handle
        var foregroundWindowHandle = User32Api.GetForegroundWindow();

        // Find the Chrome window with YouTube in the title
        var chromeWindowHandle = GetChromeWindowHandleWithYoutubeTitle();
        if (chromeWindowHandle == IntPtr.Zero)
        {
            // If no Chrome window with YouTube title is found, do nothing
            return;
        }

        // Bring chrome with YouTube in title to front
        User32Api.SetForegroundWindow(chromeWindowHandle);

        // Send Alt+L to remove from Watch Later playlist
        KeyboardInputGenerator.KeyCombinationPress(
            VirtualKeyCode.LeftMenu,
            VirtualKeyCode.KeyL
        );

        // Wait for 1 second
        Thread.Sleep(millisecondsTimeout: 1000);

        // Send Shift+N to play next video
        KeyboardInputGenerator.KeyCombinationPress(
            VirtualKeyCode.MediaNextTrack
        );

        // Wait for 1 second
        Thread.Sleep(millisecondsTimeout: 1000);

        // Restore original foreground window
        if (foregroundWindowHandle != IntPtr.Zero)
        {
            User32Api.SetForegroundWindow(foregroundWindowHandle);
        }
    }

    private static IntPtr GetChromeWindowHandleWithYoutubeTitle()
    {
        var result = IntPtr.Zero;
        User32Api.EnumWindows(
            (hWnd, _) =>
            {
                var windowText = User32Api.GetTextFromWindow(hWnd)?.TrimEnd(trimChar: '\0');
                if (windowText != null && windowText.EndsWith(
                        " - YouTube - Google Chrome",
                        StringComparison.OrdinalIgnoreCase
                    ))
                {
                    result = hWnd;
                    return false; // Stop enumerating windows
                }

                return true;
            },
            IntPtr.Zero
        );
        return result;
    }
}
