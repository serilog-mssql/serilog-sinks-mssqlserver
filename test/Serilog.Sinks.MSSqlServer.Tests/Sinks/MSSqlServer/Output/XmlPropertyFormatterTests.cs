using System.Collections.Generic;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer.Output;
using Xunit;

namespace Serilog.Sinks.MSSqlServer.Tests.Sinks.MSSqlServer.Output
{
    public class XmlPropertyFormatterTests
    {
        [Fact]
        public void SimplifyScalarReplacesXmlInvalidChars()
        {
            // Arrange
            var input = new ScalarValue("some<allowed>words&inbetween");
            var sut = new XmlPropertyFormatter();

            // Act
            var result = sut.Simplify(input, null);

            // Assert
            Assert.Equal("some&lt;allowed&gt;words&amp;inbetween", result);
        }

        [Fact]
        public void SimplifyDictionaryRendersElements()
        {
            // Arrange
            var input = new DictionaryValue(new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>
            {
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("Key1"), new ScalarValue("Value1")),
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("Key2"), new ScalarValue(2))
            });
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions.PropertiesColumnOptions();
            var sut = new XmlPropertyFormatter();

            // Act
            var result = sut.Simplify(input, options);

            // Assert
            Assert.Equal("<dictionary><item key='Key1'>Value1</item><item key='Key2'>2</item></dictionary>", result);
        }

        [Fact]
        public void SimplifyDictionaryHandlesOptionUsePropertyKeyAsElementName()
        {
            // Arrange
            var input = new DictionaryValue(new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>
            {
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("Key1"), new ScalarValue("Value1")),
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("Key2"), new ScalarValue(2))
            });
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions.PropertiesColumnOptions { UsePropertyKeyAsElementName = true };
            var sut = new XmlPropertyFormatter();

            // Act
            var result = sut.Simplify(input, options);

            // Assert
            Assert.Equal("<dictionary><Key1>Value1</Key1><Key2>2</Key2></dictionary>", result);
        }

        [Fact]
        public void SimplifyDictionaryReplacesXmlInvalidCharsInValue()
        {
            // Arrange
            var input = new DictionaryValue(new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>
            {
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("Key1"), new ScalarValue("some<allowed>words&inbetween"))
            });
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions.PropertiesColumnOptions();
            var sut = new XmlPropertyFormatter();

            // Act
            var result = sut.Simplify(input, options);

            // Assert
            Assert.Equal("<dictionary><item key='Key1'>some&lt;allowed&gt;words&amp;inbetween</item></dictionary>", result);
        }

        [Fact]
        public void SimplifyDictionaryReplacesXmlInvalidCharsInKey()
        {
            // Arrange
            var input = new DictionaryValue(new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>
            {
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("some<allowed>words&inbetween"), new ScalarValue("Value1"))
            });
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions.PropertiesColumnOptions();
            var sut = new XmlPropertyFormatter();

            // Act
            var result = sut.Simplify(input, options);

            // Assert
            Assert.Equal("<dictionary><item key='some&lt;allowed&gt;words&amp;inbetween'>Value1</item></dictionary>", result);
        }

        [Fact]
        public void SimplifyDictionaryHandlesOptionOmitElementIfEmpty()
        {
            // Arrange
            var input = new DictionaryValue(new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>
            {
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("Key1"), new ScalarValue(string.Empty)),
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("Key2"), new ScalarValue("Value2")),
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("Key3"), new ScalarValue(null))
            });
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions.PropertiesColumnOptions { OmitElementIfEmpty = true };
            var sut = new XmlPropertyFormatter();

            // Act
            var result = sut.Simplify(input, options);

            // Assert
            Assert.Equal("<dictionary><item key='Key2'>Value2</item></dictionary>", result);
        }

        [Fact]
        public void SimplifyDictionaryHandlesOptionOmitDictionaryContainerElement()
        {
            // Arrange
            var input = new DictionaryValue(new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>
            {
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("Key1"), new ScalarValue("Value1")),
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("Key2"), new ScalarValue("Value2"))
            });
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions.PropertiesColumnOptions { OmitDictionaryContainerElement = true };
            var sut = new XmlPropertyFormatter();

            // Act
            var result = sut.Simplify(input, options);

            // Assert
            Assert.Equal("<item key='Key1'>Value1</item><item key='Key2'>Value2</item>", result);
        }

        [Fact]
        public void SimplifyDictionaryUsesCustomDictionaryElementName()
        {
            // Arrange
            var input = new DictionaryValue(new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>
            {
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("Key1"), new ScalarValue("Value1")),
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue("Key2"), new ScalarValue("Value2"))
            });
            var options = new Serilog.Sinks.MSSqlServer.ColumnOptions.PropertiesColumnOptions { DictionaryElementName = "list" };
            var sut = new XmlPropertyFormatter();

            // Act
            var result = sut.Simplify(input, options);

            // Assert
            Assert.Equal("<list><item key='Key1'>Value1</item><item key='Key2'>Value2</item></list>", result);
        }

        [Fact]
        public void GetValidElementNameReturnsValidNameOnNull()
        {
            // Arrange
            var sut = new XmlPropertyFormatter();

            // Act
            var result = sut.GetValidElementName(null);

            // Assert
            Assert.Equal("x", result);
        }

        [Fact]
        public void GetValidElementNameReturnsValidNameOnEmpty()
        {
            // Arrange
            var sut = new XmlPropertyFormatter();

            // Act
            var result = sut.GetValidElementName(string.Empty);

            // Assert
            Assert.Equal("x", result);
        }

        [Fact]
        public void GetValidElementNameTrimsWhitespace()
        {
            // Arrange
            var sut = new XmlPropertyFormatter();

            // Act
            var result = sut.GetValidElementName("  \tname   \t");

            // Assert
            Assert.Equal("name", result);
        }

        [Fact]
        public void GetValidElementNamePrependsXIfNonLetterAtBegin()
        {
            // Arrange
            var sut = new XmlPropertyFormatter();

            // Act
            var result = sut.GetValidElementName("$Name");

            // Assert
            Assert.Equal("x$Name", result);
        }

        [Fact]
        public void GetValidElementNamePrependsXIfXmlAtBeginCaseInsensitive()
        {
            // Arrange
            var sut = new XmlPropertyFormatter();

            // Act + assert
            var result = sut.GetValidElementName("xmlName");
            Assert.Equal("xxmlName", result);

            // Act + assert
            var result2 = sut.GetValidElementName("XmlName");
            Assert.Equal("xXmlName", result2);
        }

        [Fact]
        public void GetValidElementNameTrimsWhitespaceAndPrependsXIfInvalidBegin()
        {
            // Arrange
            var sut = new XmlPropertyFormatter();

            // Act + assert
            var result = sut.GetValidElementName(" xmlName ");
            Assert.Equal("xxmlName", result);

            // Act + assert
            var result2 = sut.GetValidElementName(" 4Name ");
            Assert.Equal("x4Name", result2);
        }

        [Fact]
        public void GetValidElementNameReplacesEnclosedWhitespaceWithUnderscore()
        {
            // Arrange
            var sut = new XmlPropertyFormatter();

            // Act
            var result = sut.GetValidElementName("Name\tWith   Space");

            // Assert
            Assert.Equal("Name_With___Space", result);
        }
    }
}
