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

        public RestCatalogService()
        {
            ServicePointManager.UseNagleAlgorithm = false;

            HttpClientHandler httpHandlerWithCompression = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            VirtoCommerceApiRequestHandler handler = new VirtoCommerceApiRequestHandler(
                "c1fcfe28dd2e42908a9fc021b7d0e7c4",
                "29356b8d61ca3d41538f7bafc972dae1d6dd26c6f699276d116c4f66cc0bcd007b9f459e3bb8d09658310b569f0d64c01d16fb91e94f76aab91e5d4aed68cbdc");

            Uri uri = new Uri("http://localhost/estate-admin");

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