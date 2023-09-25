using System;
using System.Collections.Generic;

namespace CommonLib.ExceptionParser;

public static class ExceptionParser
{
    private const string ExceptionText = "Exception";

    private static bool IsException(ReadOnlySpan<char> span)
    {
        for (var i = 0; i < span.Length; i++)
        {
            if (i + 9 >= span.Length)
            {
                return false;
            }

            if (span.Slice(i, 9).Equals(ExceptionText.AsSpan(), StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Parses a stringified exception received from a Unity engine game
    ///     into an object tree containing the parsed exception and
    ///     associated stacktrace.
    /// </summary>
    /// <param name="exception">The stringified exception being parsed.</param>
    /// <returns>
    ///     A new <see cref="ExceptionResult"/> instance containing the
    ///     information parsed from the provided stringified exception.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     If the content passed wasn't an exception with an accompanying
    ///     stacktrace, an <see cref="ArgumentException"/> will be thrown
    ///     indicating that this method only accepts exceptions with
    ///     stacktraces.
    /// </exception>
    public static ExceptionResult? Parse(string exception)
    {
        // This method is responsible for parsing exceptions received from a
        // Unity engine game into an object tree that the overarching bot can
        // use to provide information to users. The parsing is done as follows:
        //
        // As this parser is primarily written to support RimWorld exceptions,
        // which may optionally have a log message attached to them, the start
        // of every parse begins by checking for a log message. This detection
        // is typically indirectly done *after* an exception type was found.
        // The text preceding the exception type is considered the log message
        // that was inserted into the exception stacktrace.
        //
        // If no message could be found, the next thing detected is an exception
        // class, like "System.TypeInitializerException". This detection is
        // typically done by looking for the word "Exception" followed by a
        // colon (:). If no exception type could be found the parse will fail.
        // A failed result will be indicated by either a runtime exception, or
        // from a `null` object.
        //
        // After an exception type has been found, the parser then looks for
        // the exception message. As there's typically a reason for exceptions,
        // the parser assumes one will be provided.
        //
        // After an exception message is found, the parser may branch off into
        // different sub-exception nodes if inner exceptions were found within
        // after the exception message. These are denoted by arrows "------->"
        // within the context this parser was written for (parsing Unity
        // exceptions). These sub-exception nodes will be populated later when
        // the stacktrace is being parsed.
        //
        // After the exception message, and optionally any sub-exceptions, the
        // parser will then parse the stacktrace. Stack traces are denoted by
        // a line starting with a varying degree of whitespace, following by
        // the word "at" OR three dashes (---). If a line starts with "at",
        // the line is parsed as a frame within the associated exception. If a
        // line starts with "---", the line is then parsed into a frame within
        // the associated sub-exception. Exception frames consist of the
        // method that was invoked, the parameters of the method, and the il
        // offset within the method.
        ReadOnlySpan<char> span = exception.AsSpan();

        if (!IsException(span))
        {
            throw new ArgumentException("An exception with a stacktrace wasn't passed.", nameof(exception));
        }

        // Parse the exception's header.
        ReadOnlySpan<char> header = GetLineSpan(span, 0);

        if (header.Length <= 0)
        {
            return null;
        }

        int index = span.Length - 1;
        List<ExceptionMethod> frames = new();

        ExceptionResult root = ParseHeader(header);
        ExceptionResult? current = root;

        while (true)
        {
            ReadOnlySpan<char> lineSpan = GetLineSpanReversed(span, index);

            if (lineSpan.Length <= 0 || current is null)
            {
                break;
            }

            if (lineSpan[3] == '-') // We'll assume we're at the end of an inner stacktrace.
            {
                current.Stacktrace = frames.ToArray();
                current = current.InnerException;
                frames.Clear();
            }
            else
            {
                ExceptionMethod? method = ParseMethod(lineSpan);

                if (method is null)
                {
                    index -= lineSpan.Length + 2;

                    continue;
                }

                frames.Add(method);
            }

            index -= lineSpan.Length + 2;
        }

        // Since the last stacktrace isn't added to the final exception parsed,
        // we'll add it before we return the exception tree.
        if (frames.Count > 0 && current is not null)
        {
            current.Stacktrace = frames.ToArray();
            frames.Clear();
        }

        return root;
    }

    private static ExceptionResult ParseHeader(ReadOnlySpan<char> span, bool parseLogMessage = true)
    {
        var boundaryStart = 0;
        var state = HeaderParsePosition.None;
        var root = new ExceptionResult();
        var isColonBlock = false;

        int innerExceptionIndex = span.IndexOf("--->".AsSpan());
        int end = innerExceptionIndex > 0 ? innerExceptionIndex : span.Length;

        ReadOnlySpan<char> chunk = span.Slice(0, end);

        for (var i = 0; i < chunk.Length; i++)
        {
            bool isNextColon = i + 1 < chunk.Length && chunk[i + 1] == ':';

            if (isColonBlock)
            {
                isColonBlock = false;

                continue;
            }

            if (chunk[i] == ':' && isNextColon)
            {
                isColonBlock = true;

                continue;
            }

            switch (chunk[i])
            {
                case ':' when state is HeaderParsePosition.None && parseLogMessage:
                    root.LogMessage = chunk.Slice(boundaryStart, i - boundaryStart).Trim().ToString();

                    boundaryStart = i + 1;
                    state = HeaderParsePosition.ExceptionType;

                    break;
                case ':' when state is HeaderParsePosition.ExceptionType or HeaderParsePosition.None:
                    root.Type = chunk.Slice(boundaryStart, i - boundaryStart).Trim().ToString();

                    boundaryStart = i + 1;
                    state = HeaderParsePosition.ExceptionMessage;

                    break;
                case ' ' when state is HeaderParsePosition.ExceptionMessage:
                case ':' when state is HeaderParsePosition.ExceptionMessage:
                    root.Message = chunk.Slice(boundaryStart, i - boundaryStart).Trim().ToString();

                    boundaryStart = i + 1;
                    state = HeaderParsePosition.None;

                    break;
            }
        }

        if (string.IsNullOrEmpty(root.Message))
        {
            root.Message = chunk.Slice(boundaryStart).Trim().ToString();
        }

        if (innerExceptionIndex > 0)
        {
            ReadOnlySpan<char> slice = span.Slice(innerExceptionIndex + 5, span.Length - innerExceptionIndex - 5);

            root.InnerException = ParseHeader(slice, false);
        }

        return root;
    }

    private static ExceptionMethod? ParseMethod(ReadOnlySpan<char> span)
    {
        if (span.Length <= 0)
        {
            return null;
        }

        // Used to track where the method's generic parameters end.
        // This value will always be the position in the `span` immediately
        // before the closing square bracket.
        int genericEnd = -1;

        // Used to track where the method's il offset value ends.
        // This value will always be the position in the `span` immediately
        // before the closing square bracket.
        int ilOffsetEnd = -1;

        // Used to track where the method's parameter declaration ends.
        // This value will always be the position in the `span` immediately
        // before the closing parenthesis.
        int paramEnd = -1;

        // Used to track where the method's name ends.
        // This value will either be immediately before the generic parameter
        // declaration, or after the method's parameter declarations.
        int methodEnd = -1;

        // Used to track where the method's containing type ends.
        // This value will always be the position in the `span` immediately
        // before the method's name sequence.
        int typeEnd = -1;

        // State booleans used to track the next value that's being parsed.
        // This could be replaced with an enum.
        var methodNext = false;
        var typeNext = false;

        string method = null!;
        string type = null!;
        string ilOffset = null!;
        string[] generics = Array.Empty<string>();
        bool isNativeWrapper = span.StartsWith("  at (wrapper managed-to-native)".AsSpan());
        bool isDynamicMethod = span.StartsWith("  at (wrapper dynamic-method)".AsSpan());
        List<ExceptionParameter> @params = new();

        for (int i = span.Length - 1; i >= 0; i--)
        {
            switch (span[i])
            {
                case '.' when methodNext:
                    int end = methodEnd - i;

                    if (span[end] == '(')
                    {
                        end--;
                    }

                    method = span.Slice(i + 1, end).Trim().ToString();
                    typeEnd = i - 1;
                    methodNext = false;
                    typeNext = true;

                    break;
                case ' ' when typeNext:
                    type = span.Slice(i + 1, typeEnd - i).Trim().ToString();
                    typeNext = false;

                    break;
                case ']' when ilOffsetEnd == -1 && paramEnd == -1:
                    ilOffsetEnd = i - 1;

                    break;
                case '[' when ilOffsetEnd > 0:
                    ilOffset = span.Slice(i + 1, ilOffsetEnd - i + 1).Trim().ToString();
                    ilOffsetEnd = -2;

                    break;
                case ')' when paramEnd == -1:
                    paramEnd = i - 1;

                    if (span[paramEnd] == '(')
                    {
                        paramEnd = -2;
                        methodNext = true;
                        methodEnd = span[i - 1] == ' ' ? i - 2 : i - 1;
                        methodEnd = span[methodEnd] == '(' ? methodEnd - 1 : methodEnd;
                    }

                    break;
                case '(' when paramEnd > 0:
                case ',' when paramEnd > 0:
                    int start;
                    int length;

                    if (span[i + 1] == ' ')
                    {
                        start = i + 2;
                        length = paramEnd - (i - 1);
                    }
                    else
                    {
                        start = i + 1;
                        length = paramEnd - i;
                    }

                    ReadOnlySpan<char> param = span.Slice(start, length >= span.Length ? span.Length - 1 : length);

                    if (span[i] == '(')
                    {
                        paramEnd = -2;
                        methodEnd = span[i - 1] == ' ' ? i - 2 : i - 1;
                        methodNext = true;
                    }

                    @params.Add(ParseParameter(param));

                    break;
                case ']' when genericEnd == -1 && paramEnd == -2:
                    genericEnd = i - 1;
                    methodNext = false;

                    break;
                case '[' when genericEnd > 0:
                    generics = ParseGenerics(span.Slice(i + 1, genericEnd - (i + 1)));
                    genericEnd = -2;
                    methodEnd = i - 1;
                    methodNext = true;

                    break;
            }
        }

        if (string.IsNullOrEmpty(type) && typeEnd > 0)
        {
            // There was no "at " prior to the type.
            ReadOnlySpan<char> chunk = span.Slice(0, typeEnd);

            if (chunk.StartsWith("(wrapper managed-to-native)".AsSpan()))
            {
                isNativeWrapper = true;
                chunk = chunk.Slice(chunk.IndexOf(' '));
            }

            type = chunk.ToString();
        }

        return new ExceptionMethod
        {
            Method = method,
            Type = type,
            IlOffset = ilOffset,
            GenericParameters = generics,
            Parameters = @params.ToArray(),
            IsNativeWrapper = isNativeWrapper
        };
    }

    private static ExceptionParameter ParseParameter(ReadOnlySpan<char> span)
    {
        ReadOnlySpan<char> typeSpan;
        var paramNameSpan = ReadOnlySpan<char>.Empty;
        var genericSpan = ReadOnlySpan<char>.Empty;

        int spaceIndex = span.IndexOf(' ');

        if (spaceIndex > 0) // We have a parameter name.
        {
            typeSpan = span.Slice(0, spaceIndex);
            paramNameSpan = span.Slice(spaceIndex + 1, span.Length - (spaceIndex + 1));
        }
        else
        {
            typeSpan = span;
        }

        int genericIndex = typeSpan.LastIndexOf('[');

        if (genericIndex > 0 && span[genericIndex] == '[' && span[genericIndex + 1] != ']')
        {
            genericSpan = typeSpan.Slice(genericIndex + 1, typeSpan.Length - (genericIndex + 1));
            typeSpan = typeSpan.Slice(0, genericIndex - 1);
        }

        return new ExceptionParameter { Type = typeSpan.ToString(), GenericParameters = ParseGenerics(genericSpan), Name = paramNameSpan.ToString() };
    }

    private static string[] ParseGenerics(ReadOnlySpan<char> span)
    {
        var boundaryStart = 0;
        var items = new List<string>();

        for (var index = 0; index < span.Length; index++)
        {
            switch (span[index])
            {
                case ',':
                    items.Add(span.Slice(boundaryStart, index - boundaryStart).ToString());
                    boundaryStart = index + 1;

                    break;
                case ' ':
                    boundaryStart = index + 1;

                    break;
            }
        }

        if (span.Length - boundaryStart > 0)
        {
            items.Add(span.Slice(boundaryStart).ToString());
        }

        return items.ToArray();
    }

    private static ReadOnlySpan<char> GetLineSpan(ReadOnlySpan<char> span, int start)
    {
        for (int index = start; index < span.Length; index++)
        {
            switch (span[index])
            {
                case '\n':
                    return span.Slice(start, index - start);
            }
        }

        return ReadOnlySpan<char>.Empty;
    }

    private static ReadOnlySpan<char> GetLineSpanReversed(ReadOnlySpan<char> span, int start)
    {
        if (span[start] == '\n')
        {
            start--;
        }

        // Iterates backwards through an exception string span to break it into
        // lines
        for (int index = start; index >= 0; index--)
        {
            switch (span[index])
            {
                case '\n':
                    return span.Slice(index + 1, start - (index + 1));
            }
        }

        return ReadOnlySpan<char>.Empty;
    }

    private enum HeaderParsePosition { None, ExceptionType, ExceptionMessage }
}
