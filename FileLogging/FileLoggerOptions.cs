using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDocsFileQueueFlusher.FileLogging
{
    public class FileLoggerOptions
    {
        public virtual string FilePath { get; set; } = "";
        public virtual string FolderPath { get; set; } = "";

    }
}
