using System.Diagnostics.CodeAnalysis;

namespace NuExt.System.Tests
{
    [SuppressMessage("Assertion", "NUnit2045:Use Assert.Multiple", Justification = "<Pending>")]
    public class EnumExtensionsTests
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
            var result = Enum.GetValue<TestEnum>("FirstValue");
            Assert.That(result, Is.EqualTo(TestEnum.FirstValue));
        }

        [Test]
        public void GetValue_CaseSensitive_Failure()
        {
            Assert.That(() => Enum.GetValue<TestEnum>("firstvalue"), Throws.ArgumentException);
        }

        [Test]
        public void GetValue_CaseInsensitive_Success()
        {
            var result = Enum.GetValue<TestEnum>("firstvalue", ignoreCase: true);
            Assert.That(result, Is.EqualTo(TestEnum.FirstValue));
        }

        [Test]
        public void GetValue_NonExistentValue_ThrowsException()
        {
            Assert.That(() => Enum.GetValue<TestEnum>("NonExistentValue"), Throws.ArgumentException);
        }

        [Test]
        public void TryGetValue_CaseSensitive_Success()
        {
            bool success = Enum.TryGetValue<TestEnum>("SecondValue", out var result);
            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo(TestEnum.SecondValue));
        }

        [Test]
        public void TryGetValue_CaseSensitive_Failure()
        {
            bool success = Enum.TryGetValue<TestEnum>("secondvalue", out var result);
            Assert.That(success, Is.False);
            Assert.That(result, Is.Default);
        }

        [Test]
        public void TryGetValue_CaseInsensitive_Success()
        {
            bool success = Enum.TryGetValue<TestEnum>("thirdvalue", ignoreCase: true, out var result);
            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo(TestEnum.ThirdValue));
        }

        [Test]
        public void TryGetValue_NullOrWhiteSpace_ReturnsFalse()
        {
            bool successNull = Enum.TryGetValue<TestEnum>(null, out var resultNull);
            bool successEmpty = Enum.TryGetValue<TestEnum>(string.Empty, out var resultEmpty);
            bool successWhiteSpace = Enum.TryGetValue<TestEnum>(" ", out var resultWhiteSpace);

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
            var resultThird = Enum.GetValue<TestEnum>("thirdvalue", ignoreCase: true);
            var resultThirdUpper = Enum.GetValue<TestEnum>("THIRDVALUE", ignoreCase: true);

            Assert.That(resultThird, Is.EqualTo(TestEnum.ThirdValue));
            Assert.That(resultThirdUpper, Is.EqualTo(TestEnum.ThirdValue));
        }

        [Test]
        public void TryGetValue_CaseInsensitive_SuccessWithDifferentCases()
        {
            bool successThird = Enum.TryGetValue<TestEnum>("thirdvalue", ignoreCase: true, out var resultThird);
            bool successThirdUpper = Enum.TryGetValue<TestEnum>("THIRDVALUE", ignoreCase: true, out var resultThirdUpper);

            Assert.That(successThird, Is.True);
            Assert.That(resultThird, Is.EqualTo(TestEnum.ThirdValue));

            Assert.That(successThirdUpper, Is.True);
            Assert.That(resultThirdUpper, Is.EqualTo(TestEnum.ThirdValue));
        }

        [Test]
        public void GetValue_CaseInsensitive_Failure()
        {
            Assert.That(() => Enum.GetValue<TestEnum>("invalidvalue", ignoreCase: true), Throws.ArgumentException);
            Assert.That(() => Enum.GetValue<TestEnum>("FIRSTVALUE", ignoreCase: false), Throws.ArgumentException);
        }

        [Test]
        public void TryGetValue_CaseInsensitive_Failure()
        {
            bool successInvalid = Enum.TryGetValue<TestEnum>("invalidvalue", ignoreCase: true, out var resultInvalid);
            bool successNonMatchingCase = Enum.TryGetValue<TestEnum>("FIRSTVALUE", ignoreCase: false, out var resultNonMatchingCase);

            Assert.That(successInvalid, Is.False);
            Assert.That(resultInvalid, Is.Default);

            Assert.That(successNonMatchingCase, Is.False);
            Assert.That(resultNonMatchingCase, Is.Default);
        }
    }
}
