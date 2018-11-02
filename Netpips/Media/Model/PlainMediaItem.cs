using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Web;
using Netpips.Core.Extensions;
using Newtonsoft.Json;

namespace Netpips.Media.Model
{
    public class MediaItem
    {
        public string Path { get; set; }

        public string Parent { get; set; }

        public long? Size { get; set; }
    }


    public class PlainMediaItem : IEqualityComparer<PlainMediaItem>
    {
        private readonly string basePath;
        [JsonIgnore]
        public FileSystemInfo FileSystemInfo { get; private set; }

        public string Path => HttpUtility.UrlDecode(new Uri(FileSystemInfo.FullName).AbsolutePath.Substring(new Uri(basePath).AbsolutePath.Length + 1));

        public long? Size => FileSystemInfo.IsDirectory() ? (long?)null : ((FileInfo)FileSystemInfo).Length;

        public string Parent
        {
            get
            {
                if (FileSystemInfo.Name == Path)
                {
                    return null;
                }
                return HttpUtility.UrlDecode(new Uri(System.IO.Path.GetDirectoryName(FileSystemInfo.FullName)).AbsolutePath.Substring(new Uri(basePath).AbsolutePath.Length + 1));
            }
        }

        [JsonIgnore]
        public MediaItem ToMediaItem => new MediaItem { Path = Path, Parent = Parent, Size = Size};

        [ExcludeFromCodeCoverage]
        public bool ShouldSerializeSize() => Size.HasValue;

        [ExcludeFromCodeCoverage]
        public bool ShouldSerializeParent() => Parent != null;

        public PlainMediaItem(FileSystemInfo fsInfo, string basePath)
        {
            if (!fsInfo.FullName.StartsWith(basePath) || basePath.StartsWith(fsInfo.FullName))
            {
                throw new InvalidOperationException("Invalid path: " + fsInfo.FullName + " basepath: " + basePath);
            }

            this.basePath = basePath;
            FileSystemInfo = fsInfo;
        }

        [JsonIgnore]
        public bool IsRootMediaFolder => Path.Split('/').Length < 2;

        public void Rename(string newName)
        {
            if (IsRootMediaFolder)
            {
                throw new InvalidOperationException("Cannot rename root media folder");
            }
            FileSystemInfo = FileSystemInfo.Rename(newName.ToCleanFileName());
        }


        public void Delete()
        {
            if (IsRootMediaFolder)
            {
                throw new InvalidOperationException("Cannot delete root media folder");
            }
            FileSystemInfo.Remove();
        }

        [ExcludeFromCodeCoverage]
        public bool Equals(PlainMediaItem x, PlainMediaItem y)
        {
            return x.Path == y.Path && x.Parent == y.Parent && x.Size == y.Size;
        }

        [ExcludeFromCodeCoverage]
        public int GetHashCode(PlainMediaItem obj)
        {
            return (int)(obj.Path.GetHashCode() * obj.Parent.GetHashCode() * obj.Size.GetValueOrDefault());
        }
    }
}