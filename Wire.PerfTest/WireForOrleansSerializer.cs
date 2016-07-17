using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Runtime;
using Orleans.Serialization;

namespace Wire.PerfTest
{
    class WireForOrleansSerializer : IExternalSerializer
    {
        private readonly Serializer _serializer;
        private TraceLogger _logger;

        public WireForOrleansSerializer()
        {
            _serializer = new Serializer(new SerializerOptions(preserveObjectReferences:true));
        }

        public void Initialize(TraceLogger logger)
        {
            _logger = logger;
        }

        public bool IsSupportedType(Type itemType)
        {
            return true;
        }

        public object DeepCopy(object source)
        {
            using (var stream = new MemoryStream())
            {
                _serializer.Serialize(source, stream);
                stream.Position = 0;
                var res = _serializer.Deserialize(stream);
                return res;
            }
        }

        public void Serialize(object item, BinaryTokenStreamWriter writer, Type expectedType)
        {
            using (var stream = new MemoryStream())
            {
                _serializer.Serialize(item, stream);
                var bytes = stream.ToArray();
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
        }

        public object Deserialize(Type expectedType, BinaryTokenStreamReader reader)
        {
            var n = reader.ReadInt();
            var bytes = reader.ReadBytes(n);
            using (var stream = new MemoryStream(bytes))
            {
                var res = _serializer.Deserialize(stream);
                return res;
            }
        }
    }
}
