using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        // Display usage if no arguments provided
        if (args.Length == 0)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run <website_url> [target_page]");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("  dotnet run example.com");
            Console.WriteLine("  dotnet run https://example.com about");
            Console.WriteLine("  dotnet run example.com /contact");
            return;
        }

        // Get the base URL from the first argument
        string baseUrl = args[0].StartsWith("http") ? args[0] : $"https://{args[0]}";
        string targetPage = args.Length > 1 ? args[1].TrimStart('/') : null;

        try
        {
            // Set up Chrome options for headless mode
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");

            // Initialize the Chrome WebDriver
            using (var driver = new ChromeDriver(options))
            {
                // Track page load time
                var stopwatch = new Stopwatch();
                
                Console.WriteLine($"Navigating to: {baseUrl}");
                
                // Start timer and navigate to the page
                stopwatch.Start();
                driver.Navigate().GoToUrl(baseUrl);
                
                // Wait for the page to load completely
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                
                // Stop timer and calculate load time
                stopwatch.Stop();
                TimeSpan loadTime = stopwatch.Elapsed;

                // Extract headings (h1 and h2)
                var headings = driver.FindElements(By.CssSelector("h1, h2"));
                
                // Extract first paragraph
                var firstParagraph = driver.FindElements(By.CssSelector("p")).FirstOrDefault()?.Text ?? "No paragraph found";

                // Try to navigate to the target page if specified, otherwise look for a contact page
                string navigationStatus = "No target page specified";
                
                if (!string.IsNullOrEmpty(targetPage))
                {
                    try
                    {
                        // Try direct navigation first
                        string targetUrl = baseUrl.TrimEnd('/') + "/" + targetPage.TrimStart('/');
                        driver.Navigate().GoToUrl(targetUrl);
                        navigationStatus = $"Navigated to: {targetUrl}";
                    }
                    catch (Exception ex)
                    {
                        // If direct navigation fails, try to find a link
                        try
                        {
                            var targetLinks = driver.FindElements(By.CssSelector("a"))
                                .Where(link => 
                                {
                                    string href = link.GetAttribute("href")?.ToLower() ?? "";
                                    string text = link.Text?.ToLower() ?? "";
                                    return href.Contains(targetPage.ToLower()) || 
                                           text.Contains(targetPage.ToLower()) ||
                                           href.EndsWith($"/{targetPage.ToLower()}");
                                })
                                .Take(1)
                                .ToList();

                            if (targetLinks.Any())
                            {
                                string targetUrl = targetLinks[0].GetAttribute("href");
                                if (!string.IsNullOrEmpty(targetUrl))
                                {
                                    driver.Navigate().GoToUrl(targetUrl);
                                    navigationStatus = $"Navigated to: {targetUrl}";
                                }
                            }
                            else
                            {
                                navigationStatus = $"Could not find link containing: {targetPage}";
                            }
                        }
                        catch (Exception innerEx)
                        {
                            navigationStatus = $"Error navigating to target page: {innerEx.Message}";
                        }
                    }
                }
                

                // Generate and display the report
                Console.WriteLine("\n=== WEB PAGE ANALYSIS REPORT ===");
                Console.WriteLine($"URL: {baseUrl}");
                Console.WriteLine($"Page Load Time: {loadTime.TotalSeconds:F2} seconds");
                
                Console.WriteLine("\n--- HEADINGS ---");
                foreach (var heading in headings.Take(5)) // Limit to first 5 headings
                {
                    Console.WriteLine($"{heading.TagName.ToUpper()}: {heading.Text.Trim()}");
                }

                Console.WriteLine("\n--- FIRST PARAGRAPH ---");
                Console.WriteLine(firstParagraph);
                
                Console.WriteLine("\n--- PAGE NAVIGATION ---");
                Console.WriteLine(navigationStatus);
                
                Console.WriteLine("\n=== END OF REPORT ===\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            Environment.Exit(1);
        }
    }
}
