using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetDocsFileQueueFlusher.Models;
using RestSharp;
using static NetDocsFileQueueFlusher.Helpers.Enums;

namespace NetDocsFileQueueFlusher.Helpers
{
    public class RestHelper : IRestHelper
    {
        public async Task<Result<IRestResponse>> executeRequest(RestRequest request, string accessToken)
        {
            try
            {
                RestClient restClient = new RestClient();
                restClient.ConfigureWebRequest(r => r.ProtocolVersion = HttpVersion.Version10);
                restClient.Timeout = 5000 * 60 * 10;

                request.Timeout = 5000 * 60 * 10;
                request.AddHeader("Authorization", "Bearer " + accessToken);
                var response = await restClient.ExecuteAsync(request);

                //if(response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent) return Result<IRestResponse>.Failure(response.Content);

                return Result<IRestResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<IRestResponse>.Failure($"Execute Rest Request Failed : {ex.Message}");
            }
        }


        public RestRequest FormRequest(RestType rtype, string data)
        {
            Method method;
            switch (rtype)
            {
                case RestType.Get:
                    method = Method.GET;
                    break;
                case RestType.Post:
                    method = Method.POST;
                    break;
                case RestType.Put:
                    method = Method.PUT;
                    break;
                case RestType.Delete:
                    method = Method.DELETE;
                    break;
                default: return null;
            }

            RestRequest request = new RestRequest(data, method);

            return request;
        }
    }
}
