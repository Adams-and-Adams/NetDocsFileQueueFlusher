using RestSharp;

namespace NetDocsFileQueueFlusher.Helpers
{
    public interface IRestHelper
    {
        Task<Result<IRestResponse>> executeRequest(RestRequest request, string accessToken);
        RestRequest FormRequest(Enums.RestType rtype, string data);
    }
}