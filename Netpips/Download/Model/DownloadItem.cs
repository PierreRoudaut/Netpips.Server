using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Netpips.Identity.Model;
using Netpips.Media.Model;
using Newtonsoft.Json;

namespace Netpips.Download.Model
{
    public enum DownloadType
    {
        DDL,
        P2P
    }

    public enum DownloadState
    {
        Downloading,
        Canceled,
        Completed,
        Processing
    }
    public class DownloadItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Token { get; set; }
        public string Name { get; set; }
        public long TotalSize { get; set; }
        public string FileUrl { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public DateTime DownloadedAt { get; set; }
        public DateTime CanceledAt { get; set; }
        public DownloadState State { get; set; }
        public DownloadType Type { get; set; }
        public string Hash { get; set; }
        public bool Archived { get; set; }


        [NotMapped]
        public long DownloadedSize { get; set; }

        internal string _movedFiles { get; set; }
        [NotMapped]
        public List<MediaItem> MovedFiles
        {
            get =>
                !string.IsNullOrEmpty(_movedFiles)
                    ? JsonConvert.DeserializeObject<List<MediaItem>>(_movedFiles)
                    : new List<MediaItem>();
            set => _movedFiles = JsonConvert.SerializeObject(value);
        }


        [JsonIgnore]
        public string MainFilename
        {
            get
            {
                var file = MovedFiles?.OrderByDescending(f => f.Size).FirstOrDefault()?.Path;
                file = !string.IsNullOrEmpty(file) ? Path.GetFileNameWithoutExtension(file) : Name;
                return file;
            }
        }

        // nav property
        public User Owner { get; set; }
        public Guid OwnerId { get; set; }

        [ExcludeFromCodeCoverage]
        public bool ShouldSerializeHash() => Type == DownloadType.P2P;
        [ExcludeFromCodeCoverage]
        public bool ShouldSerializeDownloadedSize() => State == DownloadState.Downloading;
        [ExcludeFromCodeCoverage]
        public bool ShouldSerializeCompletedAt() => State == DownloadState.Completed;
        [ExcludeFromCodeCoverage]
        public bool ShouldSerializeDownloadedAt() => State == DownloadState.Processing || State == DownloadState.Completed;
        [ExcludeFromCodeCoverage]
        public bool ShouldSerializeCanceledAt() => State == DownloadState.Canceled;
        [ExcludeFromCodeCoverage]
        public bool ShouldSerializeMovedFiles() => State == DownloadState.Completed;
    }
}