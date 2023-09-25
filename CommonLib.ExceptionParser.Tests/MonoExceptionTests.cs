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

using System.Text;
using Xunit;

namespace CommonLib.ExceptionParser.Tests;

public class MonoExceptionTests
{
    private readonly ExceptionResult? _exception = ParseException();

    [Fact]
    public void Mono_TestLogMessage()
    {
        Assert.Equal("This is a log message", _exception?.LogMessage);
    }

    [Fact]
    public void Mono_TestExceptionType()
    {
        Assert.Equal("System.TypeInitializationException", _exception?.Type);
    }

    [Fact]
    public void Mono_TestExceptionMessage()
    {
        Assert.Equal("The type initializer for 'Project.Mod.Data' threw an exception.", _exception?.Message);
    }

    [Fact]
    public void Mono_TestInnerType()
    {
        Assert.Equal("System.NullReferenceException", _exception?.InnerException?.Type);
    }

    [Fact]
    public void Mono_TestInnerExceptionMessage()
    {
        Assert.Equal("Object reference not set to an instance of an object.", _exception?.InnerException?.Message);
    }

    [Fact]
    public void Mono_TestMethodName()
    {
        ExceptionMethod? method = _exception?.Stacktrace[0];

        Assert.Equal("DoWindowContents", method?.Method);
    }

    [Fact]
    public void Mono_TestMethodClass()
    {
        ExceptionMethod? method = _exception?.Stacktrace[0];

        Assert.Equal("Framework.WindowSettings", method?.Type);
    }

    [Fact]
    public void Mono_TestMethodParamType()
    {
        ExceptionMethod? method = _exception?.Stacktrace[0];
        ExceptionParameter? parameter = method?.Parameters[0];

        Assert.Equal("UnityEngine.Rect", parameter?.Type);
    }

    [Fact]
    public void Mono_TestMethodParamName()
    {
        ExceptionMethod? method = _exception?.Stacktrace[0];
        ExceptionParameter? parameter = method?.Parameters[0];

        Assert.Equal("inRect", parameter?.Name);
    }

    [Fact]
    public void Mono_TestInnerMethodParamName()
    {
        ExceptionResult? exception = _exception?.InnerException;

        Assert.NotNull(exception);

        ExceptionMethod? method = exception.Stacktrace[exception.Stacktrace.Length - 1];

        Assert.NotNull(method);

        ExceptionParameter? parameter = method.Parameters[0];

        Assert.Equal("model", parameter?.Name);
    }

    [Fact]
    public void Mono_TestInnerMethodParamType()
    {
        ExceptionResult? exception = _exception?.InnerException;

        Assert.NotNull(exception);

        ExceptionMethod? method = exception.Stacktrace[exception.Stacktrace.Length - 1];

        Assert.NotNull(method);

        ExceptionParameter? parameter = method.Parameters[0];

        Assert.Equal("LegacyProject.Mod.Model", parameter?.Type);
    }

    [Fact]
    public void Mono_TestInnerMethodName()
    {
        ExceptionMethod? method = _exception?.InnerException?.Stacktrace[0];

        Assert.Equal(".cctor", method?.Method);
    }

    [Fact]
    public void Mono_TestInnerMethodType()
    {
        ExceptionMethod? method = _exception?.InnerException?.Stacktrace[0];

        Assert.Equal("Project.Mod.Data", method?.Type);
    }

    [Fact]
    public void Mono_TestInnerExistence()
    {
        Assert.NotNull(_exception?.InnerException);
    }

    [Fact]
    public void Mono_TestExistence()
    {
        Assert.NotNull(_exception);
    }

    [Fact]
    public void Mono_TestNativeWrapper()
    {
        Assert.NotNull(_exception);

        Assert.True(_exception.Stacktrace[_exception.Stacktrace.Length - 1].IsNativeWrapper);
    }

    [Fact]
    public void Mono_TestDynamicMethod()
    {
        Assert.NotNull(_exception);

        Assert.True(_exception.Stacktrace[3].IsDynamicMethod);
    }

    [Fact]
    public void Mono_TestIlOffset()
    {
        Assert.Equal("0x000e5", _exception?.Stacktrace[0].IlOffset);
    }

    [Fact]
    public void Mono_TestInnerIlOffset()
    {
        Assert.Equal("0x0019f" ,_exception?.InnerException?.Stacktrace[0].IlOffset);
    }

    [Fact]
    public void Mono_TestInnerGenericParam()
    {
        ExceptionResult? exception = _exception?.InnerException;

        Assert.NotNull(exception);
        Assert.Contains("T", exception.Stacktrace[3].Parameters[0].GenericParameters);
    }

    [Fact]
    public void Mono_TestInnerMethodGeneric()
    {
        ExceptionResult? exception = _exception?.InnerException;

        Assert.NotNull(exception);
        Assert.Contains("TSource", exception.Stacktrace[3].GenericParameters);
    }

    private static ExceptionResult? ParseException()
    {
        var builder = new StringBuilder();

        builder.Append("This is a log message: System.TypeInitializationException: The type initializer for 'Project.Mod.Data' threw an exception.");
        builder.Append(" ---> System.NullReferenceException: Object reference not set to an instance of an object.\n");
        builder.Append("  at Project.Mod.Models.Model.FromLegacy (LegacyProject.Mod.Model model) [0x00054] in <eb98w39832398eease83>:0\n");
        builder.Append("  at System.Linq.Enumerable+WhereSelectListIterator`2[TSource,TResult].ToList () [0x00025] in <23423ebaes7878asebse3>:0\n");
        builder.Append("  at System.Linq.Enumerable.ToList[TSource] (System.Collections.Generic.IEnumerable`1[T] source) [0x0001f] in <23i82983m2oi3m9283>:0\n");
        builder.Append("  at Project.Mod.Data.DumpCommands () [0x0005e] in <we898e92832938e9283>:0\n");
        builder.Append("  at Project.Mod.Data.DumpAllData () [0x00019] in <9283j982m938cm9283u9>:0\n");
        builder.Append("  at Project.Mod.Data..cctor () [0x0019f] in <2938jm92j83j9283j9283>:0\n");
        builder.Append("   --- End of inner exception stack trace ---\n");
        builder.Append("  at (wrapper managed-to-native) System.Object.__icall_wrapper_mono_generic_class_init(intptr)\n");
        builder.Append("  at Project.Mod.Windows.Window.Generate () [0x00299] in <92839283j928j3234>:0\n");
        builder.Append("  at Project.Mod.Windows.Window.PreOpen () [0x0001f] in <asoeianoi3n2iun323i2>:0\n");
        builder.Append("  at Framework.WindowStack.Add (Framework.Window window) [0x0001f] in <23232ni3uniun2u3i>:0\n");
        builder.Append("  at (wrapper dynamic-method) Project.Mod.Settings.Settings_Object.DoWindowContents_Patch0(UnityEngine.Rect,Framework.Listing)\n");
        builder.Append("  at Project.Mod.Settings.DoWindowContents (UnityEngine.Rect rect) [0x00418] in <23ni2un3i2un3iu2ni3>:0\n");
        builder.Append("  at Project.Mod.ModClass.DoSettingsWindowContents (UnityEngine.Rect inRect) [0x00007] in <29832983jamisun3>:0\n");
        builder.Append("  at Framework.WindowSettings.DoWindowContents (UnityEngine.Rect inRect) [0x000e5] in <2i3n2i23oi23oij2jo3ix>:0\n");

        return ExceptionParser.Parse(builder.ToString());
    }
}
