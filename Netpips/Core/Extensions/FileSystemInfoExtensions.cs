using System.IO;

namespace Netpips.Core.Extensions
{
    public static class FileSystemInfoExtensions
    {
        public static bool IsDirectory(this FileSystemInfo fsInfo) =>
            fsInfo.Attributes.HasFlag(FileAttributes.Directory);

        public static FileSystemInfo Rename(this FileSystemInfo fsInfo, string newName)
        {
            // cannot use DirectoryInfo/FileInfo .MoveTo due to the added trailing backslashes to the FullName
            var newPath = Path.Combine(Path.GetDirectoryName(fsInfo.FullName), newName);
            if (fsInfo is DirectoryInfo)
            {
                Directory.Move(fsInfo.FullName, newPath);
                return new DirectoryInfo(newPath);
            }
            File.Move(fsInfo.FullName, newPath);
            return new FileInfo(newPath);
        }

        public static void Remove(this FileSystemInfo fsInfo)
        {
            if (fsInfo.IsDirectory())
            {
                ((DirectoryInfo)fsInfo).Delete(true);
            }
            else
            {
                fsInfo.Delete();
            }
            fsInfo.Refresh();
        }
    }
}
