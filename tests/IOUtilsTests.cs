using System.IO;

namespace NuExt.System.Tests
{
    public class IOUtilsTests
    {
        [Test]
        public void SmartEndTrimTest()
        {
            var originalFileName = "This name too long.txt";
            var fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 12);
            Assert.That(fileName, Is.EqualTo("This.txt"));
            fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 13);
            Assert.That(fileName, Is.EqualTo("This name.txt"));
            fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 14);
            Assert.That(fileName, Is.EqualTo("This name.txt"));

            originalFileName = "This_name_too_long.txt";
            fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 12);
            Assert.That(fileName, Is.EqualTo("This.txt"));
            fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 13);
            Assert.That(fileName, Is.EqualTo("This_name.txt"));
            fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 14);
            Assert.That(fileName, Is.EqualTo("This_name.txt"));

            originalFileName = "This-name-too-long.txt";
            fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 12);
            Assert.That(fileName, Is.EqualTo("This.txt"));
            fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 13);
            Assert.That(fileName, Is.EqualTo("This-name.txt"));
            fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 14);
            Assert.That(fileName, Is.EqualTo("This-name.txt"));

            originalFileName = "This@name@too@long.txt";
            fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 12);
            Assert.That(fileName, Is.EqualTo("This@nam.txt"));
            fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 13);
            Assert.That(fileName, Is.EqualTo("This@name.txt"));
            fileName = IOUtils.TruncateFileNamePreservingExtension(originalFileName, 14);
            Assert.That(fileName, Is.EqualTo("This@name@.txt"));
        }

        [Test]
        public void ClearFileNameTest()
        {
            var originalFileName = "This:name:too:long.txt";
            var fileName = IOUtils.SanitizeFileName(originalFileName, 12);
            Assert.That(fileName, Is.EqualTo("This.txt"));
            fileName = IOUtils.SanitizeFileName(originalFileName, 13);
            Assert.That(fileName, Is.EqualTo("This_name.txt"));
            fileName = IOUtils.SanitizeFileName(originalFileName, 14);
            Assert.That(fileName, Is.EqualTo("This_name.txt"));

            originalFileName = "This?name?too?long.txt";
            fileName = IOUtils.SanitizeFileName(originalFileName, 12);
            Assert.That(fileName, Is.EqualTo("This.txt"));
            fileName = IOUtils.SanitizeFileName(originalFileName, 13);
            Assert.That(fileName, Is.EqualTo("This_name.txt"));
            fileName = IOUtils.SanitizeFileName(originalFileName, 14);
            Assert.That(fileName, Is.EqualTo("This_name.txt"));

            originalFileName = "This*name*too*long.txt";
            fileName = IOUtils.SanitizeFileName(originalFileName, 12);
            Assert.That(fileName, Is.EqualTo("This.txt"));
            fileName = IOUtils.SanitizeFileName(originalFileName, 13);
            Assert.That(fileName, Is.EqualTo("This_name.txt"));
            fileName = IOUtils.SanitizeFileName(originalFileName, 14);
            Assert.That(fileName, Is.EqualTo("This_name.txt"));

            originalFileName = "This@name@too@long.txt";
            fileName = IOUtils.SanitizeFileName(originalFileName, 12);
            Assert.That(fileName, Is.EqualTo("This@nam.txt"));
            fileName = IOUtils.SanitizeFileName(originalFileName, 13);
            Assert.That(fileName, Is.EqualTo("This@name.txt"));
            fileName = IOUtils.SanitizeFileName(originalFileName, 14);
            Assert.That(fileName, Is.EqualTo("This@name@.txt"));
        }
    }
}
