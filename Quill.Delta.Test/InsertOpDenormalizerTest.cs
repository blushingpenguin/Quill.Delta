using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace Quill.Delta.Test
{
    public class InsertOpDenormalizerTest
    {
        [Test]
        public void DenormalizeNL()
        {
            var op = JObject.Parse("{insert: \"\\n\"}");
            var act = InsertOpDenormalizer.Denormalize(op);
            act.Should().BeEquivalentTo(new JToken[] { op });
        }

        [Test]
        public void DenormalizeSimpleInsert()
        {
            var op = JObject.Parse("{insert: \"abc\"}");
            var act = InsertOpDenormalizer.Denormalize(op);
            act.Should().BeEquivalentTo(new JToken[] { op });
        }

        [Test]
        public void DenormalizeInsertWithLinkAttribute()
        {
            var op = JObject.Parse("{insert: \"abc\\n\", attributes: {link: 'cold'}}");
            var act = InsertOpDenormalizer.Denormalize(op);
            act.Count().Should().Be(2);
            act.First()["insert"].Value<string>().Should().Be("abc");
            act.First()["attributes"]["link"].Value<string>().Should().Be("cold");
        }

        [Test]
        public void DenormalizeInsertWithBoldAttribute()
        {
            var op = JObject.Parse("{insert: \"\\n\\n\", attributes: {bold: true}}");
            var act = InsertOpDenormalizer.Denormalize(op);
            act.Count().Should().Be(2);
            act.ElementAt(1)["insert"].Value<string>().Should().Be("\n");
        }

        [Test]
        public void DenormalizeNull()
        {
            var act = InsertOpDenormalizer.Denormalize(null);
            act.Should().BeEquivalentTo(new JToken[] { });
        }

        [Test]
        public void DenormalizeString()
        {
            var act = InsertOpDenormalizer.Denormalize("..");
            act.Should().BeEquivalentTo(new JToken[] { });
        }

        [Test]
        [TestCase("", new string[] { "" })]
        [TestCase("\n", new string[] { "\n" })]
        [TestCase("abc", new string[] { "abc" })]
        [TestCase("abc\nd", new string[] { "abc", "\n", "d" })]
        [TestCase("\n\n", new string[] { "\n", "\n" })]
        [TestCase("\n \n", new string[] { "\n", " ", "\n" })]
        [TestCase(" \nabc\n", new string[] { " ", "\n", "abc", "\n" })]
        [TestCase("\n\nabc\n\n6\n", new string[] { "\n", "\n", "abc", "\n", "\n", "6", "\n" })]
        public void TokenizeWithNewLinesTest(string test, string[] expected)
        {
            var result = InsertOpDenormalizer.TokenizeWithNewLines(test);
            result.Should().Equal(expected);
        }
    }
}
