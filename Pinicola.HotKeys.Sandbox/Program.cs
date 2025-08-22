using Dapplo.Windows.User32;
using Dapplo.Windows.User32.Enums;
using Pinicola.HotKeys;

var foregroundWindowHandle = User32Api.GetForegroundWindow();
Console.WriteLine($"Previous foreground window handle: {foregroundWindowHandle} {foregroundWindowHandle:X}");

var chromeHandle = HookRegister.GetChromeWindowHandleWithYoutubeTitle();
Console.WriteLine($"Chrome handle: {chromeHandle} {chromeHandle:X}");

Console.WriteLine("Bringing Chrome to top...");
User32Api.ShowWindow(chromeHandle, ShowWindowCommands.Restore);
User32Api.SetForegroundWindow(chromeHandle);
