using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace Quill.Delta.Test
{
    public class HtmlHelpersTest
    {
        [Test]
        public void MakeStartTagA()
        {
            var act = HtmlHelpers.MakeStartTag("a");
            act.Should().Be("<a>");
        }

        [Test]
        public void MakeStartTagEmpty()
        {
            var act = HtmlHelpers.MakeStartTag("");
            act.Should().Be("");
        }

        [Test]
        public void MakeStartTagBr()
        {
            var act = HtmlHelpers.MakeStartTag("br");
            act.Should().Be("<br/>");
        }

        [Test]
        public void MakeStartTagImg()
        {
            var act = HtmlHelpers.MakeStartTag("img", new List<TagKeyValue>() {
                new TagKeyValue("src", "http://") });
            act.Should().Be("<img src=\"http://\"/>");
        }

        [Test]
        public void MakeStartTagClass()
        {
            var act = HtmlHelpers.MakeStartTag("p", new List<TagKeyValue>() {
                new TagKeyValue("class", " cl1 cl2"),
                new TagKeyValue("style", "color:#333")
            });
            act.Should().Be("<p class=\" cl1 cl2\" style=\"color:#333\">");
        }

        [Test]
        public void MakeStartTagCheckedAttribute()
        {
            var act = HtmlHelpers.MakeStartTag("p", new List<TagKeyValue>(){
                new TagKeyValue("checked", null)
            });
            act.Should().Be("<p checked>");
        }

        [Test]
        public void MakeEndTag()
        {
            var act = HtmlHelpers.MakeEndTag("a");
            act.Should().Be("</a>");
        }

        [Test]
        public void MakeEndTagEmpty()
        {
            var act = HtmlHelpers.MakeEndTag("");
            act.Should().Be("");
        }

        [Test]
        public void EncodeHtml()
        {
            var act = HtmlHelpers.EncodeHtml("hello\"my<lovely\'/>&amp;friend&here()", false);
            act.Should().Be("hello&quot;my&lt;lovely&#x27;&#x2F;&gt;&amp;amp;friend&amp;here()");
        }

        [Test]
        public void EncodeHtmlDoubleGuard()
        {
            var act = HtmlHelpers.EncodeHtml("hello\"my<lovely\'/>&amp;friend&here()");
            act.Should().Be("hello&quot;my&lt;lovely&#x27;&#x2F;&gt;&amp;friend&amp;here()");
        }
        [Test]
        public void DecodeHtml()
        {
            var act = HtmlHelpers.DecodeHtml("hello&quot;my&lt;lovely&#x27;&#x2F;&gt;&amp;friend&amp;here");
            act.Should().Be("hello\"my<lovely\'/>&friend&here");
        }

        [Test]
        public void EncodeLink()
        {
            var act = HtmlHelpers.EncodeLink("http://www.yahoo.com/?a=b&c=<>()\"\'");
            act.Should().Be("http://www.yahoo.com/?a=b&amp;c=&lt;&gt;&#40;&#41;&quot;&#x27;");
        }
    }
}
