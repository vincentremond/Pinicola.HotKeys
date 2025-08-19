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
        var timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd--HH-mm-ss");
        ClipboardService.SetText(timestamp);
    }

    private static void SetDayStampToClipBoard()
    {
        var timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd");
        ClipboardService.SetText(timestamp);
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
                                    await Task.Delay(50);
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

        // Bring chrome with Youtube in title to front
        User32Api.SetForegroundWindow(chromeWindowHandle);

        // Send Alt+L to remove from Watch Later playlist
        KeyboardInputGenerator.KeyCombinationPress(
            VirtualKeyCode.LeftMenu,
            VirtualKeyCode.KeyL
        );

        // Wait for 1 second
        Thread.Sleep(1000);

        // Send Shift+N to play next video
        KeyboardInputGenerator.KeyCombinationPress(
            VirtualKeyCode.MediaNextTrack
        );

        // Wait for 1 second
        Thread.Sleep(1000);

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
