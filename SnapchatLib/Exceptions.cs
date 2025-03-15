using System;

namespace SnapchatLib.Exceptions;

public class DeviceNotSetException: Exception
{
    public DeviceNotSetException() : base("The value in SnapchatConfig.Device is needed. No exceptions, abuse is not tolerated")
    {
    }
}

public class InstallNotSetException: Exception
{
    public InstallNotSetException() : base("The value in SnapchatConfig.Install is needed. No exceptions, abuse is not tolerated")
    {
    }
}

public abstract class ContactJustinException : Exception
{
    protected ContactJustinException(string error, Exception exception = null) : base($"${error}. Contact Justinx to report a bug.{exception?.Message}", exception)
    {
    }
}

public class SignerException : ContactJustinException
{
    public SignerException(string error): base(error)
    {
    }
}

public class AuthTokenNotSetException : Exception
{
    public AuthTokenNotSetException(): base("AuthToken is not defined. Use SnapchatLib.Login first")
    {
    }
}

public class FailedToInitClient : Exception
{
    public FailedToInitClient() : base("Init Client Failed Retry")
    {
    }
}

public class DeserializationException : ContactJustinException
{
    public DeserializationException(string typeName) : base($"Unable to deserialize data into type \"{typeName}\"")
    {
    }
}

public class SerializationException : ContactJustinException
{
    public SerializationException(string typeName, Exception innerException) : base($"Unable to deserialize data into type \"{typeName}\"", innerException)
    {
    }
}

public class FailedToPredictGenderException : ContactJustinException
{
    public FailedToPredictGenderException() : base($"Failed to predict gender")
    {
    }
}

public class EmptyIEnumerableException : ContactJustinException
{
    public EmptyIEnumerableException() : base($"The code was expecting elements in side a collection but it was empty")
    {
    }
}

public class DeadProxyException : Exception
{
    public DeadProxyException() : base("Dead Proxy")
    {
    }
}
