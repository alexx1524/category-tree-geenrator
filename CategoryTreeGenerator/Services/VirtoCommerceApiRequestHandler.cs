using Microsoft.Rest;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace CategoryTreeGenerator.Services
{
    public class VirtoCommerceApiRequestHandler : ServiceClientCredentials
    {
        private readonly string _appId;
        private readonly string _secretKey;

        public VirtoCommerceApiRequestHandler(string appId, string secretKey)
        {
            _appId = appId;
            _secretKey = secretKey;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            AddAuthorization(request);

            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }

        private void AddAuthorization(HttpRequestMessage request)
        {
            ApiRequestSignature signature = new ApiRequestSignature { AppId = _appId };

            NameValuePair[] parameters = new[]
            {
                new NameValuePair(null, _appId),
                new NameValuePair(null, signature.TimestampString)
            };

            signature.Hash = HmacUtility.GetHashString(key => new HMACSHA256(key), _secretKey, parameters);

            request.Headers.Authorization = new AuthenticationHeaderValue("HMACSHA256", signature.ToString());
        }
    }
}