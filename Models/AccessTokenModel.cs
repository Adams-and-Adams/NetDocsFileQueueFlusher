using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDocsFileQueueFlusher.Models
{
    public class AccessTokenModel
    {
        public string AccessTokenUrl { get; set; } = "";
        public string AccessTokenClientId { get; set; } = "";
        public string AccessTokenClientSecret { get; set; } = "";
        public string AccessTokenRespositoryId { get; set; } = "";
        public AccessTokenObject AccessTokenObject { get; set; } = new AccessTokenObject();
        public AccessTokenBody AccessTokenBody { get; set; } = new AccessTokenBody();
    }

    public class AccessTokenBody
    {
        public string GrandType { get; set; } = "";
        public string Scope { get; set; } = "";
    }

    public class AccessTokenObject
    {
        public string access_token { get; set; } = "";
        public string expires_in { get; set; } = "";
        public string token_type { get; set; } = "";
        public string error { get; set; } = "";
    }

}
