using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiskTools;
using Vanara.PInvoke;

namespace DIskTools.Console
{
    class Program
    {
        public static int WriteBufferSize
        {
            get { return 1024 * 1024; }
        }

        static void Main(string[] args)
        {
            //DiskManager.CreateVirtualDisk("D:\\test2.vhdx", VirtualDiskType.Vhdx, 10 * WriteBufferSize, VirtualDiskCreateOptions.None);
            DiskManager dm = new DiskManager("D:\\test.vhdx");

            dm.DetachVirtualDisk();
            //dm.AttachVirtualDisk();

            //dm.DiskVolumes.First().RemoveLetter();
            //dm.InitializeVirtualDiskPartition();
            //dm.FortmatAllVolumesOfDrive(FileSystemType.NTFS);
            //dm.SetDriveLetter(dm.DiskVolumes.First(), 'x');
            //dm.DetachVirtualDisk();

            //dm.ExpandVirtualDisk(1024 * (ulong)WriteBufferSize);
            //DiskIO.UpdateDiskPartition(dm.VirtualDisk.GetAttachedPath(), 512 * WriteBufferSize);

            string path = "D:\\test.vhdx";

            System.Console.WriteLine("Press ENTER to create VHD");
            System.Console.ReadLine();

            //CreateVhd(path, 500 * (ulong)WriteBufferSize);
            CreateVhdx(path, 512 * WriteBufferSize);

            //CreateVhdDynamic("D:\\test.vhd", 500 * WriteBufferSize);
            System.Console.WriteLine("Press ENTER to attach and initialize the VHD");
            System.Console.ReadLine();

            AttachVhd(path, true);

            System.Console.WriteLine("Press ENTER to format the VHD volume");
            System.Console.ReadLine();

            FormatDrive(path, "Test");

            System.Console.WriteLine("Press ENTER to set drive letter of the VHD");
            System.Console.ReadLine();

            ChangeDriveLetter(path, "X:");

            System.Console.WriteLine("Press ENTER to detach the VHD");
            System.Console.ReadLine();

            DetachVhd(path);

            System.Console.WriteLine("Press ENTER to delete the VHD");
            System.Console.ReadLine();

            File.Delete(path);
        }

        private static bool CreateVhd(string fileName, ulong size)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.WriteThrough))
            {
                ReFS.RemoveIntegrityStream(stream.SafeFileHandle);

                HardDiskFooter footer = new HardDiskFooter();
                footer.BeginUpdate();
                footer.CreatorApplication = VhdCreatorApplication.MicrosoftWindows;
                footer.CreatorVersion = new Version();
                footer.SetSize(size);
                footer.OriginalSize = footer.CurrentSize;
                footer.DiskType = VhdDiskType.FixedHardDisk;

                footer.EndUpdate();

                byte[] buffer = new byte[WriteBufferSize];
                ulong remaining = footer.CurrentSize;
                while (remaining > 0)
                {

                    ulong count = (ulong)buffer.Length;
                    if (count > remaining) { count = remaining; }
                    stream.Write(buffer, 0, (int)count);
                    remaining -= count;
                }
                buffer = footer.Bytes;
                stream.Write(buffer, 0, buffer.Length);
            }
            return true;
        }

        private static bool CreateVhdx(string path, long size)
        {
            try
            {
                using (VirtualDisk disk = new VirtualDisk(path))
                {
                    disk.Create(size, VirtualDiskCreateOptions.FullPhysicalAllocation, 0, 512, VirtualDiskType.Vhdx);
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static bool CreateVhdDynamic(string path, long size)
        {
            using (VirtualDisk vhd = new VirtualDisk(path))
            {
                try
                {
                    vhd.Create(size, VirtualDiskCreateOptions.None, 0, 512, VirtualDiskType.Vhd);
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        private static bool AttachVhd(string path, bool initialize, bool readOnly = false)
        {
            try
            {
                using (VirtualDisk disk = new VirtualDisk(path))
                {
                    VirtualDiskAccessMask access = VirtualDiskAccessMask.All;
                    VirtualDiskAttachOptions options = VirtualDiskAttachOptions.PermanentLifetime | VirtualDiskAttachOptions.NoDriveLetter;
                    if (readOnly)
                    {
                        if (initialize == false)
                        {
                            access = VirtualDiskAccessMask.AttachReadOnly;
                        }
                        options |= VirtualDiskAttachOptions.ReadOnly;
                    }
                    disk.Open(access);
                    disk.Attach(options);
                    if (initialize) { path = disk.GetAttachedPath(); }
                }
                if (initialize)
                {
                    DiskIO.InitializeDisk(path);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool DetachVhd(string path)
        {
            using (VirtualDisk disk = new VirtualDisk(path))
            {
                try
                {
                    disk.Open(VirtualDiskAccessMask.Detach);
                    disk.Detach();

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        private static bool ChangeDriveLetter(string path, string letter)
        {
            string attachedDevice = null;

            try
            {
                using (VirtualDisk document = new VirtualDisk(path))
                {
                    document.Open(VirtualDiskAccessMask.GetInfo);
                    attachedDevice = document.GetAttachedPath();
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            if (attachedDevice != null)
            {
                List<Volume> volumes = new List<Volume>(Volume.GetVolumesOnPhysicalDrive(attachedDevice));
                if (volumes != null && volumes.Count > 0)
                {
                    Volume volume = volumes.First();

                    try
                    {
                        volume.ChangeLetter(letter);
                        return true;
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private static bool FormatDrive(string path, string label)
        {
            string attachedDevice = null;

            try
            {
                using (VirtualDisk disk = new VirtualDisk(path))
                {
                    disk.Open(VirtualDiskAccessMask.GetInfo);
                    attachedDevice = disk.GetAttachedPath();
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            if (attachedDevice != null)
            {
                List<Volume> volumes = new List<Volume>(Volume.GetVolumesOnPhysicalDrive(attachedDevice));
                if (volumes != null && volumes.Count > 0)
                {
                    Volume volume = volumes.First();

                    try
                    {
                        return FormatDisk.FormatDrive_Win32Api(volume.VolumeName.Replace("\\", "\\\\"), label);
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
            }

            return false;
        }
    }
}
