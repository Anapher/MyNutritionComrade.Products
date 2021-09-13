using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BuildProducts.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BuildProducts
{
    public class Builder
    {
        public static int Execute(string[] args, DirectoryInfo productsDirectory, FileInfo[] productFiles)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No output directory specified");
                return 1;
            }

            var outputDirectory = new DirectoryInfo(args[0]);
            outputDirectory.Create();

            var allProducts = new Dictionary<string, Product>();

            foreach (var productFile in productFiles)
            {
                Product product;
                try
                {
                    product = Linter.ValidateFile(productFile);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Error occurred validating file {productFile.FullName}, skip");
                    continue;
                }

                product = PatchProduct(product);
                allProducts.Add(product.Id, product);
            }

            Console.WriteLine($"{allProducts.Count} products are valid and will be available.");

            var result = JsonConvert.SerializeObject(allProducts, Formatting.None,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                });

            var productsFilename = Path.Combine(outputDirectory.FullName, "products.json");
            File.WriteAllText(productsFilename, result);

            Console.WriteLine($"Products -> {productsFilename} ({new FileInfo(productsFilename).Length / 1024} KiB)");

            return 0;
        }

        private static Product PatchProduct(Product product)
        {
            if (product.Tags is { Count: 0 })
            {
                product = product with { Tags = null };
            }

            product = product with
            {
                Label = product.Label.ToDictionary(x => x.Key,
                    x => x.Value with { Tags = x.Value.Tags?.Length == 0 ? null : x.Value.Tags })
            };

            return product;
        }
    }
}