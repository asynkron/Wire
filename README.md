# Wire

A high performance polymorphic serializer for the .NET framework.

## Polymorphic serializations

Wire was designed to safely transfer messages in distributed systems, for example service bus or actor model based systems.
In message based systems, it is common to receive different types of messages and apply pattern matching over those messages.
If the messages does not carry over all the relevant type information to the receiveing side, the message might no longer match exactly what your system expect.

Consilder the following case:

```csharp
public class Envelope
{
     //untyped property
     public object Payload {get;set;}
     //other properties..
     ...
}

...

var envelope = new Envelope { Payload = (float)1.2 };
```

If you for example are using a Json based serializer, it is very likely that the value `1.2` will be deserialized as a `double` as Json has no way to describe the type of the decimal value.
While if you use some sort of binary serializer like Google Protobuf, all messages needs to be designed with a strict contract up front.
Wire solves this by encoding a manifest for each value - a single byte prefix for primitive values, and fully qualified assembly names for complex types.

## Surrogates

Sometimes, you might have objects that simply can't be serialized in a safe way, the object might be contextual in some way.
Wire can solve those problems using "Surrogates", surrogates are a way to translate an object from and back to the context bound representation.

```csharp
var surrogate = Surrogate.Create<IMyContextualInterface,IMySurrogate>(original => original.ToSurrogate(), surrogate => surrogate.Restore(someContext));
var options = new SerializerOptions(surrogates: new [] { surrogate });
var serializer = new Serializer(options);
```

