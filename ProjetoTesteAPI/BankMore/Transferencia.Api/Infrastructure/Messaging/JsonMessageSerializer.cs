using System.Text.Json;
using KafkaFlow;

namespace Transferencia.Api.Infrastructure.Messaging
{
    public class JsonMessageSerializer : ISerializer, IDeserializer
    {
        public object Deserialize(byte[] data, Type type)
        {
            return JsonSerializer.Deserialize(data, type) ?? Activator.CreateInstance(type)!;
        }

        public byte[] Serialize(object message, Type type)
        {
            return JsonSerializer.SerializeToUtf8Bytes(message, type);
        }

        public async Task<object> DeserializeAsync(Stream stream, Type type, ISerializerContext context)
        {
            var result = await JsonSerializer.DeserializeAsync(stream, type);
            return result ?? Activator.CreateInstance(type)!;
        }

        public Task SerializeAsync(object message, Stream stream, ISerializerContext context)
        {
            return JsonSerializer.SerializeAsync(stream, message, message.GetType());
        }
    }
}
