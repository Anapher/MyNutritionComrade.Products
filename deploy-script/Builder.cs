using System;
using System.Collections.Generic;
using System.IO;
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

                allProducts.Add(product.Id, product);
            }

            Console.WriteLine($"{allProducts.Count} products are valid and will be available.");

            var result = JsonConvert.SerializeObject(allProducts, Formatting.None,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            var productsFilename = Path.Combine(outputDirectory.FullName, "products.json");
            File.WriteAllText(productsFilename, result);

            Console.WriteLine($"Products -> {productsFilename} ({new FileInfo(productsFilename).Length / 1024} KiB)");

            return 0;
        }
    }
}