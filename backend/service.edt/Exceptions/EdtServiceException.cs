namespace edt.service.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public class EdtServiceException : Exception
{
    public EdtServiceException() : base() { }
    public EdtServiceException(string message) : base(message ?? "EDT Service not responding") { }

    protected EdtServiceException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }

}
