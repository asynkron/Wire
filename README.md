# Wire

A high performance polymorphic serializer for the .NET framework.

> Wire is still in beta and may have breaking changes to both the API and serialization format on the road to 1.0

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

This is essential for frameworks like Akka.NET where we need to be able to resolve live Actor References in the deserializing system.

## Version Tolerance

Wire has been designed to work in multiple modes in terms of version tolerance vs. performance.

1. Pre Register Types, when using "Pre registered types", Wire will only emit a type ID in the output stream.
   This results in the best performance, but is also fragile if different clients have different versions of the contract types.
2. Non Versioned, this is largely the same as the above, but the serializer does not need to know about your types up front. it will embed the fully qualified typename
   in the outputstream. this results in a larger payload and some performance overhead.
3. Versioned, in this mode, Wire will emit both type names and field information in the output stream.
   This allows systems to have slightly different versions of the contract types where some fields may have been added or removed.

### Durable persistence and version tolerance

Wire has been designed as a wire format, point to point for soft realtime scenarios.
If you need a format that is durable for persistence over time.
e.g. EventSourcing or for message queues, then **Protobuf**, **Thrift**, **Flatbuffers** or **MS Bond** will be a better choise as those formats have been designed for true versiom tolerance.

## Performance

Wire has been designed with a performance first mindset.
It is not _the_ most important aspect of Wire, Surrogates and polymorphism is more critical for what we want to solve.
But even with it's rich featureset, Wire performs extremely well.

```text
Wire - preregister types
   Serialize                      312 ms
   Deserialize                    261 ms
   Size                           38 bytes
   Total                          573 ms
Wire - no version data
   Serialize                      327 ms
   Deserialize                    354 ms
   Size                           73 bytes
   Total                          681 ms
Wire - preserve object refs
   Serialize                      400 ms
   Deserialize                    369 ms
   Size                           73 bytes
   Total                          769 ms
MS Bond
   Serialize                      429 ms
   Deserialize                    404 ms
   Size                           50 bytes
   Total                          833 ms
Wire - version tolerant
   Serialize                      423 ms
   Deserialize                    674 ms
   Size                           195 bytes
   Total                          1097 ms
Protobuf.NET
   Serialize                      638 ms
   Deserialize                    721 ms
   Size                           42 bytes
   Total                          1359 ms
Jil
   Serialize                      1448 ms
   Deserialize                    714 ms
   Size                           123 bytes
   Total                          2162 ms
Net Serializer
   Serialize                      1289 ms
   Deserialize                    1113 ms
   Size                           39 bytes
   Total                          2402 ms
Json.NET
   Serialize                      3767 ms
   Deserialize                    5936 ms
   Size                           187 bytes
   Total                          9703 ms
Binary formatter
   Serialize                      10784 ms
   Deserialize                    11374 ms
   Size                           362 bytes
   Total                          22158 ms
```

This test was run using the following object definition:

```
public class Poco
{
    public string StringProp { get; set; }      //using the text "hello"
    public int IntProp { get; set; }            //123
    public Guid GuidProp { get; set; }          //Guid.NewGuid()
    public DateTime DateProp { get; set; }      //DateTime.Now
}
```

> Big disclaimer: The above results change drastically depending on your contracts, e.g. using smaller messages favor both NetSerializer and Jil.
> There is no "best" or "fastest" serializer, it all depends on context and requirements.
