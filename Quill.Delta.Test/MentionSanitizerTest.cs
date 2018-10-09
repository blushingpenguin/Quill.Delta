using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Quill.Delta.Test
{
    public class MentionSanitizerTest
    {
        [Test]
        public void SanitizeSomeData()
        {
            var jo = JObject.Parse(@"{
                'class': 'A-cls-9',
                id: 'An-id_9:.',
                target: '_blank',
                avatar: 'http://www.yahoo.com',
                'end-point': 'http://abc.com',
                slug: 'my-name'
            }");
            var sanitized = MentionSanitizer.Sanitize(jo);
            sanitized.Should().BeEquivalentTo(new Mention()
            {
                Class = "A-cls-9",
                Id = "An-id_9:.",
                Target = "_blank",
                Avatar = "http://www.yahoo.com",
                EndPoint = "http://abc.com",
                Slug = "my-name"
            }, opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }

        [Test]
        public void SanitizeEmptyObject()
        {
            var result = MentionSanitizer.Sanitize(new JObject());
            result.Should().BeEquivalentTo(new Mention(),
                opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }

        [Test]
        public void SanitizeSimpleData()
        {
            var jo = JObject.Parse("{ id: 'sb'}");
            var result = MentionSanitizer.Sanitize(jo);
            result.Should().BeEquivalentTo(new Mention() { Id = "sb" },
                opts => opts.RespectingRuntimeTypes().WithStrictOrdering());
        }
    }
}
