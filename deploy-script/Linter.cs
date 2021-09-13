using System;
using System.IO;
using BuildProducts.Models;
using BuildProducts.Validation;
using FluentValidation;
using Newtonsoft.Json;

namespace BuildProducts
{
    public static class Linter
    {
        public static int Execute(string[] args, DirectoryInfo productsDirectory, FileInfo[] productFiles)
        {
            var success = true;
            var validatedCounter = 0;

            foreach (var productFile in productFiles)
            {
                try
                {
                    ValidateFile(productFile);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Validation failed for file '{productFile.FullName}'");
                    Console.WriteLine(e);
                    success = false;
                }

                validatedCounter++;
            }

            Console.WriteLine($"Found {validatedCounter} valid files");

            return success ? 0 : 1;
        }

        public static Product ValidateFile(FileInfo file)
        {
            var validator = new ProductInfoValidator();
            var jsonText = File.ReadAllText(file.FullName);

            var obj = JsonConvert.DeserializeObject<Product>(jsonText);

            if (obj == null)
            {
                throw new NullReferenceException("The object is null");
            }

            if (file.Name != $"{obj.Id}.json")
            {
                throw new ArgumentException(
                    $"The product has the id {obj.Id}, but the file is named {file.Name}. Ids do not match");
            }

            validator.ValidateAndThrow(obj);

            return obj;
        }
    }
}