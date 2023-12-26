using Newtonsoft.Json;

namespace Configuration;

public class Base64JsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        // This converter can be applied to any type, as long as it's used as an attribute on a property.
        return true;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            var base64String = reader.Value?.ToString()
                ?? throw new Exception("Reader.Value does not contain a value. The code does not handle this situation. Feel free to update.");
            var data = Convert.FromBase64String(base64String);
            var originalValue = System.Text.Encoding.UTF8.GetString(data);
            return originalValue;
        }
        else
        {
            // If the value is not a string, use the default deserialization.
            return serializer.Deserialize(reader, objectType)
                ?? throw new Exception("Cannot deserialize property as it is null,. Feel free to update the code.");
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        // Convert the value to a byte array and then to a Base64 string.
        var data = System.Text.Encoding.UTF8.GetBytes(
            value?.ToString() ?? throw new Exception("Value is null when serializing. Feel free to update the code to handle this.")) ;
        var base64String = Convert.ToBase64String(data);

        // Write the Base64 string to the JSON output.
        writer.WriteValue(base64String);
    }
}