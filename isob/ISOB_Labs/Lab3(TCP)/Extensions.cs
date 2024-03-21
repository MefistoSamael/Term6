using System.Text;
using System.Text.Json;

namespace Lab3
{
    public static class Constants
    {
        public const int ClientPort = 1001;
        public const int ServerPort = 1000;
    }


    public static class Extensions
    {
        public static byte[] ToJsonBytes(this NetworkSegment packet)
        {
            string jsonString = JsonSerializer.Serialize(packet);
            return Encoding.UTF32.GetBytes(jsonString);
        }

        public static byte[] ToUtf32Bytes(this string str)
        {
            return Encoding.UTF32.GetBytes(str);
        }

        public static string Utf32BytesToString(this byte[] bytes, int offset)
        {
            return Encoding.UTF32.GetString(bytes, 0, offset);
        }

        public static string Utf32BytesToString(this byte[] bytes)
        {
            return Encoding.UTF32.GetString(bytes);
        }

        public static NetworkSegment DeserializeTcpPacket(this byte[] data)
        {
            string json = data.Utf32BytesToString();
            return JsonSerializer.Deserialize<NetworkSegment>(json) ?? throw new JsonException("Deserialization failed.");
        }
    }
}
