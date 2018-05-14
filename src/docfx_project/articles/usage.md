## Usage
Each codec extends <xref:Fs.Binary.Codecs.BinaryCodec>, which provides the implementation
that is common to all codecs. At it simplest, each codec provides <xref:Fs.Binary.Codecs.BinaryCodec.GetBytes(System.String)>,
<xref:Fs.Binary.Codecs.BinaryCodec.GetChars(System.Byte[])> and similar overloads. The methods decode and encode sequences of characters and bytes that are completely loaded into memory.

As an example, encoding an array of bytes with the `Base16` codec can be achieved with:

```` C#
var inputData = new byte[]{0, 1, 2, 3, 4, 5};
var encodedData = BinaryCodecs.Base16Standard.GetChars(inputData);
var decodedData = BinaryCodecs.Base16Standard.GetBytes(encodedData);
````

## Streaming
When you have an indeterminate amount of input or your simply don't want to load all of the data into memory, use the <xref:Fs.Binary.Codecs.Streaming.BinaryDecoderStream> or <xref:Fs.Binary.Codecs.Streaming.BinaryEncoderStream> classes. These classes provide a `Stream` interface that reads from or writes to an underlying `Stream` or `TextReader`, encoding or decoding the data as it is requested.

