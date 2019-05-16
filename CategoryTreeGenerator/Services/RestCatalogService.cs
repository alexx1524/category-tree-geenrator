using Microsoft.Rest;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;

namespace CategoryTreeGenerator.Services
{
    public class RestCatalogService : ICatalogService
    {
        private readonly CatalogModuleCategories _categories;
        private readonly CatalogModuleProducts _products;

        public RestCatalogService(Uri uri, string appId, string secretKey)
        {
            ServicePointManager.UseNagleAlgorithm = false;

            HttpClientHandler httpHandlerWithCompression = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            VirtoCommerceApiRequestHandler handler = new VirtoCommerceApiRequestHandler(appId, secretKey);

            _categories =
                new CatalogModuleCategories(
                    new VirtoCommerceCatalogRESTAPIdocumentation(uri, handler, httpHandlerWithCompression));

            _products =
                new CatalogModuleProducts(
                    new VirtoCommerceCatalogRESTAPIdocumentation(uri, handler, httpHandlerWithCompression));
        }

        public async Task<HttpResponseMessage> CreateCategory(Category c)
        {
            HttpOperationResponse result = await _categories.CreateOrUpdateCategoryWithHttpMessagesAsync(c);

            return result.Response;
        }

        public async Task<Product> CreateProduct(Product p)
        {
            HttpOperationResponse<Product> result = await _products.SaveProductWithHttpMessagesAsync(p);

            return result.Body;
        }
    }
}