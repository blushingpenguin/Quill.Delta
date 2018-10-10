using System;

namespace Quill.Delta
{
    public class ScriptConverter
    {
        public static string GetStringValue(ScriptType? script) =>
            script.HasValue ? GetStringValue(script.Value) : "";

        public static string GetStringValue(ScriptType script) =>
            script == ScriptType.Sub ? "sub" :
            script == ScriptType.Super ? "super" : "";

        public static string GetTag(ScriptType script)
        {
            string tag =
                script == ScriptType.Sub ? "sub" :
                script == ScriptType.Super ? "sup" : null;
            if (tag == null)
            {
                throw new InvalidOperationException($"Unable to find a tag for {script}");
            }
            return tag;
        }

        public static ScriptType? GetEnumValue(string script) =>
            script == "sub" ? ScriptType.Sub :
            script == "super" ? (ScriptType?)ScriptType.Super : null;
    }
}
