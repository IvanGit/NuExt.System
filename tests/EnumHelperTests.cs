using System.Diagnostics.CodeAnalysis;

namespace NuExt.System.Tests
{
    [SuppressMessage("Assertion", "NUnit2045:Use Assert.Multiple", Justification = "<Pending>")]
    public class EnumHelperTests
    {
        public enum TestEnum
        {
            FirstValue,
            SecondValue,
            ThirdValue,
            ThirdVALUE
        }

        [Test]
        public void GetValue_CaseSensitive_Success()
        {
            var result = EnumHelper<TestEnum>.GetValue("FirstValue");
            Assert.That(result, Is.EqualTo(TestEnum.FirstValue));
        }

        [Test]
        public void GetValue_CaseSensitive_Failure()
        {
            Assert.That(() => EnumHelper<TestEnum>.GetValue("firstvalue"), Throws.ArgumentException);
        }

        [Test]
        public void GetValue_CaseInsensitive_Success()
        {
            var result = EnumHelper<TestEnum>.GetValue("firstvalue", ignoreCase: true);
            Assert.That(result, Is.EqualTo(TestEnum.FirstValue));
        }

        [Test]
        public void GetValue_NonExistentValue_ThrowsException()
        {
            Assert.That(() => EnumHelper<TestEnum>.GetValue("NonExistentValue"), Throws.ArgumentException);
        }

        [Test]
        public void TryGetValue_CaseSensitive_Success()
        {
            bool success = EnumHelper<TestEnum>.TryGetValue("SecondValue", out var result);
            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo(TestEnum.SecondValue));
        }

        [Test]
        public void TryGetValue_CaseSensitive_Failure()
        {
            bool success = EnumHelper<TestEnum>.TryGetValue("secondvalue", out var result);
            Assert.That(success, Is.False);
            Assert.That(result, Is.Default);
        }

        [Test]
        public void TryGetValue_CaseInsensitive_Success()
        {
            bool success = EnumHelper<TestEnum>.TryGetValue("thirdvalue", ignoreCase: true, out var result);
            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo(TestEnum.ThirdValue));
        }

        [Test]
        public void TryGetValue_NullOrWhiteSpace_ReturnsFalse()
        {
            bool successNull = EnumHelper<TestEnum>.TryGetValue(null, out var resultNull);
            bool successEmpty = EnumHelper<TestEnum>.TryGetValue(string.Empty, out var resultEmpty);
            bool successWhiteSpace = EnumHelper<TestEnum>.TryGetValue(" ", out var resultWhiteSpace);

            Assert.That(successNull, Is.False);
            Assert.That(resultNull, Is.Default);

            Assert.That(successEmpty, Is.False);
            Assert.That(resultEmpty, Is.Default);

            Assert.That(successWhiteSpace, Is.False);
            Assert.That(resultWhiteSpace, Is.Default);
        }

        [Test]
        public void GetValue_CaseInsensitive_SuccessWithDifferentCases()
        {
            var resultThird = EnumHelper<TestEnum>.GetValue("thirdvalue", ignoreCase: true);
            var resultThirdUpper = EnumHelper<TestEnum>.GetValue("THIRDVALUE", ignoreCase: true);

            Assert.That(resultThird, Is.EqualTo(TestEnum.ThirdValue));
            Assert.That(resultThirdUpper, Is.EqualTo(TestEnum.ThirdValue));
        }

        [Test]
        public void TryGetValue_CaseInsensitive_SuccessWithDifferentCases()
        {
            bool successThird = EnumHelper<TestEnum>.TryGetValue("thirdvalue", ignoreCase: true, out var resultThird);
            bool successThirdUpper = EnumHelper<TestEnum>.TryGetValue("THIRDVALUE", ignoreCase: true, out var resultThirdUpper);

            Assert.That(successThird, Is.True);
            Assert.That(resultThird, Is.EqualTo(TestEnum.ThirdValue));

            Assert.That(successThirdUpper, Is.True);
            Assert.That(resultThirdUpper, Is.EqualTo(TestEnum.ThirdValue));
        }

        [Test]
        public void GetValue_CaseInsensitive_Failure()
        {
            Assert.That(() => EnumHelper<TestEnum>.GetValue("invalidvalue", ignoreCase: true), Throws.ArgumentException);
            Assert.That(() => EnumHelper<TestEnum>.GetValue("FIRSTVALUE", ignoreCase: false), Throws.ArgumentException);
        }

        [Test]
        public void TryGetValue_CaseInsensitive_Failure()
        {
            bool successInvalid = EnumHelper<TestEnum>.TryGetValue("invalidvalue", ignoreCase: true, out var resultInvalid);
            bool successNonMatchingCase = EnumHelper<TestEnum>.TryGetValue("FIRSTVALUE", ignoreCase: false, out var resultNonMatchingCase);

            Assert.That(successInvalid, Is.False);
            Assert.That(resultInvalid, Is.Default);

            Assert.That(successNonMatchingCase, Is.False);
            Assert.That(resultNonMatchingCase, Is.Default);
        }
    }
}
