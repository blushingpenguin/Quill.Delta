using FluentAssertions;
using NUnit.Framework;

namespace Quill.Delta.Test
{
    public class InsertDataTest
    {
        [Test]
        public void InsertDataVideo()
        {
            var t = new InsertDataVideo("https://");
            t.Type.Should().Be(DataType.Video);
            t.Value.Should().Be("https://");
        }

        [Test]
        public void InsertDataText()
        {
            var t = new InsertDataText("hello");
            t.Type.Should().Be(DataType.Text);
            t.Value.Should().Be("hello");
        }

        [Test]
        public void InsertDataCustom()
        {
            var t = new InsertDataCustom("biu", new { Value = "test" });
            t.Type.Should().Be(DataType.Custom);
            t.CustomType.Should().Be("biu");
            t.Value.Should().BeEquivalentTo(new { Value = "test" },
                opts => opts.RespectingRuntimeTypes());
        }
    }
}
