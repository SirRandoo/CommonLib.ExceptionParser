namespace CommonLib.ExceptionParser;

/// <summary>
///     Represents an method call within an exception stacktrace.
/// </summary>
public record ExceptionMethod
{
    /// <summary>
    ///     The qualified name to the class that contains the method called.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    ///     The name of the method that was called.
    /// </summary>
    public string Method { get; set; } = null!;

    /// <summary>
    ///     The optional IL offset of the method.
    /// </summary>
    public string? IlOffset { get; set; }

    /// <summary>
    ///     The parameters the method called was declared with.
    /// </summary>
    public ExceptionParameter[] Parameters { get; set; } = null!;

    /// <summary>
    ///     The names of any generic parameters that were declared in the
    ///     method's signature.
    /// </summary>
    public string[] GenericParameters { get; set; } = null!;

    /// <summary>
    ///     Whether the method was a wrapper for a native method contained
    ///     in a natively compiled library.
    /// </summary>
    public bool IsNativeWrapper { get; set; }
}