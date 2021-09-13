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

            var repos = new List<RepositoryReference>
                { CreateRepository(productsDirectory, outputDirectory, "products") };

            foreach (var directory in
                productsDirectory.GetDirectories("*", SearchOption.AllDirectories))
            {
                var name = "products-" + directory.FullName.Substring(productsDirectory.FullName.Length)
                    .Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace(Path.DirectorySeparatorChar, '-').Replace(Path.AltDirectorySeparatorChar, '-');

                repos.Add(CreateRepository(directory, outputDirectory, name));
            }

            CreateIndexFile(repos, outputDirectory);

            return 0;
        }

        private static void CreateIndexFile(IReadOnlyList<RepositoryReference> repos, DirectoryInfo outputDirectory)
        {
            var indexFile = new FileInfo(Path.Combine(outputDirectory.FullName, "index.json"));

            var result = JsonConvert.SerializeObject(repos, Formatting.None,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

            File.WriteAllText(indexFile.FullName, result);

            Console.WriteLine($"Created file {indexFile.Name}");
        }

        private static RepositoryReference CreateRepository(DirectoryInfo directory, DirectoryInfo outputDirectory,
            string productRepositoryName)
        {
            var allProducts = new List<Product>();

            Console.WriteLine($"[{productRepositoryName}] Process directory {directory.FullName}");

            var maxTimestamp = 0L;
            foreach (var productFile in directory.GetFiles("*", SearchOption.TopDirectoryOnly))
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
                allProducts.Add(product);

                maxTimestamp = Math.Max(maxTimestamp,
                    new DateTimeOffset(productFile.LastWriteTimeUtc).ToUnixTimeMilliseconds());
            }

            Console.WriteLine(
                $"{allProducts.Count} products are valid and will be available in {productRepositoryName}.");

            var result = JsonConvert.SerializeObject(allProducts, Formatting.None,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                });

            var filename = $"{productRepositoryName}.json";
            var path = Path.Combine(outputDirectory.FullName, $"{productRepositoryName}.json");
            File.WriteAllText(path, result);

            Console.WriteLine($"Products -> {filename} ({new FileInfo(path).Length / 1024} KiB)");

            return new RepositoryReference("../" + filename, maxTimestamp,
                DateTimeOffset.FromUnixTimeMilliseconds(maxTimestamp));
        }

        public static Product PatchProduct(Product product)
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

    public record RepositoryReference(string Url, long Version, DateTimeOffset Timestamp);
}