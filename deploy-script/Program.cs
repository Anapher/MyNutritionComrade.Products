using System;
using System.IO;
using System.Linq;

namespace BuildProducts
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("First argument must be either 'lint' or 'build'");
                return 1;
            }

            if (args.Length < 2)
            {
                Console.WriteLine("The product directory must be passed as second argument.");
                return 1;
            }

            var productsDirectory = new DirectoryInfo(args[1]);
            Console.WriteLine($"Products directory: {productsDirectory.FullName}");

            if (!productsDirectory.Exists)
            {
                Console.WriteLine("Products directory does not exist");
                return 1;
            }

            var productFiles = productsDirectory.GetFiles();
            Console.WriteLine($"Found {productFiles.Length} product files.");

            switch (args[0])
            {
                case "lint":
                    return Linter.Execute(args.Skip(2).ToArray(), productsDirectory, productFiles);
                case "build":
                    return Builder.Execute(args.Skip(2).ToArray(), productsDirectory, productFiles);
                default:
                    Console.WriteLine("First argument must be either 'lint' or 'build'");
                    return 1;
            }
        }
    }
}