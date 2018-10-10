using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Quill.Delta.Test
{
    public class XmlConverterTest
    {
        JArray _hugeOps = JArray.Parse(@"[
            { insert: 'huge', attributes: { size: 'huge' } },
            { insert: ""\n"" }
        ]");

        JArray _deltaOps = JArray.Parse(@"[
            { ""insert"": ""link"", ""attributes"": { ""link"": ""http://a.com/?x=a&b=()"" } },
            { ""insert"": ""This "" },
            { ""attributes"": { ""font"": ""monospace"" }, ""insert"": ""is"" },
            { ""insert"": "" a "" }, { ""attributes"": { ""size"": ""large"" }, ""insert"": ""test"" },
            { ""insert"": "" "" },
            { ""attributes"": { ""italic"": true, ""bold"": true }, ""insert"": ""data"" },
            { ""insert"": "" "" },
            { ""attributes"": { ""underline"": true, ""strike"": true }, ""insert"": ""that"" },
            { ""insert"": "" is "" }, { ""attributes"": { ""color"": ""#e60000"" }, ""insert"": ""will"" },
            { ""insert"": "" "" }, { ""attributes"": { ""background"": ""#ffebcc"" }, ""insert"": ""test"" },
            { ""insert"": "" "" }, { ""attributes"": { ""script"": ""sub"" }, ""insert"": ""the"" },
            { ""insert"": "" "" }, { ""attributes"": { ""script"": ""super"" }, ""insert"": ""rendering"" },
            { ""insert"": "" of "" }, { ""attributes"": { ""link"": ""http://yahoo"" }, ""insert"": ""inline"" },
            { ""insert"": "" "" },
            { ""insert"": { ""formula"": ""x=data"" } },
            { ""insert"": "" formats.\n"" },
            { ""insert"": ""list"" },
            { ""insert"": ""\n"", ""attributes"": { ""list"": ""bullet"" } },
            { ""insert"": ""list"" },
            { ""insert"": ""\n"", ""attributes"": { ""list"": ""checked"" } },
            {""insert"": ""some code"", ""attributes"":{code:true, bold:true}},
            {""attributes"":{""italic"":true,""link"":""#top"",""code"":true},""insert"":""Top""},
            {""insert"":""\n""},
        ]");

        string _deltaXml =
            "<template>" +
            "<p><a href=\"http://a.com/?x=a&amp;b=()\" target=\"_blank\">link</a>This <span class=\"noz-font-monospace\">is</span>" +
            " a <span class=\"noz-size-large\">test</span> " +
            "<strong><em>data</em></strong> " +
            "<s><u>that</u></s>" +
            " is <span style=\"color:#e60000\">will</span> " +
            "<span style=\"background-color:#ffebcc\">test</span> " +
            "<sub>the</sub> <sup>rendering</sup> of " +
            "<a href=\"http://yahoo\" target=\"_blank\">inline</a> <span class=\"noz-formula\">x=data</span>" +
            " formats.</p>" +
            "<ul><li>list</li></ul>" +
            "<ul><li data-checked=\"true\">list</li></ul>" +
            "<p><strong><code>some code</code></strong>" +
            "<a href=\"#top\" target=\"_blank\"><em><code>Top</code></em></a></p>" +
            "</template>";

        [Test]
        public void ConstructorReturnsProperXml()
        {
            var qdc = new XmlConverter(_deltaOps,
                new XmlConverterOptions
                {
                    ClassPrefix = "noz"
                }
            );
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be(_deltaXml);
        }

        [Test]
        public void ConstructorSetsDefaultInlineStyles()
        {
            var qdc = new XmlConverter(_hugeOps,
                new XmlConverterOptions() { InlineStyles = new InlineStyles() });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Contain("<span style=\"font-size: 2.5em\">huge</span>");
        }

        [Test]
        public void ConstructorAllowsSettingInlineStyles()
        {
            var stylesDic = new Dictionary<string, string>
            {
                { "huge", "font-size: 6em" }
             };
            var qdc = new XmlConverter(_hugeOps,
                new XmlConverterOptions
                {
                    InlineStyles = new InlineStyles
                    {
                        Size = InlineStyles.MakeLookup(stylesDic)
                    }
                });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Contain("<span style=\"font-size: 6em\">huge</span>");
        }

        [Test]
        public void SimpleOps()
        {
            var ops = JArray.Parse(@"[
                { insert: ""this is text"" },
                { insert: ""\n"" },
                { insert: ""this is code"" },
                { insert: ""\n"", attributes: { 'code-block': true } },
                { insert: ""this is code TOO!"" },
                { insert: ""\n"", attributes: { 'code-block': true } },
            ]");

            var qdc = new XmlConverter(ops);
            var xml = qdc.Convert().OuterXml;
            xml.Should().Contain("<pre>this is code");
        }

        [Test]
        public void Mention()
        {
            var ops = JArray.Parse(@"[{
                insert: ""mention"", attributes: {
                    mentions: true,
                    mention: {
                        'end-point': 'http://abc.com',
                        slug: 'a',
                        class: 'abc', target: '_blank'
                    }
                }
            }]");
            var qdc = new XmlConverter(ops);
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be("<template><p><a class=\"abc\"" +
               " href=\"http://abc.com/a\" target=\"_blank\"" +
               ">mention</a></p></template>");
        }

        [Test]
        public void Mention2()
        {
            var ops = JArray.Parse(@"[{
                insert: 'mention', attributes: {
                mentions: true, mention: { slug: 'aa' }
                }
            }]");
            var qdc = new XmlConverter(ops);
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be("<template><p><a href=\"about:blank\">mention</a></p></template>");
        }

        [Test]
        public void OpensAndClosesListTags()
        {
            var ops4 = JArray.Parse(@"[
                { insert: ""mr\n"" },
                { insert: ""hello"" },
                { insert: ""\n"", attributes: { list: 'ordered' } },
                { insert: ""there"" },
                { insert: ""\n"", attributes: { list: 'bullet' } },
                { insert: ""\n"", attributes: { list: 'ordered' } },
            ]");
            var qdc = new XmlConverter(ops4);
            var xml = qdc.Convert().OuterXml;

            xml.Should().Contain("<p>mr");
            xml.Should().Contain("</ol><ul><li>there");
        }

        [Test]
        public void SeparateParagraphs()
        {
            var ops4 = JArray.Parse(@"[
                { insert: ""hello\nhow areyou?\n\nbye"" }
            ]");
            var qdc = new XmlConverter(ops4,
                new XmlConverterOptions { MultiLineParagraph = false });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be("<template><p>hello</p><p>how areyou?</p><p><br /></p><p>bye</p></template>");
        }

        [Test]
        public void CheckedAndUncheckedLists()
        {
            var ops4 = JArray.Parse(@"[
                { insert: ""hello"" },
                { insert: ""\n"", attributes: { list: 'checked' } },
                { insert: ""there"" },
                { insert: ""\n"", attributes: { list: 'unchecked' } },
                { insert: ""man"" },
                { insert: ""\n"", attributes: { list: 'checked' } },
                { insert: 'not done'},
                { insert: ""\n"", attributes: {indent:1, list: 'unchecked'}}
             ]");
            var qdc = new XmlConverter(ops4);
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be(
                "<template>" +
               "<ul>" +
               "<li data-checked=\"true\">hello</li>" +
               "<li data-checked=\"false\">there</li>" +
               "<li data-checked=\"true\">man" +
                   "<ul><li data-checked=\"false\">not done</li></ul>" +
               "</li>" +
               "</ul>" +
               "</template>");
        }

        JArray _posOps = JArray.Parse(@"[
            { insert: ""mr"" },
            { insert: ""\n"", attributes: { align: 'center' } },
            { insert: ""\n"", attributes: { direction: 'rtl' } },
            { insert: ""\n"", attributes: { indent: 2 } }
        ]");

        [Test]
        public void PositionalStylesUseSpecifiedTag()
        {
            var qdc = new XmlConverter(_posOps,
               new XmlConverterOptions() { ParagraphTag = "div" });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Contain("<div class=\"ql-align");
            xml.Should().Contain("<div class=\"ql-direction");
            xml.Should().Contain("<div class=\"ql-indent");
        }

        [Test]
        public void PositionalStylesUseDefaultTag()
        {
            var qdc = new XmlConverter(_posOps);
            var xml = qdc.Convert().OuterXml;
            xml.Should().Contain("<p class=\"ql-align");
            xml.Should().Contain("<p class=\"ql-direction");
            xml.Should().Contain("<p class=\"ql-indent");
        }

        JArray _targetOps = JArray.Parse(@"[
            { ""attributes"": { ""target"": ""_self"", ""link"": ""http://#"" }, ""insert"": ""A"" },
            { ""attributes"": { ""target"": ""_blank"", ""link"": ""http://#"" }, ""insert"": ""B"" },
            { ""attributes"": { ""link"": ""http://#"" }, ""insert"": ""C"" }, { ""insert"": ""\n"" }
        ]");

        [Test]
        public void TargetAttrNoLinkTarget()
        {
            var qdc = new XmlConverter(_targetOps,
               new XmlConverterOptions { LinkTarget = "" });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be(
               "<template><p><a href=\"http://#\" target=\"_self\">A</a>" +
               "<a href=\"http://#\" target=\"_blank\">B</a>" +
               "<a href=\"http://#\">C</a></p></template>");
        }

        [Test]
        public void TargetAttrDefaultLinkTarget()
        {
            var qdc = new XmlConverter(_targetOps);
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be(
               "<template><p><a href=\"http://#\" target=\"_self\">A</a>" +
               "<a href=\"http://#\" target=\"_blank\">B</a>" +
               "<a href=\"http://#\" target=\"_blank\">C</a></p></template>");
        }

        [Test]
        public void TargetAttrTopTarget()
        {
            var qdc = new XmlConverter(_targetOps,
               new XmlConverterOptions() { LinkTarget = "_top" });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be(
               "<template><p><a href=\"http://#\" target=\"_self\">A</a>" +
               "<a href=\"http://#\" target=\"_blank\">B</a>" +
               "<a href=\"http://#\" target=\"_top\">C</a></p></template>");
        }

        [Test]
        public void CustomBlotEmptyStringWithNoRenderer()
        {
            var ops = JArray.Parse(@"[
                { insert: { customstuff: 'my val' } }
            ]");
            var qdc = new XmlConverter(ops);
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be("<template><p /></template>");
        }

        [Test]
        public void CustomBlotUsesGivenRenderer()
        {
            var ops = JArray.Parse(@"[
                { insert: { bolditalic: 'my text' } },
                { insert: { blah: 1 } }
             ]");
            XmlCustomRenderer renderer = (doc, op, contextOp) =>
            {
                var insert = (InsertDataCustom)op.Insert;
                if (insert.CustomType == "bolditalic")
                {
                    var b = doc.CreateElement("b");
                    var i = b.AppendChild(doc.CreateElement("i"))
                                .AppendChild(doc.CreateTextNode(
                                    insert.Value.ToString()));
                    return b;
                }
                return doc.CreateTextNode("unknown");
            };
            var qdc = new XmlConverter(ops,
                new XmlConverterOptions { CustomRenderer = renderer });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be("<template><p><b><i>my text</i></b>unknown</p></template>");
        }

        [Test]
        public void CustomInsertTypesRenderAsBlock()
        {
            var ops = JArray.Parse(@"[
               {insert: 'hello '},
               { insert: { myblot: 'my friend' } },
               { insert: '!' },
               {insert: {myblot: 'how r u?'}, attributes: {renderAsBlock: true}}
            ]");
            XmlCustomRenderer renderer = (doc, op, contextOp) =>
            {
                var insert = (InsertDataCustom)op.Insert;
                if (insert.CustomType == "myblot")
                {
                    var textNode = doc.CreateTextNode(insert.Value.ToString());
                    if (op.Attributes.RenderAsBlock == true)
                    {
                        var div = doc.CreateElement("div");
                        div.AppendChild(textNode);
                        return div;
                    }
                    return textNode;
                }
                return doc.CreateTextNode("unknown");
            };
            var qdc = new XmlConverter(ops,
                new XmlConverterOptions { CustomRenderer = renderer });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be("<template><p>hello my friend!</p><div>how r u?</div></template>");
        }

        JArray _customTypeCodeBlockOps = JArray.Parse(@"[
            { insert: { colonizer: ':' } },
            { insert: ""\n"", attributes: { 'code-block': true } },
            { insert: ""code1"" },
            { insert: ""\n"", attributes: { 'code-block': true } },
            { insert: { colonizer: ':' } },
            { insert: ""\n"", attributes: { 'code-block': true } }
            ]");


        XmlCustomRenderer _customTypeCodeBlockRenderer = (doc, op, contextOp) =>
        {
            var insert = (InsertDataCustom)op.Insert;
            if (insert.CustomType == "colonizer")
            {
                return doc.CreateTextNode(insert.Value.ToString());
            }
            return doc.CreateTextNode("");
        };

        [Test]
        public void CustomInsertTypesInCodeBlocks()
        {
            var qdc = new XmlConverter(new JArray(
                _customTypeCodeBlockOps[0], _customTypeCodeBlockOps[1]),
                new XmlConverterOptions {
                    CustomRenderer = _customTypeCodeBlockRenderer });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be("<template><pre>:</pre></template>");
        }

        [Test]
        public void CustomInsertTypesInCodeBlocks2()
        {
            var qdc = new XmlConverter(
                _customTypeCodeBlockOps,
                new XmlConverterOptions
                {
                    CustomRenderer = _customTypeCodeBlockRenderer,
                    RootNodeTag = "x"
                });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be("<x><pre>:\ncode1\n:</pre></x>");
        }

        JArray _customTypeHeaderOps = JArray.Parse(@"[
           { insert: { colonizer: ':' } },
           { insert: ""\n"", attributes: { header: 1 } },
           { insert: ""hello"" },
           { insert: ""\n"", attributes: { header: 1 } },
           { insert: { colonizer: ':' } },
           { insert: ""\n"", attributes: { header: 1 } }
        ]");

        XmlCustomRenderer _customTypeHeaderRenderer = (doc, op, contextOp) =>
        {
            var insert = (InsertDataCustom)op.Insert;
            if (insert.CustomType == "colonizer")
            {
                return doc.CreateTextNode(insert.Value.ToString());
            }
            return doc.CreateTextNode("");
        };

        [Test]
        public void CustomInsertTypesInHeaders()
        {
            var qdc = new XmlConverter(new JArray(
                _customTypeHeaderOps[0], _customTypeHeaderOps[1]),
                new XmlConverterOptions()
                { CustomRenderer = _customTypeHeaderRenderer });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be("<template><h1>:</h1></template>");
        }

        [Test]
        public void CustomInsertTypesInHeaders2()
        {
            var qdc = new XmlConverter(
                _customTypeHeaderOps,
                new XmlConverterOptions()
                { CustomRenderer = _customTypeHeaderRenderer });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be("<template><h1>:<br />hello<br />:</h1></template>");
        }

        [Test]
        public void GetListTagOrdered()
        {
            var op = new DeltaInsertOp("\n",
               new OpAttributes { List = ListType.Ordered });
            var qdc = new XmlConverter(_deltaOps);
            qdc.GetListTag(op).Should().Be("ol");
        }

        [Test]
        public void GetListTagBullet()
        {
            var op = new DeltaInsertOp("\n",
               new OpAttributes { List = ListType.Bullet });
            var qdc = new XmlConverter(_deltaOps);
            qdc.GetListTag(op).Should().Be("ul");
        }

        [Test]
        public void GetListTagchecked()
        {
            var op = new DeltaInsertOp("\n",
               new OpAttributes { List = ListType.Bullet });
            var qdc = new XmlConverter(_deltaOps);
            qdc.GetListTag(op).Should().Be("ul");
        }

        [Test]
        public void GetListTagUnchecked()
        {
            var op = new DeltaInsertOp("\n",
               new OpAttributes { List = ListType.Bullet });
            var qdc = new XmlConverter(_deltaOps);
            qdc.GetListTag(op).Should().Be("ul");
        }

        DeltaInsertOp[] _inlineOps = new DeltaInsertOp[] {
            new DeltaInsertOp("Hello"),
            new DeltaInsertOp(" my ", new OpAttributes { Italic = true }),
            new DeltaInsertOp("\n", new OpAttributes { Italic = true }),
            new DeltaInsertOp(" name is joey")
         };

        [Test]
        public void RenderInlinesSimple()
        {
            var qdc = new XmlConverter(new JArray());
            qdc._document = new XmlDocument();
            var inlines = qdc.RenderInlines(_inlineOps);
            inlines.OuterXml.Should().Be("<p>Hello" +
               "<em> my </em><br /> name is joey</p>");
        }

        [Test]
        public void RenderInlinesCustomParagraphTag()
        {
            var qdc = new XmlConverter(new JArray(),
                new XmlConverterOptions { ParagraphTag = "div" });
            qdc._document = new XmlDocument();
            var inlines = qdc.RenderInlines(_inlineOps);
            inlines.OuterXml.Should().Be(
               "<div>Hello<em> my </em><br /> name is joey</div>");
        }

        [Test]
        public void RenderInlinesEmptyParagraphTag()
        {
            var qdc = new XmlConverter(new JArray(),
                new XmlConverterOptions { ParagraphTag = "" });
            qdc._document = new XmlDocument();
            var inlines = qdc.RenderInlines(_inlineOps);
            inlines.OuterXml.Should().Be("Hello<em> my </em><br /> name is joey");
        }

        [Test]
        public void RenderInlinesPlainNL()
        {
            var ops = new DeltaInsertOp[] { new DeltaInsertOp("\n") };
            var qdc = new XmlConverter(new JArray());
            qdc._document = new XmlDocument();
            var xml = qdc.RenderInlines(ops).OuterXml;
            xml.Should().Be("<p><br /></p>");
        }

        [Test]
        public void RenderInlinesStyledNL()
        {
            var ops = new DeltaInsertOp[] { new DeltaInsertOp("\n",
                new OpAttributes { Font = "arial" }) };
            var qdc = new XmlConverter(new JArray());
            qdc._document = new XmlDocument();
            var xml = qdc.RenderInlines(ops).OuterXml;
            xml.Should().Be("<p><br /></p>");
        }

        [Test]
        public void RenderInlinesStyledNLNoParagraphTag()
        {
            var ops = new DeltaInsertOp[] { new DeltaInsertOp("\n",
                new OpAttributes { Font = "arial" }) };
            var qdc = new XmlConverter(new JArray(),
                new XmlConverterOptions { ParagraphTag = "" });
            qdc._document = new XmlDocument();
            var xml = qdc.RenderInlines(ops).OuterXml;
            xml.Should().Be("<br />");
        }

        [Test]
        public void RenderInlinesWhenFirstLineNL()
        {
            var ops = new DeltaInsertOp[] {
                new DeltaInsertOp("\n"), new DeltaInsertOp("aa") };
            var qdc = new XmlConverter(new JArray());
            qdc._document = new XmlDocument();
            var xml = qdc.RenderInlines(ops).OuterXml;
            xml.Should().Be("<p><br />aa</p>");
        }

        [Test]
        public void RenderInlinesWhenLastLineNL()
        {
            var ops = new DeltaInsertOp[] {
                new DeltaInsertOp("aa"), new DeltaInsertOp("\n") };
            var qdc = new XmlConverter(new JArray());
            qdc._document = new XmlDocument();
            var xml = qdc.RenderInlines(ops).OuterXml;
            xml.Should().Be("<p>aa</p>");
        }

        [Test]
        public void RenderInlinesWithMixedLines()
        {
            var ops = new DeltaInsertOp[] {
                new DeltaInsertOp("aa"), new DeltaInsertOp("bb") };
            var nlop = new DeltaInsertOp("\n");
            var stylednlop = new DeltaInsertOp("\n",
                new OpAttributes { Color = "#333", Italic = true });
            var qdc = new XmlConverter(new JArray());
            qdc._document = new XmlDocument();
            var xml = qdc.RenderInlines(ops).OuterXml;
            xml.Should().Be("<p>aabb</p>");

            var ops0 = new DeltaInsertOp[] {
                nlop, ops[0], nlop, ops[1]
            };
            xml = qdc.RenderInlines(ops0).OuterXml;
            xml.Should().Be("<p><br />aa<br />bb</p>");

            var ops4 = new DeltaInsertOp[] {
                ops[0], stylednlop, stylednlop, stylednlop, ops[1]
            };
            xml = qdc.RenderInlines(ops4).OuterXml;
            xml.Should().Be("<p>aa<br /><br /><br />bb</p>");
        }

        [Test]
        public void RenderBlockString()
        {
            var op = new DeltaInsertOp("\n",
               new OpAttributes { Header = 3, Indent = 2 });
            var inlineop = new DeltaInsertOp("hi there");

            var qdc = new XmlConverter(new JArray());
            qdc._document = new XmlDocument();
            var blockhtml = qdc.RenderBlock(op, new DeltaInsertOp[] { inlineop }).OuterXml;
            blockhtml.Should().Be("<h3 class=\"ql-indent-2\">hi there</h3>");
        }

        [Test]
        public void RenderEmptyBlock()
        {
            var op = new DeltaInsertOp("\n",
               new OpAttributes { Header = 3, Indent = 2 });
            var qdc = new XmlConverter(new JArray());
            qdc._document = new XmlDocument();
            var blockhtml = qdc.RenderBlock(op, new DeltaInsertOp[0]).OuterXml;
            blockhtml.Should().Be("<h3 class=\"ql-indent-2\"><br /></h3>");
        }

        JArray _codeBlockOps = JArray.Parse(@"[
            {
                ""insert"": ""line 1""
            },
            {
                ""attributes"": {
                    ""code-block"": true
                },
                ""insert"": ""\n""
            },
            {
                ""insert"": ""line 2""
            },
            {
                ""attributes"": {
                    ""code-block"": true
                },
                ""insert"": ""\n""
            },
            {
                ""insert"": ""line 3""
            },
            {
                ""attributes"": {
                    ""code-block"": true
                },
                ""insert"": ""\n""
            },
            {
                ""insert"": ""<p>line 4</p>""
            },
            {
                ""attributes"": {
                    ""code-block"": true
                },
                ""insert"": ""\n""
            }
        ]");

        [Test]
        public void CodeBlockSimple()
        {
            //console.log(encodeHtml("<p>line 4</p>"));
            var qdc = new XmlConverter(_codeBlockOps);
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be("<template><pre>line 1\nline 2\nline 3\n" +
               XmlHelpers.EncodeXml("<p>line 4</p>") +
               "</pre></template>");
        }

        [Test]
        public void CodeBlockNoMultiline()
        {
            var qdc = new XmlConverter(_codeBlockOps,
                new XmlConverterOptions { MultiLineCodeblock = false });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be(
               "<template><pre>line 1</pre><pre>line 2</pre><pre>line 3</pre>" +
               "<pre>" + XmlHelpers.EncodeXml("<p>line 4</p>") + "</pre></template>");
        }

        [Test]
        public void CodeBlockOneLine()
        {
            var qdc = new XmlConverter(new JArray(
                _codeBlockOps[0], _codeBlockOps[1]));
            var xml = qdc.Convert().OuterXml;
            xml.Should().Be("<template><pre>line 1</pre></template>");
        }

        JArray _beforeAndAfterOps = JArray.Parse(@"[
            { insert: 'hello', attributes: { bold: true } },
            { insert: '\n', attributes: { bold: true } },
            { insert: 'how r u?' },
            { insert: 'r u fine' },
            { insert: '\n', attributes: { blockquote: true } },
            { insert: { video: 'http://' } },
            { insert: 'list item 1' },
            { insert: '\n', attributes: { list: 'bullet' } },
            { insert: 'list item 1 indented' },
            { insert: '\n', attributes: { list: 'bullet', indent: 1 } }
         ]");

        [Test]
        public void BeforeAndAfterRenderCallbacks()
        {
            XmlBeforeRenderer beforeRenderer = (doc, groupType, data) =>
            {
                if (groupType == GroupType.InlineGroup)
                {
                    var op = ((InlineGroup)data).Ops[0];
                    op.Attributes.Bold.Should().BeTrue();
                }
                else if (groupType == GroupType.Video)
                {
                    var op = (VideoItem)data;
                    op.Op.Insert.Should().BeOfType<InsertDataVideo>();
                }
                else if (groupType == GroupType.Block)
                {
                    var bg = (BlockGroup)data;
                    bg.Op.Attributes.Blockquote.Should().BeTrue();
                    bg.Ops.Count.Should().Be(2);
                }
                else if (groupType == GroupType.List)
                {
                    var lg = (ListGroup)data;
                    lg.Items.Count.Should().Be(1);
                }
                else
                {
                    throw new Exception($"Unknown group type {groupType}");
                }
                return null;
            };
            XmlAfterRenderer afterRenderer = (doc, groupType, node) =>
            {
                if (groupType == GroupType.InlineGroup)
                {
                    node.OuterXml.Should().Contain("<strong>hello");
                }
                else if (groupType == GroupType.Video)
                {
                    node.OuterXml.Should().StartWith("<iframe");
                }
                else if (groupType == GroupType.Block)
                {
                    node.OuterXml.Should().StartWith("<blockquote");
                }
                else
                {
                    node.OuterXml.Should().Contain("list item 1<ul><li");
                }
                return node;
            };

            var qdc = new XmlConverter(_beforeAndAfterOps,
                new XmlConverterOptions()
                {
                    BeforeRenderer = beforeRenderer,
                    AfterRenderer = afterRenderer
                });
            qdc.Convert();
        }

        [Test]
        public void BeforeRenderCalledWithBlockGroupTypeForAlignIndentAndDirection()
        {
            var ops = JArray.Parse(@"[
                { insert: 'align' },
                { insert: '\n', attributes: { align: 'right' } },
                { insert: 'rtl' },
                { insert: '\n', attributes: { direction: 'rtl' } },
                { insert: 'indent 1' },
                { insert: '\n', attributes: { indent: 1 } },
            ]");

            int blockCount = 0;
            XmlBeforeRenderer beforeRenderer = (doc, gtype, group) =>
            {
                if (gtype == GroupType.Block)
                {
                    ++blockCount;
                }
                return null;
            };

            var qdc = new XmlConverter(ops,
                new XmlConverterOptions()
                {
                    BeforeRenderer = beforeRenderer
                });
            qdc.Convert();
            blockCount.Should().Be(3);
        }

        [Test]
        public void CustomHtmlFromCallbackUsed()
        {
            var ops = JArray.Parse(
                @"[{ insert: { video: ""http"" } }, { insert: 'aa' }]");
            XmlBeforeRenderer beforeRenderer = (doc, groupType, group) =>
                doc.CreateElement("my-custom-video-xml");
            var qdc = new XmlConverter(ops,
                new XmlConverterOptions { BeforeRenderer = beforeRenderer });
            var xml = qdc.Convert().OuterXml;
            xml.Should().Contain("<my-custom");
         }

        [Test]
        public void CustomRenderCallbacksAreUsed()
        {
            var ops = JArray.Parse(
                "[{ insert: { video: \"http\" } }, { insert: 'aa' }]");

            var qdc = new XmlConverter(ops);
            var xml = qdc.Convert().OuterXml;
            xml.Should().Contain("iframe");

            qdc = new XmlConverter(ops,
                new XmlConverterOptions
                { BeforeRenderer = (doc, gt, g) => null });
            xml = qdc.Convert().OuterXml;
            xml.Should().Contain("<iframe");
            xml.Should().Contain("aa");

            qdc = new XmlConverter(ops,
                new XmlConverterOptions
                { AfterRenderer = (doc, gt, g) => null });
            xml = qdc.Convert().OuterXml;
            xml.Should().Be("<template />");
        }
    }
}
