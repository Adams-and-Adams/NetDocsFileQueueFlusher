using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDocsFileQueueFlusher.Helpers
{
    public class Enums
    {
        public enum RestType
        {
            None,       //none 
            Get,        //Rest Get Request
            Post,       //Rest Post Request
            Put,        //Rest Put Request
            Delete		//Rest Delete Request
        }
    }
}
