using System.Globalization;
using System.Text;

namespace NuExt.System.Tests
{
    public class ValueStringBuilderTests
    {
        [Test]
        public void AppendFormat_SimpleString()
        {
            var builder = new ValueStringBuilder();
            builder.AppendFormat(null, "Hello, {0}!", new ReadOnlySpan<object?>(["World"]));
            Assert.That(builder.ToString(), Is.EqualTo("Hello, World!"));
        }

        [Test]
        public void AppendFormat_MultipleArguments()
        {
            var builder = new ValueStringBuilder();
            builder.AppendFormat(null, "{0}, {1} and {2}", new ReadOnlySpan<object?>(["Alice", "Bob", "Charlie"]));
            Assert.That(builder.ToString(), Is.EqualTo("Alice, Bob and Charlie"));
        }

        [Test]
        public void AppendFormat_FormattedNumbers()
        {
            var builder = new ValueStringBuilder();
            builder.AppendFormat(new CultureInfo("en-US"), "{0:N2}, {1:C}", new ReadOnlySpan<object?>([12345.6789, 56.78]));
            Assert.That(builder.ToString(), Is.EqualTo("12,345.68, $56.78"), "Expected formatted numbers in en-US culture");
        }

        [Test]
        public void AppendFormat_NullArgument()
        {
            var builder = new ValueStringBuilder();
            builder.AppendFormat(null, "Value: {0}", new ReadOnlySpan<object?>([null]));
            Assert.That(builder.ToString(), Is.EqualTo("Value: "));
        }

        [Test]
        public void AppendFormat_EmptyFormat()
        {
            var builder = new ValueStringBuilder();
            builder.AppendFormat(null, "", new ReadOnlySpan<object?>(["Unused"]));
            Assert.That(builder.ToString(), Is.EqualTo(""));
        }

        [Test]
        public void AppendFormat_ProviderInvariantCulture()
        {
            var builder = new ValueStringBuilder();
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0:0.00}", new ReadOnlySpan<object?>([1234.5678]));
            Assert.That(builder.ToString(), Is.EqualTo("1234.57"));
        }

        [Test]
        public void AppendFormat_FormatWithEscapedBraces()
        {
            var builder = new ValueStringBuilder();
            builder.AppendFormat(null, "Literal {{braces}} and value: {0}", new ReadOnlySpan<object?>([42]));
            Assert.That(builder.ToString(), Is.EqualTo("Literal {braces} and value: 42"));
        }
    }
}
