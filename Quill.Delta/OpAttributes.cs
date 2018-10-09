using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Quill.Delta
{
    public class OpAttributes
    {
        public string Background { get; set; }
        public string Color { get; set; }
        public string Font { get; set; }
        public string Size { get; set; }
        public string Width { get; set; }

        public string Link { get; set; }
        public bool? Bold { get; set; }
        public bool? Italic { get; set; }
        public bool? Underline { get; set; }
        public bool? Strike { get; set; }
        public ScriptType? Script { get; set; }

        public bool? Code { get; set; }

        public ListType? List { get; set; }
        public bool? Blockquote { get; set; }
        public bool? CodeBlock { get; set; }
        public int? Header { get; set; }
        public AlignType? Align { get; set; }
        public DirectionType? Direction { get; set; }
        public int? Indent { get; set; }

        public bool Mentions { get; set; }
        public Mention Mention { get; set; }
        public string Target { get; set; }

        // should this custom blot be rendered as block?
        public bool? RenderAsBlock { get; set; }

        public Dictionary<string, JToken> CustomAttributes { get; set; }
    }
}
