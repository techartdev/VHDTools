using Prism.Commands;
using Prism.Mvvm;
using VHDTools.App.Services;

namespace VHDTools.App.ViewModels
{
    public class HomeViewModel : BindableBase
    {
        private readonly IDiskService _diskService;

        private string _vhdPath = string.Empty;
        private string _vhdSize = "100";

        public HomeViewModel(IDiskService diskService)
        {
            _diskService = diskService;
            CreateVhdCommand = new DelegateCommand(CreateVhd);
            AttachVhdCommand = new DelegateCommand(AttachVhd);
            DetachVhdCommand = new DelegateCommand(DetachVhd);
        }

        public string VhdPath
        {
            get => _vhdPath;
            set => SetProperty(ref _vhdPath, value);
        }

        public string VhdSize
        {
            get => _vhdSize;
            set => SetProperty(ref _vhdSize, value);
        }

        public DelegateCommand CreateVhdCommand { get; }
        public DelegateCommand AttachVhdCommand { get; }
        public DelegateCommand DetachVhdCommand { get; }

        private void CreateVhd()
        {
            if (long.TryParse(VhdSize, out long sizeMb) && !string.IsNullOrWhiteSpace(VhdPath))
            {
                _diskService.CreateVirtualDisk(VhdPath, sizeMb * 1024 * 1024);
            }
        }

        private void AttachVhd()
        {
            if (!string.IsNullOrWhiteSpace(VhdPath))
            {
                _diskService.AttachVirtualDisk(VhdPath);
            }
        }

        private void DetachVhd()
        {
            if (!string.IsNullOrWhiteSpace(VhdPath))
            {
                _diskService.DetachVirtualDisk(VhdPath);
            }
        }
    }
}
