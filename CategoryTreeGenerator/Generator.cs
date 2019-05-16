using CategoryTreeGenerator.Models;
using CategoryTreeGenerator.Services;
using CategoryTreeGenerator.Sources;
using CategoryTreeGenerator.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;
using Type = CategoryTreeGenerator.Models.Type;

namespace CategoryTreeGenerator
{
    /// <summary>
    /// Класс формирования дерева категорий
    /// </summary>
    public static class Generator
    {
        private static IDataSource _dataSource;
        private static ICatalogService _catalogService;
        private static LocationCategories _categoriesIds;

        public static void BuildForSale(string path, IDataSource source)
        {
            BuildHierarchy(path, source.TypesForSale, source);
        }

        public static void BuildForRent(string path, IDataSource source)
        {
            BuildHierarchy(path, source.TypesForRent, source);
        }

        private static void BuildHierarchy(string path, IEnumerable<Type> types, IDataSource source)
        {
            _dataSource = source;
            _catalogService = new RestCatalogService();

            Directory.CreateDirectory(path);

            string rootId = CreateCategory(path);

            _categoriesIds = new LocationCategories();

            foreach (Type type in types)
            {
                File.WriteAllText($"{path}\\{type.Description}.txt", type.Url);

                CreateProduct(type.Description, type.Url, rootId);

                BuildTags(path, type, rootId);

                foreach (Location location in source.Locations)
                {
                    BuildLocation(path, type, location, rootId);
                }
            }
        }

        private static void BuildTags(string path, BaseItem item, string rootId)
        {
            List<Tag> tags = _dataSource.Tags.ToList();

            foreach (Tag tag in tags)
            {
                string name = $"{item.Description} {tag.Description}";
                string url = $"{item.Url}/{tag.Url}";

                File.WriteAllText($"{path}\\{name}.txt", url);

                CreateProduct(name, url, rootId);
            }

            IEnumerable pairs = Combinations.MakeCombinations(tags, 2);

            foreach (IEnumerable<Tag> pair in pairs)
            {
                List<Tag> items = pair.ToList();

                string name = $"{item.Description} {items.First().Description} {items.Last().Description}";
                string fileName = $"{path}\\{name}.txt";
                string url = $"{item.Url}/{items.First().Url}-and-{items.Last().Url}";

                File.WriteAllText(fileName, url);

                CreateProduct(name, url, rootId);
            }
        }

        private static void AttachTags(string basePath, string nameWithoutExtension, string parentUrl, string categoryId)
        {
            List<Tag> tags = _dataSource.Tags.ToList();

            foreach (Tag tag in tags)
            {
                string name = $"{nameWithoutExtension} {tag.Description}";
                string url = $"{parentUrl}/{tag.Url}";

                File.WriteAllText($"{basePath} {name}.txt", url);

                CreateProduct(name, url, categoryId);
            }

            IEnumerable pairs = Combinations.MakeCombinations(tags, 2);

            foreach (IEnumerable<Tag> pair in pairs)
            {
                List<Tag> items = pair.ToList();

                string name = $"{nameWithoutExtension} {items.First().Description} {items.Last().Description}";
                string url = $"{parentUrl}/{items.First().Url}-and-{items.Last().Url}";

                File.WriteAllText($"{basePath} {name}.txt", url);

                CreateProduct(name, url, categoryId);
            }
        }

        private static void BuildLocation(string path, BaseItem item, Location l, string parentId)
        {
            (string costaPath, string costaId) = BuildCosta(path, item, l, parentId);
            (string provincePath, string provinceId) = BuildProvince(costaPath, item, l, costaId);
            (string areaPath, string areaId) = BuildArea(provincePath, item, l, provinceId);
            (string cityPath, string cityId) = BuildCity(areaPath, item, l, areaId);
            (_, _) = BuildEndLocation(cityPath, item, l, cityId);
        }

        private static (string, string) BuildCosta(string path, BaseItem parent, Location l, string parentId)
        {
            string costaPath = path + "\\costa";
            string costaId = _categoriesIds.CostaId;

            if (!Directory.Exists(costaPath))
            {
                Directory.CreateDirectory(costaPath);
            }

            if (string.IsNullOrEmpty(costaId))
            {
                costaId = _categoriesIds.CostaId = CreateCategory("costa", parentId);
            }

            string name = $"{parent.Description} in {l.Costa.Description}";
            string basePath = $"{costaPath}\\{name}";
            string url = $"{parent.Url}/{l.Costa.Url}";

            File.WriteAllText(basePath + ".txt", url);

            AttachTags(basePath, name, url, costaId);

            return (costaPath, costaId);
        }

        private static (string, string) BuildProvince(string path, BaseItem parent, Location l, string costaId)
        {
            string provincePath = path + "\\province";
            string provinceId = _categoriesIds.ProvinceId;

            if (!Directory.Exists(provincePath))
            {
                Directory.CreateDirectory(provincePath);
            }

            if (string.IsNullOrEmpty(provinceId))
            {
                provinceId = _categoriesIds.ProvinceId = CreateCategory("province", costaId);
            }

            string name = $"{parent.Description} in {l.Province.Description} ({l.Costa.Description})";
            string baseName =
                $"{provincePath}\\{name})";
            string url = $"{parent.Url}/{l.Costa.Url}/{l.Province.Url}";

            File.WriteAllText(baseName + ".txt", url);

            AttachTags(baseName, name, url, provinceId);

            return (provincePath, provinceId);
        }

        private static (string, string) BuildArea(string path, BaseItem parent, Location l, string provinceId)
        {
            string areaPath = path + "\\area";
            string areaId = _categoriesIds.AreaId;

            if (!Directory.Exists(areaPath))
            {
                Directory.CreateDirectory(areaPath);
            }

            if (string.IsNullOrEmpty(areaId))
            {
                areaId = _categoriesIds.AreaId = CreateCategory("area", provinceId);
            }

            string name = $"{parent.Description} in {l.Area.Description} ({l.Costa.Description}, {l.Province.Description})";
            string baseName =
                $"{areaPath}\\{name}";

            string url = $"{parent.Url}/{l.Costa.Url}/{l.Province.Url}/{l.Area.Url}";

            File.WriteAllText(baseName + ".txt", url);

            AttachTags(baseName, name, url, areaId);

            return (areaPath, areaId);
        }

        private static (string, string) BuildCity(string path, BaseItem parent, Location l, string provinceId)
        {
            string cityPath = path + "\\area";
            string cityId = _categoriesIds.CityId;

            if (!Directory.Exists(cityPath))
            {
                Directory.CreateDirectory(cityPath);
            }

            if (string.IsNullOrEmpty(cityId))
            {
                cityId = _categoriesIds.CityId = CreateCategory("city", provinceId);
            }

            string name = $"{parent.Description} in {l.City.Description} " +
                          $"({l.Costa.Description}, {l.Province.Description}, {l.Area.Description})";

            string baseName =
                $"{cityPath}\\{name}";

            string url = $"{parent.Url}/{l.Costa.Url}/{l.Province.Url}/{l.Area.Url}/{l.City.Url}";

            File.WriteAllText(baseName + ".txt", url);

            AttachTags(baseName, name, url, cityId);

            return (cityPath, cityId);
        }

        private static (string, string) BuildEndLocation(string path, BaseItem parent, Location l, string cityId)
        {
            string endLocationPath = path + "\\end_location";
            string endLocationId = _categoriesIds.EndLocation;

            if (!Directory.Exists(endLocationPath))
            {
                Directory.CreateDirectory(endLocationPath);
            }

            if (string.IsNullOrEmpty(endLocationId))
            {
                endLocationId = _categoriesIds.EndLocation = CreateCategory("end_location", cityId);
            }

            string name = $"{parent.Description} in {l.City.Description} - {l.EndLocation.Description} " +
                          $"({l.Costa.Description}, {l.Province.Description}, {l.Area.Description})";

            string baseName =
                $"{endLocationPath}\\{name}";

            string url = $"{parent.Url}/{l.Costa.Url}/{l.Province.Url}/{l.Area.Url}/{l.City.Url}/{l.EndLocation.Url}";

            File.WriteAllText(baseName + ".txt", url);

            AttachTags(baseName, name, url, endLocationId);

            name = $"{parent.Description} in {l.City.Description} " +
                   $"- {l.EndLocation2.Description} in {l.EndLocation.Description} " +
                   $"({l.Costa.Description}, {l.Province.Description}, {l.Area.Description})";

            baseName =
                $"{endLocationPath}\\{name}";

            url =
                $"{parent.Url}/{l.Costa.Url}/{l.Province.Url}/{l.Area.Url}/{l.City.Url}/{l.EndLocation2.Url}-in-{l.EndLocation.Url}";

            File.WriteAllText(baseName + ".txt", url);

            AttachTags(baseName, name, url, endLocationId);


            return (endLocationPath, endLocationId);
        }

        private static string CreateCategory(string name, string parentId = null)
        {
            string categoryId = Guid.NewGuid().ToString();
            string code = categoryId.Substring(0, 5);

            Console.WriteLine($"Created category [{name}]: {categoryId}, parent: {parentId}");

            System.Net.Http.HttpResponseMessage result = _catalogService.CreateCategory(new Category()
            {
                Id = categoryId.ToString(),
                ParentId = parentId,
                CatalogId = "bbe2d2cc7dda4a8dbbb6a44ed63b6670",
                Name = name,
                IsVirtual = false,
                Code = code
            }).Result;

            return categoryId;
        }

        private static string CreateProduct(string name, string url, string categoryId)
        {
            string productId = Guid.NewGuid().ToString();

            try
            {
                Product result = _catalogService.CreateProduct(new Product()
                {
                    //Id = productId,
                    Code = productId.Substring(0, 5),
                    CategoryId = categoryId,
                    CatalogId = "bbe2d2cc7dda4a8dbbb6a44ed63b6670",
                    Name = name,
                    IsActive = true,
                    ProductType = "Physical",
                    Properties = new List<Property>(),
                    SeoInfos = new List<SeoInfo>()
                    {
                        new SeoInfo()
                        {
                            Id = Guid.NewGuid().ToString(),
                            LanguageCode = "en-US",
                            SemanticUrl = url,
                            IsActive = true,
                        }
                    },
                }).Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine($"Created product [{name}]: {productId}, category: {categoryId}");

            return productId;
        }
    }
}