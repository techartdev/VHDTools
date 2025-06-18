using Prism.Commands;
using Prism.Mvvm;
using VHDTools.App.Services;

namespace VHDTools.App.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private readonly ISettingsService _settingsService;
        private string _lastUsedPath = string.Empty;

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _lastUsedPath = settingsService.Settings.LastVhdPath ?? string.Empty;
            SaveCommand = new DelegateCommand(Save);
        }

        public string LastUsedPath
        {
            get => _lastUsedPath;
            set => SetProperty(ref _lastUsedPath, value);
        }

        public DelegateCommand SaveCommand { get; }

        private void Save()
        {
            _settingsService.Settings.LastVhdPath = LastUsedPath;
            _settingsService.Save();
        }
    }
}
