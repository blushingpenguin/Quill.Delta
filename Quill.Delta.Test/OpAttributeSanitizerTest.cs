using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Quill.Delta.Test
{
    public class OpAttributeSanitizerTest
    {
        [Test]
        public void IsValidHexColour()
        {
            OpAttributeSanitizer.IsValidHexColor("#234").Should().BeTrue();
            OpAttributeSanitizer.IsValidHexColor("#f23").Should().BeTrue();
            OpAttributeSanitizer.IsValidHexColor("#fFe234").Should().BeTrue();
            OpAttributeSanitizer.IsValidHexColor("#g34").Should().BeFalse();
            OpAttributeSanitizer.IsValidHexColor("e34").Should().BeFalse();
            OpAttributeSanitizer.IsValidHexColor("123434").Should().BeFalse();
        }

        [Test]
        public void IsValidFontName()
        {
            OpAttributeSanitizer.IsValidFontName("gooD-ol times 2").Should().BeTrue();
            OpAttributeSanitizer.IsValidHexColor("bad\"times?").Should().BeFalse();
        }

        [Test]
        public void IsValidSize()
        {
            OpAttributeSanitizer.IsValidSize("bigfaT-size").Should().BeTrue();
            OpAttributeSanitizer.IsValidSize("small.sizetimes?").Should().BeFalse();
        }

        [Test]
        public void IsValidWidth()
        {
            OpAttributeSanitizer.IsValidWidth("150").Should().BeTrue();
            OpAttributeSanitizer.IsValidWidth("100px").Should().BeTrue();
            OpAttributeSanitizer.IsValidWidth("150em").Should().BeTrue();
            OpAttributeSanitizer.IsValidWidth("10%").Should().BeTrue();
            OpAttributeSanitizer.IsValidWidth("250%px").Should().BeFalse();
            OpAttributeSanitizer.IsValidWidth("250% border-box").Should().BeFalse();
            OpAttributeSanitizer.IsValidWidth("250.80").Should().BeFalse();
            OpAttributeSanitizer.IsValidWidth("250x").Should().BeFalse();
        }

        [Test]
        public void IsValidColorLiteral()
        {
            OpAttributeSanitizer.IsValidColorLiteral("yellow").Should().BeTrue();
            OpAttributeSanitizer.IsValidColorLiteral("r").Should().BeTrue();
            OpAttributeSanitizer.IsValidColorLiteral("#234").Should().BeFalse();
            OpAttributeSanitizer.IsValidColorLiteral("#fFe234").Should().BeFalse();
            OpAttributeSanitizer.IsValidColorLiteral("red1").Should().BeFalse();
            OpAttributeSanitizer.IsValidColorLiteral("red-green").Should().BeFalse();
            OpAttributeSanitizer.IsValidColorLiteral("").Should().BeFalse();
        }

        [Test]
        public void SanitizeDuffInputReturnsEmptyObject()
        {
            OpAttributeSanitizer.Sanitize(null)
                .Should().BeEquivalentTo(new OpAttributes(),
                    opts => opts.RespectingRuntimeTypes());
            OpAttributeSanitizer.Sanitize(new JValue(3))
                .Should().BeEquivalentTo(new OpAttributes());
            OpAttributeSanitizer.Sanitize(JValue.CreateUndefined())
                .Should().BeEquivalentTo(new OpAttributes());
            OpAttributeSanitizer.Sanitize(new JValue("fd"))
                .Should().BeEquivalentTo(new OpAttributes());
        }

        [Test]
        public void SanitizeSanitizesAttributes()
        {
            var attrs = JObject.Parse(@"{
                bold: 'nonboolval',
                color: '#12345H',
                background: '#333',
                font: 'times new roman',
                size: 'x.large',
                link: 'http://<',
                script: 'supper',
                list: 'ordered',
                header: '3',
                indent: 40,
                direction: 'rtl',
                align: 'center',
                width: '3',
                customAttr1:'shouldnt be touched',
                mentions: true,
                mention: {
                   'class': 'A-cls-9',
                   id: 'An-id_9:.',
                   target: '_blank',
                   avatar: 'http://www.yahoo.com',
                   'end-point': 'http://abc.com',
                   slug: 'my-name'
                }
            }");

            var result = OpAttributeSanitizer.Sanitize(attrs);
            result.Should().BeEquivalentTo(new OpAttributes()
            {
                Bold = null, // original parses guff as "truthy" -- being more strict seems ok
                Background = "#333",
                Font = "times new roman",
                Link = "http://<",
                List = ListType.Ordered,
                Header = 3,
                Indent = 30,
                Direction = DirectionType.Rtl,
                Align = AlignType.Center,
                Width = "3",
                CustomAttributes = new Dictionary<string, JToken>()
                {
                    {  "customAttr1", JValue.CreateString("shouldnt be touched") }
                },
                Mentions = true,
                Mention = new Mention() {
                  Class = "A-cls-9",
                  Id = "An-id_9:.",
                  Target = "_blank",
                  Avatar = "http://www.yahoo.com",
                  EndPoint = "http://abc.com",
                  Slug = "my-name"
                }
            }, opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }

        [Test]
        public void SanitizeHeader1()
        {
            OpAttributeSanitizer.Sanitize(
                JObject.Parse("{header: 1}")
            ).Should().BeEquivalentTo(
                new OpAttributes()
                {
                    Header = 1
                },
                opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }


        [Test]
        public void SanitizeUndefinedHeaderIgnored()
        {
            OpAttributeSanitizer.Sanitize(
                JObject.Parse("{header: undefined}")
            ).Should().BeEquivalentTo(
                new OpAttributes(),
                opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }

        [Test]
        public void SanitizeHeader100ClampsTo6()
        {
            OpAttributeSanitizer.Sanitize(
                JObject.Parse("{header: 100}")
            ).Should().BeEquivalentTo(
                new OpAttributes()
                {
                    Header = 6
                },
                opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }

        [Test]
        public void SanitizeAlignCenter()
        {
            OpAttributeSanitizer.Sanitize(
                JObject.Parse("{align: \"center\"}")
            ).Should().BeEquivalentTo(
                new OpAttributes()
                {
                    Align = AlignType.Center
                },
                opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }

        [Test]
        public void SanitizeDirectionRtl()
        {
            OpAttributeSanitizer.Sanitize(
                JObject.Parse("{direction: \"rtl\"}")
            ).Should().BeEquivalentTo(
                new OpAttributes()
                {
                    Direction = DirectionType.Rtl
                },
                opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }

        [Test]
        public void SanitizeIndent()
        {
            OpAttributeSanitizer.Sanitize(
                JObject.Parse("{indent: 2}")
            ).Should().BeEquivalentTo(
                new OpAttributes()
                {
                    Indent = 2
                },
                opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }
    }
}
