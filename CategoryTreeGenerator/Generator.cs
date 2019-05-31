using CategoryTreeGenerator.Models;
using CategoryTreeGenerator.Services;
using CategoryTreeGenerator.Sources;
using CategoryTreeGenerator.Tools;
using Microsoft.Extensions.Configuration;
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
        private static string _catalogId;
        private static IDataSource _dataSource;
        private static ICatalogService _catalogService;
        private static LocationCategories _categoriesIds;
        private static LocationMasterData _locationMasterData;

        private static readonly Dictionary<string, KeyValuePair<string, string>> Types =
            new Dictionary<string, KeyValuePair<string, string>>();

        public static void Build(string path, IDataSource source, IConfiguration configuration)
        {
            BuildHierarchy(path, source.Types, source, configuration);
        }

        private static void BuildHierarchy(string path, IEnumerable<Type> types, IDataSource source,
            IConfiguration configuration)
        {
            _dataSource = source;
            _catalogId = configuration.GetSection("Catalog:Id").Value;

            _catalogService = new RestCatalogService(new Uri(configuration.GetSection("Endpoint:Url").Value),
                configuration.GetSection("Endpoint:AppId").Value,
                configuration.GetSection("Endpoint:SecretKey").Value);


            //генерация мастер данных тегов на 0 уровне
            string tagsCategoryId = CreateCategory("tags");

            foreach (Tag tag in _dataSource.Tags)
            {
                List<Property> properties = new List<Property>();

                //добавление свойства is_condition, если это условный тег
                if (tag.IsCondition)
                {
                    properties.Add(new Property
                    {
                        CatalogId = _catalogId,
                        Name = "is_condition",
                        Dictionary = false,
                        IsNew = true,
                        Multilanguage = false,
                        ValueType = "Boolean",
                        Values = new List<PropertyValue>
                        {
                            new PropertyValue
                            {
                                PropertyName = "is_condition",
                                Value = true,
                                ValueType = "Boolean"
                            }
                        }
                    });
                }

                CreateProduct(tag.Description, tag.Url, tagsCategoryId, true, alias: tag.Alias,
                    properties: properties);
            }

            //генерация дерева мастер данных и посадочных страниц
            Directory.CreateDirectory(path);

            string rootId = CreateCategory(path);

            _categoriesIds = new LocationCategories();

            List<string> typesMasterData = new List<string>();

            foreach (Type type in types)
            {
                File.WriteAllText($"{path}\\{type.Description}.txt", type.Url);

                AddTypeMasterData(type, rootId, typesMasterData);

                BuildTags(path, type, rootId);

                foreach (Location location in source.Locations)
                {
                    BuildLocation(path, type, location, rootId);
                }
            }
        }

        /// <summary>
        /// Метод добавления мастер данных и посадочных страниц для типа недвижимости и аренды или продажи
        /// </summary>
        private static void AddTypeMasterData(Type type, string rootId, List<string> createdData)
        {
            string dataTypeName;
            string dataTypeUrl;

            // добавление посадочной страницы для типа other_type (property for sale, property for rent)
            if (type.Url.StartsWith("property-"))
            {
                if (!createdData.Contains(type.Url))
                {
                    CreateProduct(type.Description, type.Url, rootId);

                    createdData.Add(type.Url);
                }

                //добавление мастер данных для (for rent и for sale)
                int pos = type.Description.IndexOf(" for ", StringComparison.Ordinal);

                dataTypeName = type.Description.Substring(pos + 1, type.Description.Length - pos - 1);

                dataTypeName = dataTypeName.First().ToString().ToUpper() + string.Join("", dataTypeName.Skip(1));

                dataTypeUrl = type.Url.Replace("property-", "");

                string id = CreateProduct(dataTypeName, dataTypeUrl, rootId, true, alias: type.Alias);

                if (!Types.ContainsKey(dataTypeUrl))
                {
                    Types.Add(dataTypeUrl, new KeyValuePair<string, string>(id, dataTypeName));
                }
            }
            // добавление мастер данных для отдельных типов без dealtype (for rent, for sale)
            else
            {
                dataTypeUrl = type.Url.Substring(0, type.Url.IndexOf("-for-", StringComparison.Ordinal));
                dataTypeName =
                    type.Description.Substring(0, type.Description.IndexOf(" for ", StringComparison.Ordinal));

                if (!Types.ContainsKey(dataTypeUrl))
                {
                    string id = null;

                    //если у типа задан ULR ассоциации, то выполняем поиск среди зарегистрированных типов
                    if (!string.IsNullOrEmpty(type.AssociatedTypeUrl))
                    {
                        string dataTypeAssUrl = type.AssociatedTypeUrl.Substring(0,
                            type.AssociatedTypeUrl.IndexOf("-for-", StringComparison.Ordinal));

                        if (Types.ContainsKey(dataTypeAssUrl))
                        {
                            id = CreateProduct(dataTypeName, dataTypeUrl, rootId, true,
                                Types[dataTypeAssUrl].Key, Types[dataTypeAssUrl].Value, alias: type.Alias);
                        }
                    }
                    else
                    {
                        id = CreateProduct(dataTypeName, dataTypeUrl, rootId, true, alias: type.Alias);
                    }

                    if (!string.IsNullOrEmpty(id))
                    {
                        Types.Add(dataTypeUrl, new KeyValuePair<string, string>(id, dataTypeName));
                    }
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

        private static void AttachTags(string basePath, string nameWithoutExtension, string parentUrl,
            string categoryId)
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
            _locationMasterData = new LocationMasterData();

            (string costaPath, string costaCategoryId) = BuildCosta(path, item, l, parentId);
            (string provincePath, string provinceId) = BuildProvince(costaPath, item, l, costaCategoryId);
            (string areaPath, string areaId) = BuildArea(provincePath, item, l, provinceId);
            (string cityPath, string cityId) = BuildCity(areaPath, item, l, areaId);

            if (l.EndLocation != null)
            {
                (string endLocationPath, string endLocationId) = BuildEndLocation(cityPath, item, l, cityId);

                if (l.EndLocation2 != null)
                {
                    BuildEndLocation2(endLocationPath, item, l, endLocationId);
                }
            }
        }

        private static (string, string) BuildCosta(string path, BaseItem parent, Location l, string parentId)
        {
            string costaPath = path + "\\costa";
            string costaId = _categoriesIds.CostaId;

            if (!Directory.Exists(costaPath))
            {
                Directory.CreateDirectory(costaPath);
            }

            //добавление вложенной папки
            if (string.IsNullOrEmpty(costaId))
            {
                costaId = _categoriesIds.CostaId = CreateCategory("costa", parentId);
            }

            //добавление мастер данных
            if (!_categoriesIds.CostaMasterData.ContainsKey(l.Costa.Url))
            {
                _locationMasterData.CostaId =
                    CreateProduct(l.Costa.Description, l.Costa.Url, costaId, true, alias: l.Costa.Alias);

                _categoriesIds.CostaMasterData.Add(l.Costa.Url, _locationMasterData.CostaId);
            }
            else
            {
                _locationMasterData.CostaId = _categoriesIds.CostaMasterData[l.Costa.Url];
            }

            string name = $"{parent.Description} in {l.Costa.Description}";
            string basePath = $"{costaPath}\\{name}";
            string url = $"{parent.Url}/{l.Costa.Url}";

            File.WriteAllText(basePath + ".txt", url);

            CreateProduct(name, url, costaId);

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

            //добавление мастер данных
            if (!_categoriesIds.ProvinceMasterData.ContainsKey(l.Province.Url))
            {
                _locationMasterData.ProvinceId =
                    CreateProduct(l.Province.Description, l.Province.Url, provinceId, true,
                        _locationMasterData.CostaId, l.Costa.Description, alias: l.Province.Alias);

                _categoriesIds.ProvinceMasterData.Add(l.Province.Url, _locationMasterData.ProvinceId);
            }
            else
            {
                _locationMasterData.ProvinceId = _categoriesIds.ProvinceMasterData[l.Province.Url];
            }

            string name = $"{parent.Description} in {l.Province.Description} ({l.Costa.Description})";
            string baseName =
                $"{provincePath}\\{name})";
            string url = $"{parent.Url}/{l.Costa.Url}/{l.Province.Url}";

            File.WriteAllText(baseName + ".txt", url);

            CreateProduct(name, url, provinceId);

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

            //добавление мастер данных
            if (!_categoriesIds.AreaMasterData.ContainsKey(l.Area.Url))
            {
                _locationMasterData.AreaId =
                    CreateProduct(l.Area.Description, l.Area.Url, areaId, true,
                        _locationMasterData.ProvinceId, l.Province.Description, l.Area.Alias);

                _categoriesIds.AreaMasterData.Add(l.Area.Url, _locationMasterData.AreaId);
            }
            else
            {
                _locationMasterData.AreaId = _categoriesIds.AreaMasterData[l.Area.Url];
            }

            string name =
                $"{parent.Description} in {l.Area.Description} ({l.Costa.Description}, {l.Province.Description})";
            string baseName =
                $"{areaPath}\\{name}";

            string url = $"{parent.Url}/{l.Costa.Url}/{l.Province.Url}/{l.Area.Url}";

            File.WriteAllText(baseName + ".txt", url);

            CreateProduct(name, url, areaId);

            AttachTags(baseName, name, url, areaId);

            return (areaPath, areaId);
        }

        private static (string, string) BuildCity(string path, BaseItem parent, Location l, string provinceId)
        {
            string cityPath = path + "\\city";
            string cityId = _categoriesIds.CityId;

            if (!Directory.Exists(cityPath))
            {
                Directory.CreateDirectory(cityPath);
            }

            if (string.IsNullOrEmpty(cityId))
            {
                cityId = _categoriesIds.CityId = CreateCategory("city", provinceId);
            }

            //добавление мастер данных
            if (!_categoriesIds.CityMasterData.ContainsKey(l.City.Url))
            {
                _locationMasterData.CityId =
                    CreateProduct(l.City.Description, l.City.Url, cityId, true,
                        _locationMasterData.AreaId, l.Area.Description, l.City.Alias);

                _categoriesIds.CityMasterData.Add(l.City.Url, _locationMasterData.CityId);
            }
            else
            {
                _locationMasterData.CityId = _categoriesIds.CityMasterData[l.City.Url];
            }

            string name = $"{parent.Description} in {l.City.Description} " +
                          $"({l.Costa.Description}, {l.Province.Description}, {l.Area.Description})";

            string baseName =
                $"{cityPath}\\{name}";

            string url = $"{parent.Url}/{l.Costa.Url}/{l.Province.Url}/{l.City.Url}";

            File.WriteAllText(baseName + ".txt", url);

            CreateProduct(name, url, cityId);

            AttachTags(baseName, name, url, cityId);

            return (cityPath, cityId);
        }

        private static (string, string) BuildEndLocation(string path, BaseItem parent, Location l, string cityId)
        {
            string endLocationPath = path + "\\end_location";
            string endLocationId = _categoriesIds.EndLocationId;

            if (!Directory.Exists(endLocationPath))
            {
                Directory.CreateDirectory(endLocationPath);
            }

            if (string.IsNullOrEmpty(endLocationId))
            {
                endLocationId = _categoriesIds.EndLocationId = CreateCategory("end_location", cityId);
            }

            //добавление мастер данных
            if (!_categoriesIds.EndLocationMasterData.ContainsKey(l.EndLocation.Url))
            {
                _locationMasterData.EndLocationId = CreateProduct(l.EndLocation.Description, l.EndLocation.Url,
                    endLocationId, true, _locationMasterData.CityId, l.City.Description, l.EndLocation.Alias);

                _categoriesIds.EndLocationMasterData.Add(l.EndLocation.Url, _locationMasterData.EndLocationId);
            }
            else
            {
                _locationMasterData.EndLocationId = _categoriesIds.EndLocationMasterData[l.EndLocation.Url];
            }

            string name = $"{parent.Description} in {l.City.Description} - {l.EndLocation.Description} " +
                          $"({l.Costa.Description}, {l.Province.Description}, {l.Area.Description})";

            string baseName =
                $"{endLocationPath}\\{name}";

            string url = $"{parent.Url}/{l.Costa.Url}/{l.Province.Url}/{l.City.Url}/{l.EndLocation.Url}";

            File.WriteAllText(baseName + ".txt", url);

            CreateProduct(name, url, endLocationId);

            AttachTags(baseName, name, url, endLocationId);

            return (endLocationPath, endLocationId);
        }

        private static void BuildEndLocation2(string path, BaseItem parent, Location l, string cityId)
        {
            string endLocation2Path = path + "\\end_location2";
            string endLocation2Id = _categoriesIds.EndLocation2Id;

            if (!Directory.Exists(endLocation2Path))
            {
                Directory.CreateDirectory(endLocation2Path);
            }

            if (string.IsNullOrEmpty(endLocation2Id))
            {
                endLocation2Id = _categoriesIds.EndLocation2Id = CreateCategory("end_location2", cityId);
            }

            //добавление мастер данных
            if (!_categoriesIds.EndLocation2MasterData.ContainsKey(l.EndLocation2.Url))
            {
                _locationMasterData.EndLocation2Id = CreateProduct(l.EndLocation2.Description, l.EndLocation2.Url,
                    endLocation2Id, true, _locationMasterData.EndLocationId,
                    l.EndLocation.Description, l.EndLocation2.Alias);

                _categoriesIds.EndLocation2MasterData.Add(l.EndLocation2.Url, _locationMasterData.EndLocationId);
            }
            else
            {
                _locationMasterData.EndLocation2Id = _categoriesIds.EndLocation2MasterData[l.EndLocation2.Url];
            }

            string name = $"{parent.Description} in {l.City.Description} " +
                          $"- {l.EndLocation2.Description} in {l.EndLocation.Description} " +
                          $"({l.Costa.Description}, {l.Province.Description}, {l.Area.Description})";

            string baseName =
                $"{endLocation2Path}\\{name}";

            string url =
                $"{parent.Url}/{l.Costa.Url}/{l.Province.Url}/{l.City.Url}/{l.EndLocation2.Url}-in-{l.EndLocation.Url}";

            File.WriteAllText(baseName + ".txt", url);

            CreateProduct(name, url, endLocation2Id);

            AttachTags(baseName, name, url, endLocation2Id);
        }

        private static string CreateCategory(string name, string parentId = null)
        {
            string categoryId = Guid.NewGuid().ToString();
            string code = categoryId.Substring(0, 5);

            Console.WriteLine($"Created category [{name}]: {categoryId}, parent: {parentId}");

            _catalogService.CreateCategory(new Category()
            {
                Id = categoryId,
                ParentId = parentId,
                CatalogId = _catalogId,
                Name = name,
                IsVirtual = false,
                Code = code
            }).Wait();

            return categoryId;
        }

        private static string CreateProduct(string name, string url, string categoryId, bool masterData = false,
            string associationId = null, string associasionName = null, string alias = null,
            List<Property> properties = null)
        {
            string productId = Guid.NewGuid().ToString();

            string code = masterData
                ? $"DATA_{productId.Substring(0, 5)}"
                : $"PAGE_{productId.Substring(0, 5)}";

            try
            {
                List<ProductAssociation> associations = new List<ProductAssociation>();
                if (!string.IsNullOrEmpty(associationId))
                {
                    associations.Add(new ProductAssociation
                    {
                        Type = "Related Items",
                        AssociatedObjectId = associationId,
                        AssociatedObjectName = associasionName ?? "No name",
                        AssociatedObjectType = "product"
                    });
                }

                Product product = _catalogService.CreateProduct(new Product
                {
                    Id = productId,
                    Gtin = alias,
                    Code = code,
                    CategoryId = categoryId,
                    CatalogId = _catalogId,
                    Name = name,
                    IsActive = true,
                    ProductType = "Physical",
                    Properties = properties ?? new List<Property>(),
                    Associations = associations,
                    SeoInfos = new List<SeoInfo>
                    {
                        new SeoInfo
                        {
                            Id = Guid.NewGuid().ToString(),
                            LanguageCode = "en-US",
                            SemanticUrl = url,
                            IsActive = true,
                        }
                    }
                }).Result;

                Console.WriteLine($"Created product [{name}]: {product.Id}, category: {categoryId}");

                return product.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating product {productId}, category={categoryId}: {e.Message}");
            }

            return null;
        }
    }
}
