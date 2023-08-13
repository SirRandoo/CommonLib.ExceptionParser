// MIT License
//
// Copyright (c) 2023 SirRandoo
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

namespace CommonLib.ExceptionParser;

/// <summary>
///     A class containing the information that was parsed from the
///     exception.
/// </summary>
public record ExceptionResult
{
    /// <summary>
    ///     The optional log message that was prefixed to the exception.
    /// </summary>
    /// <remarks>
    ///     This property will only be filled when the message parsed
    ///     originated from a source that embeds exceptions into log
    ///     messages.
    /// </remarks>
    public string? LogMessage { get; set; }

    /// <summary>
    ///     The qualified name of the exception type that was thrown.
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    ///     The message that accompanied the thrown exception.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    ///     An optional inner exception that this exception encapsulated.
    /// </summary>
    public ExceptionResult? InnerException { get; set; }

    /// <summary>
    ///     The list of methods that were called up until this exception was
    ///     thrown.
    /// </summary>
    /// <remarks>
    ///     The stacktrace is ordered in the most outer method to the inner
    ///     most method. In a printed stacktrace, this would be ordered from
    ///     the bottom of the stacktrace to the top.<br/>
    ///     <br/>
    ///     The stacktrace represented within this property will only be the
    ///     method invocations that lead up to this exception. Should there
    ///     be
    ///     an inner exception, or this exception is an inner exception, the
    ///     stacktrace will represent the method invocations that lead up to
    ///     the inner exception relative to the outer exception.
    /// </remarks>
    public ExceptionMethod[] Stacktrace { get; set; } = Array.Empty<ExceptionMethod>();
}
