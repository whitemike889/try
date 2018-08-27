using System;
using Newtonsoft.Json;

namespace WorkspaceServer.Models.Execution
{
    [JsonConverter(typeof(BufferIdConverter))]
    public class BufferId
    {
        public BufferId(string fileName, string regionName = null)
        {
            FileName = fileName ?? "";
            RegionName = regionName;
        }

        public string FileName { get; }

        public string RegionName { get; }

        public override bool Equals(object obj)
        {
            var other = obj as BufferId;
            return other != null &&
                   FileName == other.FileName &&
                   RegionName == other.RegionName;
        }

        public override int GetHashCode() => HashCode.Combine(FileName, RegionName);

        public static bool operator ==(BufferId left, BufferId right) => Equals(left, right);

        public static bool operator !=(BufferId left, BufferId right) => !Equals(left, right);

        public override string ToString() => string.IsNullOrWhiteSpace(RegionName)
                                                 ? FileName
                                                 : $"{FileName}@{RegionName}";

        public static BufferId Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Empty;
            }

            var parts = value.Split("@");

            return new BufferId(parts[0].Trim(), parts.Length > 1 ? parts[1].Trim() : null);
        }

        public static implicit operator BufferId(string value)
        {
            return Parse(value);
        }

        public static BufferId Empty { get; } = new BufferId("");

        internal class BufferIdConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return Parse(reader?.Value?.ToString());
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(BufferId);
            }
        }
    }
}
