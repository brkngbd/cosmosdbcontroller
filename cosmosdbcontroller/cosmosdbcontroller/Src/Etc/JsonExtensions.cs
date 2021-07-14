namespace CosmosDbController
{
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;

    public static class JsonExtensions
    {
        /// <summary>
        /// Serialization options
        /// </summary>
        private static readonly JsonSerializerOptions SerializerOptions =
            new()
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };

        /// <summary>
        /// Method is used to deserialize the json string to the specified type.
        /// </summary>
        /// <param name="text">The string.</param>
        /// <typeparam name="T">The return value type.</typeparam>
        public static T ToObject<T>(this string text)
        {
            return JsonSerializer.Deserialize<T>(text, SerializerOptions);
        }

        /// <summary>Method is used to deserialize the stream to the specified type.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream">The stream.</param>
        public static async Task<T> ToObject<T>(this Stream stream)
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions);
        }
    }
}
