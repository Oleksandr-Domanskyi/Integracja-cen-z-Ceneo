using HtmlAgilityPack;
using System;
using System.Net;

namespace Ceneo
{
    public class CeneoPl
    {
        private const string BaseUrl = "https://www.ceneo.pl";
        private const string SearchUrlTemplate = "https://www.ceneo.pl/;szukaj-{0}";
        private const string BaseUrlFirstPage = "https://www.ceneo.pl/;szukaj-polecaneceneofree";
        private const string BaseUrlNextPages = "https://www.ceneo.pl/;szukaj-polecaneceneofree;0020-30-0-0-{0}.htm";

        public static void Main(string[] args)
        {
            Console.WriteLine("Enter product name:");
            string productName = Console.ReadLine();

            try
            {
                string searchUrl = string.Format(SearchUrlTemplate, WebUtility.UrlEncode(productName));

                var web = new HtmlWeb();
                var searchDocument = web.Load(searchUrl);

                var productLink = searchDocument.DocumentNode.SelectSingleNode("//a[contains(@class, 'go-to-product js_conv')]");

                if (productLink != null)
                {
                    string productUrl = BaseUrl + productLink.GetAttributeValue("href", "");
                    var productDocument = web.Load(productUrl);

                    var products = GetProducts(productDocument);

                    if (products.Any())
                    {
                        Console.WriteLine($"Product Name: {productName}");
                        Console.WriteLine("Details and Prices:");

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
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
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
                    var logoNode = node.SelectSingleNode(".//div[1]/div[1]/div[1]/div[1]/a/img");
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

        public class ProductInfo
        {
            public string? Details { get; set; }
            public string? Price { get; set; }
            public string? Ocena { get; set; }
            public string? IlostOpinji { get; set; }
        }

        public void GetProductInformation()
        {
            try
            {
                // Start with the first page
                string url = BaseUrlFirstPage;
                int pageNumber = 1;
                bool hasNextPage = true;

                while (hasNextPage)
                {
                    var web = new HtmlWeb();
                    var document = web.Load(url);

                    // Use XPath to select the product containers
                    var productNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'cat-prod-row') and contains(@class, 'js_category-list-item')]");

                    if (productNodes != null && productNodes.Count > 0)
                    {
                        foreach (var productNode in productNodes)
                        {
                            // Extract the product ID
                            string productId = productNode.GetAttributeValue("data-pid", "");

                            // Example XPath to get product name and price (adjust based on actual HTML structure)
                            var nameNode = productNode.SelectSingleNode(".//strong/a");
                            var priceNode = productNode.SelectSingleNode(".//div[contains(@class, 'cat-prod-row__price')]//span[contains(@class, 'price-format')]");

                            if (nameNode != null && priceNode != null)
                            {
                                var name = nameNode.InnerText.Trim();
                                var price = priceNode.InnerText.Trim();

                                Console.WriteLine($"Product ID: {productId}");
                                Console.WriteLine($"Product Name: {name}");
                                Console.WriteLine($"Product Price: {price}");
                                Console.WriteLine("-----------------------------");
                            }
                        }

                        // Increment page number for the next iteration
                        pageNumber++;
                        url = string.Format(BaseUrlNextPages, pageNumber);
                    }
                    else
                    {
                        Console.WriteLine("No product nodes found on the page.");
                        hasNextPage = false; // Exit loop if no products found
                    }
                    if (pageNumber == 2)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
