using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDocsFileQueueFlusher.Models
{
    public class SettingsModel
    {
        public string DocumentServerFolder { get; set; } = "";
        public string DocumentMapDrive { get; set; } = "";
        public AccessTokenModel AccessTokenModel { get; set; } = new AccessTokenModel();
    }
}

