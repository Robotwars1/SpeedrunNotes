using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Windowing;
using Microsoft.UI;

namespace SpeedrunNotes;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
// I have no clue how or why this code works but it allows all popout-windows to be always on top
#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(windows =>
            {
                windows.OnWindowCreated(xamlWindow =>
                {
                    var window = xamlWindow as MauiWinUIWindow;
                    var MainWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);

                    var windowId = Win32Interop.GetWindowIdFromWindow(MainWindowHandle);
                    var appWindow = AppWindow.GetFromWindowId(windowId);
                    var presenter = appWindow.Presenter as OverlappedPresenter;

                    //appWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay);
                    presenter.IsAlwaysOnTop = true;
                });

            });
        });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
