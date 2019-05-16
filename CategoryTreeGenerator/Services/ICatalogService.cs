using System.Net.Http;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;

namespace CategoryTreeGenerator.Services
{
    public interface ICatalogService
    {
        Task<HttpResponseMessage> CreateCategory(Category c);

        Task<Product> CreateProduct(Product p);
    }
}