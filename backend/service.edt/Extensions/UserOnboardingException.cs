namespace edt.service.Extensions;
using System.Runtime.Serialization;

[Serializable]
public class UserOnboardingException : Exception
{
    public UserOnboardingException() : base() { }
    public UserOnboardingException(string message) : base(message ?? "User not a valid Justin User.") { }
    public UserOnboardingException(string message, Exception innerException) : base(message ?? "UserId not found.", innerException) { }
    protected UserOnboardingException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
}
