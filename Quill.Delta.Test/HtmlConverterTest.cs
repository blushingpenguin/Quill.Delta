using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Quill.Delta.Test
{
    public class HtmlConverterTest
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

        string _deltaHtml =
            "<p><a href=\"http://a.com/?x=a&amp;b=&#40;&#41;\" target=\"_blank\">link</a>This <span class=\"noz-font-monospace\">is</span>" +
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
            "<a href=\"#top\" target=\"_blank\"><em><code>Top</code></em></a></p>";

        [Test]
        public void ConstructorReturnsProperHtml()
        {
            var qdc = new HtmlConverter(_deltaOps,
                new HtmlConverterOptions
                {
                    ClassPrefix = "noz"
                }
            );
            var html = qdc.Convert();
            html.Should().Be(_deltaHtml);
        }

        [Test]
        public void ConstructorSetsDefaultInlineStyles()
        {
            var qdc = new HtmlConverter(_hugeOps,
                new HtmlConverterOptions() { InlineStyles = new InlineStyles() });
            var html = qdc.Convert();
            html.Should().Contain("<span style=\"font-size: 2.5em\">huge</span>");
        }

        [Test]
        public void ConstructorAllowsSettingInlineStyles()
        {
            var stylesDic = new Dictionary<string, string>
            {
                { "huge", "font-size: 6em" }
             };
            var qdc = new HtmlConverter(_hugeOps,
                new HtmlConverterOptions
                {
                    InlineStyles = new InlineStyles
                    {
                        Size = InlineStyles.MakeLookup(stylesDic)
                    }
                });
            var html = qdc.Convert();
            html.Should().Contain("<span style=\"font-size: 6em\">huge</span>");
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

            var qdc = new HtmlConverter(ops);
            var html = qdc.Convert();
            html.Should().Contain("<pre>this is code");
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
            var qdc = new HtmlConverter(ops);
            var html = qdc.Convert();
            html.Should().Be("<p><a class=\"abc\"" +
               " href=\"http://abc.com/a\" target=\"_blank\"" +
               ">mention</a></p>");
        }

        [Test]
        public void Mention2()
        {
            var ops = JArray.Parse(@"[{
                insert: 'mention', attributes: {
                mentions: true, mention: { slug: 'aa' }
                }
            }]");
            var qdc = new HtmlConverter(ops);
            var html = qdc.Convert();
            html.Should().Be("<p><a href=\"about:blank\">mention</a></p>");
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
            var qdc = new HtmlConverter(ops4);
            var html = qdc.Convert();

            html.Should().Contain("<p>mr");
            html.Should().Contain("</ol><ul><li>there");
        }

        [Test]
        public void SeparateParagraphs()
        {
            var ops4 = JArray.Parse(@"[
                { insert: ""hello\nhow areyou?\n\nbye"" }
            ]");
            var qdc = new HtmlConverter(ops4,
                new HtmlConverterOptions { MultiLineParagraph = false });
            var html = qdc.Convert();
            html.Should().Be("<p>hello</p><p>how areyou?</p><p><br/></p><p>bye</p>");
        }

        [Test]
        public void SeparateParagraphsEndWithNL()
        {
            var ops4 = JArray.Parse(@"[
                { insert: ""hello\nhow areyou?\n\nbye\n\n\n"" }
            ]");
            var qdc = new HtmlConverter(ops4,
                new HtmlConverterOptions { MultiLineParagraph = false });
            var html = qdc.Convert();
            html.Should().Be("<p>hello</p><p>how areyou?</p><p><br/></p><p>bye</p><p><br/></p><p><br/></p>");
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
            var qdc = new HtmlConverter(ops4);
            var html = qdc.Convert();
            html.Should().Be(
               "<ul>" +
               "<li data-checked=\"true\">hello</li>" +
               "<li data-checked=\"false\">there</li>" +
               "<li data-checked=\"true\">man" +
                   "<ul><li data-checked=\"false\">not done</li></ul>" +
               "</li>" +
               "</ul>");
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
            var qdc = new HtmlConverter(_posOps,
               new HtmlConverterOptions() { ParagraphTag = "div" });
            var html = qdc.Convert();
            html.Should().Contain("<div class=\"ql-align");
            html.Should().Contain("<div class=\"ql-direction");
            html.Should().Contain("<div class=\"ql-indent");
        }

        [Test]
        public void PositionalStylesUseDefaultTag()
        {
            var qdc = new HtmlConverter(_posOps);
            var html = qdc.Convert();
            html.Should().Contain("<p class=\"ql-align");
            html.Should().Contain("<p class=\"ql-direction");
            html.Should().Contain("<p class=\"ql-indent");
        }

        JArray _targetOps = JArray.Parse(@"[
            { ""attributes"": { ""target"": ""_self"", ""link"": ""http://#"" }, ""insert"": ""A"" },
            { ""attributes"": { ""target"": ""_blank"", ""link"": ""http://#"" }, ""insert"": ""B"" },
            { ""attributes"": { ""link"": ""http://#"" }, ""insert"": ""C"" }, { ""insert"": ""\n"" }
        ]");

        [Test]
        public void TargetAttrNoLinkTarget()
        {
            var qdc = new HtmlConverter(_targetOps,
               new HtmlConverterOptions { LinkTarget = "" });
            var html = qdc.Convert();
            html.Should().Be(
               "<p><a href=\"http://#\" target=\"_self\">A</a>" +
               "<a href=\"http://#\" target=\"_blank\">B</a>" +
               "<a href=\"http://#\">C</a></p>");
        }

        [Test]
        public void TargetAttrDefaultLinkTarget()
        {
            var qdc = new HtmlConverter(_targetOps);
            var html = qdc.Convert();
            html.Should().Be(
               "<p><a href=\"http://#\" target=\"_self\">A</a>" +
               "<a href=\"http://#\" target=\"_blank\">B</a>" +
               "<a href=\"http://#\" target=\"_blank\">C</a></p>");
        }

        [Test]
        public void TargetAttrTopTarget()
        {
            var qdc = new HtmlConverter(_targetOps,
               new HtmlConverterOptions() { LinkTarget = "_top" });
            var html = qdc.Convert();
            html.Should().Be(
               "<p><a href=\"http://#\" target=\"_self\">A</a>" +
               "<a href=\"http://#\" target=\"_blank\">B</a>" +
               "<a href=\"http://#\" target=\"_top\">C</a></p>");
        }

        [Test]
        public void CustomBlotEmptyStringWithNoRenderer()
        {
            var ops = JArray.Parse(@"[
                { insert: { customstuff: 'my val' } }
            ]");
            var qdc = new HtmlConverter(ops);
            var html = qdc.Convert();
            html.Should().Be("<p></p>");
        }

        [Test]
        public void CustomBlotUsesGivenRenderer()
        {
            var ops = JArray.Parse(@"[
                { insert: { bolditalic: 'my text' } },
                { insert: { blah: 1 } }
             ]");
            CustomRenderer renderer = (op, contextOp) =>
            {
                var insert = (InsertDataCustom)op.Insert;
                if (insert.CustomType == "bolditalic")
                {
                    return $"<b><i>{insert.Value}</i></b>";
                }
                return "unknown";
            };
            var qdc = new HtmlConverter(ops,
                new HtmlConverterOptions { CustomRenderer = renderer });
            var html = qdc.Convert();
            html.Should().Be("<p><b><i>my text</i></b>unknown</p>");
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
            CustomRenderer renderer = (op, contextOp) =>
            {
                var insert = (InsertDataCustom)op.Insert;
                if (insert.CustomType == "myblot")
                {
                    return op.Attributes.RenderAsBlock == true ?
                       $"<div>{insert.Value}</div>" : insert.Value.ToString();
                }
                return "unknown";
            };
            var qdc = new HtmlConverter(ops,
                new HtmlConverterOptions { CustomRenderer = renderer });
            var html = qdc.Convert();
            html.Should().Be("<p>hello my friend!</p><div>how r u?</div>");
        }

        JArray _customTypeCodeBlockOps = JArray.Parse(@"[
            { insert: { colonizer: ':' } },
            { insert: ""\n"", attributes: { 'code-block': true } },
            { insert: ""code1"" },
            { insert: ""\n"", attributes: { 'code-block': true } },
            { insert: { colonizer: ':' } },
            { insert: ""\n"", attributes: { 'code-block': true } }
            ]");


        CustomRenderer _customTypeCodeBlockRenderer = (op, contextOp) =>
        {
            var insert = (InsertDataCustom)op.Insert;
            if (insert.CustomType == "colonizer")
            {
                return insert.Value.ToString();
            }
            return "";
        };

        [Test]
        public void CustomInsertTypesInCodeBlocks()
        {
            var qdc = new HtmlConverter(new JArray(
                _customTypeCodeBlockOps[0], _customTypeCodeBlockOps[1]),
                new HtmlConverterOptions {
                    CustomRenderer = _customTypeCodeBlockRenderer });
            var html = qdc.Convert();
            html.Should().Be("<pre>:</pre>");
        }

        [Test]
        public void CustomInsertTypesInCodeBlocks2()
        {
            var qdc = new HtmlConverter(
                _customTypeCodeBlockOps,
                new HtmlConverterOptions
                {
                    CustomRenderer = _customTypeCodeBlockRenderer
                });
            var html = qdc.Convert();
            html.Should().Be("<pre>:\ncode1\n:</pre>");
        }

        JArray _customTypeHeaderOps = JArray.Parse(@"[
           { insert: { colonizer: ':' } },
           { insert: ""\n"", attributes: { header: 1 } },
           { insert: ""hello"" },
           { insert: ""\n"", attributes: { header: 1 } },
           { insert: { colonizer: ':' } },
           { insert: ""\n"", attributes: { header: 1 } }
        ]");

        CustomRenderer _customTypeHeaderRenderer = (op, contextOp) =>
        {
            var insert = (InsertDataCustom)op.Insert;
            if (insert.CustomType == "colonizer")
            {
                return insert.Value.ToString();
            }
            return "";
        };

        [Test]
        public void CustomInsertTypesInHeaders()
        {
            var qdc = new HtmlConverter(new JArray(
                _customTypeHeaderOps[0], _customTypeHeaderOps[1]),
                new HtmlConverterOptions()
                { CustomRenderer = _customTypeHeaderRenderer });
            var html = qdc.Convert();
            html.Should().Be("<h1>:</h1>");
        }

        [Test]
        public void CustomInsertTypesInHeaders2()
        {
            var qdc = new HtmlConverter(
                _customTypeHeaderOps,
                new HtmlConverterOptions()
                { CustomRenderer = _customTypeHeaderRenderer });
            var html = qdc.Convert();
            html.Should().Be("<h1>:<br/>hello<br/>:</h1>");
        }

        [Test]
        public void GetListTagOrdered()
        {
            var op = new DeltaInsertOp("\n",
               new OpAttributes { List = ListType.Ordered });
            var qdc = new HtmlConverter(_deltaOps);
            qdc.GetListTag(op).Should().Be("ol");
        }

        [Test]
        public void GetListTagBullet()
        {
            var op = new DeltaInsertOp("\n",
               new OpAttributes { List = ListType.Bullet });
            var qdc = new HtmlConverter(_deltaOps);
            qdc.GetListTag(op).Should().Be("ul");
        }

        [Test]
        public void GetListTagChecked()
        {
            var op = new DeltaInsertOp("\n",
               new OpAttributes { List = ListType.Checked });
            var qdc = new HtmlConverter(_deltaOps);
            qdc.GetListTag(op).Should().Be("ul");
        }

        [Test]
        public void GetListTagUnchecked()
        {
            var op = new DeltaInsertOp("\n",
               new OpAttributes { List = ListType.Unchecked });
            var qdc = new HtmlConverter(_deltaOps);
            qdc.GetListTag(op).Should().Be("ul");
        }

        [Test]
        public void GetListTagInvalidTag()
        {
            var op = new DeltaInsertOp("\n",
               new OpAttributes { List = (ListType)99 });
            var qdc = new HtmlConverter(_deltaOps);
            qdc.GetListTag(op).Should().Be("");
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
            var qdc = new HtmlConverter(new JArray());
            var inlines = qdc.RenderInlines(_inlineOps);
            inlines.Should().Be("<p>Hello" +
               "<em> my </em><br/> name is joey</p>");
        }

        [Test]
        public void RenderInlinesCustomParagraphTag()
        {
            var qdc = new HtmlConverter(new JArray(),
                new HtmlConverterOptions { ParagraphTag = "div" });
            var inlines = qdc.RenderInlines(_inlineOps);
            inlines.Should().Be(
               "<div>Hello<em> my </em><br/> name is joey</div>");
        }

        [Test]
        public void RenderInlinesEmptyParagraphTag()
        {
            var qdc = new HtmlConverter(new JArray(),
                new HtmlConverterOptions { ParagraphTag = "" });
            var inlines = qdc.RenderInlines(_inlineOps);
            inlines.Should().Be("Hello<em> my </em><br/> name is joey");
        }

        [Test]
        public void RenderInlinesPlainNL()
        {
            var ops = new DeltaInsertOp[] { new DeltaInsertOp("\n") };
            var qdc = new HtmlConverter(new JArray());
            var html = qdc.RenderInlines(ops);
            html.Should().Be("<p><br/></p>");
        }

        [Test]
        public void RenderInlinesStyledNL()
        {
            var ops = new DeltaInsertOp[] { new DeltaInsertOp("\n",
                new OpAttributes { Font = "arial" }) };
            var qdc = new HtmlConverter(new JArray());
            var html = qdc.RenderInlines(ops);
            html.Should().Be("<p><br/></p>");
        }

        [Test]
        public void RenderInlinesStyledNLNoParagraphTag()
        {
            var ops = new DeltaInsertOp[] { new DeltaInsertOp("\n",
                new OpAttributes { Font = "arial" }) };
            var qdc = new HtmlConverter(new JArray(),
                new HtmlConverterOptions { ParagraphTag = "" });
            var html = qdc.RenderInlines(ops);
            html.Should().Be("<br/>");
        }

        [Test]
        public void RenderInlinesWhenFirstLineNL()
        {
            var ops = new DeltaInsertOp[] {
                new DeltaInsertOp("\n"), new DeltaInsertOp("aa") };
            var qdc = new HtmlConverter(new JArray());
            var html = qdc.RenderInlines(ops);
            html.Should().Be("<p><br/>aa</p>");
        }

        [Test]
        public void RenderInlinesWhenLastLineNL()
        {
            var ops = new DeltaInsertOp[] {
                new DeltaInsertOp("aa"), new DeltaInsertOp("\n") };
            var qdc = new HtmlConverter(new JArray());
            var html = qdc.RenderInlines(ops);
            html.Should().Be("<p>aa</p>");
        }

        [Test]
        public void RenderInlinesWithMixedLines()
        {
            var ops = new DeltaInsertOp[] {
                new DeltaInsertOp("aa"), new DeltaInsertOp("bb") };
            var nlop = new DeltaInsertOp("\n");
            var stylednlop = new DeltaInsertOp("\n",
                new OpAttributes { Color = "#333", Italic = true });
            var qdc = new HtmlConverter(new JArray());
            var html = qdc.RenderInlines(ops);
            html.Should().Be("<p>aabb</p>");

            var ops0 = new DeltaInsertOp[] {
                nlop, ops[0], nlop, ops[1]
            };
            html = qdc.RenderInlines(ops0);
            html.Should().Be("<p><br/>aa<br/>bb</p>");

            var ops4 = new DeltaInsertOp[] {
                ops[0], stylednlop, stylednlop, stylednlop, ops[1]
            };
            html = qdc.RenderInlines(ops4);
            html.Should().Be("<p>aa<br/><br/><br/>bb</p>");
        }

        [Test]
        public void RenderBlockString()
        {
            var op = new DeltaInsertOp("\n",
               new OpAttributes { Header = 3, Indent = 2 });
            var inlineop = new DeltaInsertOp("hi there");

            var qdc = new HtmlConverter(new JArray());
            var blockhtml = qdc.RenderBlock(op, new DeltaInsertOp[] { inlineop });
            blockhtml.Should().Be("<h3 class=\"ql-indent-2\">hi there</h3>");
        }

        [Test]
        public void RenderEmptyBlock()
        {
            var op = new DeltaInsertOp("\n",
               new OpAttributes { Header = 3, Indent = 2 });
            var qdc = new HtmlConverter(new JArray());
            var blockhtml = qdc.RenderBlock(op, new DeltaInsertOp[0]);
            blockhtml.Should().Be("<h3 class=\"ql-indent-2\"><br/></h3>");
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
            var qdc = new HtmlConverter(_codeBlockOps);
            var html = qdc.Convert();
            html.Should().Be("<pre>line 1\nline 2\nline 3\n" +
               HtmlHelpers.EncodeHtml("<p>line 4</p>") +
               "</pre>");
        }

        [Test]
        public void CodeBlockNoMultiline()
        {
            var qdc = new HtmlConverter(_codeBlockOps,
                new HtmlConverterOptions { MultiLineCodeblock = false });
            var html = qdc.Convert();
            html.Should().Be(
               "<pre>line 1</pre><pre>line 2</pre><pre>line 3</pre>" +
               "<pre>" + HtmlHelpers.EncodeHtml("<p>line 4</p>") + "</pre>");
        }

        [Test]
        public void CodeBlockOneLine()
        {
            var qdc = new HtmlConverter(new JArray(
                _codeBlockOps[0], _codeBlockOps[1]));
            var html = qdc.Convert();
            html.Should().Be("<pre>line 1</pre>");
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
            BeforeRenderer beforeRenderer = (groupType, data) =>
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
                return "";
            };
            AfterRenderer afterRenderer = (groupType, html) =>
            {
                if (groupType == GroupType.InlineGroup)
                {
                    html.Should().Contain("<strong>hello");
                }
                else if (groupType == GroupType.Video)
                {
                    html.Should().StartWith("<iframe");
                }
                else if (groupType == GroupType.Block)
                {
                    html.Should().StartWith("<blockquote");
                }
                else
                {
                    html.Should().Contain("list item 1<ul><li");
                }
                return html;
            };

            var qdc = new HtmlConverter(_beforeAndAfterOps,
                new HtmlConverterOptions()
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
            BeforeRenderer beforeRenderer = (gtype, group) =>
            {
                if (gtype == GroupType.Block)
                {
                    ++blockCount;
                }
                return "";
            };

            var qdc = new HtmlConverter(ops,
                new HtmlConverterOptions()
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
            BeforeRenderer beforeRenderer = (groupType, group) =>
                "<my custom video html>";
            var qdc = new HtmlConverter(ops,
                new HtmlConverterOptions { BeforeRenderer = beforeRenderer });
            var html = qdc.Convert();
            html.Should().Contain("<my custom");
         }

        [Test]
        public void CustomRenderCallbacksAreUsed()
        {
            var ops = JArray.Parse(
                "[{ insert: { video: \"http\" } }, { insert: 'aa' }]");

            var qdc = new HtmlConverter(ops);
            var html = qdc.Convert();
            html.Should().Contain("iframe");

            qdc = new HtmlConverter(ops,
                new HtmlConverterOptions
                { BeforeRenderer = (gt, g) => "" });
            html = qdc.Convert();
            html.Should().Contain("<iframe");
            html.Should().Contain("aa");

            qdc = new HtmlConverter(ops,
                new HtmlConverterOptions
                { AfterRenderer = (gt, g) => "" });
            html = qdc.Convert();
            html.Should().BeEmpty();
        }
    }
}
