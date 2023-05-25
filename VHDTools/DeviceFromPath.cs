﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiskTools
{
    public static class DeviceFromPath
    {

        public static string GetDevice(string path)
        {
            string device = FindCdRom(path) ?? FindPhysicalDrive(path);
            return device;
        }


        #region PhysicalDrive

        private static string FindPhysicalDrive(string path)
        {
            FileSystemInfo iDirectory = null;
            ObjectQuery wmiQuery = new ObjectQuery("SELECT Antecedent, Dependent FROM Win32_LogicalDiskToPartition");
            ManagementObjectSearcher wmiSearcher = new ManagementObjectSearcher(wmiQuery);

            FileInfo iFile = new FileInfo(path);
            try
            {
                if ((iFile.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    iDirectory = new DirectoryInfo(iFile.FullName);
                }
                else
                {
                    iDirectory = iFile;
                    throw new FormatException("Argument is not a directory.");
                }
            }
            catch (IOException)
            {
                iDirectory = new DirectoryInfo(iFile.FullName);
            }


            foreach (ManagementBaseObject iReturn in wmiSearcher.Get())
            {
                string disk = GetSubsubstring((string)iReturn["Antecedent"], "Win32_DiskPartition.DeviceID", "Disk #", ",");
                string partition = GetSubsubstring((string)iReturn["Dependent"], "Win32_LogicalDisk.DeviceID", "", "");
                if (iDirectory.Name.StartsWith(partition, StringComparison.InvariantCultureIgnoreCase))
                {
                    int wmiPhysicalDiskNumber = -1;
                    if (int.TryParse(disk, NumberStyles.Integer, CultureInfo.InvariantCulture, out wmiPhysicalDiskNumber))
                    {
                        return @"\\?\PHYSICALDRIVE" + wmiPhysicalDiskNumber.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        throw new FormatException("Cannot retrieve physical disk number.");
                    }
                }
            }
            return null;
        }

        private static string GetSubsubstring(string value, string type, string start, string end)
        {
            int xStart0 = value.IndexOf(":" + type + "=\"", StringComparison.Ordinal);
            if (xStart0 < 0) { return null; }
            int xStart1 = value.IndexOf("\"", xStart0 + 1, StringComparison.Ordinal);
            if (xStart1 < 0) { return null; }
            int xEnd1 = value.IndexOf("\"", xStart1 + 1, StringComparison.Ordinal);
            if (xEnd1 < 0) { return null; }
            string extract = value.Substring(xStart1 + 1, xEnd1 - xStart1 - 1);

            int xStart2 = 0;
            if (!string.IsNullOrEmpty(start)) { xStart2 = extract.IndexOf(start, StringComparison.Ordinal); }
            if (xStart2 < 0) { return null; }

            int xEnd2 = extract.Length;
            if (!string.IsNullOrEmpty(end)) { xEnd2 = extract.IndexOf(end, StringComparison.Ordinal); }
            if (xEnd2 < 0) { return null; }

            return extract.Substring(xStart2 + start.Length, xEnd2 - xStart2 - start.Length);
        }

        #endregion


        #region CdRom

        private static string FindCdRom(string path)
        {
            string dosDevice = path[0] + ":";
            StringBuilder sb = new StringBuilder(64);
            if (NativeMethods.QueryDosDeviceW(dosDevice, sb, (uint)sb.Capacity) > 0)
            {
                string dosPath = sb.ToString();
                Debug.WriteLine(sb.ToString() + " is at " + dosDevice);
                if (dosPath.StartsWith(@"\Device\CdRom", StringComparison.OrdinalIgnoreCase))
                {
                    int cdromNumber = 0;
                    if (int.TryParse(dosPath.Substring(13), NumberStyles.Integer, CultureInfo.InvariantCulture, out cdromNumber))
                    {
                        return @"\\?\CDROM" + cdromNumber.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
            return null;
        }


        private static class NativeMethods
        {

            [DllImport("kernel32.dll", EntryPoint = "QueryDosDeviceW")]
            public static extern uint QueryDosDeviceW(
                [In] [MarshalAs(UnmanagedType.LPWStr)] string lpDeviceName,
                [Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpTargetPath,
                uint ucchMax);

        }

        #endregion

    }
}
