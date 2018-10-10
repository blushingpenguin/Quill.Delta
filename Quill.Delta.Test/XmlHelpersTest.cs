using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Xml;

namespace Quill.Delta.Test
{
    public class XmlHelpersTest
    {
        [Test]
        public void MakeStartTagA()
        {
            var doc = new XmlDocument();
            var act = XmlHelpers.MakeElement(doc, "a");
            act.OuterXml.Should().Be("<a />");
        }

        [Test]
        public void MakeStartTagBr()
        {
            var doc = new XmlDocument();
            var act = XmlHelpers.MakeElement(doc, "br");
            act.OuterXml.Should().Be("<br />");
        }

        [Test]
        public void MakeStartTagImg()
        {
            var doc = new XmlDocument();
            var act = XmlHelpers.MakeElement(doc, "img", new List<TagKeyValue>() {
                new TagKeyValue("src", "http://") });
            act.OuterXml.Should().Be("<img src=\"http://\" />");
        }

        [Test]
        public void MakeStartTagClass()
        {
            var doc = new XmlDocument();
            var act = XmlHelpers.MakeElement(doc, "p", new List<TagKeyValue>() {
                new TagKeyValue("class", " cl1 cl2"),
                new TagKeyValue("style", "color:#333")
            });
            act.OuterXml.Should().Be("<p class=\" cl1 cl2\" style=\"color:#333\" />");
        }

        [Test]
        public void MakeStartTagCheckedAttribute()
        {
            var doc = new XmlDocument();
            var act = XmlHelpers.MakeElement(doc, "p", new List<TagKeyValue>(){
                new TagKeyValue("checked", null)
            });
            act.OuterXml.Should().Be("<p checked=\"\" />");
        }

        [Test]
        public void EncodeXml()
        {
            var act = XmlHelpers.EncodeXml("<\"doesn't this look spick & span when encoded\">");
            act.Should().Be("&lt;&quot;doesn&apos;t this look spick &amp; span when encoded&quot;&gt;");
        }
    }
}
