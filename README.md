# WebScraper

A C# console application that uses Selenium WebDriver to analyze web pages, extract content, and navigate to specific pages. The tool runs in headless mode by default and provides a detailed report of its findings.

## Features

- Measures page load time
- Extracts and displays headings (h1, h2)
- Extracts the first paragraph of text
- Navigates to specific pages (contact page by default or custom page)
- Generates a clean console report
- Runs in headless mode (no browser UI)

## Prerequisites

- [.NET 7.0 SDK](https://dotnet.microsoft.com/download/dotnet/7.0) or later
- Chrome browser installed (for ChromeDriver)

## Installation

1. Clone this repository:
   ```bash
   git clone https://github.com/gillytechpro/WebScraper.git
   cd WebScraper
   ```

2. Restore the NuGet packages:
   ```bash
   dotnet restore
   ```

## Usage

### Basic Usage

Analyze a website's main page:
```bash
dotnet run example.com
```

### Navigate to a Specific Page

Navigate to a specific page on the website:
```bash
dotnet run example.com about
```

### Use Full URL

You can also use a full URL:
```bash
dotnet run https://example.com/contact
```

## Examples

1. **Basic website analysis**:
   ```bash
   dotnet run gillytech.pro
   ```

2. **Navigate to a specific page**:
   ```bash
   dotnet run gillytech.pro services
   ```

3. **Using HTTPS URL**:
   ```bash
   dotnet run https://gillytech.pro/contact
   ```

## Output Format

The tool provides a detailed report including:
- URL analyzed
- Page load time
- Headings (h1, h2)
- First paragraph of text
- Navigation status

## Troubleshooting

- If you encounter ChromeDriver issues, ensure you have Chrome browser installed
- For permission issues, try running with administrator privileges
- Ensure the target website allows web scraping (check robots.txt)
