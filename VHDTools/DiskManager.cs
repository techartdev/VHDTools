using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskTools
{
    public class DiskManager : IDisposable
    {
        public bool IsVirtualDisk { get; private set; }

        public bool IsAttached { get; private set; }

        public bool HasDriveLetter { get; private set; }

        public List<Volume> DiskVolumes { get; private set; }

        public VirtualDisk VirtualDisk { get; private set; }

        public string DiskPath { get; private set; }

        public DiskManager(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("File not found!");
            }

            VirtualDisk = new VirtualDisk(path);

            try
            {
                VirtualDisk.Open(VirtualDiskAccessMask.All);
            }
            catch (Exception e)
            {
                throw new Exception("Drive not found. Please, create the drive first using the static method CreateVirtualDisk");
            }

            DiskPath = path;
            IsAttached = !string.IsNullOrEmpty(VirtualDisk.GetAttachedPath());
            IsVirtualDisk = true;

            if (IsAttached)
            {
                DiskVolumes = Volume.GetVolumesOnPhysicalDrive(VirtualDisk.GetAttachedPath()).ToList();
                HasDriveLetter = DiskVolumes.Any(wr => !string.IsNullOrEmpty(wr.DriveLetter3));
            }
        }

        public DiskManager(char letter)
        {
            string driveName = letter.ToString().ToUpper() + @":\";

            DriveInfo drive = DriveInfo.GetDrives().FirstOrDefault(wr => wr.Name == driveName);

            if (drive == null)
            {
                throw new Exception(string.Format("DiskManager cannot find disk with name {0}", driveName));
            }

            DiskPath = drive.RootDirectory.ToString();

            DiskVolumes = new List<Volume>();
            DiskVolumes.Add(Volume.GetFromLetter(driveName));
            IsAttached = true;
        }

        public static void CreateVirtualDisk(string path, VirtualDiskType type, long size, VirtualDiskCreateOptions createOptions = VirtualDiskCreateOptions.FullPhysicalAllocation)
        {
            using (VirtualDisk vhd = new VirtualDisk(path))
            {
                vhd.Create(size, createOptions, 0, 512, type);
                vhd.Close();
            }
        }

        public void AttachVirtualDisk(VirtualDiskAttachOptions options = VirtualDiskAttachOptions.PermanentLifetime | VirtualDiskAttachOptions.NoDriveLetter, bool readOnly = false)
        {
            if (!IsVirtualDisk)
            {
                throw new Exception("AttachVirtualDisk works only with Virtual Disks!");
            }

            if (IsAttached) return;

            if (readOnly)
            {
                options |= VirtualDiskAttachOptions.ReadOnly;
            }

            VirtualDisk.Attach(options);
            IsAttached = true;
        }

        public void DetachVirtualDisk()
        {
            if (IsAttached)
            {
                VirtualDisk.Detach();
                IsAttached = false;
            }
        }

        public void InitializeVirtualDiskPartition()
        {
            if (!IsVirtualDisk)
            {
                throw new Exception("InitializeVirtualDiskPartition works only with Virtual Disks");
            }

            if (!IsAttached)
            {
                throw new Exception("The Virtual Disk must be attached first!");
            }

            DiskIO.InitializeDisk(VirtualDisk.GetAttachedPath());

            DiskVolumes = Volume.GetVolumesOnPhysicalDrive(VirtualDisk.GetAttachedPath()).ToList();
            HasDriveLetter = DiskVolumes.Any(wr => !string.IsNullOrEmpty(wr.DriveLetter3));
        }

        public void FortmatAllVolumesOfDrive(FileSystemType fileSystemType)
        {
            if (IsVirtualDisk)
            {
                if (!IsAttached)
                {
                    throw new Exception("The Virtual Disk must be attached first!");
                }

                DiskVolumes = new List<Volume>(Volume.GetVolumesOnPhysicalDrive(VirtualDisk.GetAttachedPath()));

                foreach (Volume volume in DiskVolumes)
                {
                    FormatDisk.FormatDrive_Win32Api(volume.VolumeName.Replace("\\", "\\\\"), "", FormatDisk.ResolveFileSystemType(fileSystemType));
                }
            }
            else
            {
                foreach (Volume volume in DiskVolumes)
                {
                    FormatDisk.FormatDrive_Win32Api(volume.VolumeName.Replace("\\", "\\\\"), "", FormatDisk.ResolveFileSystemType(fileSystemType));
                }
            }
        }

        public void FormatSpecificVolume(Volume volume, FileSystemType fileSystemType = FileSystemType.NTFS, string customLabel = "")
        {
            if (DiskVolumes.All(wr => wr != volume))
            {
                throw new Exception("The specified volume is not found on the loaded drive. Make sure you call FormatVolume for the drive you loaded.");
            }

            FormatDisk.FormatDrive_Win32Api(volume.VolumeName.Replace("\\", "\\\\"), customLabel, FormatDisk.ResolveFileSystemType(fileSystemType));
        }

        public void SetDriveLetter(Volume volume, char newLetter)
        {
            string newPath = newLetter.ToString().ToUpper() + @":\";

            volume.ChangeLetter(newPath);
        }

        public void ChangeDriveLetter(char oldLetter, char newLetter)
        {
            string oldPath = oldLetter.ToString().ToUpper() + @":\";
            string newPath = newLetter.ToString().ToUpper() + @":\";

            if (DiskVolumes.All(wr => wr.DriveLetter3 != oldPath))
            {
                throw new Exception("The specified volume is not found on the loaded drive. Make sure you call FormatVolume for the drive you loaded.");
            }

            Volume volume = DiskVolumes.First(wr => wr.DriveLetter3 == oldPath);

            volume.ChangeLetter(newPath);
        }

        public void ExpandVirtualDisk(ulong newSize)
        {
            VirtualDisk.Expand(newSize);
        }

        public void Dispose()
        {
            VirtualDisk?.Close();
            VirtualDisk?.Dispose();
        }
    }
}
