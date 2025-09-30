using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TrapCam.Backend.Services;

/// <summary>
/// Service for country-related operations
/// </summary>
public class CountryService : ICountryService
{
    // Dictionary mapping ISO 3166-1 alpha-2 country codes to country names
    private static readonly Dictionary<string, string> _countryCodeToName = new Dictionary<string, string>
    {
        { "AF", "Afghanistan" },
        { "AL", "Albania" },
        { "DZ", "Algeria" },
        { "AD", "Andorra" },
        { "AO", "Angola" },
        { "AG", "Antigua and Barbuda" },
        { "AR", "Argentina" },
        { "AM", "Armenia" },
        { "AU", "Australia" },
        { "AT", "Austria" },
        { "AZ", "Azerbaijan" },
        { "BS", "Bahamas" },
        { "BH", "Bahrain" },
        { "BD", "Bangladesh" },
        { "BB", "Barbados" },
        { "BY", "Belarus" },
        { "BE", "Belgium" },
        { "BZ", "Belize" },
        { "BJ", "Benin" },
        { "BT", "Bhutan" },
        { "BO", "Bolivia" },
        { "BA", "Bosnia and Herzegovina" },
        { "BW", "Botswana" },
        { "BR", "Brazil" },
        { "BN", "Brunei" },
        { "BG", "Bulgaria" },
        { "BF", "Burkina Faso" },
        { "BI", "Burundi" },
        { "CV", "Cabo Verde" },
        { "KH", "Cambodia" },
        { "CM", "Cameroon" },
        { "CA", "Canada" },
        { "CF", "Central African Republic" },
        { "TD", "Chad" },
        { "CL", "Chile" },
        { "CN", "China" },
        { "CO", "Colombia" },
        { "KM", "Comoros" },
        { "CG", "Congo" },
        { "CD", "Congo (Democratic Republic)" },
        { "CR", "Costa Rica" },
        { "CI", "Côte d'Ivoire" },
        { "HR", "Croatia" },
        { "CU", "Cuba" },
        { "CY", "Cyprus" },
        { "CZ", "Czech Republic" },
        { "DK", "Denmark" },
        { "DJ", "Djibouti" },
        { "DM", "Dominica" },
        { "DO", "Dominican Republic" },
        { "EC", "Ecuador" },
        { "EG", "Egypt" },
        { "SV", "El Salvador" },
        { "GQ", "Equatorial Guinea" },
        { "ER", "Eritrea" },
        { "EE", "Estonia" },
        { "SZ", "Eswatini" },
        { "ET", "Ethiopia" },
        { "FJ", "Fiji" },
        { "FI", "Finland" },
        { "FR", "France" },
        { "GA", "Gabon" },
        { "GM", "Gambia" },
        { "GE", "Georgia" },
        { "DE", "Germany" },
        { "GH", "Ghana" },
        { "GR", "Greece" },
        { "GD", "Grenada" },
        { "GT", "Guatemala" },
        { "GN", "Guinea" },
        { "GW", "Guinea-Bissau" },
        { "GY", "Guyana" },
        { "HT", "Haiti" },
        { "HN", "Honduras" },
        { "HU", "Hungary" },
        { "IS", "Iceland" },
        { "IN", "India" },
        { "ID", "Indonesia" },
        { "IR", "Iran" },
        { "IQ", "Iraq" },
        { "IE", "Ireland" },
        { "IL", "Israel" },
        { "IT", "Italy" },
        { "JM", "Jamaica" },
        { "JP", "Japan" },
        { "JO", "Jordan" },
        { "KZ", "Kazakhstan" },
        { "KE", "Kenya" },
        { "KI", "Kiribati" },
        { "KP", "Korea (North)" },
        { "KR", "Korea (South)" },
        { "KW", "Kuwait" },
        { "KG", "Kyrgyzstan" },
        { "LA", "Laos" },
        { "LV", "Latvia" },
        { "LB", "Lebanon" },
        { "LS", "Lesotho" },
        { "LR", "Liberia" },
        { "LY", "Libya" },
        { "LI", "Liechtenstein" },
        { "LT", "Lithuania" },
        { "LU", "Luxembourg" },
        { "MG", "Madagascar" },
        { "MW", "Malawi" },
        { "MY", "Malaysia" },
        { "MV", "Maldives" },
        { "ML", "Mali" },
        { "MT", "Malta" },
        { "MH", "Marshall Islands" },
        { "MR", "Mauritania" },
        { "MU", "Mauritius" },
        { "MX", "Mexico" },
        { "FM", "Micronesia" },
        { "MD", "Moldova" },
        { "MC", "Monaco" },
        { "MN", "Mongolia" },
        { "ME", "Montenegro" },
        { "MA", "Morocco" },
        { "MZ", "Mozambique" },
        { "MM", "Myanmar" },
        { "NA", "Namibia" },
        { "NR", "Nauru" },
        { "NP", "Nepal" },
        { "NL", "Netherlands" },
        { "NZ", "New Zealand" },
        { "NI", "Nicaragua" },
        { "NE", "Niger" },
        { "NG", "Nigeria" },
        { "MK", "North Macedonia" },
        { "NO", "Norway" },
        { "OM", "Oman" },
        { "PK", "Pakistan" },
        { "PW", "Palau" },
        { "PS", "Palestine" },
        { "PA", "Panama" },
        { "PG", "Papua New Guinea" },
        { "PY", "Paraguay" },
        { "PE", "Peru" },
        { "PH", "Philippines" },
        { "PL", "Poland" },
        { "PT", "Portugal" },
        { "QA", "Qatar" },
        { "RO", "Romania" },
        { "RU", "Russia" },
        { "RW", "Rwanda" },
        { "KN", "Saint Kitts and Nevis" },
        { "LC", "Saint Lucia" },
        { "VC", "Saint Vincent and the Grenadines" },
        { "WS", "Samoa" },
        { "SM", "San Marino" },
        { "ST", "Sao Tome and Principe" },
        { "SA", "Saudi Arabia" },
        { "SN", "Senegal" },
        { "RS", "Serbia" },
        { "SC", "Seychelles" },
        { "SL", "Sierra Leone" },
        { "SG", "Singapore" },
        { "SK", "Slovakia" },
        { "SI", "Slovenia" },
        { "SB", "Solomon Islands" },
        { "SO", "Somalia" },
        { "ZA", "South Africa" },
        { "SS", "South Sudan" },
        { "ES", "Spain" },
        { "LK", "Sri Lanka" },
        { "SD", "Sudan" },
        { "SR", "Suriname" },
        { "SE", "Sweden" },
        { "CH", "Switzerland" },
        { "SY", "Syria" },
        { "TW", "Taiwan" },
        { "TJ", "Tajikistan" },
        { "TZ", "Tanzania" },
        { "TH", "Thailand" },
        { "TL", "Timor-Leste" },
        { "TG", "Togo" },
        { "TO", "Tonga" },
        { "TT", "Trinidad and Tobago" },
        { "TN", "Tunisia" },
        { "TR", "Turkey" },
        { "TM", "Turkmenistan" },
        { "TV", "Tuvalu" },
        { "UG", "Uganda" },
        { "UA", "Ukraine" },
        { "AE", "United Arab Emirates" },
        { "GB", "United Kingdom" },
        { "US", "United States" },
        { "UY", "Uruguay" },
        { "UZ", "Uzbekistan" },
        { "VU", "Vanuatu" },
        { "VA", "Vatican City" },
        { "VE", "Venezuela" },
        { "VN", "Vietnam" },
        { "YE", "Yemen" },
        { "ZM", "Zambia" },
        { "ZW", "Zimbabwe" }
    };

    /// <summary>
    /// Gets the country name for a given country code
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 country code</param>
    /// <returns>Country name or null if not found</returns>
    public string? GetCountryName(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
            return null;
            
        return _countryCodeToName.TryGetValue(countryCode.ToUpper(), out var name) ? name : null;
    }

    /// <summary>
    /// Gets all country codes and names
    /// </summary>
    /// <returns>Dictionary of country codes to names</returns>
    public Dictionary<string, string> GetAllCountries()
    {
        return new Dictionary<string, string>(_countryCodeToName);
    }
    
    /// <summary>
    /// Gets the country code for coordinates using reverse geocoding
    /// </summary>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    /// <returns>ISO 3166-1 alpha-2 country code or null if not found</returns>
    public string? GetCountryCodeForCoordinates(decimal latitude, decimal longitude)
    {
        // In a real implementation, this would call a geocoding service
        // For now, we'll return a default value for testing
        return "DK";
    }
}
