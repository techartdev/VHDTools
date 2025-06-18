namespace VHDTools.App.Services
{
    public interface IDiskService
    {
        void CreateVirtualDisk(string path, long sizeBytes);
        void AttachVirtualDisk(string path);
        void DetachVirtualDisk(string path);
    }
}
