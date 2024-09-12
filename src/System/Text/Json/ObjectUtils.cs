using System.Diagnostics;
using System.Text.Json.Serialization;

namespace System.Text.Json
{
    public static class ObjectUtils
    {
        internal static readonly JsonDocumentOptions DocumentOptions = new()
        {
            CommentHandling = JsonCommentHandling.Skip,
        };

        internal static readonly JsonSerializerOptions SerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = true,
            WriteIndented = true,
        };

        public static void DeserializeObject(string json, Func<string, Type?> getObjectType, Func<string, object?, bool> setValue)
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(getObjectType);
            ArgumentNullException.ThrowIfNull(setValue);
#else
            Throw.IfNull(getObjectType);
            Throw.IfNull(setValue);
#endif
            //var json = JsonSerializer.Deserialize<JsonDocument>(json);
            try
            {
                using var document = JsonDocument.Parse(json, DocumentOptions);
                JsonElement root = document.RootElement;
                foreach (JsonProperty property in root.EnumerateObject())
                {
                    string name = property.Name;
                    var value = property.Value;
                    var type = getObjectType(name);
                    if (type == null)
                    {
                        continue;
                    }
                    try
                    {
                        var obj = value.Deserialize(type, SerializerOptions);
                        if (!setValue(name, obj))
                        {
                            Debug.Assert(false, $"Can't set property: {name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Assert(false, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
        }

        public static string? SerializeObject<T>(T source)
        {
            if (source is null)
            {
                return null;
            }
            var type = source.GetType();
            var value = JsonSerializer.Serialize(source, type, SerializerOptions);
            return value;
        }
    }
}
