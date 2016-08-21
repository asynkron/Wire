using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Wire.PerfTest.Types
{
    [ProtoContract]
    [Serializable]
    public class TypicalMessage
    {
        [ProtoMember(1)]
        public string StringProp { get; set; }

        [ProtoMember(2)]
        public int IntProp { get; set; }

        [ProtoMember(3)]
        public Guid GuidProp { get; set; }

        [ProtoMember(4)]
        public DateTime DateProp { get; set; }
    }
}
