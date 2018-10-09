using FluentAssertions;
using NUnit.Framework;

namespace Quill.Delta.Test
{
    public class UrlHelpersTest
    {
        [Test]
        [TestCase("http://www><.yahoo'.com", "http://www><.yahoo'.com")]
        [TestCase("https://abc", "https://abc")]
        [TestCase("sftp://abc", "sftp://abc")]
        [TestCase(" ftp://abc", "ftp://abc")]
        [TestCase("  file://abc", "file://abc")]
        [TestCase("   blob://abc", "blob://abc")]
        [TestCase("mailto://abc", "mailto://abc")]
        [TestCase("tel://abc", "tel://abc")]
        [TestCase("#abc", "#abc")]
        [TestCase("/abc", "/abc")]
        [TestCase(" data:image//abc", "data:image//abc")]
        [TestCase("javascript:alert('hi')", "unsafe:javascript:alert('hi')")]
        public void SanitizeUrl(string url, string sanitized)
        {
            UrlHelpers.Sanitize(url).Should().Be(sanitized);
        }
    }
}
