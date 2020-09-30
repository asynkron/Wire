// -----------------------------------------------------------------------
//   <copyright file="Bugs.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using Xunit;

namespace Wire.Tests
{
    public class Bugs
    {
        [Fact]
        public void CanSerialieCustomType_bug()
        {
            var stream = new MemoryStream();
            var serializer = new Serializer(new SerializerOptions(true));
            var root = new Recover(SnapshotSelectionCriteria.Latest);

            serializer.Serialize(root, stream);
            stream.Position = 0;
            var actual = serializer.Deserialize<Recover>(stream);
        }

        [Fact]
        public void CanSerializeMessageWithByte()
        {
            var stream = new MemoryStream();
            var msg = new ByteMessage(DateTime.UtcNow, 1, 2);
            var serializer = new Serializer(new SerializerOptions(true));
            serializer.Serialize(msg, stream);
            stream.Position = 0;
            var res = serializer.Deserialize(stream);
        }

        public class ByteMessage
        {
            public ByteMessage(DateTime utcTime, byte byteValue, long longValue)
            {
                UtcTime = utcTime;
                ByteValue = byteValue;
                LongValue = longValue;
            }

            public DateTime UtcTime { get; }
            public long LongValue { get; }
            public byte ByteValue { get; }

            public override bool Equals(object obj)
            {
                var msg = obj as ByteMessage;
                return msg != null && Equals(msg);
            }

            public bool Equals(ByteMessage other)
            {
                return UtcTime.Equals(other.UtcTime) && LongValue.Equals(other.LongValue) &&
                       ByteValue.Equals(other.ByteValue);
            }
        }

        public class SnapshotSelectionCriteria
        {
            public static SnapshotSelectionCriteria Latest { get; set; } = new SnapshotSelectionCriteria
            {
                Foo = "hello"
            };

            public string Foo { get; set; }
        }

        public sealed class Recover
        {
            public static readonly Recover Default = new Recover(SnapshotSelectionCriteria.Latest);

            public Recover(SnapshotSelectionCriteria fromSnapshot, long toSequenceNr = long.MaxValue,
                long replayMax = long.MaxValue)
            {
                FromSnapshot = fromSnapshot;
                ToSequenceNr = toSequenceNr;
                ReplayMax = replayMax;
            }

            /// <summary>
            ///     Criteria for selecting a saved snapshot from which recovery should start. Default is del youngest snapshot.
            /// </summary>
            public SnapshotSelectionCriteria FromSnapshot { get; }

            /// <summary>
            ///     Upper, inclusive sequence number bound. Default is no upper bound.
            /// </summary>
            public long ToSequenceNr { get; }

            /// <summary>
            ///     Maximum number of messages to replay. Default is no limit.
            /// </summary>
            public long ReplayMax { get; }
        }
    }
}