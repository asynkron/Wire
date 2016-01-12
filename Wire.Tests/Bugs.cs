using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wire.Tests
{
    [TestClass]
    public class Bugs
    {
        [TestMethod]
        public void CanSerialieCustomType_bug()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(preserveObjectReferences: true,versionTolerance:true));
            var root = new Recover(SnapshotSelectionCriteria.Latest);

            serializer.Serialize(root, stream);
            stream.Position = 0;
            var actual = serializer.Deserialize<Recover>(stream);
        }


        public class SnapshotSelectionCriteria
        {
            public static SnapshotSelectionCriteria Latest { get; set; } = new SnapshotSelectionCriteria()
            {
                Foo = "hello",
            };
            public string Foo { get; set; }
        }
        [Serializable]
        public sealed class Recover
        {
            public static readonly Recover Default = new Recover(SnapshotSelectionCriteria.Latest);
            public Recover(SnapshotSelectionCriteria fromSnapshot, long toSequenceNr = long.MaxValue, long replayMax = long.MaxValue)
            {
                FromSnapshot = fromSnapshot;
                ToSequenceNr = toSequenceNr;
                ReplayMax = replayMax;
            }

            /// <summary>
            /// Criteria for selecting a saved snapshot from which recovery should start. Default is del youngest snapshot.
            /// </summary>
            public SnapshotSelectionCriteria FromSnapshot { get; private set; }

            /// <summary>
            /// Upper, inclusive sequence number bound. Default is no upper bound.
            /// </summary>
            public long ToSequenceNr { get; private set; }

            /// <summary>
            /// Maximum number of messages to replay. Default is no limit.
            /// </summary>
            public long ReplayMax { get; private set; }
        }
    }
}
