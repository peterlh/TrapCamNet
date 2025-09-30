using TrapCam.Backend.Models;

namespace TrapCam.Backend.Services;

/// <summary>
/// Interface for extracting battery information from email content
/// </summary>
public interface IBatteryExtractionService
{
    /// <summary>
    /// Extracts battery information from email content
    /// </summary>
    /// <param name="emailContent">Raw email content (HTML or plain text)</param>
    /// <returns>Battery information if found, null otherwise</returns>
    BatteryInfo? ExtractBatteryInfo(string emailContent);
}
