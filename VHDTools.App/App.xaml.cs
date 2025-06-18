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
        }
    }
}
