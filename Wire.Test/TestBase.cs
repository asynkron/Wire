using System.IO;

namespace Wire.Tests
{
    
    public abstract class TestBase
    {
        private readonly Serializer serializer;
        private readonly MemoryStream stream;

        protected TestBase()
        {
            serializer = new Serializer();
            stream = new MemoryStream();
        }

        public void Reset()
        {
            stream.Position = 0;
        }

        public void Serialize(object o)
        {
            serializer.Serialize(o, stream);
        }

        public T Deserialize<T>()
        {
            return serializer.Deserialize<T>(stream);
        }
    }
}