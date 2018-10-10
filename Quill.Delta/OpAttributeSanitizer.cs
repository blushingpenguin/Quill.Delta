using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Quill.Delta
{
    public static class OpAttributeSanitizer
    {
        public static string GetColour(string val)
        {
            if (!String.IsNullOrEmpty(val) && (
                IsValidHexColor(val) || IsValidColorLiteral(val)))
            {
                return val;
            }
            return null;
        }

        readonly static HashSet<string> s_sanitizedAttributes = new HashSet<string>()
        {
             "bold", "italic", "underline", "strike", "code",
             "blockquote", "code-block", "renderAsBlock",
             "background", "color", "font", "size", "link",
             "script", "list", "header", "align", "direction",
             "indent", "mentions", "mention", "width"
        };

        public static OpAttributes Sanitize(JToken dirtyAttrs)
        {
            var cleanAttrs = new OpAttributes();

            if (dirtyAttrs == null || dirtyAttrs.Type != JTokenType.Object)
            {
                return cleanAttrs;
            }

            var font = dirtyAttrs.GetStringValue("font");
            if (!String.IsNullOrEmpty(font) && IsValidFontName(font))
            {
                cleanAttrs.Font = font;
            }

            var size = dirtyAttrs.GetStringValue("size");
            if (!String.IsNullOrEmpty(size) && IsValidSize(size))
            {
                cleanAttrs.Size = size;
            }

            var link = dirtyAttrs.GetStringValue("link");
            if (!String.IsNullOrEmpty(link))
            {
                cleanAttrs.Link = UrlHelpers.Sanitize(link);
            }

            var target = dirtyAttrs.GetStringValue("target");
            if (!String.IsNullOrEmpty(target) && IsValidTarget(target))
            {
                cleanAttrs.Target = target;
            }

            cleanAttrs.Script = ScriptConverter.GetEnumValue(
                dirtyAttrs.GetStringValue("script"));

            cleanAttrs.List = ListConverter.GetEnumValue(
                dirtyAttrs.GetStringValue("list"));

            var header = dirtyAttrs.GetIntValue("header");
            if (header.HasValue && header.Value > 0)
            {
                cleanAttrs.Header = Math.Min(header.Value, 6);
            }

            cleanAttrs.Align = AlignConverter.GetEnumValue(
                dirtyAttrs.GetStringValue("align"));

            cleanAttrs.Direction = DirectionConverter.GetEnumValue(
                dirtyAttrs.GetStringValue("direction"));

            var indent = dirtyAttrs.GetIntValue("indent");
            if (indent.HasValue)
            {
                cleanAttrs.Indent = Math.Min((int)indent.Value, 30);
            }

            var width = dirtyAttrs.GetStringValue("width");
            if (!String.IsNullOrEmpty(width) && IsValidWidth(width))
            {
                cleanAttrs.Width = width;
            }

            cleanAttrs.Bold = dirtyAttrs.GetBoolValue("bold");
            cleanAttrs.Italic = dirtyAttrs.GetBoolValue("italic");
            cleanAttrs.Underline = dirtyAttrs.GetBoolValue("underline");
            cleanAttrs.Strike = dirtyAttrs.GetBoolValue("strike");
            cleanAttrs.Code = dirtyAttrs.GetBoolValue("code");
            cleanAttrs.Blockquote = dirtyAttrs.GetBoolValue("blockquote");
            cleanAttrs.CodeBlock = dirtyAttrs.GetBoolValue("code-block");
            cleanAttrs.RenderAsBlock = dirtyAttrs.GetBoolValue("renderAsBlock");

            cleanAttrs.Background = GetColour(dirtyAttrs.GetStringValue("background"));
            cleanAttrs.Color = GetColour(dirtyAttrs.GetStringValue("color"));

            var mentions = dirtyAttrs.GetBoolValue("mentions");
            var mentionToken = dirtyAttrs["mention"];
            if (mentionToken != null)
            {
                var mention = mentionToken.Value<JObject>();
                if (mentions.HasValue && mentions.Value && mention != null)
                {
                    var sanitizedMention = MentionSanitizer.Sanitize(mention);
                    if (sanitizedMention.AnySet)
                    {
                        cleanAttrs.Mentions = true;
                        cleanAttrs.Mention = sanitizedMention;
                    }
                }
            }

            foreach (var kv in (JObject)dirtyAttrs)
            {
                if (s_sanitizedAttributes.Contains(kv.Key))
                {
                    continue;
                }

                if (cleanAttrs.CustomAttributes == null)
                {
                    cleanAttrs.CustomAttributes = new Dictionary<string, JToken>();
                }
                cleanAttrs.CustomAttributes.Add(kv.Key, kv.Value);
            }

            return cleanAttrs;
        }

        static Regex s_validHexColorRe = new Regex("^#([0-9A-F]{6}|[0-9A-F]{3})$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public static bool IsValidHexColor(string colorStr)
        {
            return s_validHexColorRe.IsMatch(colorStr);
        }

        static Regex s_validColorLiteralRe = new Regex("^[a-z]{1,50}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsValidColorLiteral(string colorStr)
        {
            return s_validColorLiteralRe.IsMatch(colorStr);
        }

        static Regex s_validFontNameRe = new Regex("^[a-z\\s0-9\\- ]{1,30}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsValidFontName(string fontName)
        {
            return s_validFontNameRe.IsMatch(fontName);
        }

        static Regex s_validSizeRe = new Regex("^[a-z0-9\\-]{1,20}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsValidSize(string size)
        {
            return s_validSizeRe.IsMatch(size);
        }

        static Regex s_validWidthRe = new Regex("^[0-9]*(px|em|%)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsValidWidth(string width)
        {
            return s_validWidthRe.IsMatch(width);
        }

        static Regex s_validTargetRe = new Regex("^[_a-zA-Z0-9\\-]{1,50}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsValidTarget(string target)
        {
            return s_validTargetRe.IsMatch(target);
        }
    }
}
