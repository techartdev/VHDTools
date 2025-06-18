namespace VHDTools.App.Services
{
    public interface ISettingsService
    {
        AppSettings Settings { get; }
        void Save();
    }
}
