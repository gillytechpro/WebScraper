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
                
                // Store initial page content before any navigation
                var initialHeadings = driver.FindElements(By.CssSelector("h1, h2"))
                    .Select(h => (h.TagName, h.Text.Trim()))
                    .ToList();
                
                var firstParagraph = driver.FindElements(By.CssSelector("p"))
                    .FirstOrDefault()?.Text ?? "No paragraph found";

                string navigationStatus = "No target page specified";
                bool pageNavigated = false;
                
                if (!string.IsNullOrEmpty(targetPage))
                {
                    try
                    {
                        // Try direct navigation first
                        string targetUrl = baseUrl.TrimEnd('/') + "/" + targetPage.TrimStart('/');
                        driver.Navigate().GoToUrl(targetUrl);
                        
                        // Wait for the new page to load
                        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                        
                        navigationStatus = $"Navigated to: {targetUrl}";
                        pageNavigated = true;
                    }
                    catch (Exception ex)
                    {
                        // If direct navigation fails, try to find a link
                        try
                        {
                            // Get all links before any navigation
                            var targetLinks = driver.FindElements(By.CssSelector("a"))
                                .Select(link => new 
                                { 
                                    Element = link,
                                    Href = link.GetAttribute("href")?.ToLower() ?? "",
                                    Text = link.Text?.ToLower() ?? ""
                                })
                                .Where(link => link.Href.Contains(targetPage.ToLower()) || 
                                              link.Text.Contains(targetPage.ToLower()) ||
                                              link.Href.EndsWith($"/{targetPage.ToLower()}"))
                                .Take(1)
                                .ToList();

                            if (targetLinks.Any())
                            {
                                string targetUrl = targetLinks[0].Href;
                                if (!string.IsNullOrEmpty(targetUrl))
                                {
                                    // Store the URL and navigate directly to avoid stale elements
                                    driver.Navigate().GoToUrl(targetUrl);
                                    
                                    // Wait for the new page to load
                                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                                    wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                                    
                                    navigationStatus = $"Navigated to: {targetUrl}";
                                    pageNavigated = true;
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
                Console.WriteLine($"Initial URL: {baseUrl}");
                Console.WriteLine($"Page Load Time: {loadTime.TotalSeconds:F2} seconds");
                
                // Display initial page content
                Console.WriteLine("\n--- INITIAL PAGE HEADINGS ---");
                foreach (var (tagName, text) in initialHeadings.Take(5))
                {
                    Console.WriteLine($"{tagName.ToUpper()}: {text}");
                }

                Console.WriteLine("\n--- FIRST PARAGRAPH ---");
                Console.WriteLine(firstParagraph);
                
                // If we navigated to another page, show its content too
                if (pageNavigated)
                {
                    try
                    {
                        var newHeadings = driver.FindElements(By.CssSelector("h1, h2"))
                            .Select(h => (h.TagName, h.Text.Trim()))
                            .ToList();
                        
                        var newFirstParagraph = driver.FindElements(By.CssSelector("p"))
                            .FirstOrDefault()?.Text ?? "No paragraph found";
                        
                        Console.WriteLine("\n--- NAVIGATED PAGE HEADINGS ---");
                        foreach (var (tagName, text) in newHeadings.Take(5))
                        {
                            Console.WriteLine($"{tagName.ToUpper()}: {text}");
                        }
                        
                        Console.WriteLine("\n--- NAVIGATED PAGE FIRST PARAGRAPH ---");
                        Console.WriteLine(newFirstParagraph);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\n--- ERROR GETTING NAVIGATED PAGE CONTENT ---");
                        Console.WriteLine($"Could not retrieve content from navigated page: {ex.Message}");
                    }
                }
                
                Console.WriteLine("\n--- NAVIGATION STATUS ---");
                Console.WriteLine(navigationStatus);
                
                Console.WriteLine("\n=== END OF REPORT ===\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Exception: {ex.InnerException.Message}");
            }
            Environment.Exit(1);
        }
    }
}
