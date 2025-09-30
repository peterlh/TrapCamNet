using System.Text.RegularExpressions;
using HtmlAgilityPack;
using TrapCam.Backend.Models;

namespace TrapCam.Backend.Services;

/// <summary>
/// Service for extracting battery information from email content
/// </summary>
public class BatteryExtractionService : IBatteryExtractionService
{
    private readonly ILogger<BatteryExtractionService> _logger;
    
    private static readonly string[] BatteryKeywords = new[]
    {
        "battery", "battery level",                 // English
        "batteri", "batterinivå",                   // Swedish
        "batterie", "niveau de batterie",           // French
        "batterij", "batterijniveau",               // Dutch
        "bateria", "nível da bateria",              // Portuguese
        "batería", "nivel de batería",              // Spanish
        "batteria", "livello batteria",             // Italian
        "akku", "akku stand",                       // German (colloquial), battery level = "Akkustand"
        "batterie", "batteriestand",                // German (formal)
        "μπαταρία", "επίπεδο μπαταρίας",            // Greek
        "バッテリー", "バッテリー残量",                 // Japanese
        "배터리", "배터리 수준",                        // Korean
        "bateria", "poziom baterii",                // Polish
        "аккумулятор", "уровень заряда",            // Russian
        "pil", "seviye pil",                        // Turkish
        "batteri", "batterinivå",                   // Norwegian / Danish
        "แบตเตอรี่", "ระดับแบตเตอรี่",                     // Thai
        "pin", "mức pin",                           // Vietnamese
        "batarya", "batarya seviyesi",              // Turkish (alt form)
        "batería", "carga de batería",              // Spanish (alt phrasing)
        "batteria", "stato batteria",               // Italian (alt)
        "电池", "电池电量",                           // Simplified Chinese
        "電池", "電池電量",                           // Traditional Chinese
        "แบต", "แบตเหลือ",                            // Thai (informal)
        "akku", "akku taso",                        // Finnish
        "batteri", "batteriniveau"                  // Danish (duplicate of Norwegian, listed again for clarity)
};

    private static readonly Regex BatteryPercentageRegex = new(
        @"(?<label>(battery\s*level|battery|batteri[e|j|a]?)[:\s]*)?(?<value>\d{1,3}(\.\d+)?\s*%)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
    private static readonly Regex BatteryVoltageRegex = new(
        @"(?<label>(battery\s*level|battery|batteri[e|j|a]?)[:\s]*)?(?<value>\d{1,3}(\.\d+)?\s*(v|volt|volts))",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public BatteryExtractionService(ILogger<BatteryExtractionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extracts battery information from email content
    /// </summary>
    /// <param name="emailContent">Raw email content (HTML or plain text)</param>
    /// <returns>Battery information if found, null otherwise</returns>
    public BatteryInfo? ExtractBatteryInfo(string emailContent)
    {
        if (string.IsNullOrEmpty(emailContent))
        {
            return null;
        }

        try
        {
            // Convert HTML to plain text
            var plainText = HtmlToText(emailContent).ToLowerInvariant();
            
            // First try to find percentage
            var percentageMatch = BatteryPercentageRegex.Match(plainText);
            if (percentageMatch.Success)
            {
                string valueStr = percentageMatch.Groups["value"].Value;
                if (double.TryParse(valueStr.Replace("%", "").Trim(), out double percentage))
                {
                    _logger.LogInformation("Extracted battery percentage: {Percentage}%", percentage);
                    return new BatteryInfo
                    {
                        RawMatch = percentageMatch.Value,
                        Percentage = percentage,
                        Voltage = null
                    };
                }
            }
            
            // If percentage not found, try voltage
            var voltageMatch = BatteryVoltageRegex.Match(plainText);
            if (voltageMatch.Success)
            {
                string valueStr = voltageMatch.Groups["value"].Value;
                string numericPart = Regex.Replace(valueStr, @"[^\d\.]", "").Trim();
                
                if (double.TryParse(numericPart, out double voltage))
                {
                    _logger.LogInformation("Extracted battery voltage: {Voltage}V", voltage);
                    return new BatteryInfo
                    {
                        RawMatch = voltageMatch.Value,
                        Percentage = null,
                        Voltage = voltage
                    };
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting battery information from email content");
            return null;
        }
    }

    /// <summary>
    /// Converts HTML content to plain text
    /// </summary>
    private static string HtmlToText(string html)
    {
        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            // Extract text from HTML
            var textNodes = doc.DocumentNode.SelectNodes("//text()[not(parent::script or parent::style)]");
            if (textNodes != null)
            {
                return string.Join(" ", textNodes.Select(node => node.InnerText.Trim()));
            }
            
            // Fallback to simple regex-based stripping if HtmlAgilityPack doesn't find text nodes
            return Regex.Replace(html, "<[^>]+>", " ");
        }
        catch
        {
            // If HTML parsing fails, fall back to simple regex-based stripping
            return Regex.Replace(html, "<[^>]+>", " ");
        }
    }
}


