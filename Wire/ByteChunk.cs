namespace Wire
{
    public unsafe struct ByteChunk
    {
        public byte* Start;
        public readonly byte* End;

        public ByteChunk(byte* start, byte* end)
        {
            Start = start;
            End = end;
        }
    }
}