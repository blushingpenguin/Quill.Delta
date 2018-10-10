using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Quill.Delta.Test
{
    public class OpToXmlConverterTest
    {
        [Test]
        public void Constructor()
        {
            var op = new DeltaInsertOp("hello");
            new OpToXmlConverter(op);
        }

        [Test]
        public void ConstructorSetsDefaultOptions()
        {
            var op = new DeltaInsertOp("hello");
            var converter = new OpToXmlConverter(op);
            converter.Options.Should().BeEquivalentTo(new OpToXmlConverterOptions(),
                opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }

        [Test]
        public void PrefixClassWithEmptyPrefix()
        {
            var op = new DeltaInsertOp("aa");
            var c = new OpToXmlConverter(op, new OpToXmlConverterOptions
            {
                ClassPrefix = ""
            });
            var act = c.PrefixClass("my-class");
            act.Should().Be("my-class");
        }

        [Test]
        public void PrefixClassWithxx()
        {
            var op = new DeltaInsertOp("aa");
            var c = new OpToXmlConverter(op, new OpToXmlConverterOptions
            {
                ClassPrefix = "xx"
            });
            var act = c.PrefixClass("my-class");
            act.Should().Be("xx-my-class");
        }

        [Test]
        public void PrefixClassDefaultsToql()
        {
            var op = new DeltaInsertOp("aa");
            var c = new OpToXmlConverter(op);
            var act = c.PrefixClass("my-class");
            act.Should().Be("ql-my-class");
        }

        [Test]
        public void GetCssStylesEmpty()
        {
            var op = new DeltaInsertOp("aa");
            var c = new OpToXmlConverter(op);
            c.GetCssStyles().Count().Should().Be(0);
        }

        [Test]
        public void GetCssStylesBackground()
        {
            var op = new DeltaInsertOp("aa");
            var o = new DeltaInsertOp("f", new OpAttributes
            {
                Background = "red"
            });
            var c = new OpToXmlConverter(o);
            c.GetCssStyles().Should().Equal(new string[] { "background-color:red" });
        }

        [Test]
        public void GetCssStylesBackgroundForeground()
        {
            var o = new DeltaInsertOp("f", new OpAttributes
            {
                Background = "red",
                Color = "blue"
            });
            var c = new OpToXmlConverter(o);
            c.GetCssStyles().Should().Equal(new string[] { "color:blue", "background-color:red" });
        }

        [Test]
        public void GetCssStylesAllowBackgroundClasses()
        {
            var o = new DeltaInsertOp("f", new OpAttributes
            {
                Background = "red",
                Color = "blue"
            });
            var c = new OpToXmlConverter(o, new OpToXmlConverterOptions()
            {
                AllowBackgroundClasses = true
            });
            c.GetCssStyles().Should().Equal(new string[] { "color:blue" });
        }

        OpAttributes _styleAttributes = new OpAttributes
        {
            Indent = 1,
            Align = AlignType.Center,
            Direction = DirectionType.Rtl,
            Font = "roman",
            Size = "small",
            Background = "red"
        };

        string[] _styleAttributesCss = new string[] {
            "background-color:red",
            "padding-right:3em",
            "text-align:center",
            "direction:rtl",
            "font-family:roman",
            "font-size: 0.75em"
        };

        OpToXmlConverterOptions _styleConverterOptions = new OpToXmlConverterOptions {
            InlineStyles = new InlineStyles()
        };

        [Test]
        public void GetCssStylesWithInlineStylesNoStyles()
        {
            var op = new DeltaInsertOp("hello");
            var c = new OpToXmlConverter(op, _styleConverterOptions);
            c.GetCssStyles().Should().Equal(new string[] { });
        }

        [Test]
        public void GetCssStylesWithInlineStylesManyStyles()
        {
            var op = new DeltaInsertOp("f", _styleAttributes);
            var c = new OpToXmlConverter(op, _styleConverterOptions);
            c.GetCssStyles().Should().BeEquivalentTo(_styleAttributesCss);
        }

        [Test]
        public void GetCssStylesImage()
        {
            var o = new DeltaInsertOp(new InsertDataImage(""), _styleAttributes);
            var c = new OpToXmlConverter(o, _styleConverterOptions);
            c.GetCssStyles().Should().BeEquivalentTo(_styleAttributesCss);
        }

        [Test]
        public void GetCssStylesVideo()
        {
            var o = new DeltaInsertOp(new InsertDataVideo(""), _styleAttributes);
            var c = new OpToXmlConverter(o, _styleConverterOptions);
            c.GetCssStyles().Should().BeEquivalentTo(_styleAttributesCss);
        }

        [Test]
        public void GetCssStylesFormula()
        {
            var o = new DeltaInsertOp(new InsertDataFormula(""), _styleAttributes);
            var c = new OpToXmlConverter(o, _styleConverterOptions);
            c.GetCssStyles().Should().BeEquivalentTo(_styleAttributesCss);
        }

        [Test]
        public void GetCssStylesText()
        {
            var o = new DeltaInsertOp("f", _styleAttributes);
            var c = new OpToXmlConverter(o, _styleConverterOptions);
            c.GetCssStyles().Should().BeEquivalentTo(_styleAttributesCss);
        }

        [Test]
        public void GetCssStylesRtl()
        {
            var o = new DeltaInsertOp(new InsertDataImage(""),
                new OpAttributes { Direction = DirectionType.Rtl });
            var c = new OpToXmlConverter(o, _styleConverterOptions);
            c.GetCssStyles().Should().BeEquivalentTo(new string[] {
                "direction:rtl; text-align:inherit"
            });
        }

        [Test]
        public void GetCssStylesIndent()
        {
            var o = new DeltaInsertOp(new InsertDataImage(""),
                new OpAttributes { Indent = 2 });
            var c = new OpToXmlConverter(o, _styleConverterOptions);
            c.GetCssStyles().Should().BeEquivalentTo(new string[] {
                "padding-left:6em"
            });
        }

        [Test]
        public void GetCssStylesCustomStyle()
        {
            var op = new DeltaInsertOp("f",
                new OpAttributes { Size = "huge" });
            var styleDic = new Dictionary<string, string>()
            {
                { "huge", "font-size: 6em" }
            };
            var c = new OpToXmlConverter(op, new OpToXmlConverterOptions()
            {
                InlineStyles = new InlineStyles()
                {
                    Size = (value, dop) => InlineStyles.LookupValue(styleDic, value)
                }
            });
            c.GetCssStyles().Should().BeEquivalentTo(new string[] {
                "font-size: 6em"
            });
        }

        [Test]
        public void GetCssStylesUsesDefaultsWhereUnspecified()
        {
            // Here there's no inlineStyle specified for "size", but we still render it
            // because we fall back to the default.
            var op = new DeltaInsertOp("f",
                new OpAttributes { Size = "huge" });
            var styleDic = new Dictionary<string, string>()
            {
                { "serif", "font-family: serif" }
            };
            var c = new OpToXmlConverter(op, new OpToXmlConverterOptions()
            {
                InlineStyles = new InlineStyles()
                {
                    Font = (value, dop) => InlineStyles.LookupValue(styleDic, value)
                }
            });
            c.GetCssStyles().Should().BeEquivalentTo(new string[] {
                "font-size: 2.5em"
            });
        }

        [Test]
        public void GetCssStylesRendersDefaultFontsCorrectly()
        {
            var op = new DeltaInsertOp("f",
                new OpAttributes { Font = "monospace" });
            var c = new OpToXmlConverter(op, _styleConverterOptions);
            c.GetCssStyles().Should().BeEquivalentTo(new string[] {
                "font-family: Monaco, Courier New, monospace"
            });
        }

        [Test]
        public void GetCssStylesReturnsNothingWhereNoEntryMapped()
        {
            var op = new DeltaInsertOp("f",
                new OpAttributes { Size = "biggest" });
            var styleDic = new Dictionary<string, string>()
            {
                { "small", "font-size: 0.75em" }
            };
            var c = new OpToXmlConverter(op, new OpToXmlConverterOptions {
                InlineStyles = new InlineStyles()
                {
                    Size = InlineStyles.MakeLookup(styleDic)
                }
            });
            c.GetCssStyles().Should().BeEquivalentTo(new string[] { });
        }

        [Test]
        public void GetCssStylesReturnsNothingWhereConverterIsNull()
        {
            var op = new DeltaInsertOp("f",
                new OpAttributes { Size = "biggest" });
            var c = new OpToXmlConverter(op, new OpToXmlConverterOptions()
            {
                InlineStyles = new InlineStyles()
                {
                    Size = null
                }
            });
            c.GetCssStyles().Should().BeEquivalentTo(new string[] { });
        }

        [Test]
        public void GetCssClassesReturnsEmptyArrayWithNoClasses()
        {
            var op = new DeltaInsertOp("hello");
            var c = new OpToXmlConverter(op);
            c.GetCssClasses().Should().BeEquivalentTo(new string[] { });
        }

        string[] _styleClasses = new string[] { "ql-indent-1", "ql-align-center", "ql-direction-rtl",
            "ql-font-roman", "ql-size-small" };

        [Test]
        public void GetCssClassesReturnsClasses()
        {
            var o = new DeltaInsertOp("f", _styleAttributes);
            var c = new OpToXmlConverter(o);
            c.GetCssClasses().Should().BeEquivalentTo(_styleClasses);
        }

        [Test]
        public void GetCssClassesReturnsClassesForImage()
        {
            var o = new DeltaInsertOp(new InsertDataImage(""), _styleAttributes);
            var c = new OpToXmlConverter(o);
            c.GetCssClasses().Should().BeEquivalentTo(_styleClasses.Concat(
                Enumerable.Repeat("ql-image", 1)));
        }

        [Test]
        public void GetCssClassesReturnsClassesForVideo()
        {
            var o = new DeltaInsertOp(new InsertDataVideo(""), _styleAttributes);
            var c = new OpToXmlConverter(o);
            c.GetCssClasses().Should().BeEquivalentTo(_styleClasses.Concat(
                Enumerable.Repeat("ql-video", 1)));
        }

        [Test]
        public void GetCssClassesReturnsClassesForFormula()
        {
            var o = new DeltaInsertOp(new InsertDataFormula(""), _styleAttributes);
            var c = new OpToXmlConverter(o);
            c.GetCssClasses().Should().BeEquivalentTo(_styleClasses.Concat(
                Enumerable.Repeat("ql-formula", 1)));
        }

        [Test]
        public void GetCssClassesWithBackgroundClasses()
        {
            var o = new DeltaInsertOp("f", _styleAttributes);
            var c = new OpToXmlConverter(o, new OpToXmlConverterOptions()
            {
                AllowBackgroundClasses = true
            });
            c.GetCssClasses().Should().BeEquivalentTo(_styleClasses.Concat(
                Enumerable.Repeat("ql-background-red", 1)));
        }

        [Test]
        public void GetCssClassesReturnsNoClassesWithInlineStyles()
        {
            var o = new DeltaInsertOp("f", _styleAttributes);
            var c = new OpToXmlConverter(o, _styleConverterOptions);
            c.GetCssClasses().Should().BeEquivalentTo(new string[] { });
        }

        [Test]
        public void GetTagsNoTags()
        {
            var op = new DeltaInsertOp("hello");
            var c = new OpToXmlConverter(op);
            c.GetTags().Should().BeEquivalentTo(new string[] { });
        }

        [Test]
        public void GetTagsCodeTag()
        {
            var o = new DeltaInsertOp("",
                new OpAttributes { Code = true });
            var c = new OpToXmlConverter(o);
            c.GetTags().Should().BeEquivalentTo(new string[] {
                "code"
            });
        }

        [Test]
        public void GetTagsForImage()
        {
            var o = new DeltaInsertOp(new InsertDataImage(""));
            var c = new OpToXmlConverter(o);
            c.GetTags().Should().BeEquivalentTo(new string[] {
                "img"
            });
        }

        [Test]
        public void GetTagsForVideo()
        {
            var o = new DeltaInsertOp(new InsertDataVideo(""));
            var c = new OpToXmlConverter(o);
            c.GetTags().Should().BeEquivalentTo(new string[] {
                "iframe"
            });
        }

        [Test]
        public void GetTagsForFormula()
        {
            var o = new DeltaInsertOp(new InsertDataFormula(""));
            var c = new OpToXmlConverter(o);
            c.GetTags().Should().BeEquivalentTo(new string[] {
                "span"
            });
        }

        [Test]
        public void GetTagsForBlockquote()
        {
            var o = new DeltaInsertOp("",
                new OpAttributes { Blockquote = true });
            var c = new OpToXmlConverter(o);
            c.GetTags().Should().BeEquivalentTo(new string[] {
                "blockquote"
            });
        }

        [Test]
        public void GetTagsForCodeBlock()
        {
            var o = new DeltaInsertOp("",
                new OpAttributes { CodeBlock = true });
            var c = new OpToXmlConverter(o);
            c.GetTags().Should().BeEquivalentTo(new string[] {
                "pre"
            });
        }

        [Test]
        public void GetTagsForList()
        {
            var o = new DeltaInsertOp("",
                new OpAttributes { List = ListType.Bullet });
            var c = new OpToXmlConverter(o);
            c.GetTags().Should().BeEquivalentTo(new string[] {
                "li"
            });
        }

        [Test]
        public void GetTagsForHeader()
        {
            var o = new DeltaInsertOp("",
                new OpAttributes { Header = 2 });
            var c = new OpToXmlConverter(o);
            c.GetTags().Should().BeEquivalentTo(new string[] {
                "h2"
            });
        }

        [Test]
        public void GetTagsForMulti()
        {
            var o = new DeltaInsertOp("",
                new OpAttributes
                {
                    Link = "http",
                    Script = ScriptType.Sub,
                    Bold = true,
                    Italic = true,
                    Strike = true,
                    Underline = true
                });
            var c = new OpToXmlConverter(o);
            c.GetTags().Should().BeEquivalentTo(new string[] {
                "a", "sub", "strong", "em", "s", "u"
            });
        }

        [Test]
        public void GetTagAttributesEmpty()
        {
            var op = new DeltaInsertOp("hello");
            var c = new OpToXmlConverter(op);
            c.GetTagAttributes().Should().BeEquivalentTo(new TagKeyValue[] { });
        }

        [Test]
        public void GetTagAttributesEmptyText()
        {
            var o = new DeltaInsertOp("",
                new OpAttributes { Code = true, Color = "red" });
            var c = new OpToXmlConverter(o);
            c.GetTagAttributes().Should().BeEquivalentTo(new TagKeyValue[] { });
        }

        [Test]
        public void GetTagAttributesImageIgnoresColour()
        {
            var o = new DeltaInsertOp(new InsertDataImage("http:"),
                new OpAttributes { Color = "red" });
            var c = new OpToXmlConverter(o);
            c.GetTagAttributes().Should().BeEquivalentTo(new TagKeyValue[] {
                new TagKeyValue("class", "ql-image"),
                new TagKeyValue("src", "http:")
            });
        }

        [Test]
        public void GetTagAttributesImageIncludesWidth()
        {
            var o = new DeltaInsertOp(new InsertDataImage("http:"),
                new OpAttributes { Width = "200" });
            var c = new OpToXmlConverter(o);
            c.GetTagAttributes().Should().BeEquivalentTo(new TagKeyValue[] {
                new TagKeyValue("class", "ql-image"),
                new TagKeyValue("width", "200"),
                new TagKeyValue("src", "http:")
            });
        }

        [Test]
        public void GetTagAttributesFormulaIgnoresColour()
        {
            var o = new DeltaInsertOp(new InsertDataFormula("-"),
                new OpAttributes { Color = "red" });
            var c = new OpToXmlConverter(o);
            c.GetTagAttributes().Should().BeEquivalentTo(new TagKeyValue[] {
                new TagKeyValue("class", "ql-formula")
            });
        }

        [Test]
        public void GetTagAttributesVideoIgnoresColour()
        {
            var o = new DeltaInsertOp(new InsertDataVideo("http:"),
                new OpAttributes { Color = "red" });
            var c = new OpToXmlConverter(o);
            c.GetTagAttributes().Should().BeEquivalentTo(new TagKeyValue[] {
                new TagKeyValue("class", "ql-video"),
                new TagKeyValue("frameborder", "0"),
                new TagKeyValue("allowfullscreen", "true"),
                new TagKeyValue("src", "http:")
            });
        }

        [Test]
        public void GetTagAttributesLinkUsesColour()
        {
            var o = new DeltaInsertOp("link",
                new OpAttributes { Color = "red", Link = "l" });
            var c = new OpToXmlConverter(o);
            c.GetTagAttributes().Should().BeEquivalentTo(new TagKeyValue[] {
                new TagKeyValue("style", "color:red"),
                new TagKeyValue("href", "l")
            });
        }

        [Test]
        public void GetTagAttributesLinkNoFollowOptionWorks()
        {
            var o = new DeltaInsertOp("link",
                new OpAttributes { Color = "red", Link = "l" });
            var c = new OpToXmlConverter(o,
                new OpToXmlConverterOptions { LinkRel = "nofollow" });
            c.GetTagAttributes().Should().BeEquivalentTo(new TagKeyValue[] {
                new TagKeyValue("style", "color:red"),
                new TagKeyValue("href", "l"),
                new TagKeyValue("rel", "nofollow")
            });
        }

        [Test]
        public void GetContentIgnoresTextIfIndented() // wtf?
        {
            var o = new DeltaInsertOp("aa",
                new OpAttributes { Indent = 1 });
            var c = new OpToXmlConverter(o);
            c.GetContent().Should().Be("");
        }

        [Test]
        public void GetContentReturnsFormulaText()
        {
            var o = new DeltaInsertOp(new InsertDataFormula("ff"),
                new OpAttributes { Bold = true });
            var c = new OpToXmlConverter(o);
            c.GetContent().Should().Be("ff");
        }

        [Test]
        public void GetContentReturnsNoTextForVideo()
        {
            var o = new DeltaInsertOp(new InsertDataVideo("ff"),
                new OpAttributes { Bold = true });
            var c = new OpToXmlConverter(o);
            c.GetContent().Should().Be("");
        }

        [Test]
        public void GetHtmlPartsEmptyInput()
        {
            var op = new DeltaInsertOp("");
            var c1 = new OpToXmlConverter(op);
            var doc = new XmlDocument();
            var act = c1.GetXmlParts(doc);
            act.OuterNode.Should().BeNull();
            act.InnerNode.Should().BeNull();
        }

        DeltaInsertOp _htmlPartsOp = new DeltaInsertOp("aaa", new OpAttributes
        {
            Link = "http://",
            Bold = true,
            Italic = true,
            Underline = true,
            Strike = true,
            Script = ScriptType.Super,
            Font = "verdana",
            Size = "small",
            Color = "red",
            Background = "#fff"
        });
        string _htmlPartsResult =
            "<a class=\"ql-font-verdana ql-size-small\"" +
            " style=\"color:red;background-color:#fff\" href=\"http://\">" +
            "<sup>" +
            "<strong><em><s><u>aaa</u></s></em></strong>" +
            "</sup>" +
            "</a>";

        [Test]
        public void GetHtmlPartsWorks()
        {
            var c1 = new OpToXmlConverter(_htmlPartsOp);
            var doc = new XmlDocument();
            var act = c1.GetXmlParts(doc);
            act.InnerNode.AppendChild(doc.CreateTextNode(act.Content));
            act.OuterNode.OuterXml.Should().Be(_htmlPartsResult);
        }

        [Test]
        public void GetHtmlWorks()
        {
            var c1 = new OpToXmlConverter(_htmlPartsOp);
            var doc = new XmlDocument();
            var act = c1.GetXml(doc);
            act.OuterXml.Should().Be(_htmlPartsResult);
        }

        [Test]
        public void GetHtmlForNLIgnoresBold()
        {
            var op = new DeltaInsertOp("\n",
                new OpAttributes { Bold = true });
            var c1 = new OpToXmlConverter(op);
            var doc = new XmlDocument();
            c1.GetXml(doc).OuterXml.Should().Be("\n");
        }

        [Test]
        public void GetHtmlForNLIgnoresColour()
        {
            var op = new DeltaInsertOp("\n",
                new OpAttributes { Color = "#fff" });
            var c1 = new OpToXmlConverter(op);
            var doc = new XmlDocument();
            c1.GetXml(doc).OuterXml.Should().Be("\n");
        }

        [Test]
        public void GetHtmlForImageWorks()
        {
            var op = new DeltaInsertOp(new InsertDataImage("http://"));
            var c1 = new OpToXmlConverter(op);
            var doc = new XmlDocument();
            c1.GetXml(doc).OuterXml.Should().Be(
                "<img class=\"ql-image\" src=\"http://\"></img>");
        }

        [Test]
        public void GetXmlWithNullInputString()
        {
            var op = new DeltaInsertOp(
                new InsertDataText(null),
                new OpAttributes { Bold = true });
            var converter = new OpToXmlConverter(op);
            var doc = new XmlDocument();
            converter.GetXml(doc).OuterXml.Should().Be("<strong></strong>");
        }
    }
}
