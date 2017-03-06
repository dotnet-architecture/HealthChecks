using System;

static class Guard
{
    public static void ArgumentNotNull(string argumentName, object value)
    {
        if (value == null)
            throw new ArgumentNullException(argumentName);
    }

    public static void ArgumentNotNullOrWhitespace(string argumentName, string value)
    {
        if (value == null)
            throw new ArgumentNullException(argumentName);
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value must contain a non-whitespace value", argumentName);
    }

    public static void ArgumentValid(bool valid, string argumentName, string exceptionMessage)
    {
        if (!valid)
            throw new ArgumentException(exceptionMessage, argumentName);
    }
}
