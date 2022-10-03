using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDocsFileQueueFlusher.Models
{
    public class FileObject
    {
        public string Guid { get; set; } = "";
        public string SourceFile { get; set; } = "";
        public string NdUrl { get; set; } = "";
        public string FolderId { get; set; } = "";

    }
}
