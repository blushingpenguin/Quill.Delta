using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quill.Delta
{
    /// <summary>
    /// Denormalization is splitting a text insert operation that has new lines into multiple 
    /// ops where each op is either a new line or a text containing no new lines. 
    /// 
    /// Why? It makes things easier when picking op that needs to be inside a block when 
    /// rendering to html
    /// 
    /// Example: 
    ///  {insert: 'hello\n\nhow are you?\n', attributes: {bold: true}}
    /// 
    /// Denormalized:
    ///  [
    ///      {insert: 'hello', attributes: {bold: true}},
    ///      {insert: '\n', attributes: {bold: true}},
    ///      {insert: '\n', attributes: {bold: true}},
    ///      {insert: 'how are you?', attributes: {bold: true}},
    ///      {insert: '\n', attributes: {bold: true}}
    ///  ]
    /// </summary>
    public static class InsertOpDenormalizer
    {
        static readonly JToken[] s_emptyArray = new JToken[0];

        /// <summary>
        /// Splits by new line character ("\n") by putting new line characters into the 
        /// array as well. Ex: "hello\n\nworld\n " => ["hello", "\n", "\n", "world", "\n", " "]
        /// </summary>
        /// XXX: looks funny to me, like the original strips off the final array element whatever it is
        /// 
        public static string[] TokenizeWithNewLines(string str)
        {
            if (str == "\n")
            {
                return new string[] { str };
            }

            var lines = str.Split('\n');
            if (lines.Length == 1)
            {
                return lines;
            }

            var result = new List<string>(lines.Length * 2);
            for (int i = 0; i < lines.Length; ++i)
            {
                var line = lines[i];
                if (i != lines.Length - 1)
                {
                    if (!String.IsNullOrEmpty(line))
                    {
                        result.Add(line);
                        result.Add("\n");
                    }
                    else
                    {
                        result.Add("\n");
                    }
                }
                else if (!string.IsNullOrEmpty(line))
                {
                    result.Add(line);
                }
            }
            return result.ToArray();
        }

        public static IEnumerable<JToken> Denormalize(JToken op)
        {
            if (op == null || op.Type != JTokenType.Object)
            {
                return s_emptyArray;
            }

            var insert = op["insert"];
            if (insert.Type == JTokenType.Object || (
                insert.Type == JTokenType.String && insert.Value<string>() == "\\n"))
            {
                return new JToken[] { op };
            }

            var newlinedArray = TokenizeWithNewLines(insert.Value<string>());
            if (newlinedArray.Length == 1)
            {
                return new JToken[] { op };
            }

            var nl = new JObject((JObject)op);
            nl["insert"] = "\n";

            return newlinedArray.Select(line =>
            {
                if (line == "\n")
                {
                    return nl;
                }
                var obj = new JObject((JObject)op);
                obj["insert"] = line;
                return obj;
            });
        }
    }
}
