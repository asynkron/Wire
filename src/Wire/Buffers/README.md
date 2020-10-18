These buffer reader/writer abstractions are from the Hagar Serializer by Reuben Bond.

https://github.com/ReubenBond/Hagar

# Hagar

[![Build status](https://dev.azure.com/reubenbond/Hagar/_apis/build/status/Azure%20Pipelines%20CI)](https://dev.azure.com/reubenbond/Hagar/_build/latest?definitionId=4) [![](https://codecov.io/gh/ReubenBond/Hagar/branch/master/graph/badge.svg)](https://codecov.io/gh/ReubenBond/Hagar)


There are many existing serialization libraries and formats which are efficient, fast, and support schema evolution, so why create this?

Existing serialization libraries which support version tolerance tend to restrict how data is modelled, usually by providing a very restricted type system which supports few of the features found in common type systems, features such as:

* Polymorphism
* Generics (parametric types)
* References, including cyclic references

Hagar is a new serialization library which does supports these features, is fast & compact, supports schema evolution, and requires minimal input from the developer.

## Encoding

* Fields are tagged with compact field ids. Those field ids are provided by developers.
* Fields are encoded into primitives which fall into 4 categories:
  * **Fixed length** - most numerics, unless specifically annotated.
  * **Variable length** - for variable-length integer encoding, useful for length, count, index type properties (relatively small and 0-based in nature).
  * **Length-prefixed** - strings, arrays of fixed-width primitives.
  * **Tag-delimited** - objects, collections of non-primitive types.
* Type information is embedded, but not required for parsing.
  * Separation of wire type & runtime type.
  * Library of application-defined runtime types are available during encoding & decoding. These types can be given short ids to reduce the size of the serialized payload.
  * Types can be parameterized (support for generics).
  * Types which are not specified in the type library can be explicitly named.
    * These named types are runtime specific (i.e, .NET specific).
    * Note: may want to restrict this for security reasons.
* Wire format based around 1-byte tags, which are in one of two forms:
  * `[W W W] [S S] [F F F]` where:
    * `W` is a wire type bit.
    * `S` is a schema type bit.
    * `F` is a field identifier bit.
  * `[1 1 1] [E E] [X X X]` where:
    * `E` is an extended wire type bit.
    * `X` is reserved for use in the context of the extended wire type.
* The wire type, schema type, and extended wire type are detailed more below.
* When a schema type requires extra data, it is encoded after initial tag.
* When a field id cannot be encoded in 3 bits, it is encoded after schema data.
* Overall encoding takes the form: `Tag Schema FieldId FieldData`.
* Every message can be parsed without prior knowledge of any schema type because all wire types have a fixed, well-known format for determining the length of the encoded data.
* When serializing and deserializing data, there is no single, predetermined mapping between a .NET type and a wire encoding. For example, ProtoBufs dictates that an `int64` is encoded as a `Varint` and that `float32` is encoded as a fixed 32-bit field. Instead, the serializer can determine that a `long` is encoded as `VarInt`, `Fixed32`, or `Fixed64` at runtime depending on which takes up the least space.

```C#
/// <summary>
/// Represents a 3-bit wire type, shifted into position
/// </summary>
public enum WireType : byte
{
    VarInt = 0b000 << 5, // Followed by a VarInt
    TagDelimited = 0b001 << 5, // Followed by field specifiers, then an Extended tag with EndTagDelimited as the extended wire type.
    LengthPrefixed = 0b010 << 5, // Followed by VarInt length representing the number of bytes which follow.
    Fixed32 = 0b011 << 5, // Followed by 4 bytes
    Fixed64 = 0b100 << 5, // Followed by 8 bytes
    Fixed128 = 0b101 << 5, // Followed by 16 bytes
    Reference = 0b110 << 5, // Followed by a VarInt reference to a previously defined object. Note that the SchemaType and type specification must still be included.
    Extended = 0b111 << 5, // This is a control tag. The schema type and embedded field id are invalid. The remaining 5 bits are used for control information.
}

public enum SchemaType : byte
{
    Expected = 0b00 << 3, // This value has the type expected by the current schema.
    WellKnown = 0b01 << 3, // This value is an instance of a well-known type. Followed by a VarInt type id.
    Encoded = 0b10 << 3, // This value is of a named type. Followed by an encoded type name.
    Referenced = 0b11 << 3, // This value is of a type which was previously specified. Followed by a VarInt indicating which previous type is being reused.
}

public enum ExtendedWireType : byte
{
    EndTagDelimited = 0b00 << 3, // This tag marks the end of a tag-delimited object. Field id is invalid.
    EndBaseFields = 0b01 << 3, // This tag marks the end of a base object in a tag-delimited object.
}
```

* If a type has base types, the fields of the base types are serialized before the subtype fields. Between the base type fields and its sub type is an `EndBaseFields` tag. This allows base types and sub types to have overlapping field ids without ambiguity. Therefore object encoding follows this pattern: `[StartTagDelimited] [Base Fields]* [EndBaseFields] [Sub Type fields]* [EndTagDelimited]`.

* Third-party serializers such as ProtoBuf, Bond, .NET's BinaryFormatter, JSON.NET, etc, are supported by serializing using a serializer-specific type id and including the payload via the length-prefixed wire type. This has the advantage of supporting any number of well-known serializers and does not require double-encoding the concrete type, since the external serializer is responsible for that.

## Security

Allowing arbitrary types to be specified in a serialized payload is a vector for security vulnerabilities. Because of this, all types should be checked against a whitelist.

## Rules

Version Tolerance is supported provided the developer follows a set of rules when modifying types. If the developer is familiar with systems such as ProtoBuf and Bond, then these rules will come as no surprise.

### Composites (`class` & `struct`)

* Inheritance is supported, but modifying the inheritance hierarchy of an object is not supported. The base class of a class cannot be added, changed to another class, or removed.
* With the exception of some numeric types, described in the Numerics section, field types cannot be changed.
* Fields can be added or removed at any point in an inheritance hierarchy.
* Field ids cannot be changed.
* Field ids must be unique for each level in a type hierarchy, but can be reused between base-classes and sub-classes. For example, `Base` class can declare a field with id `0` and a different field can be declared by `Sub : Base` with the same id, `0`.

### Numerics

* The *signedness* of a numeric field cannot be changed.
  * Conversions between `int` & `uint` are invalid.
* The *width* of a numeric field can be changed.
  * Eg: conversions from `int` to `long` or `ulong` to `ushort` are supported.
  * Conversions which narrow the width will throw if the runtime value of a field would cause an overflow.
    * Conversion from `ulong` to `ushort` are only supported if the value at runtime is less than `ushort.MaxValue`.
    * Conversions from `double` to `float` are only supported if the runtime value is between `float.MinValue` and `float.MaxValue`.
    * Similarly for `decimal`, which has a narrower range than both `double` and `float`.

### Types

* Types can be added to the system and used as long as either:
  * They are only used for newly added fields.
  * They are never used on older versions which do not have access to that type.
* Type names cannot be changed unless the type was always registered as a `WellKnown` type or a `TypeCodec` is used to translate between the old and new name.

## Packages

Hagar is still in development and pakages have not been published yet. Running `build.ps1` will build and locally publish packages for testing purposes.
