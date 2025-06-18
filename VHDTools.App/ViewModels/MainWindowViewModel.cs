using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace VHDTools.App.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        public DelegateCommand<string> NavigateCommand { get; }

        private void Navigate(string view)
        {
            if (!string.IsNullOrEmpty(view))
            {
                _regionManager.RequestNavigate("ContentRegion", view);
            }
        }
    }
}
