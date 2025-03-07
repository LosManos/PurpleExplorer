using System.IO;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PurpleExplorer.Helpers;
using PurpleExplorer.Models;
using PurpleExplorer.Services;
using PurpleExplorer.ViewModels;
using PurpleExplorer.Views;
using ReactiveUI;
using Splat;

namespace PurpleExplorer;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var appStatePath = "appstate.json";
        if (!File.Exists(appStatePath))
        {
            File.Create(appStatePath).Close();
        }

        // Create the AutoSuspendHelper.
        var suspension = new AutoSuspendHelper(ApplicationLifetime);
        RxApp.SuspensionHost.CreateNewAppState = () => new AppState();
        RxApp.SuspensionHost.SetupDefaultSuspendResume(new NewtonsoftJsonSuspensionDriver(appStatePath));
        suspension.OnFrameworkInitializationCompleted();

        // Load the saved view model state.
        var state = RxApp.SuspensionHost.GetAppState<AppState>(); // ?? new AppState();

        Locator.CurrentMutable.RegisterLazySingleton(() => state, typeof(IAppState));
        Locator.CurrentMutable.RegisterLazySingleton(() => new LoggingService(), typeof(ILoggingService));
        Locator.CurrentMutable.Register(() => new TopicHelper(state.AppSettings), typeof(ITopicHelper));
        Locator.CurrentMutable.Register(() => new QueueHelper(state.AppSettings), typeof(IQueueHelper));
        Locator.CurrentMutable.Register(() => new ApplicationService(), typeof(IApplicationService));
        Locator.CurrentMutable.Register(() => new ModalWindowService(
            Locator.Current.GetService<IApplicationService>()!), typeof(IModalWindowService));

        new MainWindow(Locator.Current.GetService<IApplicationService>()!, Locator.Current.GetService<IModalWindowService>()!)
        {
            DataContext = new MainWindowViewModel(
                Locator.Current.GetService<ILoggingService>()!,
                Locator.Current.GetService<ITopicHelper>()!,
                Locator.Current.GetService<IQueueHelper>()!,
                Locator.Current.GetService<IAppState>()!,
                Locator.Current.GetService<IApplicationService>()!,
                Locator.Current.GetService<IModalWindowService>()
            )
        }.Show();
        base.OnFrameworkInitializationCompleted();
    }
}