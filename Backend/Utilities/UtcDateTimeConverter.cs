using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrapCam.Backend.Utilities
{
    /// <summary>
    /// Custom JSON converter for DateTime values to ensure they are always treated as UTC
    /// </summary>
    public class UtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Parse the JSON string to DateTime
            var dateTime = reader.GetDateTime();
            
            // Always treat incoming DateTime values as UTC regardless of their Kind
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Ensure the DateTime is in UTC before writing
            DateTime utcDateTime = value.Kind != DateTimeKind.Utc 
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc) 
                : value;
                
            // Write the DateTime as a string in ISO 8601 format
            writer.WriteStringValue(utcDateTime.ToString("o"));
        }
    }
    
    /// <summary>
    /// Custom JSON converter for nullable DateTime values to ensure they are always treated as UTC
    /// </summary>
    public class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            
            // Parse the JSON string to DateTime
            var dateTime = reader.GetDateTime();
            
            // Always treat incoming DateTime values as UTC regardless of their Kind
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }
            
            // Ensure the DateTime is in UTC before writing
            DateTime utcDateTime = value.Value.Kind != DateTimeKind.Utc 
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) 
                : value.Value;
                
            // Write the DateTime as a string in ISO 8601 format
            writer.WriteStringValue(utcDateTime.ToString("o"));
        }
    }
}
