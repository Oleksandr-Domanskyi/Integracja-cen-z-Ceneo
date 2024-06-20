using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

public class ProductInfo
{
    public string? Details { get; set; }
    public string? Price { get; set; }
    public string? Ocena { get; set; }
    public string? IlostOpinji { get; set; }
}

public class Program
{
    private const string SearchUrlTemplate = "https://www.ceneo.pl/;szukaj-{0}";
    private const string BaseUrl = "https://www.ceneo.pl";

    public static void Main(string[] args)
    {
        Console.WriteLine("Enter product name:");
        string productName = Console.ReadLine();

        string searchUrl = string.Format(SearchUrlTemplate, WebUtility.UrlEncode(productName));

        var web = new HtmlWeb();

        var searchDocument = web.Load(searchUrl);

        var productLink = searchDocument.DocumentNode
            .SelectNodes("//a[contains(@class, 'go-to-product js_conv')]")
            .FirstOrDefault(node => IsExactMatch(node.InnerText.Trim(), productName));

        if (productLink != null)
        {
            string productUrl = BaseUrl + productLink.GetAttributeValue("href", "");
            var productDocument = web.Load(productUrl);

            var mainPriceNode = productDocument.DocumentNode.SelectSingleNode("//*[@id='body']/div[2]/div/div/article/div/div[2]/div/div/div[1]/span/span");
            string mainPrice = mainPriceNode != null ? mainPriceNode.InnerText.Trim() : "Main price not found";

            var products = GetProducts(productDocument);

            if (products.Any())
            {
                Console.WriteLine($"Product Name: {productName} || Cena na Ceneo: {mainPrice} zł");
                Console.WriteLine("Information:");

                foreach (var product in products)
                {
                    Console.WriteLine($"{product.Ocena}");
                    Console.WriteLine($"Opinji: {product.IlostOpinji}");
                    Console.WriteLine($"Details: {product.Details}");
                    Console.WriteLine($"Price: {product.Price} zł");
                    Console.WriteLine("----------------------------------------------------------");
                }
            }
            else
            {
                Console.WriteLine("No products found on Ceneo.");
            }
        }
        else
        {
            Console.WriteLine("Product not found on Ceneo.");
        }

    }

    private static List<ProductInfo> GetProducts(HtmlDocument document)
    {
        var products = new List<ProductInfo>();

        // Get prices and details from section 1
        var products1 = GetProductsInSection(document, "//*[@id='click']/div[2]/section[1]/ul/li");

        // Get prices and details from section 2
        var products2 = GetProductsInSection(document, "//*[@id='click']/div[2]/section[2]/ul/li");

        products.AddRange(products1);
        products.AddRange(products2);

        return products;
    }

    private static List<ProductInfo> GetProductsInSection(HtmlDocument document, string xpath)
    {
        var productNodes = document.DocumentNode.SelectNodes(xpath);
        var products = new List<ProductInfo>();

        if (productNodes != null)
        {
            foreach (var node in productNodes)
            {
                var detailsNode = node.SelectSingleNode(".//div[1]/div[1]/a/span");
                var priceNode = node.SelectSingleNode(".//div[2]/div[2]/a/span/span");
                var ocenaNode = node.SelectSingleNode(".//div[1]/div[2]/span[1]/span[2]");
                var ilostOpinjiNode = node.SelectSingleNode(".//div[1]/div[2]/span[2]");

                if (detailsNode != null && priceNode != null)
                {
                    var details = detailsNode.InnerText.Trim();
                    var price = priceNode.InnerText.Trim();
                    var ocena = ocenaNode?.InnerText.Trim();
                    var ilostOpinji = ilostOpinjiNode?.InnerText.Trim();

                    products.Add(new ProductInfo
                    {
                        Details = details,
                        Price = price,
                        Ocena = ocena,
                        IlostOpinji = ilostOpinji
                    });
                }
            }
        }

        return products;
    }
    private static bool IsExactMatch(string text, string productName)
    {
        if (string.Equals(text, productName))
        {
            return true;
        }
        return productName.Contains(text) || text.Contains(productName);
    }
}
