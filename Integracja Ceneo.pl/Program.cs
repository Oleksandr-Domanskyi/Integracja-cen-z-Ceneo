using HtmlAgilityPack;
using System;

namespace Ceneo
{
    public class CeneoPl
    {
        private const string BaseUrlFirstPage = "https://www.ceneo.pl/;szukaj-polecaneceneofree";
        private const string BaseUrlNextPages = "https://www.ceneo.pl/;szukaj-polecaneceneofree;0020-30-0-0-{0}.htm";

        public static void Main(string[] args)
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

                    // Use XPath to select the desired container element
                    var containerNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'category-list-body') and contains(@class, 'js_category-list-body') and contains(@class, 'js_search-results') and contains(@class, 'js_products-list-main') and contains(@class, 'js_async-container')]");

                    if (containerNode != null)
                    {
                        var productPriceNodes = containerNode.SelectNodes(".//div[contains(@class, 'cat-prod-row__price')]//span[contains(@class, 'price-format')]");
                        var productNameNodes = containerNode.SelectNodes(".//strong/a");

                        if (productPriceNodes != null && productNameNodes != null)
                        {
                            for (int i = 0; i < productPriceNodes.Count; i++)
                            {
                                var priceNode = productPriceNodes[i];
                                var nameNode = productNameNodes[i];

                                var price = priceNode.InnerText.Trim();
                                var name = nameNode.InnerText.Trim();

                                Console.WriteLine($"Product Name: {name}");
                                Console.WriteLine($"Product Price: {price}");
                                Console.WriteLine("-----------------------------");
                            }

                            // Increment page number for the next iteration
                            pageNumber++;
                            url = string.Format(BaseUrlNextPages, pageNumber);
                        }
                        else
                        {
                            Console.WriteLine("No product names or prices found in the specified container.");
                            hasNextPage = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Container element not found.");
                        hasNextPage = false;
                    }
                    //Ograniczenie testowe
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
