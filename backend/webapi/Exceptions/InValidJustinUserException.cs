namespace Pidp.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public class InValidJustinUserException : Exception
{
    public InValidJustinUserException() : base() { }
    public InValidJustinUserException(string message) : base(message ?? "User not a valid Justin User.") { }
    public InValidJustinUserException(string message, Exception innerException) : base(message ?? "User not a valid Justin User.", innerException) { }
    protected InValidJustinUserException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
