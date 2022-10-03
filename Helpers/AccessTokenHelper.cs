using NetDocsFileQueueFlusher.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDocsFileQueueFlusher.Helpers
{
    public class AccessTokenHelper
    {
        public async Task<Result<string>> GetAccessToken(AccessTokenModel accessTokenModel)
        {
            try
            {
                var client = new RestClient(accessTokenModel.AccessTokenUrl);
                var request = new RestRequest(Method.POST);
                string autho = Base64Encode($"{accessTokenModel.AccessTokenClientId}|{accessTokenModel.AccessTokenRespositoryId}:{accessTokenModel.AccessTokenClientSecret}");

                client.Timeout = -1;
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Authorization", $"Basic {autho}");
                request.AddHeader("Accept", "application/json");
                request.AddParameter("grant_type", accessTokenModel.AccessTokenBody.GrandType);
                request.AddParameter("scope", accessTokenModel.AccessTokenBody.Scope);

                var response = await client.ExecuteAsync(request);
                AccessTokenObject? tokenObject = JsonConvert.DeserializeObject<AccessTokenObject>(response.Content);

                return Result<string>.Success(tokenObject.access_token);
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Failed to get an Access Token : {ex.Message}");
            }
        }

        public string Base64Encode(string toEncode)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(toEncode);
            return Convert.ToBase64String(byteArray);
        }
    }
}

