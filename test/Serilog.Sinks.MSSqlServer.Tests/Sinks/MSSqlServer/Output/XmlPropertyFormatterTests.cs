using Serilog.Sinks.MSSqlServer.Output;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Output
{
    public class XmlPropertyFormatterTests
    {
        [Fact]
        public void GetValidElementNameReturnsValidNameOnNull()
        {
            var sut = new XmlPropertyFormatter();

            var result = sut.GetValidElementName(null);

            Assert.Equal("x", result);
        }

        [Fact]
        public void GetValidElementNameReturnsValidNameOnEmpty()
        {
            var sut = new XmlPropertyFormatter();

            var result = sut.GetValidElementName(string.Empty);

            Assert.Equal("x", result);
        }

        [Fact]
        public void GetValidElementNameTrimsWhitespace()
        {
            var sut = new XmlPropertyFormatter();

            var result = sut.GetValidElementName("  \tname   \t");

            Assert.Equal("name", result);
        }

        [Fact]
        public void GetValidElementNamePrependsXIfNonLetterAtBegin()
        {
            var sut = new XmlPropertyFormatter();

            var result = sut.GetValidElementName("$Name");

            Assert.Equal("x$Name", result);
        }

        [Fact]
        public void GetValidElementNamePrependsXIfXmlAtBeginCaseInsensitive()
        {
            var sut = new XmlPropertyFormatter();

            var result = sut.GetValidElementName("xmlName");
            Assert.Equal("xxmlName", result);

            var result2 = sut.GetValidElementName("XmlName");
            Assert.Equal("xXmlName", result2);
        }

        [Fact]
        public void GetValidElementNameTrimsWhitespaceAndPrependsXIfInvalidBegin()
        {
            var sut = new XmlPropertyFormatter();

            var result = sut.GetValidElementName(" xmlName ");
            Assert.Equal("xxmlName", result);

            var result2 = sut.GetValidElementName(" 4Name ");
            Assert.Equal("x4Name", result2);
        }

        [Fact]
        public void GetValidElementNameReplacesEnclosedWhitespaceWithUnderscore()
        {
            var sut = new XmlPropertyFormatter();

            var result = sut.GetValidElementName("Name\tWith   Space");

            Assert.Equal("Name_With___Space", result);
        }
    }
}
