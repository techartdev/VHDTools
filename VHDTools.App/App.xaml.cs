using System.Windows;
using Prism.DryIoc;
using Prism.Ioc;

namespace VHDTools.App
{
    public partial class App : PrismApplication
    {
        protected override Window CreateShell() => Container.Resolve<MainWindow>();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<Services.IDiskService, Services.DiskService>();
            containerRegistry.RegisterSingleton<Services.ISettingsService, Services.SettingsService>();

            containerRegistry.RegisterForNavigation<Views.HomeView>();
            containerRegistry.RegisterForNavigation<Views.SettingsView>();
        }
    }
}
