using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using NetDocsFileQueueFlusher.Helpers;
using NetDocsFileQueueFlusher.Models;

namespace NetDocsFileQueueFlusher.Helpers
{
    public interface IRestHelper
    {
        Task<Result<IRestResponse>> executeRequest(RestRequest request, string accessToken);
        RestRequest FormRequest(Enums.RestType rtype, string data);
    }
}