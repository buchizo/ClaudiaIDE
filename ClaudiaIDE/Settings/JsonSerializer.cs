using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ClaudiaIDE.Settings
{
    public static class JsonSerializer<TType> where TType : class
    {
        public static string Serialize(TType instance)
        {
            var serializer = new DataContractJsonSerializer(typeof(TType));
            using (var stream = new MemoryStream())
            using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, true, true))
            {
                serializer.WriteObject(writer, instance);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray(), 0, stream.ToArray().Count());
            }
        }

        public static TType DeSerialize(string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(TType));
                return serializer.ReadObject(stream) as TType;
            }
        }
    }
}