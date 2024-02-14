# VHDTools

![Nuget](https://img.shields.io/nuget/v/VHDTools)

Provides functionality for managing disk operations, including virtual disks and attached physical drives.

## Introduction

The library is designed to handle disk management tasks such as creating virtual disks, attaching and detaching disks, initializing partitions, formatting volumes, and changing drive letters. It provides a convenient interface for working with both virtual and physical disks.

### DiskManager 

To create an instance of the DiskManager class, you can use one of the available constructors.

```csharp
    // Example 1: Create a DiskManager for a virtual disk
    string virtualDiskPath = "path/to/virtual/disk.vhd";
    DiskManager diskManager = new DiskManager(virtualDiskPath);
    
    // Example 2: Create a DiskManager for an attached physical drive
    char driveLetter = 'C';
    DiskManager diskManager = new DiskManager(driveLetter);
```

### Perform Disk Operations

Once you have an instance of the DiskManager class, you can perform various disk operations.

```csharp
    // Example: Attach a virtual disk
    diskManager.AttachVirtualDisk();
    
    // Example: Detach the attached virtual disk
    diskManager.DetachVirtualDisk();
    
    // Example: Initialize the virtual disk partition
    diskManager.InitializeVirtualDiskPartition();
    
    // Example: Format all volumes of the drive
    diskManager.FortmatAllVolumesOfDrive(FileSystemType.NTFS);
    
    // Example: Format a specific volume
    Volume volume = diskManager.DiskVolumes.First();
    diskManager.FormatSpecificVolume(volume, FileSystemType.NTFS);
    
    // Example: Set a drive letter for a volume
    Volume volume = diskManager.DiskVolumes.First();
    char newDriveLetter = 'D';
    diskManager.SetDriveLetter(volume, newDriveLetter);
    
    // Example: Change the drive letter of a volume
    char oldDriveLetter = 'C';
    char newDriveLetter = 'D';
    diskManager.ChangeDriveLetter(oldDriveLetter, newDriveLetter);
    
    // Example: Expand the size of a virtual disk
    ulong newSizeInBytes = 1024 * 1024 * 1024; // 1GB
    diskManager.ExpandVirtualDisk(newSizeInBytes);
```

## DiskIO Class

### InitializeDisk
The DiskIO class provides methods for disk initialization and updating disk partitions in a Windows environment.

```csharp
    public static void InitializeDisk(string path)
```

This method initializes a disk at the specified path. It performs the following operations:

1. Opens the disk using the CreateFile function.
2. Checks if the handle is invalid and throws a Win32Exception if it is.
3. Generates a random signature for the disk.
4. Creates a disk using the DeviceIoControl function with the IOCTL_DISK_CREATE_DISK control code.
5. Updates the disk properties cache using the DeviceIoControl function with the IOCTL_DISK_UPDATE_PROPERTIES control code.
6. Retrieves the partition information using the DeviceIoControl function with the IOCTL_DISK_GET_PARTITION_INFO control code.
7. Constructs a new drive layout with one partition.
8. Sets the drive layout using the DeviceIoControl function with the IOCTL_DISK_SET_DRIVE_LAYOUT_EX control code.
9. Updates the disk properties cache again.

### UpdateDiskPartition

```csharp
    public static void UpdateDiskPartition(string path, long growSize)
```

This method updates the size of a disk partition at the specified path by increasing its size. It performs the following operations:

1. Opens the disk using the CreateFile function.
2. Checks if the handle is invalid and throws a Win32Exception if it is.
3. Retrieves the partition information using the DeviceIoControl function with the IOCTL_DISK_GET_PARTITION_INFO control code.
4. Creates a DISK_GROW_PARTITION structure with the new size to grow the partition.
5. Calls the DeviceIoControl function with the IOCTL_DISK_GROW_PARTITION control code to grow the partition.
6. Updates the disk properties cache using the DeviceIoControl function with the IOCTL_DISK_UPDATE_PROPERTIES control code.
7. Retrieves the disk geometry using the DeviceIoControl function with the DiskUpdateDriveSize control code.
8. Retrieves the extended partition information using the DeviceIoControl function with the DiskGetPartitionInfoEx control code.
9. Calculates the new length of the partition in sectors.
10. Calls the DeviceIoControl function with the FsctlExtendVolume control code to extend the volume.

### Usage

```csharp
    // Example usage of the DiskIO class
    string diskPath = "C:\\MyDisk";
    long growSize = 1024 * 1024 * 1024; // 1 GB
    
    try
    {
        DiskIO.InitializeDisk(diskPath);
        DiskIO.UpdateDiskPartition(diskPath, growSize);
    }
    catch (Win32Exception ex)
    {
        Console.WriteLine("An error occurred: " + ex.Message);
    }
```

## FormatDisk Class

### FormatDrive_Shell32

```csharp
    [Obsolete("Unsupported by Microsoft nowadays. Prefer the FormatDrive() method")]
    public static bool FormatDrive_Shell32(char driveLetter, string label = "", bool quickFormat = true)
```

This method formats a drive using Shell32.dll. It accepts the following parameters:
- driveLetter: The drive letter to format (e.g., 'A', 'B', 'C', ...).
- label (optional): The label to assign to the drive.
- quickFormat (optional): A boolean indicating whether to perform a quick format. Default is true.
Returns: true if the format is successful, false otherwise.

### SetLabel

```csharp
    public static bool SetLabel(char driveLetter, string label = "")
```

This method sets the label for a drive. It accepts the following parameters:

- driveLetter: The drive letter to set the label for (e.g., 'A', 'B', 'C', ...).
- label (optional): The label to assign to the drive. If not specified, the label will be empty.

Returns: true if the label is set successfully, false otherwise.

### FormatDrive_Win32Api

```csharp
    public static bool FormatDrive_Win32Api(char driveLetter, string label = "", string fileSystem = "NTFS", bool quickFormat = true, bool enableCompression = false, int clusterSize = 8192)
```

This method formats a drive using the Win32 API. It accepts the following parameters:

- driveLetter: The drive letter to format (e.g., 'A', 'B', 'C', ...).
- label (optional): The label to assign to the drive.
- fileSystem (optional): The file system to use for the format. Possible values are "FAT", "FAT32", "EXFAT", "NTFS", "UDF". Default is "NTFS".
- quickFormat (optional): A boolean indicating whether to perform a quick format. Default is true.
- enableCompression (optional): A boolean indicating whether to enable drive compression. Default is false.
- clusterSize (optional): The cluster size for the file system. The possible values depend on the file system. Default is 8192.

Returns: true if the format is successful, false otherwise.

### FormatDrive_Win32Api

```csharp
    public static bool FormatDrive_Win32Api(string volumeName, string label = "", string fileSystem = "NTFS", bool quickFormat = true, bool enableCompression = false, int clusterSize = 8192)
```

This overload of the FormatDrive_Win32Api method formats a drive using the Win32 API. It accepts the following parameters:

- volumeName: The name of the volume to format.
- label (optional): The label to assign to the drive.
- fileSystem (optional): The file system to use for the format. Possible values are "FAT", "FAT32", "EXFAT", "NTFS", "UDF". Default is "NTFS".
- quickFormat (optional): A boolean indicating whether to perform a quick format. Default is true.
- enableCompression (optional): A boolean indicating whether to enable drive compression. Default is false.

### IsFileSystemValid

```csharp
    public static bool IsFileSystemValid(string fileSystem)
```

This method checks if a provided file system value is valid. It accepts the following parameter:

- fileSystem: The file system value to check.

Returns: true if the file system is valid, false otherwise.

### ResolveFileSystemType

```csharp
    public static string ResolveFileSystemType(FileSystemType type)
```

This method resolves a FileSystemType enumeration value to its corresponding string representation. It accepts the following parameter:

- type: The FileSystemType enumeration value to resolve.

Returns: The string representation of the file system type.

## HardDiskFooter

The HardDiskFooter class represents a footer structure for a hard disk image. It contains various properties and methods to manipulate and retrieve information from the footer.

### Constructors
- HardDiskFooter(): Creates a new instance of the HardDiskFooter class with default values.
- HardDiskFooter(byte[] bytes): Creates a new instance of the HardDiskFooter class from existing footer bytes.

### Properties
- Cookie: Gets or sets the cookie that identifies the original creator of the hard disk image.
- Features: Gets or sets the features enabled for the hard disk image.
- FileFormatVersion: Gets or sets the version of the file format specification used in creating the file.
- DataOffset: Gets or sets the byte offset to the next structure in the file.
- TimeStamp: Gets or sets the creation time of the hard disk image.
- CreatorApplication: Gets or sets the application that created the hard disk image.
- CreatorVersion: Gets or sets the version of the application that created the hard disk image.
- CreatorHostOs: Gets or sets the type of host operating system the disk image is created on.
- OriginalSize: Gets or sets the size of the hard disk in bytes at creation time.
- CurrentSize: Gets or sets the current size of the hard disk in bytes.
- DiskGeometryCylinders: Gets or sets the cylinder value for the hard disk.
- DiskGeometryHeads: Gets or sets the heads value for the hard disk.
- DiskGeometrySectors: Gets or sets the sectors per track value for the hard disk.
- DiskType: Gets or sets the type of virtual disk.
- Checksum: Gets or sets the checksum of the hard disk footer.
- IsChecksumCorrect: Gets whether the checksum is correct.
- Bytes: Gets the byte representation of the footer structure.

### Methods

- BeginUpdate(): Stops processing checksum updates until EndUpdate() is called.
- EndUpdate(): Recalculates fields not updated since BeginUpdate().
- UpdateChecksum(): Updates the checksum of the footer.
- SetSize(UInt64 size): Sets the size of the hard disk image and updates related fields.

## ReFS

The ReFS class provides static methods for working with ReFS (Resilient File System) features.

### Methods

- RemoveIntegrityStream(FileInfo file): Removes the integrity stream from the specified file.
- RemoveIntegrityStream(SafeFileHandle handle): Removes the integrity stream from the file associated with the specified file handle.
- HasIntegrityStream(FileInfo file): Checks if the specified file has an integrity stream.
- HasIntegrityStream(SafeFileHandle handle): Checks if the file associated with the specified file handle has an integrity stream.

Please note that these methods rely on native methods and Win32 API calls to interact with ReFS features.

```csharp
    FileInfo fileInfo = new FileInfo("path/to/file.txt");
    
    if (ReFS.HasIntegrityStream(fileInfo))
    {
        ReFS.RemoveIntegrityStream(fileInfo);
    }
```

Make sure to handle any exceptions that may be thrown during the ReFS operations.

*Note: This class assumes familiarity with ReFS concepts and features.*

# VirtualDisk Class

The `VirtualDisk` class allows manipulation with Virtual Disk files.

## Constructor

Creates new instance.

### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| `fileName` | `string` | Full path of VHD file. |

## Properties

| Name | Type | Description |
| ---- | ---- | ----------- |
| `FileName` | `string` | Gets file name of VHD. |
| `DiskType` | `VirtualDiskType` | Gets type of open virtual device. If device is not open, type will be AutoDetect. Once device is opened, type will change to either Iso or Vhd. |
| `IsOpen` | `bool` | Gets whether connection to file is currently open. |

## Methods

### `public void Open()`

Opens connection to file.

### `public void Open(VirtualDiskAccessMask fileAccess)`

Opens connection to file.

### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| `fileAccess` | `VirtualDiskAccessMask` | Defines required access. |

### `private void Open(VirtualDiskAccessMask fileAccess, VirtualDiskType type)`

Opens connection to file.

### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| `fileAccess` | `VirtualDiskAccessMask` | Defines required access. |
| `type` | `VirtualDiskType` | Disk type. |

### `public void Dispose()`

Disposes current `VirtualDisk` instance.

### `public void Create(long size)`

Creates new virtual disk.

### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| `size` | `long` | Size in bytes. |

### Exceptions

| Exception | Description |
| --------- | ----------- |
| `ArgumentException` | Invalid parameter. |
| `Win32Exception` | Native error. |
| `FileNotFoundException` | File not found. |
| `InvalidDataException` | File type not recognized. |
| `IOException` | File already exists. -or- Virtual disk creation could not be completed due to a file system limitation. |

## DiskTools.Volume Class

The `DiskTools.Volume` class represents a disk volume on a Windows system. This class provides methods to perform various operations on a volume, such as getting drive letter information, changing volume drive letter and removing volume drive letter.

### Constructor

- `Volume(string volumeName)` : Initializes a new instance of the `Volume` class with the specified volume name.

### Properties

- `VolumeName`: Gets the name of the volume.
- `DriveLetter3`: Returns drive letter with colon (:) and trailing backslash (\).
- `DriveLetter2`: Returns drive letter with colon (:) but without trailing backslash (\).
- `PhysicalDriveNumber`: Gets the physical drive number of the volume.
- `PhysicalDriveExtentOffset`: Gets the starting offset on the disk of the volume.
- `PhysicalDriveExtentLength`: Gets the length of the volume in bytes.

### Methods

- `ChangeLetter(string newLetter)`: Changes the drive letter of the volume to the specified value.
- `RemoveLetter()`: Removes the drive letter of the volume.
- `GetFromLetter(string driveLetter)`: Returns a `Volume` instance for a given drive letter.
- `GetVolumesOnPhysicalDrive(string physicalDrive)`: Returns a list of `Volume` instances for a given physical drive. 

### Usage

```csharp
using DiskTools;
// Get Volume instance by drive letter 
Volume volume = Volume.GetFromLetter("F:\");
// Change drive letter for the volume 
volume.ChangeLetter("G:\");
// Remove drive letter for the volume 
volume.RemoveLetter();
// Get all volumes for physical drive number 0 
var volumes = Volume.GetVolumesOnPhysicalDrive(0);
```

## Contact
Visit https://scrapeweb.site

Or join the **Web Scraping and Automation** community on Discord: https://discord.gg/f3EfBQamnT

---

If you find this app helpful and would like to support the development of more tools and projects, consider buying me a coffee. Your support is greatly appreciated!

[![Buy Me A Coffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/techartdev)

Thank you for your support!

