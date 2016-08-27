namespace Wire.ValueSerializers
{
    // ReSharper disable once TypeParameterCanBeVariant
    public delegate TElementType ReadChunk<TElementType>(ref ByteChunk chunk);
}