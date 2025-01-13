using Path = System.IO.PathBuilder;

namespace NuExt.System.Tests
{
    public class PathBuilderTests
    {
        [Test]
        public void Test1()
        {
            var p = new Path() { DirectorySeparatorChar = '/' };
            Assert.That(p.Length, Is.EqualTo(0));

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
            var p = new Path("initial/path");
            Assert.That(p.Length, Is.EqualTo(12));

            p += "/additional/path";
            Assert.That(p.Length, Is.EqualTo(28));

            Assert.That(p.ToString(), Is.EqualTo("initial/path/additional/path"));
        }

        [Test]
        public void Test3()
        {
            var p = new Path();
            p /= "initial/path";
            p = p / "/additional/path";
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
            Assert.That(destination.Slice(0, charsWritten).ToString(), Is.EqualTo(@"C:\Users\Test"));
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

                string result = new string(ptr, 0, p.Length);
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

                 string result = new string(ptr, 0, p.Length);
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

                string result = new string(ptr);
                Assert.That(result, Is.EqualTo(@"C:\Users\Test"));
            }
        }
    }
}
