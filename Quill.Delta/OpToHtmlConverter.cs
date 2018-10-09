﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Quill.Delta
{
    public delegate string InlineStyleType(string value, DeltaInsertOp op);

    public class InlineStyles
    {
        public InlineStyleType Indent { get; set; }
        public InlineStyleType Align { get; set; }
        public InlineStyleType Color { get; set; }
        public InlineStyleType Background { get; set; }
        public InlineStyleType Direction { get; set; }
        public InlineStyleType Font { get; set; }
        public InlineStyleType Size { get; set; }

        public static string LookupValue(Dictionary<string, string> dic, string key, string def = "")
        {
            string value;
            return dic.TryGetValue(key, out value) ? value : def;
        }

        public static InlineStyleType MakeLookup(Dictionary<string, string> dic, string def = "") =>
            (value, op) => LookupValue(dic, value);
    }

    public class OpToHtmlConverterOptions
    {
        public string ClassPrefix { get; set; } = "ql";
        public InlineStyles InlineStyles { get; set; }
        public bool EncodeHtml { get; set; } = true;
        public string ListItemTag { get; set; } = "li";
        public string ParagraphTag { get; set; } = "p";
        public string LinkRel { get; set; }
        public string LinkTarget { get; set; }
        public bool? AllowBackgroundClasses { get; set; }
    }

    public struct HtmlParts
    {
        public string OpeningTag { get; set; }
        public string Content { get; set; }
        public string ClosingTag { get; set; }
    }

    public class OpToHtmlConverter
    {
        static Dictionary<string, string> DEFAULT_INLINE_FONTS = new Dictionary<string, string>()
        {
            { "serif", "font-family: Georgia, Times New Roman, serif" },
            { "monospace", "font-family: Monaco, Courier New, monospace" }
        };

        static Dictionary<string, string> DEFAULT_SIZES = new Dictionary<string, string>()
        {
            { "small", "font-size: 0.75em" },
            { "large", "font-size: 1.5em" },
            { "huge", "font-size: 2.5em" }
        };

        static InlineStyles DEFAULT_INLINE_STYLES = new InlineStyles()
        {
            Font = (value, op) => InlineStyles.LookupValue(DEFAULT_INLINE_FONTS, value, "font-family:" + value),
            Size = InlineStyles.MakeLookup(DEFAULT_SIZES),
            Indent = (value, op) =>
            {
                int indentSize = Int32.Parse(value) * 3;
                var side = op.Attributes != null &&
                    op.Attributes.Direction == DirectionType.Rtl ? "right" : "left";
                return "padding-" + side + ":" + indentSize + "em";
            },
            Direction = (value, op) =>
            {
                if (value == "rtl")
                {
                    var hasAlign = op.Attributes != null && op.Attributes.Align.HasValue;
                    return $"direction:rtl{(hasAlign ? "" : "; text-align:inherit")}";
                }
                else
                {
                    return null;
                }
            }
        };

        OpToHtmlConverterOptions _options;
        DeltaInsertOp _op;

        public T Default<T>(T value, T def) => value == null ? def : value;

        public OpToHtmlConverter(DeltaInsertOp op, OpToHtmlConverterOptions options = null)
        {
            _op = op;
            _options = options ?? new OpToHtmlConverterOptions();

        }

        public string PrefixClass(string className) =>
            String.IsNullOrEmpty(_options.ClassPrefix) ?
                (className ?? "") : $"{_options.ClassPrefix}-{className ?? ""}";

        public string GetHtml()
        {
            var parts = GetHtmlParts();
            return $"{parts.OpeningTag}{parts.Content}{parts.ClosingTag}";
        }

        public HtmlParts GetHtmlParts()
        {
            if (_op.IsJustNewline() && !_op.IsContainerBlock())
            {
                return new HtmlParts() { OpeningTag = "", ClosingTag = "", Content = "\n" };
            }

            var tags = GetTags();
            var attrs = GetTagAttributes();
            if (tags.Count == 0 && attrs.Count > 0)
            {
                tags.Add("span");
            }

            var beginTags = new List<string>();
            var endTags = new List<string>();

            foreach (var tag in tags)
            {
                beginTags.Add(HtmlHelpers.MakeStartTag(tag, attrs));
                endTags.Add(tag == "img" ? "" : HtmlHelpers.MakeEndTag(tag));
                // consumed in first tag
                attrs = null;
            }
            endTags.Reverse();

            return new HtmlParts()
            {
                OpeningTag = String.Join("", beginTags),
                Content = GetContent(),
                ClosingTag = String.Join("", endTags)
            };
        }

        public string GetContent()
        {
            if (_op.IsContainerBlock())
            {
                return "";
            }

            if (_op.IsMentions())
            {
                return ((InsertDataText)_op.Insert).Value;
            }

            var content = _op.IsFormula() || _op.IsText() ?
                ((InsertDataString)_op.Insert).Value : "";

            return _options.EncodeHtml ?
                HtmlHelpers.EncodeHtml(content) : content;
        }

        public IList<string> GetCssClasses()
        {
            var attrs = _op.Attributes;
            if (_options.InlineStyles != null)
            {
                return new string[0];
            }

            var classes = new List<string>();
            if (attrs.Indent > 0)
            {
                classes.Add(PrefixClass($"indent-{attrs.Indent.Value}"));
            }
            if (attrs.Align.HasValue)
            {
                classes.Add(PrefixClass($"align-{Align.GetStringValue(attrs.Align)}"));
            }
            if (attrs.Direction.HasValue)
            {
                string dirnValue =
                    attrs.Direction == DirectionType.Rtl ? "rtl" : "";
                classes.Add(PrefixClass($"direction-{dirnValue}"));
            }
            if (!String.IsNullOrEmpty(attrs.Font))
            {
                classes.Add(PrefixClass($"font-{attrs.Font}"));
            }
            if (!String.IsNullOrEmpty(attrs.Size))
            {
                classes.Add(PrefixClass($"size-{attrs.Size}"));
            }
            if (_options.AllowBackgroundClasses == true &&
                !String.IsNullOrEmpty(attrs.Background) &&
                OpAttributeSanitizer.IsValidColorLiteral(attrs.Background))
            {
                classes.Add(PrefixClass($"background-{attrs.Background}"));
            }
            if (_op.IsFormula())
            {
                classes.Add(PrefixClass("formula"));
            }
            if (_op.IsVideo())
            {
                classes.Add(PrefixClass("video"));
            }
            if (_op.IsImage())
            {
                classes.Add(PrefixClass("image"));
            }
            return classes;
        }

        public IList<string> GetCssStyles()
        {
            var attrs = _op.Attributes;
            var result = new List<string>();

            Action<InlineStyleType, string, string> convert =
                (converter, value, name) =>
            {
                if (!String.IsNullOrEmpty(value))
                {
                    if (converter != null)
                    {
                        var converted = converter(value, _op);
                        if (!String.IsNullOrEmpty(converted))
                        {
                            result.Add(converted);
                        }
                    }
                    else
                    {
                        result.Add($"{name}:{value}");
                    }
                }
            };

            var iss = _options.InlineStyles;
            convert(iss?.Color ?? DEFAULT_INLINE_STYLES.Color,
                attrs.Color, "color");

            if (_options.InlineStyles != null ||
                !_options.AllowBackgroundClasses.HasValue ||
                !_options.AllowBackgroundClasses.Value)
            {
                convert(iss?.Background ?? DEFAULT_INLINE_STYLES.Background,
                    attrs.Background, "background-color");
            }
            if (_options.InlineStyles != null)
            {
                if (attrs.Indent.HasValue)
                {
                    convert(iss.Indent ?? DEFAULT_INLINE_STYLES.Indent,
                        attrs.Indent.Value.ToString(), "indent");
                }
                convert(iss.Align ?? DEFAULT_INLINE_STYLES.Align,
                    Align.GetStringValue(attrs.Align), "text-align");
                convert(iss.Direction ?? DEFAULT_INLINE_STYLES.Direction,
                    Direction.GetStringValue(attrs.Direction), "direction");
                convert(iss.Font ?? DEFAULT_INLINE_STYLES.Font,
                    attrs.Font, "font-family");
                convert(iss.Size ?? DEFAULT_INLINE_STYLES.Size,
                    attrs.Size, "size");
            }
            return result;
        }

        public IList<TagKeyValue> GetTagAttributes()
        {
            if (_op.Attributes.Code == true && !_op.IsLink())
            {
                return new TagKeyValue[0];
            }

            var result = new List<TagKeyValue>();
            Action<string, string> add = (string key, string value) =>
                result.Add(new TagKeyValue(key, value));

            var classes = GetCssClasses();
            if (classes.Any())
            {
                add("class", String.Join(" ", classes));
            }

            if (_op.IsImage())
            {
                if (!String.IsNullOrEmpty(_op.Attributes.Width))
                {
                    add("width", _op.Attributes.Width);
                }
                add("src", UrlHelpers.Sanitize(((InsertDataImage)_op.Insert).Value ?? ""));
                return result;
            }

            if (_op.IsACheckList()) 
            {
                add("data-checked", _op.IsCheckedList() ? "true" : "false");
                return result;
            }

            if (_op.IsFormula())
            {
                return result;
            }

            if (_op.IsVideo())
            {
                add("frameborder", "0");
                add("allowfullscreen", "true");
                add("src", UrlHelpers.Sanitize(((InsertDataVideo)_op.Insert).Value ?? ""));
                return result;
            }

            if (_op.IsMentions())
            {
                var mention = _op.Attributes.Mention;
                if (!String.IsNullOrEmpty(mention.Class))
                {
                    add("class", mention.Class);
                }
                if (!String.IsNullOrEmpty(mention.EndPoint) && !String.IsNullOrEmpty(mention.Slug))
                {
                    add("href", HtmlHelpers.EncodeLink(mention.EndPoint + "/" + mention.Slug));
                }
                else
                {
                    add("href", "about:blank");
                }
                if (!String.IsNullOrEmpty(mention.Target))
                {
                    add("target", mention.Target);
                }
                return result;
            }

            var styles = GetCssStyles();
            if (styles.Any())
            {
                add("style", String.Join(";", styles));
            }

            if (_op.IsContainerBlock())
            {
                return result;
            }

            if (_op.IsLink())
            {
                add("href", HtmlHelpers.EncodeLink(_op.Attributes.Link));
                var target = String.IsNullOrEmpty(_op.Attributes.Target) ?
                    _options.LinkTarget : _op.Attributes.Target;
                if (!String.IsNullOrEmpty(target))
                {
                    add("target", target);
                }
                if (!String.IsNullOrEmpty(_options.LinkRel) &&
                    IsValidRel(_options.LinkRel))
                {
                    add("rel", _options.LinkRel);
                }
            }

            return result;
        }

        public IList<string> GetTags()
        {
            var attrs = _op.Attributes;

            // embeds
            if (!_op.IsText())
            {
                return new string[] {
                    _op.IsVideo() ? "iframe" :
                    _op.IsImage() ? "img" : "span" // formula
                };
            }

            // blocks
            var positionTag = String.IsNullOrEmpty(_options.ParagraphTag) ?
                "p" : _options.ParagraphTag;

            if (attrs.Blockquote == true)
            {
                return new string[] { "blockquote" };
            }
            if (attrs.CodeBlock == true)
            {
                return new string[] { "pre" };
            }
            if (attrs.List.HasValue)
            {
                return new string[] { _options.ListItemTag };
            }
            if (attrs.Header > 0)
            {
                return new string[] { "h" + attrs.Header.Value };
            }
            if (attrs.Align.HasValue || attrs.Direction.HasValue ||
                attrs.Indent.HasValue)
            {
                return new string[] { positionTag };
            }

            // inlines
            var result = new List<string>();
            if (!String.IsNullOrEmpty(attrs.Link))
            {
                result.Add("a");
            }
            if (attrs.Mentions)
            {
                result.Add("a");
            }
            if (attrs.Script.HasValue)
            {
                result.Add(attrs.Script.Value == ScriptType.Sub ? "sub" : "sup");
            }
            if (attrs.Bold == true)
            {
                result.Add("strong");
            }
            if (attrs.Italic == true)
            {
                result.Add("em");
            }
            if (attrs.Strike == true)
            {
                result.Add("s");
            }
            if (attrs.Underline == true)
            {
                result.Add("u");
            }
            if (attrs.Code == true)
            {
                result.Add("code");
            }
            return result;
        }

        static Regex s_isValidRelRe = new Regex("^[a-z\\s]{1,50}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsValidRel(string relStr)
        {
            return s_isValidRelRe.IsMatch(relStr);
        }
    }
}
