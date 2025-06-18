using DiskTools;

namespace VHDTools.App.Services
{
    public class DiskService : IDiskService
    {
        public void CreateVirtualDisk(string path, long sizeBytes)
        {
            DiskManager.CreateVirtualDisk(path, VirtualDiskType.Vhdx, sizeBytes);
        }

        public void AttachVirtualDisk(string path)
        {
            using var manager = new DiskManager(path);
            manager.AttachVirtualDisk();
        }

        public void DetachVirtualDisk(string path)
        {
            using var manager = new DiskManager(path);
            manager.DetachVirtualDisk();
        }
    }
}
