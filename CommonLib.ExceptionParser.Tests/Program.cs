// See https://aka.ms/new-console-template for more information


using System.Text;
using CommonLib.ExceptionParser;

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

Console.WriteLine("Running parser...");
Console.WriteLine("Input: ");
Console.WriteLine(builder.ToString());
ExceptionResult? e = ExceptionParser.Parse(builder.ToString());

if (e == null)
{
    Console.WriteLine("Exception could not be parsed.");
}
