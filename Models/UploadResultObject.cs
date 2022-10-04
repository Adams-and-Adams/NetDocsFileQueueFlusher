using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDocsFileQueueFlusher.Models
{
    public class UploadResultObject
    {
        public int[]? LatestVersionLabel { get; set; }
        public string? VersionLabel { get; set; }
        public string? aclStatus { get; set; }
        public DateTime created { get; set; }
        public string? createdBy { get; set; }
        public string? createdByGuid { get; set; }
        public string? envId { get; set; }
        public string? extension { get; set; }
        public string id { get; set; }
        public int latestVersionNumber { get; set; }
        public bool locked { get; set; }
        public DateTime modified { get; set; }
        public string? modifiedBy { get; set; }
        public string? modifiedByGuid { get; set; }
        public string? name { get; set; }
        public int officialVer { get; set; }
        public int size { get; set; }
        public long syncMod { get; set; }
        public string? url { get; set; }
        public int versions { get; set; }
    }

}
