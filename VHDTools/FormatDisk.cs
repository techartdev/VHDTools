using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiskTools
{
    public static class FormatDisk
    {
        [DllImport("shell32.dll")]
        private static extern uint SHFormatDrive(IntPtr hwnd, uint drive, SHFormatFlags fmtID, SHFormatOptions options);

        private enum SHFormatFlags : uint
        {
            SHFMT_ID_DEFAULT = 0xFFFF,
            /// <summary>
            /// A general error occured while formatting. This is not an indication that the drive cannot be formatted though.
            /// </summary>
            SHFMT_ERROR = 0xFFFFFFFF,
            /// <summary>
            /// The drive format was cancelled by user/OS.
            /// </summary>
            SHFMT_CANCEL = 0xFFFFFFFE,
            /// <summary>
            /// A serious error occured while formatting. The drive is unable to be formatted by the OS.
            /// </summary>
            SHFMT_NOFORMAT = 0xFFFFFFD
        }

        [Flags]
        private enum SHFormatOptions : uint
        {
            /// <summary>
            /// Full formatting
            /// </summary>
            SHFMT_OPT_COMPLETE = 0x0,
            /// <summary>
            /// Quick Format
            /// </summary>
            SHFMT_OPT_FULL = 0x1,
            /// <summary>
            /// MS-DOS System Boot Disk
            /// </summary>
            SHFMT_OPT_SYSONLY = 0x2
        }

        /// <summary>
        /// Format a drive using Shell32.dll
        /// </summary>
        /// <param name="driveLetter">drive letter. Example : 'A', 'B', 'C', 'D', ..., 'Z'.</param>
        /// <param name="label">label for the drive</param>
        /// <param name="quickFormat">quick formatting?</param>
        /// <returns>true if success, false if failure</returns>
        [Obsolete("Unsupported by Microsoft nowadays. Prefer the FormatDrive() method")]
        public static bool FormatDrive_Shell32(char driveLetter, string label = "", bool quickFormat = true)
        {
            if (!char.IsLetter(driveLetter))
            {
                return false;
            }

            bool success = false;
            string drive = driveLetter + ":";
            try
            {
                var di = new DriveInfo(drive);
                var bytes = Encoding.ASCII.GetBytes(di.Name.ToCharArray());
                uint driveNumber = Convert.ToUInt32(bytes[0] - Encoding.ASCII.GetBytes(new[] { 'A' })[0]);
                var options = SHFormatOptions.SHFMT_OPT_COMPLETE;
                if (quickFormat)
                    options = SHFormatOptions.SHFMT_OPT_FULL;

                uint returnCode = SHFormatDrive(IntPtr.Zero, driveNumber, SHFormatFlags.SHFMT_ID_DEFAULT, options);
                if (returnCode == (uint)SHFormatFlags.SHFMT_ERROR)
                    throw new Exception("An error occurred during the format. This does not indicate that the drive is unformattable.");
                else if (returnCode == (uint)SHFormatFlags.SHFMT_CANCEL)
                    throw new OperationCanceledException("The format was canceled.");
                else if (returnCode == (uint)SHFormatFlags.SHFMT_NOFORMAT)
                    throw new IOException("The drive cannot be formatted.");

                SetLabel(driveLetter, label);
                success = true;
            }
            catch (Exception) { }
            return success;
        }

        /// <summary>
        /// set a drive label to the desired value
        /// </summary>
        /// <param name="driveLetter">drive letter. Example : 'A', 'B', 'C', 'D', ..., 'Z'.</param>
        /// <param name="label">label for the drive</param>
        /// <returns>true if success, false if failure</returns>
        public static bool SetLabel(char driveLetter, string label = "")
        {
            if (!char.IsLetter(driveLetter))
            {
                return false;
            }
            if (label == null)
            {
                label = "";
            }

            try
            {
                DriveInfo di = DriveInfo.GetDrives().FirstOrDefault(d => d.Name.StartsWith(driveLetter.ToString()));
                if (di != null)
                {
                    di.VolumeLabel = label;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Format a drive using Win32 API (Might have troubles formatting ram drives!)
        /// </summary>
        /// <param name="driveLetter">drive letter. Example : 'A', 'B', 'C', 'D', ..., 'Z'.</param>
        /// <param name="label">label for the drive</param>
        /// <param name="fileSystem">file system. Possible values : "FAT", "FAT32", "EXFAT", "NTFS", "UDF".</param>
        /// <param name="quickFormat">quick formatting?</param>
        /// <param name="enableCompression">enable drive compression?</param>
        /// <param name="clusterSize">cluster size. Possible value depends on the file system : 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, ...</param>
        /// <returns>true if success, false if failure</returns>
        public static bool FormatDrive_Win32Api(char driveLetter, string label = "", string fileSystem = "NTFS", bool quickFormat = true, bool enableCompression = false, int clusterSize = 8192)
        {
            #region args check

            if (!char.IsLetter(driveLetter) ||
                !IsFileSystemValid(fileSystem))
            {
                return false;
            }

            #endregion
            bool success = false;
            try
            {
                var moSearcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_Volume WHERE DriveLetter='" + driveLetter + ":'");
                foreach (ManagementObject mo in moSearcher.Get())
                {
                    mo.InvokeMethod("Format", new object[] { fileSystem, quickFormat, clusterSize, label, enableCompression });
                    success = true;
                }
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Format a drive using Win32 API (Might have troubles formatting ram drives!)
        /// </summary>
        /// <param name="volumeName">Name of the volume</param>
        /// <param name="label">label for the drive</param>
        /// <param name="fileSystem">file system. Possible values : "FAT", "FAT32", "EXFAT", "NTFS", "UDF".</param>
        /// <param name="quickFormat">quick formatting?</param>
        /// <param name="enableCompression">enable drive compression?</param>
        /// <param name="clusterSize">cluster size. Possible value depends on the file system : 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, ...</param>
        /// <returns>true if success, false if failure</returns>
        public static bool FormatDrive_Win32Api(string volumeName, string label = "", string fileSystem = "NTFS", bool quickFormat = true, bool enableCompression = false, int clusterSize = 8192)
        {
            bool success = false;
            try
            {
                var moSearcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_Volume WHERE DeviceID='" + volumeName + "'");
                foreach (ManagementObject mo in moSearcher.Get())
                {
                    mo.InvokeMethod("Format", new object[] { fileSystem, quickFormat, clusterSize, label, enableCompression });
                    success = true;
                }
            }
            catch (Exception ex)
            {
                success = false;
            }
            return success;
        }

        /// <summary>
        /// test if the provided filesystem value is valid
        /// </summary>
        /// <param name="fileSystem">file system. Possible values : "FAT", "FAT32", "EXFAT", "NTFS", "UDF".</param>
        /// <returns>true if valid, false if invalid</returns>
        public static bool IsFileSystemValid(string fileSystem)
        {
            #region args check

            if (fileSystem == null)
            {
                return false;
            }

            #endregion
            switch (fileSystem)
            {
                case "FAT":
                case "FAT32":
                case "EXFAT":
                case "NTFS":
                case "UDF":
                    return true;
                default:
                    return false;
            }
        }

        public static string ResolveFileSystemType(FileSystemType type)
        {
            switch (type)
            {
                case FileSystemType.FAT:
                    return "FAT";
                case FileSystemType.FAT32:
                    return "FAT32";
                case FileSystemType.EXFAT:
                    return "EXFAT";
                case FileSystemType.NTFS:
                    return "NTFS";
                case FileSystemType.UDF:
                    return "UDF";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }

    public enum FileSystemType
    {
        FAT,
        FAT32,
        EXFAT,
        NTFS,
        UDF
    }
}
