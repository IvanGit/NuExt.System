using System.Diagnostics.CodeAnalysis;
using Path = System.IO.PathBuilder;

namespace NuExt.System.Tests
{
    [SuppressMessage("Assertion", "NUnit2045:Use Assert.Multiple", Justification = "<Pending>")]
    public class PathBuilderTests
    {
        [Test]
        public void Test1()
        {
            var p = new Path() { DirectorySeparatorChar = '/' };
            Assert.That(p.Length, Is.Zero);

            p.Add('/');
            Assert.That(p.Length, Is.EqualTo(1));

            p.Add("home".AsSpan());
            Assert.That(p.Length, Is.EqualTo(5));

            p /= "user";
            Assert.That(p.Length, Is.EqualTo(10));

            p /= "documents".AsSpan();
            Assert.That(p.Length, Is.EqualTo(20));

            Assert.That(p.ToString(), Is.EqualTo("/home/user/documents"));
        }

        [Test]
        public void Test2()
        {
            var p = new Path("initial/path") { DirectorySeparatorChar = '/' };
            Assert.That(p.Length, Is.EqualTo(12));

            p += "/additional/path";
            Assert.That(p.Length, Is.EqualTo(28));

            Assert.That(p.ToString(), Is.EqualTo("initial/path/additional/path"));
        }

        [Test]
        public void Test3()
        {
            var p = new Path() { DirectorySeparatorChar = '/' };
            p /= "initial/path";
            p /= "/additional/path";
            Assert.That(p.Length, Is.EqualTo(28));

            Assert.That(p.ToString(), Is.EqualTo("initial/path/additional/path"));
        }

        [Test]
        public void Test4()
        {
            var p = new Path() { DirectorySeparatorChar = '/' };
            p /= "initial/path";
            p /= "additional/path";
            Assert.That(p.Length, Is.EqualTo(28));

            var arr = new char[28];
            bool result = p.TryCopyTo(arr.AsSpan(), out var numWritten);
            Assert.That(result, Is.True);
            Assert.That(numWritten, Is.EqualTo(28));

            Assert.That(p.ToString(), Is.EqualTo("initial/path/additional/path"));
        }

        [Test]
        public void Test5()
        {
            var p = new Path();
            Assert.That(p.HasExtension(), Is.False);
            Assert.That(p.GetExtension().IsEmpty, Is.True);

            p /= "file.txt";
            Assert.That(p.HasExtension(), Is.True);
            Assert.That(p.GetExtension().ToString(), Is.EqualTo(".txt"));

            Assert.That(p.ToString(), Is.EqualTo("file.txt"));
        }

        [Test]
        public void TestAddEmpty()
        {
            var p = new Path();
            p.Add("");
            Assert.That(p.ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestAddNull()
        {
            var p = new Path();
            p.Add(null);
            Assert.That(p.ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestAppendEmpty()
        {
            var p = new Path();
            p.Append("");
            Assert.That(p.ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestAppendNull()
        {
            var p = new Path();
            p.Append(null);
            Assert.That(p.ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestInitialCapacity()
        {
            var p = new Path(100);
            Assert.That(p.Capacity, Is.GreaterThanOrEqualTo(100));
        }

        [Test]
        public void TestEnsuresCapacity()
        {
            var p = new Path(10);
            p.EnsureCapacity(50);
            Assert.That(p.Capacity, Is.GreaterThanOrEqualTo(50));
        }

        [Test]
        public void TestClearPath()
        {
            var p = new Path();
            p.Append(@"C:\Users\Test");
            p.Clear();
            Assert.That(p.ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestHasExtension()
        {
            var p = new Path();
            p.Append("file.txt");
            Assert.That(p.HasExtension(), Is.True);

            p.Clear();
            p.Append("folder\\file");
            Assert.That(p.HasExtension(), Is.False);
        }

        [Test]
        public void TestGetExtension()
        {
            var p = new Path();
            p.Append("file.txt");
            Assert.That(p.GetExtension().ToString(), Is.EqualTo(".txt"));

            p.Clear();
            p.Append("folder\\file");
            Assert.That(p.GetExtension().ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void TestChangeExtension()
        {
            string path = "file";
            var p = new Path(path);

            path = global::System.IO.Path.ChangeExtension(path, ".txt");
            p.ChangeExtension(".txt");
            Assert.That(global::System.IO.Path.GetExtension(path), Is.EqualTo(".txt"));
            Assert.That(p.GetExtension().ToString(), Is.EqualTo(".txt"));

            path = global::System.IO.Path.ChangeExtension(path, ".cs");
            p.ChangeExtension(".cs");
            Assert.That(global::System.IO.Path.GetExtension(path), Is.EqualTo(".cs"));
            Assert.That(p.GetExtension().ToString(), Is.EqualTo(".cs"));

            path = global::System.IO.Path.ChangeExtension(path, null);
            p.ChangeExtension(null);
            Assert.That(global::System.IO.Path.HasExtension(path), Is.False);
            Assert.That(p.HasExtension(), Is.False);
            Assert.That(path, Is.EqualTo("file"));
            Assert.That(p.AsSpan().ToString(), Is.EqualTo("file"));

            path = global::System.IO.Path.ChangeExtension(path, "");
            p.ChangeExtension("");
            Assert.That(global::System.IO.Path.HasExtension(path), Is.False);
            Assert.That(p.HasExtension(), Is.False);
            Assert.That(global::System.IO.Path.GetExtension(path), Is.EqualTo(""));
            Assert.That(p.GetExtension().ToString(), Is.EqualTo(""));
            Assert.That(path, Is.EqualTo("file."));
            Assert.That(p.AsSpan().ToString(), Is.EqualTo("file."));

            path = global::System.IO.Path.ChangeExtension(path, null);
            p.ChangeExtension(null);
            Assert.That(global::System.IO.Path.HasExtension(path), Is.False);
            Assert.That(p.HasExtension(), Is.False);

            Assert.That(path, Is.EqualTo("file"));
            Assert.That(p.ToString(), Is.EqualTo("file"));
        }

        [Test]
        public void TestAsSpan()
        {
            var p = new Path();
            p.Append(@"C:\Users\Test");
            var span = p.AsSpan(3, 5);
            Assert.That(span.ToString(), Is.EqualTo("Users"));
        }

        [Test]
        public void TestTryCopyTo()
        {
            var p = new Path();
            p.Append(@"C:\Users\Test");
            Span<char> destination = stackalloc char[20];
            bool success = p.TryCopyTo(destination, out int charsWritten);
            Assert.That(success, Is.True);
            Assert.That(charsWritten, Is.EqualTo(13)); // "C:\\Users\\Test".Length
#if NETFRAMEWORK
            Assert.That(destination.Slice(0, charsWritten).ToString(), Is.EqualTo(@"C:\Users\Test"));
#else
            Assert.That(destination[..charsWritten].ToString(), Is.EqualTo(@"C:\Users\Test"));
#endif
        }

        [Test]
        public void TestDispose()
        {
            var p = new Path();
            p.Append(@"C:\Users\Test");
            p.Dispose();
            Assert.That(p.ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public unsafe void TestGetPinnableReference()
        {
            var p = new Path(@"C:\Users\Test");

            fixed (char* ptr = p)
            {
                Assert.That(*ptr, Is.EqualTo('C'));

                var result = new string(ptr, 0, p.Length);
                Assert.That(result, Is.EqualTo(@"C:\Users\Test"));
            }
        }

        [Test]
        public unsafe void TestGetPinnableReferenceWithoutTerminate()
        {
            var p = new Path(@"C:\Users\Test");
            fixed (char* ptr = &p.GetPinnableReference(false))
            {
                Assert.That(*ptr, Is.EqualTo('C'));

                var result = new string(ptr, 0, p.Length);
                Assert.That(result, Is.EqualTo(@"C:\Users\Test"));
            }
        }

        [Test]
        public unsafe void TestGetPinnableReferenceWithTerminate()
        {
            var p = new Path(@"C:\Users\Test");

            fixed (char* ptr = &p.GetPinnableReference(true))
            {
                Assert.That(*ptr, Is.EqualTo('C'));

                Assert.That(*(ptr + p.Length), Is.EqualTo('\0'));

                var result = new string(ptr);
                Assert.That(result, Is.EqualTo(@"C:\Users\Test"));
            }
        }

        [Test]
        public void TestGetDirectoryName_RootPath()
        {
            string path = "/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var directoryName = p.GetDirectoryName();
            var expectedDirectoryName = global::System.IO.Path.GetDirectoryName(path)!.Replace('\\', '/');

            Assert.That(directoryName.ToString(), Is.EqualTo(expectedDirectoryName));
        }

        [Test]
        public void TestGetDirectoryName_SubDirectoryPath()
        {
            string path = "/subdir/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var directoryName = p.GetDirectoryName();
            var expectedDirectoryName = global::System.IO.Path.GetDirectoryName(path)!.Replace('\\', '/');

            Assert.That(directoryName.ToString(), Is.EqualTo(expectedDirectoryName));
        }

        [Test]
        public void TestGetDirectoryName_NestedSubDirectoryPath()
        {
            string path = "/subdir/nested/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var directoryName = p.GetDirectoryName();
            var expectedDirectoryName = global::System.IO.Path.GetDirectoryName(path)!.Replace('\\', '/');

            Assert.That(directoryName.ToString(), Is.EqualTo(expectedDirectoryName));
        }

        [Test]
        public void TestGetDirectoryName_NoDirectoryPath()
        {
            string path = "file.txt";
            var p = new Path(path);

            var directoryName = p.GetDirectoryName();
            var expectedDirectoryName = global::System.IO.Path.GetDirectoryName(path);

            Assert.That(directoryName.ToString(), Is.EqualTo(expectedDirectoryName));
        }

        [Test]
        public void TestGetDirectoryName_WindowsPath()
        {
            string path = "C:\\subdir\\file.txt";
            var p = new Path(path);

            var directoryName = p.GetDirectoryName();
            var expectedDirectoryName = global::System.IO.Path.GetDirectoryName(path);

            Assert.That(directoryName.ToString(), Is.EqualTo(expectedDirectoryName));
        }

        [Test]
        public void TestGetDirectoryName_WindowsRootPath()
        {
            string path = "C:\\file.txt";
            var p = new Path(path);

            var directoryName = p.GetDirectoryName();
            var expectedDirectoryName = global::System.IO.Path.GetDirectoryName(path);

            Assert.That(directoryName.ToString(), Is.EqualTo(expectedDirectoryName));
        }

        [Test]
        public void TestGetDirectoryName_UnixLikePathWithoutTrailingSlash()
        {
            string path = "/usr/local/bin";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var directoryName = p.GetDirectoryName();
            var expectedDirectoryName = global::System.IO.Path.GetDirectoryName(path)!.Replace('\\', '/');

            Assert.That(directoryName.ToString(), Is.EqualTo(expectedDirectoryName));
        }

        [Test]
        public void TestGetDirectoryName_UnixLikePathWithTrailingSlash()
        {
            string path = "/usr/local/bin/";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var directoryName = p.GetDirectoryName();
            var expectedDirectoryName = global::System.IO.Path.GetDirectoryName(path)!.Replace('\\', '/');

            Assert.That(directoryName.ToString(), Is.EqualTo(expectedDirectoryName));
        }

        [Test]
        public void TestGetFileName_RootPath()
        {
            string path = "/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var fileName = p.GetFileName();
            var expectedFileName = global::System.IO.Path.GetFileName(path);

            Assert.That(fileName.ToString(), Is.EqualTo(expectedFileName));
        }

        [Test]
        public void TestGetFileName_SubDirectoryPath()
        {
            string path = "/subdir/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var fileName = p.GetFileName();
            var expectedFileName = global::System.IO.Path.GetFileName(path);

            Assert.That(fileName.ToString(), Is.EqualTo(expectedFileName));
        }

        [Test]
        public void TestGetFileName_NestedSubDirectoryPath()
        {
            string path = "/subdir/nested/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var fileName = p.GetFileName();
            var expectedFileName = global::System.IO.Path.GetFileName(path);

            Assert.That(fileName.ToString(), Is.EqualTo(expectedFileName));
        }

        [Test]
        public void TestGetFileName_NoDirectoryPath()
        {
            string path = "file.txt";
            var p = new Path(path);

            var fileName = p.GetFileName();
            var expectedFileName = global::System.IO.Path.GetFileName(path);

            Assert.That(fileName.ToString(), Is.EqualTo(expectedFileName));
        }

        [Test]
        public void TestGetFileName_WindowsPath()
        {
            string path = "C:\\subdir\\file.txt";
            var p = new Path(path);

            var fileName = p.GetFileName();
            var expectedFileName = global::System.IO.Path.GetFileName(path);

            Assert.That(fileName.ToString(), Is.EqualTo(expectedFileName));
        }

        [Test]
        public void TestGetFileName_WindowsRootPath()
        {
            string path = "C:\\file.txt";
            var p = new Path(path);

            var fileName = p.GetFileName();
            var expectedFileName = global::System.IO.Path.GetFileName(path);

            Assert.That(fileName.ToString(), Is.EqualTo(expectedFileName));
        }

        [Test]
        public void TestGetFileName_UnixLikePathWithoutTrailingSlash()
        {
            string path = "/usr/local/bin/somefile";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var fileName = p.GetFileName();
            var expectedFileName = global::System.IO.Path.GetFileName(path);

            Assert.That(fileName.ToString(), Is.EqualTo(expectedFileName));
        }

        [Test]
        public void TestGetFileName_UnixLikePathWithTrailingSlash()
        {
            string path = "/usr/local/bin/";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var fileName = p.GetFileName();
            var expectedFileName = global::System.IO.Path.GetFileName(path);

            Assert.That(fileName.ToString(), Is.EqualTo(expectedFileName));
        }

        [Test]
        public void TestGetFileNameWithoutExtension_RootPath()
        {
            string path = "/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var fileNameWithoutExtension = p.GetFileNameWithoutExtension();
            var expectedFileNameWithoutExtension = global::System.IO.Path.GetFileNameWithoutExtension(path);

            Assert.That(fileNameWithoutExtension.ToString(), Is.EqualTo(expectedFileNameWithoutExtension));
        }

        [Test]
        public void TestGetFileNameWithoutExtension_SubDirectoryPath()
        {
            string path = "/subdir/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var fileNameWithoutExtension = p.GetFileNameWithoutExtension();
            var expectedFileNameWithoutExtension = global::System.IO.Path.GetFileNameWithoutExtension(path);

            Assert.That(fileNameWithoutExtension.ToString(), Is.EqualTo(expectedFileNameWithoutExtension));
        }

        [Test]
        public void TestGetFileNameWithoutExtension_NestedSubDirectoryPath()
        {
            string path = "/subdir/nested/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var fileNameWithoutExtension = p.GetFileNameWithoutExtension();
            var expectedFileNameWithoutExtension = global::System.IO.Path.GetFileNameWithoutExtension(path);

            Assert.That(fileNameWithoutExtension.ToString(), Is.EqualTo(expectedFileNameWithoutExtension));
        }

        [Test]
        public void TestGetFileNameWithoutExtension_NoDirectoryPath()
        {
            string path = "file.txt";
            var p = new Path(path);

            var fileNameWithoutExtension = p.GetFileNameWithoutExtension();
            var expectedFileNameWithoutExtension = global::System.IO.Path.GetFileNameWithoutExtension(path);

            Assert.That(fileNameWithoutExtension.ToString(), Is.EqualTo(expectedFileNameWithoutExtension));
        }

        [Test]
        public void TestGetFileNameWithoutExtension_WindowsPath()
        {
            string path = "C:\\subdir\\file.txt";
            var p = new Path(path);

            var fileNameWithoutExtension = p.GetFileNameWithoutExtension();
            var expectedFileNameWithoutExtension = global::System.IO.Path.GetFileNameWithoutExtension(path);

            Assert.That(fileNameWithoutExtension.ToString(), Is.EqualTo(expectedFileNameWithoutExtension));
        }

        [Test]
        public void TestGetFileNameWithoutExtension_WindowsRootPath()
        {
            string path = "C:\\file.txt";
            var p = new Path(path);

            var fileNameWithoutExtension = p.GetFileNameWithoutExtension();
            var expectedFileNameWithoutExtension = global::System.IO.Path.GetFileNameWithoutExtension(path);

            Assert.That(fileNameWithoutExtension.ToString(), Is.EqualTo(expectedFileNameWithoutExtension));
        }

        [Test]
        public void TestGetFileNameWithoutExtension_UnixLikePathWithoutTrailingSlash()
        {
            string path = "/usr/local/bin/somefile";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var fileNameWithoutExtension = p.GetFileNameWithoutExtension();
            var expectedFileNameWithoutExtension = global::System.IO.Path.GetFileNameWithoutExtension(path);

            Assert.That(fileNameWithoutExtension.ToString(), Is.EqualTo(expectedFileNameWithoutExtension));
        }

        [Test]
        public void TestGetFileNameWithoutExtension_FileWithMultipleDots()
        {
            string path = "/usr/local/bin/file.name.with.dots.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var fileNameWithoutExtension = p.GetFileNameWithoutExtension();
            var expectedFileNameWithoutExtension = global::System.IO.Path.GetFileNameWithoutExtension(path);

            Assert.That(fileNameWithoutExtension.ToString(), Is.EqualTo(expectedFileNameWithoutExtension));
        }

        [Test]
        public void TestGetFileNameWithoutExtension_FileWithoutExtension()
        {
            string path = "/usr/local/bin/filenamewithoutdot";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var fileNameWithoutExtension = p.GetFileNameWithoutExtension();
            var expectedFileNameWithoutExtension = global::System.IO.Path.GetFileNameWithoutExtension(path);

            Assert.That(fileNameWithoutExtension.ToString(), Is.EqualTo(expectedFileNameWithoutExtension));
        }

        [Test]
        public void TestGetFileNameWithoutExtension_EmptyPath()
        {
            string path = "";
            var p = new Path(path);

            var fileNameWithoutExtension = p.GetFileNameWithoutExtension();
            var expectedFileNameWithoutExtension = global::System.IO.Path.GetFileNameWithoutExtension(path);

            Assert.That(fileNameWithoutExtension.ToString(), Is.EqualTo(expectedFileNameWithoutExtension));
        }

#if !NETFRAMEWORK

        [Test]
        public void TestIsPathFullyQualified_RootPath()
        {
            string path = "/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool isFullyQualified = p.IsPathFullyQualified();
            bool expectedIsFullyQualified = true;

            Assert.That(isFullyQualified, Is.EqualTo(expectedIsFullyQualified));
        }

        [Test]
        public void TestIsPathFullyQualified_SubDirectoryPath()
        {
            string path = "/subdir/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool isFullyQualified = p.IsPathFullyQualified();
            bool expectedIsFullyQualified = true;

            Assert.That(isFullyQualified, Is.EqualTo(expectedIsFullyQualified));
        }

        [Test]
        public void TestIsPathFullyQualified_NestedSubDirectoryPath()
        {
            string path = "/subdir/nested/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool isFullyQualified = p.IsPathFullyQualified();
            bool expectedIsFullyQualified = true;

            Assert.That(isFullyQualified, Is.EqualTo(expectedIsFullyQualified));
        }

        [Test]
        public void TestIsPathFullyQualified_NoDirectoryPath()
        {
            string path = "file.txt";
            var p = new Path(path);

            bool isFullyQualified = p.IsPathFullyQualified();
            bool expectedIsFullyQualified = global::System.IO.Path.IsPathFullyQualified(path);

            Assert.That(isFullyQualified, Is.EqualTo(expectedIsFullyQualified));
        }

        [Test]
        public void TestIsPathFullyQualified_WindowsPath()
        {
            string path = "C:\\subdir\\file.txt";
            var p = new Path(path);

            bool isFullyQualified = p.IsPathFullyQualified();
            bool expectedIsFullyQualified = global::System.IO.Path.IsPathFullyQualified(path);

            Assert.That(isFullyQualified, Is.EqualTo(expectedIsFullyQualified));
        }

        [Test]
        public void TestIsPathFullyQualified_WindowsRootPath()
        {
            string path = "C:\\file.txt";
            var p = new Path(path);

            bool isFullyQualified = p.IsPathFullyQualified();
            bool expectedIsFullyQualified = global::System.IO.Path.IsPathFullyQualified(path);

            Assert.That(isFullyQualified, Is.EqualTo(expectedIsFullyQualified));
        }

        [Test]
        public void TestIsPathFullyQualified_UnixLikePathWithoutTrailingSlash()
        {
            string path = "/usr/local/bin";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool isFullyQualified = p.IsPathFullyQualified();
            bool expectedIsFullyQualified = true;

            Assert.That(isFullyQualified, Is.EqualTo(expectedIsFullyQualified));
        }

        [Test]
        public void TestIsPathFullyQualified_UnixLikePathWithTrailingSlash()
        {
            string path = "/usr/local/bin/";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool isFullyQualified = p.IsPathFullyQualified();
            bool expectedIsFullyQualified = true;

            Assert.That(isFullyQualified, Is.EqualTo(expectedIsFullyQualified));
        }

#endif

        [Test]
        public void TestIsPathRooted_RootPath()
        {
            string path = "/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool isRooted = p.IsPathRooted();
            bool expectedIsRooted = global::System.IO.Path.IsPathRooted(path);

            Assert.That(isRooted, Is.EqualTo(expectedIsRooted));
        }

        [Test]
        public void TestIsPathRooted_SubDirectoryPath()
        {
            string path = "/subdir/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool isRooted = p.IsPathRooted();
            bool expectedIsRooted = global::System.IO.Path.IsPathRooted(path);

            Assert.That(isRooted, Is.EqualTo(expectedIsRooted));
        }

        [Test]
        public void TestIsPathRooted_NestedSubDirectoryPath()
        {
            string path = "/subdir/nested/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool isRooted = p.IsPathRooted();
            bool expectedIsRooted = global::System.IO.Path.IsPathRooted(path);

            Assert.That(isRooted, Is.EqualTo(expectedIsRooted));
        }

        [Test]
        public void TestIsPathRooted_NoDirectoryPath()
        {
            string path = "file.txt";
            var p = new Path(path);

            bool isRooted = p.IsPathRooted();
            bool expectedIsRooted = global::System.IO.Path.IsPathRooted(path);

            Assert.That(isRooted, Is.EqualTo(expectedIsRooted));
        }

        [Test]
        public void TestIsPathRooted_WindowsPath()
        {
            string path = "C:\\subdir\\file.txt";
            var p = new Path(path);

            bool isRooted = p.IsPathRooted();
            bool expectedIsRooted = global::System.IO.Path.IsPathRooted(path);

            Assert.That(isRooted, Is.EqualTo(expectedIsRooted));
        }

        [Test]
        public void TestIsPathRooted_WindowsRootPath()
        {
            string path = "C:\\file.txt";
            var p = new Path(path);

            bool isRooted = p.IsPathRooted();
            bool expectedIsRooted = global::System.IO.Path.IsPathRooted(path);

            Assert.That(isRooted, Is.EqualTo(expectedIsRooted));
        }

        [Test]
        public void TestIsPathRooted_UnixLikePathWithoutTrailingSlash()
        {
            string path = "/usr/local/bin";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool isRooted = p.IsPathRooted();
            bool expectedIsRooted = global::System.IO.Path.IsPathRooted(path);

            Assert.That(isRooted, Is.EqualTo(expectedIsRooted));
        }

        [Test]
        public void TestIsPathRooted_UnixLikePathWithTrailingSlash()
        {
            string path = "/usr/local/bin/";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool isRooted = p.IsPathRooted();
            bool expectedIsRooted = global::System.IO.Path.IsPathRooted(path);

            Assert.That(isRooted, Is.EqualTo(expectedIsRooted));
        }

        [Test]
        public void TestGetPathRoot_RootPath()
        {
            string path = "/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var root = p.GetPathRoot();
            var expectedRoot = global::System.IO.Path.GetPathRoot(path)!.Replace('\\', '/');
            Assert.That(expectedRoot, Is.EqualTo("/"));

            Assert.That(root.ToString(), Is.EqualTo(expectedRoot));
        }

        [Test]
        public void TestGetPathRoot_SubDirectoryPath()
        {
            string path = "/subdir/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var root = p.GetPathRoot();
            var expectedRoot = global::System.IO.Path.GetPathRoot(path)!.Replace('\\', '/');
            Assert.That(expectedRoot, Is.EqualTo("/"));

            Assert.That(root.ToString(), Is.EqualTo(expectedRoot));
        }

        [Test]
        public void TestGetPathRoot_NestedSubDirectoryPath()
        {
            string path = "/subdir/nested/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var root = p.GetPathRoot();
            var expectedRoot = global::System.IO.Path.GetPathRoot(path)!.Replace('\\', '/');
            Assert.That(expectedRoot, Is.EqualTo("/"));

            Assert.That(root.ToString(), Is.EqualTo(expectedRoot));
        }

        [Test]
        public void TestGetPathRoot_NoDirectoryPath()
        {
            string path = "file.txt";
            var p = new Path(path);

            var root = p.GetPathRoot();
            var expectedRoot = global::System.IO.Path.GetPathRoot(path);
            Assert.That(expectedRoot, Is.EqualTo(""));

            Assert.That(root.ToString(), Is.EqualTo(expectedRoot));
        }

        [Test]
        public void TestGetPathRoot_WindowsPath()
        {
            string path = "C:\\subdir\\file.txt";
            var p = new Path(path);

            var root = p.GetPathRoot();
            var expectedRoot = global::System.IO.Path.GetPathRoot(path);
            Assert.That(expectedRoot, Is.EqualTo("C:\\"));

            Assert.That(root.ToString(), Is.EqualTo(expectedRoot));
        }

        [Test]
        public void TestGetPathRoot_WindowsRootPath()
        {
            string path = "C:\\file.txt";
            var p = new Path(path);

            var root = p.GetPathRoot();
            var expectedRoot = global::System.IO.Path.GetPathRoot(path);
            Assert.That(expectedRoot, Is.EqualTo("C:\\"));

            Assert.That(root.ToString(), Is.EqualTo(expectedRoot));
        }

        [Test]
        public void TestGetPathRoot_UnixLikePathWithoutTrailingSlash()
        {
            string path = "/usr/local/bin";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var root = p.GetPathRoot();
            var expectedRoot = global::System.IO.Path.GetPathRoot(path)!.Replace('\\', '/');
            Assert.That(expectedRoot, Is.EqualTo("/"));

            Assert.That(root.ToString(), Is.EqualTo(expectedRoot));
        }

        [Test]
        public void TestGetPathRoot_UnixLikePathWithTrailingSlash()
        {
            string path = "/usr/local/bin/";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var root = p.GetPathRoot();
            var expectedRoot = global::System.IO.Path.GetPathRoot(path)!.Replace('\\', '/');
            Assert.That(expectedRoot, Is.EqualTo("/"));

            Assert.That(root.ToString(), Is.EqualTo(expectedRoot));
        }

#if !NETFRAMEWORK

        [Test]
        public void TestTrimEndingDirectorySeparator_RootPath()
        {
            string path = "/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool result = p.TrimEndingDirectorySeparator();
            bool expectedResult = global::System.IO.Path.TrimEndingDirectorySeparator(path).Replace('\\', '/') != path;

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(global::System.IO.Path.TrimEndingDirectorySeparator(path).Replace('\\', '/')));
        }

        [Test]
        public void TestTrimEndingDirectorySeparator_SubDirectoryPath_WithTrailingSlash()
        {
            string path = "/subdir/file.txt/";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool result = p.TrimEndingDirectorySeparator();
            bool expectedResult = true; // Path had a trailing slash to trim

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(global::System.IO.Path.TrimEndingDirectorySeparator(path).Replace('\\', '/')));
        }

        [Test]
        public void TestTrimEndingDirectorySeparator_SubDirectoryPath_WithoutTrailingSlash()
        {
            string path = "/subdir/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool result = p.TrimEndingDirectorySeparator();
            bool expectedResult = false; // Path already without trailing slash

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(global::System.IO.Path.TrimEndingDirectorySeparator(path).Replace('\\', '/')));
        }

        [Test]
        public void TestTrimEndingDirectorySeparator_NestedSubDirectoryPath_WithTrailingSlash()
        {
            string path = "/subdir/nested/file.txt/";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool result = p.TrimEndingDirectorySeparator();
            bool expectedResult = true; // Path had a trailing slash to trim

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(global::System.IO.Path.TrimEndingDirectorySeparator(path).Replace('\\', '/')));
        }

        [Test]
        public void TestTrimEndingDirectorySeparator_NestedSubDirectoryPath_WithoutTrailingSlash()
        {
            string path = "/subdir/nested/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool result = p.TrimEndingDirectorySeparator();
            bool expectedResult = false; // Path already without trailing slash

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(global::System.IO.Path.TrimEndingDirectorySeparator(path).Replace('\\', '/')));
        }

        [Test]
        public void TestTrimEndingDirectorySeparator_PathWithMultipleSeparators()
        {
            string path = "/subdir///nested//file.txt//";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool result = p.TrimEndingDirectorySeparator();
            bool expectedResult = true; // Path had multiple trailing slashes

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(global::System.IO.Path.TrimEndingDirectorySeparator(path).Replace('\\', '/')));
        }

        [Test]
        public void TestTrimEndingDirectorySeparator_WindowsPath_WithTrailingSlash()
        {
            string path = "C:\\subdir\\file.txt\\";
            var p = new Path(path);

            bool result = p.TrimEndingDirectorySeparator();
            bool expectedResult = true; // Path had a trailing slash to trim

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(global::System.IO.Path.TrimEndingDirectorySeparator(path)));
        }

        [Test]
        public void TestTrimEndingDirectorySeparator_WindowsPath_WithoutTrailingSlash()
        {
            string path = "C:\\subdir\\file.txt";
            var p = new Path(path);

            bool result = p.TrimEndingDirectorySeparator();
            bool expectedResult = false; // Path already without trailing slash

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(global::System.IO.Path.TrimEndingDirectorySeparator(path)));
        }

        [Test]
        public void TestTrimEndingDirectorySeparator_WindowsRootPath_WithTrailingSlash()
        {
            string path = "C:\\file.txt\\";
            var p = new Path(path);

            bool result = p.TrimEndingDirectorySeparator();
            bool expectedResult = true; // Path had a trailing slash to trim

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(global::System.IO.Path.TrimEndingDirectorySeparator(path)));
        }

        [Test]
        public void TestTrimEndingDirectorySeparator_UnixLikePathWithTrailingSlash()
        {
            string path = "/usr/local/bin/";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool result = p.TrimEndingDirectorySeparator();
            bool expectedResult = true; // Path had a trailing slash to trim

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(global::System.IO.Path.TrimEndingDirectorySeparator(path).Replace('\\', '/')));
        }

        [Test]
        public void TestTrimEndingDirectorySeparator_UnixLikePathWithoutTrailingSlash()
        {
            string path = "/usr/local/bin";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool result = p.TrimEndingDirectorySeparator();
            bool expectedResult = false; // Path already without trailing slash

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(global::System.IO.Path.TrimEndingDirectorySeparator(path).Replace('\\', '/')));
        }

#endif

        [Test]
        public void TestIsRoot_WindowsRootPath()
        {
            string path = "C:\\";
            var p = new Path(path);

            bool result = p.IsRoot();
            bool expectedResult = true; // "C:\\" is a root directory

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(path));
        }

        [Test]
        public void TestIsRoot_WindowsUNCPath()
        {
            string path = "\\\\server\\share";
            var p = new Path(path);

            bool result = p.IsRoot();
            bool expectedResult = true; // "\\server\\share" is a UNC root directory

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(path));
        }

        [Test]
        public void TestIsRoot_UnixRootPath()
        {
            string path = "/";
            var p = new Path(path);

            bool result = p.IsRoot();
            bool expectedResult = true; // "/" is the root directory on Unix-like systems

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(path));
        }

        [Test]
        public void TestIsRoot_UnixSubDirectoryPath()
        {
            string path = "/home/user";
            var p = new Path(path);

            bool result = p.IsRoot();
            bool expectedResult = false; // "/home/user" is not a root directory

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(path));
        }

        [Test]
        public void TestIsRoot_WindowsSubDirectoryPath()
        {
            string path = "C:\\folder";
            var p = new Path(path);

            bool result = p.IsRoot();
            bool expectedResult = false; // "C:\\folder" is not a root directory

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(path));
        }

        [Test]
        public void TestIsRoot_RelativePath()
        {
            string path = "folder\\file.txt";
            var p = new Path(path);

            bool result = p.IsRoot();
            bool expectedResult = false; // Relative path is not a root directory

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(path));
        }

        [Test]
        public void TestNormalizeDirectorySeparators_SingleSlash()
        {
            string path = "path/to/some/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool normalized = p.NormalizeDirectorySeparators();

            Assert.That(normalized, Is.False);
            Assert.That(p.ToString(), Is.EqualTo("path/to/some/file.txt"));
        }

        [Test]
        public void TestNormalizeDirectorySeparators_MultipleSlashes()
        {
            string path = "path//to///some////file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool normalized = p.NormalizeDirectorySeparators();

            Assert.That(normalized, Is.True);
            Assert.That(p.ToString(), Is.EqualTo("path/to/some/file.txt"));
        }

        [Test]
        public void TestNormalizeDirectorySeparators_Backslashes()
        {
            string path = "path\\to\\some\\file.txt";
            var p = new Path(path);

            bool normalized = p.NormalizeDirectorySeparators();

            Assert.That(normalized, Is.False);
            Assert.That(p.ToString(), Is.EqualTo("path\\to\\some\\file.txt"));
        }

        [Test]
        public void TestNormalizeDirectorySeparators_MixedSlashes()
        {
            string path = "path\\to/some\\file.txt";
            var p = new Path(path);

            bool normalized = p.NormalizeDirectorySeparators();

            Assert.That(normalized, Is.True);
            Assert.That(p.ToString(), Is.EqualTo("path\\to\\some\\file.txt"));
        }

        [Test]
        public void TestNormalizeDirectorySeparators_RootPath()
        {
            string path = "/file.txt";
            var p = new Path(path);

            bool normalized = p.NormalizeDirectorySeparators();

            Assert.That(normalized, Is.True);
            Assert.That(p.ToString(), Is.EqualTo("\\file.txt"));
        }

        [Test]
        public void TestNormalizeDirectorySeparators_SubDirectoryPath()
        {
            string path = "/subdir/file.txt";
            var p = new Path(path);

            bool normalized = p.NormalizeDirectorySeparators();

            Assert.That(normalized, Is.True);
            Assert.That(p.ToString(), Is.EqualTo("\\subdir\\file.txt"));
        }

        [Test]
        public void TestNormalizeDirectorySeparators_MultipleBackslashesAtStart()
        {
            string path = "\\\\server\\share\\folder/file.txt";
            var p = new Path(path);

            bool normalized = p.NormalizeDirectorySeparators();

            Assert.That(normalized, Is.True);
            Assert.That(p.ToString(), Is.EqualTo("\\\\server\\share\\folder\\file.txt"));
        }

        [Test]
        public void TestNormalizeDirectorySeparators_NoNormalizationNeeded()
        {
            string path = "already/normalized/path/file.txt";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool normalized = p.NormalizeDirectorySeparators();

            Assert.That(normalized, Is.False);
            Assert.That(p.ToString(), Is.EqualTo("already/normalized/path/file.txt"));
        }

        [Test]
        public void TestChangeFileName_Success()
        {
            var path = new Path("directory/oldname.txt");

            bool result = path.ChangeFileName("newname.txt");

            Assert.That(result, Is.True);
            Assert.That(path.ToString(), Is.EqualTo("directory/newname.txt"));
        }

        [Test]
        public void TestChangeFileName_RemoveFileName()
        {
            var path = new Path("directory/oldname.txt");

            bool result = path.ChangeFileName(null);

            Assert.That(result, Is.True);
            Assert.That(path.ToString(), Is.EqualTo("directory/"));
        }

        [Test]
        public void TestChangeFileName_NoRemoveFileName()
        {
            var path = new Path("foo/");

            bool result = path.ChangeFileName(null);

            Assert.That(result, Is.False);
            Assert.That(path.ToString(), Is.EqualTo("foo/"));
        }

        [Test]
        public void TestChangeFileName_RootRemoveFileName()
        {
            var path = new Path("/foo");

            bool result = path.ChangeFileName(null);

            Assert.That(result, Is.True);
            Assert.That(path.ToString(), Is.EqualTo("/"));
        }

        [Test]
        public void TestChangeFileName_OnlyRootRemoveFileName()
        {
            var path = new Path("/");

            bool result = path.ChangeFileName(null);

            Assert.That(result, Is.False);
            Assert.That(path.ToString(), Is.EqualTo("/"));
        }

        [Test]
        public void TestChangeFileName_RemoveFileNameEmptyPath()
        {
            var path = new Path("");

            bool result = path.ChangeFileName(null);

            Assert.That(result, Is.False);
            Assert.That(path.ToString(), Is.EqualTo(""));
        }

        [Test]
        public void TestChangeFileName_EmptyPath()
        {
            var path = new Path("");

            bool result = path.ChangeFileName("newname.txt");

            Assert.That(result, Is.True);
            Assert.That(path.ToString(), Is.EqualTo("newname.txt"));
        }

        [Test]
        public void TestChangeFileName_NoExtension()
        {
            var path = new Path("directory/oldname");

            bool result = path.ChangeFileName("newname");

            Assert.That(result, Is.True);
            Assert.That(path.ToString(), Is.EqualTo("directory/newname"));
        }

        [Test]
        public void TestChangeFileName_FromNullToValidName()
        {
            var path = new Path("directory/");

            bool result = path.ChangeFileName("newname.txt");

            Assert.That(result, Is.True);
            Assert.That(path.ToString(), Is.EqualTo("directory/newname.txt"));
        }

        [Test]
        public void TestChangeFileName_EmptyInputOnRootPath()
        {
            var path = new Path("/");

            bool result = path.ChangeFileName("");

            Assert.That(result, Is.False);
            Assert.That(path.ToString(), Is.EqualTo("/"));
        }

        [Test]
        public void TestChangeFileName_NullInputOnRootPath()
        {
            var path = new Path("/");

            bool result = path.ChangeFileName(null);

            Assert.That(result, Is.False);
            Assert.That(path.ToString(), Is.EqualTo("/"));
        }

        [Test]
        public void TestChangeFileName_EmptyString()
        {
            var path = new Path("directory/oldname.txt");

            bool result = path.ChangeFileName("");

            Assert.That(result, Is.True);
            Assert.That(path.ToString(), Is.EqualTo("directory/"));
        }

        [Test]
        public void TestChangeFileName_SameName()
        {
            var path = new Path("directory/samefile.txt");

            bool result = path.ChangeFileName("samefile.txt");

            Assert.That(result, Is.True);//TODO
            Assert.That(path.ToString(), Is.EqualTo("directory/samefile.txt"));
        }

        [Test]
        public void TestChangeFileName_WithSpaces()
        {
            var path = new Path("directory/old name with spaces.txt");

            bool result = path.ChangeFileName("new name with spaces.txt");

            Assert.That(result, Is.True);
            Assert.That(path.ToString(), Is.EqualTo("directory/new name with spaces.txt"));
        }

        [Test]
        public void TestChangeFileName_Success_ReadOnlySpan()
        {
            var path = new Path("directory/oldname.txt");

            bool result = path.ChangeFileName("newname.txt".AsSpan());

            Assert.That(result, Is.True);
            Assert.That(path.ToString(), Is.EqualTo("directory/newname.txt"));
        }

        [Test]
        public void TestChangeFileName_RemoveFileName_ReadOnlySpan()
        {
            var path = new Path("directory/oldname.txt");

            bool result = path.ChangeFileName([]);

            Assert.That(result, Is.True);
            Assert.That(path.ToString(), Is.EqualTo("directory/"));
        }

        [Test]
        public void TestChangeFileName_EmptyPath_ReadOnlySpan()
        {
            var path = new Path("");

            bool result = path.ChangeFileName("newname.txt".AsSpan());

            Assert.That(result, Is.True);
            Assert.That(path.ToString(), Is.EqualTo("newname.txt"));
        }

        [Test]
        public void TestChangeFileName_NoExtension_ReadOnlySpan()
        {
            var path = new Path("directory/oldname");

            bool result = path.ChangeFileName("newname".AsSpan());

            Assert.That(result, Is.True);
            Assert.That(path.ToString(), Is.EqualTo("directory/newname"));
        }

        [Test]
        public void TestChangeFileName_FromNullToValidName_ReadOnlySpan()
        {
            var path = new Path("directory/");

            bool result = path.ChangeFileName("validname".AsSpan());

            Assert.That(result, Is.True);
            Assert.That(path.ToString(), Is.EqualTo("directory/validname"));
        }

        [Test]
        public void TestChangeFileName_SameFileName_ReadOnlySpan()
        {
            var path = new Path("directory/samefile.txt");

            bool result = path.ChangeFileName("samefile.txt".AsSpan());

            Assert.That(result, Is.True);//TODO
            Assert.That(path.ToString(), Is.EqualTo("directory/samefile.txt"));
        }

        [Test]
        public void TestEnsureEndingDirectorySeparator_WindowsPath_WithoutTrailingSlash()
        {
            string path = "C:\\subdir\\file.txt";
            var p = new Path(path);

            bool result = p.EnsureTrailingSeparator();
            bool expectedResult = true; // Path did not have a trailing slash

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(path + '\\'));
        }

        [Test]
        public void TestEnsureEndingDirectorySeparator_WindowsRootPath_WithTrailingSlash()
        {
            string path = "C:\\file.txt\\";
            var p = new Path(path);

            bool result = p.EnsureTrailingSeparator();
            bool expectedResult = false; // Path already ends with a trailing slash

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(path));
        }

        [Test]
        public void TestEnsureEndingDirectorySeparator_UnixLikePathWithTrailingSlash()
        {
            string path = "/usr/local/bin/";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool result = p.EnsureTrailingSeparator();
            bool expectedResult = false; // Path already has a trailing slash

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(path));
        }

        [Test]
        public void TestEnsureEndingDirectorySeparator_UnixLikePathWithoutTrailingSlash()
        {
            string path = "/usr/local/bin";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool result = p.EnsureTrailingSeparator();
            bool expectedResult = true; // Path did not have a trailing slash

            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(p.ToString(), Is.EqualTo(path + '/'));
        }

        [Test]
        public void TestStartsWithDirectorySeparator_UnixRootPath()
        {
            string path = "/";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool result = p.StartsWithDirectorySeparator();

            Assert.That(result, Is.True);
        }

        [Test]
        public void TestStartsWithDirectorySeparator_UnixSubdirectoryPath()
        {
            string path = "/usr/local/bin";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool result = p.StartsWithDirectorySeparator();

            Assert.That(result, Is.True);
        }

        [Test]
        public void TestStartsWithDirectorySeparator_UnixFilePathWithoutLeadingSlash()
        {
            string path = "usr/local/bin";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            bool result = p.StartsWithDirectorySeparator();

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestStartsWithDirectorySeparator_WindowsRootPath()
        {
            string path = "C:\\";
            var p = new Path(path);

            bool result = p.StartsWithDirectorySeparator();

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestStartsWithDirectorySeparator_WindowsSubdirectoryPath()
        {
            string path = "C:\\Program Files\\";
            var p = new Path(path);

            bool result = p.StartsWithDirectorySeparator();

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestStartsWithDirectorySeparator_WindowsNetworkPath()
        {
            string path = "\\\\server\\share\\folder";
            var p = new Path(path);

            bool result = p.StartsWithDirectorySeparator();

            Assert.That(result, Is.True);
        }

        [Test]
        public void TestStartsWithDirectorySeparator_EmptyPath()
        {
            string path = "";
            var p = new Path(path);

            bool result = p.StartsWithDirectorySeparator();

            Assert.That(result, Is.False);
        }

        [Test]
        public void TestGetPathSegments_UnixRootPath()
        {
            string path = "/";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var segments = p.GetPathSegments();

            Assert.That(segments, Has.Count.EqualTo(1));
            Assert.That(segments[0], Is.EqualTo("/"));
        }


        [Test]
        public void TestGetPathSegments_UnixSubdirectoryPath()
        {
            string path = "/usr/local/bin";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var segments = p.GetPathSegments();

            Assert.That(segments, Has.Count.EqualTo(4));
            Assert.That(segments[0], Is.EqualTo("/"));
            Assert.That(segments[1], Is.EqualTo("usr"));
            Assert.That(segments[2], Is.EqualTo("local"));
            Assert.That(segments[3], Is.EqualTo("bin"));
        }

        [Test]
        public void TestGetPathSegments_UnixRelativePath()
        {
            string path = "usr/local/bin";
            var p = new Path(path) { DirectorySeparatorChar = '/' };

            var segments = p.GetPathSegments();

            Assert.That(segments, Has.Count.EqualTo(3));
            Assert.That(segments[0], Is.EqualTo("usr"));
            Assert.That(segments[1], Is.EqualTo("local"));
            Assert.That(segments[2], Is.EqualTo("bin"));
        }

        [Test]
        public void TestGetPathSegments_WindowsRootPath()
        {
            string path = "C:\\";
            var p = new Path(path);

            var segments = p.GetPathSegments();

            Assert.That(segments, Has.Count.EqualTo(1));
            Assert.That(segments[0], Is.EqualTo("C:\\"));
        }

        [Test]
        public void TestGetPathSegments_WindowsSubdirectoryPath()
        {
            string path = "C:\\Program Files\\Common Files";
            var p = new Path(path);

            var segments = p.GetPathSegments();

            Assert.That(segments, Has.Count.EqualTo(3));
            Assert.That(segments[0], Is.EqualTo("C:\\"));
            Assert.That(segments[1], Is.EqualTo("Program Files"));
            Assert.That(segments[2], Is.EqualTo("Common Files"));
        }

        [Test]
        public void TestGetPathSegments_WindowsNetworkPath()
        {
            string path = "\\\\server\\share\\folder";
            var p = new Path(path);

            var segments = p.GetPathSegments();

            Assert.That(segments, Has.Count.EqualTo(2));
            Assert.That(segments[0], Is.EqualTo("\\\\server\\share"));
            Assert.That(segments[1], Is.EqualTo("folder"));
        }

        [Test]
        public void TestGetPathSegments_EmptyPath()
        {
            string path = "";
            var p = new Path(path);

            var segments = p.GetPathSegments();

            Assert.That(segments, Has.Count.EqualTo(0));
        }

        [TestCase("/", "/", '/', true)]
        [TestCase("/", "/", '\\', true)]
        [TestCase("/usr/local/bin", "/usr/local/bin", '/', true)]
        [TestCase("/usr/local/bin/", "/usr/local/bin", '/', true)]
        [TestCase("/USR/LOCAL/BIN", "/usr/local/bin", '\\', true)]
        [TestCase("/USR/LOCAL/BIN", "/usr/local/bin", '/', false)]
        [TestCase("C:\\Program Files", "C:\\Program Files", '\\', true)]
        [TestCase("C:\\Program Files\\", "C:\\Program Files", '\\', true)]
        [TestCase("C:\\PROGRAM FILES", "C:\\Program Files", '\\', true)]
        [TestCase("C:\\dir1\\dir2", "C:/dir1/dir2", '\\', true)]
        [TestCase("\\\\server\\share", "\\\\server\\share", '\\', true)]
        [TestCase("\\\\server\\share", "\\\\server\\share\\", '\\', true)]
        [TestCase("\\\\server\\share", "\\\\server\\share\\\\", '\\', true)]
        [TestCase("", "", '\\', true)]
        [TestCase("", "", '/', true)]
        [TestCase("", "C:\\", '\\', false)]

        [TestCase("C:\\Users\\User\\Documents", "C:/Users/User/Documents", '\\', true)]
        [TestCase("C:\\Users\\User\\Documents", "C:/Users/User/Documents/", '\\', true)]
        //[TestCase("C:\\Users\\User\\Documents\\..\\User\\Documents", "C:/Users/User/Documents", '\\', true)]
        //[TestCase("/usr/local/bin/../bin", "/usr/local/bin", '/', true)]
        [TestCase("//server/share/folder", "\\\\server\\share\\folder", '\\', true)] 
        [TestCase("//server/share/folder", "//server/share/folder", '/', true)]
        //[TestCase("/a/b/../../../../c", "/c", '/', true)]
        //[TestCase("C:\\Folder\\.\\SubFolder", "C:\\Folder\\SubFolder", '\\', true)]
        //[TestCase("C:\\Folder\\..\\Folder\\SubFolder", "C:\\Folder\\SubFolder", '\\', true)]
        //[TestCase("C:\\Folder\\..\\AnotherFolder", "C:\\AnotherFolder", '\\', true)]
        public void TestEqualTo(string path1, string path2, char directorySeparatorChar, bool expected)
        {
            var p = new Path(path1) { DirectorySeparatorChar = directorySeparatorChar };

            Assert.That(p.PathEquals(path2.AsSpan()), Is.EqualTo(expected));
        }

        [TestCase("relative/path", "/base/path", '/', "/base/path/relative/path")]
        [TestCase("../up/one/level", "/base/path", '/', "/base/up/one/level")]
        [TestCase(".", "/base/path", '/', "/base/path")]
        [TestCase("..", "/base/path", '/', "/base")]
        [TestCase("../../../../c", "/a/b", '/', "/c")]
        [TestCase("Folder\\SubFolder", "C:\\BasePath", '\\', "C:\\BasePath\\Folder\\SubFolder")]
        [TestCase(".\\SubFolder", "C:\\BasePath", '\\', "C:\\BasePath\\SubFolder")]
        [TestCase("..\\AnotherFolder", "C:\\BasePath", '\\', "C:\\AnotherFolder")]
        [TestCase("\\absolute\\path", "C:\\BasePath", '\\', "C:\\absolute\\path")]
        [TestCase("/absolute/path", "/base/path", '/', "/absolute/path")]
        [TestCase(".\\folder", "C:\\base\\path", '\\', "C:\\base\\path\\folder")]
        [TestCase("..\\..\\new_folder", "C:\\base\\path", '\\', "C:\\new_folder")]
        [TestCase("new\\folder", "C:\\base\\path", '\\', "C:\\base\\path\\new\\folder")]
        [TestCase("..\\..\\folder", "D:\\base\\path", '\\', "D:\\folder")]
        [TestCase("folder/subfolder", "/home/user", '/', "/home/user/folder/subfolder")]
        [TestCase("./subdir", "/usr/local", '/', "/usr/local/subdir")]
        [TestCase("../prevdir", "/home/user/docs", '/', "/home/user/prevdir")]

        [TestCase("../../../folder", "/base/a/b/c", '/', "/base/folder")]
        [TestCase("../../d/e/../f", "/a/b/c", '/', "/a/d/f")]
        [TestCase("..\\..\\d\\e\\..\\f", "C:\\a\\b\\c", '\\', "C:\\a\\d\\f")]
        [TestCase("relative\\path", "C:\\with spaces\\and\\special\\chars", '\\', "C:\\with spaces\\and\\special\\chars\\relative\\path")]
        [TestCase("../up/mixed/slashes/in/path", "/base/path", '/', "/base/up/mixed/slashes/in/path")]
        [TestCase("../up\\mixed/slashes\\in/path", "C:\\base\\path", '\\', "C:\\base\\up\\mixed\\slashes\\in\\path")]
        [TestCase("../up/mixed\\slashes/in\\path", "C:\\base\\path", '\\', "C:\\base\\up\\mixed\\slashes\\in\\path")]
        [TestCase("./././same_dir", "/base/path", '/', "/base/path/same_dir")]
        [TestCase(".\\.\\.\\same_dir", "C:\\base\\path", '\\', "C:\\base\\path\\same_dir")]
        [TestCase("folder1/./folder2/../folder3", "/base/path", '/', "/base/path/folder1/folder3")]
        [TestCase("folder1\\.\\folder2\\..\\folder3", "C:\\base\\path", '\\', "C:\\base\\path\\folder1\\folder3")]

        [TestCase("/a/b/../../../../c", "/", '/', "/c")]
        [TestCase("C:\\Users\\User\\Documents\\..\\..\\User\\Documents", "C:\\BasePath", '\\', "C:\\Users\\User\\Documents")]
        [TestCase("/usr/local/bin/../bin", "/", '/', "/usr/local/bin")]
        [TestCase("C:\\Folder\\.\\SubFolder", "C:\\", '\\', "C:\\Folder\\SubFolder")]
        [TestCase("C:\\Folder\\..\\Folder\\SubFolder", "C:\\", '\\', "C:\\Folder\\SubFolder")]
        [TestCase("C:\\Folder\\..\\AnotherFolder", "C:\\", '\\', "C:\\AnotherFolder")]
        public void TestGetFullPath(string relativePath, string basePath, char directorySeparatorChar, string expected)
        {
            var p = new Path(relativePath) { DirectorySeparatorChar = directorySeparatorChar };

            var result = p.GetFullPath(basePath.AsSpan());

#if !NETFRAMEWORK
            if (directorySeparatorChar == '\\')
            {
                var fullPath = global::System.IO.Path.GetFullPath(relativePath, basePath);
                Assert.That(result.ToString(), Is.EqualTo(fullPath));
                Assert.That(fullPath, Is.EqualTo(expected));
            }
#endif
            Assert.That(result.ToString(), Is.EqualTo(expected));
        }

        [TestCase("C:\\Foo", "C:\\Bar", '\\', "..\\Bar")]
        [TestCase("C:\\Foo", "C:\\Foo\\Bar", '\\', "Bar")]
        [TestCase("C:\\Foo\\Bar", "C:\\Bar\\Bar", '\\', "..\\..\\Bar\\Bar")]
        [TestCase("C:\\Foo\\Foo", "C:\\Foo\\Bar", '\\', "..\\Bar")]
        [TestCase("C:\\Users\\User\\Documents", "C:\\Users\\User\\Pictures", '\\', "..\\Pictures")]
        [TestCase("C:\\Users\\User", "C:\\Users\\User\\Documents\\..", '\\', ".")] 
        [TestCase("C:\\", "C:\\Users\\User", '\\', "Users\\User")]
        [TestCase("C:\\Users\\User\\..\\Desktop", "C:\\Users\\User", '\\', "..\\User")]
        [TestCase("C:\\Users\\User\\Documents\\.\\SomeFile.txt", "C:\\Users\\User\\Documents", '\\', "..")]
        [TestCase("C:\\Users\\User\\Documents", "C:\\Users\\OtherUser\\Documents", '\\', "..\\..\\OtherUser\\Documents")]
        [TestCase("C:\\Users\\User\\Documents", "C:\\Users\\User\\Documents\\..", '\\', "..")]
        [TestCase("C:\\Users\\User", "C:\\Users\\User\\", '\\', ".")]
        [TestCase("C:\\Users\\User\\..\\..", "C:\\Users\\User\\Documents", '\\', "Users\\User\\Documents")]
        public void TestGetRelativePath(string relativeTo, string basePath, char directorySeparatorChar, string expected)
        {
            var p = new Path(relativeTo) { DirectorySeparatorChar = directorySeparatorChar };

            var result = p.GetRelativePath(basePath);
            if (directorySeparatorChar == '\\')
            {
                var relativePath = global::System.IO.Path.GetRelativePath(relativeTo, basePath);
                Assert.That(result.ToString(), Is.EqualTo(relativePath));
                Assert.That(relativePath, Is.EqualTo(expected));
            }
            Assert.That(result.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void GetRelativePath_WithBasePath_ResolvesCorrectly()
        {
            ReadOnlySpan<char> basePath = @"C:\Users\Test".AsSpan();
            ReadOnlySpan<char> relativeTo = "projectA".AsSpan();
            ReadOnlySpan<char> path = @"projectB\file.cs".AsSpan();

            var p = new Path(relativeTo.ToString());

            var result = p.GetRelativePath(path, basePath);
            Assert.That(result.ToString(), Is.EqualTo(@"..\projectB\file.cs"));
        }

        [TestCase("C:\\Users\\User\\Documents\\..\\..\\User\\Documents", '\\', "C:\\Users\\User\\Documents")]
        [TestCase("/usr/local/bin/../bin", '/', "/usr/local/bin")]
        [TestCase("C:\\Folder\\.\\SubFolder", '\\', "C:\\Folder\\SubFolder")]
        [TestCase("C:\\Folder\\..\\Folder\\SubFolder", '\\', "C:\\Folder\\SubFolder")]
        [TestCase("C:\\Folder\\..\\AnotherFolder", '\\', "C:\\AnotherFolder")]

        [TestCase("\\\\", '\\', "\\\\")]
        [TestCase("//", '/', "/")]
        [TestCase("C:\\\\Folder", '\\', "C:\\Folder")]
        [TestCase(".\\Folder", '\\', ".\\Folder")]
        [TestCase("..\\Folder", '\\', "..\\Folder")]
        [TestCase("C:\\Users\\User\\Documents\\..\\..\\Desktop", '\\', "C:\\Users\\Desktop")]
        [TestCase("C:\\Users\\User\\Documents\\..\\Desktop\\file.txt", '\\', "C:\\Users\\User\\Desktop\\file.txt")]
        [TestCase("C:\\Users\\User\\Documents<file.txt", '\\', "C:\\Users\\User\\Documents<file.txt")]
        [TestCase("C:\\Users\\User\\Documents", '\\', "C:\\Users\\User\\Documents")]
        [TestCase("C:\\Users\\User\\Documents\\file1.txt", '\\', "C:\\Users\\User\\Documents\\file1.txt")]
        [TestCase("C:\\Program Files\\My Program", '\\', "C:\\Program Files\\My Program")]
        [TestCase("\\\\server\\share\\folder", '\\', "\\\\server\\share\\folder")]
        [TestCase("/home/user/documents", '/', "/home/user/documents")]
        public void TestGetFullPathExpanded(string path, char directorySeparatorChar, string expected)
        {
            var p = new Path(path) { DirectorySeparatorChar = directorySeparatorChar };

            var result = p.GetFullPath();

            if (directorySeparatorChar == '\\' && !path.StartsWith('.'))
            {
                string fullPath;
                try
                {
                    fullPath = global::System.IO.Path.GetFullPath(path);
                    Assert.That(result.ToString(), Is.EqualTo(fullPath));
                    Assert.That(fullPath, Is.EqualTo(expected));
                }
                catch (Exception)
                {
#if !NETFRAMEWORK
                    throw;
#endif
                }
            }
            Assert.That(result.ToString(), Is.EqualTo(expected));
        }
    }
}
