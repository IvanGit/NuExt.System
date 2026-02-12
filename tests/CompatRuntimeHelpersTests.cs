using System.Reflection;
using System.Runtime.CompilerServices;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace NuExt.System.Tests
{
    [TestFixture]
    public class CompatRuntimeHelpersTests
    {
        // === Test Data: Struct Definitions ===
        private struct PureStruct
        {
            public int X;
            public double Y;
        }

        private struct StructWithReference
        {
            public int Id;
            public string Name; // Reference type field
        }

        private struct NestedPureStruct
        {
            public PureStruct Inner;
            public long Z;
        }

        private struct DeepNestedStructWithRef
        {
            public NestedPureStruct SafeInner;
            public StructWithReference DangerousInner;
        }

        private enum TestEnum
        {
            None = 0
        }

        private static bool IsReferenceOrContainsReferences<T>()
        {
            // The result is cached per type T using a static class.
            return TypeInfoCache<T>.IsReferenceOrContainsReferences;
        }

        // === Tests for CompatRuntimeHelpers.IsReferenceOrContainsReferences<T> ===

        [Test]
        [TestCase(typeof(int), ExpectedResult = false)]
        [TestCase(typeof(byte), ExpectedResult = false)]
        [TestCase(typeof(decimal), ExpectedResult = false)] // Decimal is a special value type
        [TestCase(typeof(DateTime), ExpectedResult = false)]
        public bool IsReferenceOrContainsReferences_PrimitiveAndSimpleValueTypes(Type type)
        {
            // Use reflection to call the generic method with the runtime type.
            var method = typeof(CompatRuntimeHelpersTests)
                .GetMethod(nameof(IsReferenceOrContainsReferences), BindingFlags.Static | BindingFlags.NonPublic)
                ?.MakeGenericMethod(type);
            return (bool)method!.Invoke(null, null)!;
        }

        [Test]
        [TestCase(typeof(string), ExpectedResult = true)]
        [TestCase(typeof(object), ExpectedResult = true)]
        [TestCase(typeof(int[]), ExpectedResult = true)] // Arrays are reference types
        [TestCase(typeof(Action), ExpectedResult = true)]
        public bool IsReferenceOrContainsReferences_ReferenceTypes(Type type)
        {
            var method = typeof(CompatRuntimeHelpersTests)
                .GetMethod(nameof(IsReferenceOrContainsReferences), BindingFlags.Static | BindingFlags.NonPublic)
                ?.MakeGenericMethod(type);
            return (bool)method!.Invoke(null, null)!;
        }

        [Test]
        public void IsReferenceOrContainsReferences_PureStruct_ReturnsFalse()
        {
            bool result = IsReferenceOrContainsReferences<PureStruct>();
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsReferenceOrContainsReferences_StructWithReference_ReturnsTrue()
        {
            bool result = IsReferenceOrContainsReferences<StructWithReference>();
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsReferenceOrContainsReferences_NestedPureStruct_ReturnsFalse()
        {
            bool result = IsReferenceOrContainsReferences<NestedPureStruct>();
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsReferenceOrContainsReferences_DeepNestedStructWithRef_ReturnsTrue()
        {
            bool result = IsReferenceOrContainsReferences<DeepNestedStructWithRef>();
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsReferenceOrContainsReferences_Enum_ReturnsFalse()
        {
            bool result = IsReferenceOrContainsReferences<TestEnum>();
            Assert.That(result, Is.False);
        }

        // === Comparative Tests (Run only on platforms where the original method exists) ===
#if NET
        [Test]
        public void IsReferenceOrContainsReferences_MatchesRuntimeHelpers_OnPrimitive()
        {
            bool compatResult = IsReferenceOrContainsReferences<int>();
            bool runtimeResult = RuntimeHelpers.IsReferenceOrContainsReferences<int>();
            Assert.That(compatResult, Is.EqualTo(runtimeResult));
        }

        [Test]
        public void IsReferenceOrContainsReferences_MatchesRuntimeHelpers_OnPureStruct()
        {
            bool compatResult = IsReferenceOrContainsReferences<PureStruct>();
            bool runtimeResult = RuntimeHelpers.IsReferenceOrContainsReferences<PureStruct>();
            Assert.That(compatResult, Is.EqualTo(runtimeResult));
        }

        [Test]
        public void IsReferenceOrContainsReferences_MatchesRuntimeHelpers_OnStructWithReference()
        {
            bool compatResult = IsReferenceOrContainsReferences<StructWithReference>();
            bool runtimeResult = RuntimeHelpers.IsReferenceOrContainsReferences<StructWithReference>();
            Assert.That(compatResult, Is.EqualTo(runtimeResult));
        }

        [Test]
        public void IsReferenceOrContainsReferences_MatchesRuntimeHelpers_OnReferenceType()
        {
            bool compatResult = IsReferenceOrContainsReferences<string>();
            bool runtimeResult = RuntimeHelpers.IsReferenceOrContainsReferences<string>();
            Assert.That(compatResult, Is.EqualTo(runtimeResult));
        }

        // A more comprehensive parameterized test for key types
        [Test]
        [TestCase(typeof(int))]
        [TestCase(typeof(string))]
        [TestCase(typeof(PureStruct))]
        [TestCase(typeof(StructWithReference))]
        [TestCase(typeof(int[]))]
        public void IsReferenceOrContainsReferences_MatchesRuntimeHelpers_ForType(Type type)
        {
            var compatMethod = typeof(CompatRuntimeHelpersTests)
                .GetMethod(nameof(IsReferenceOrContainsReferences), BindingFlags.Static | BindingFlags.NonPublic)
                ?.MakeGenericMethod(type);
            var runtimeMethod = typeof(RuntimeHelpers)
                .GetMethod(nameof(RuntimeHelpers.IsReferenceOrContainsReferences))
                ?.MakeGenericMethod(type);

            bool compatResult = (bool)compatMethod!.Invoke(null, null)!;
            bool runtimeResult = (bool)runtimeMethod!.Invoke(null, null)!;

            Assert.That(compatResult, Is.EqualTo(runtimeResult),
                $"Mismatch for type {type.FullName}. Compat: {compatResult}, Runtime: {runtimeResult}");
        }
#endif

        // === Performance/Smoke Test (Optional) ===
        [Test]
        public void IsReferenceOrContainsReferences_CachedResult_IsConsistent()
        {
            // Call multiple times. The result should be identical (cached).
            bool firstCall = IsReferenceOrContainsReferences<DeepNestedStructWithRef>();
            for (int i = 0; i < 5; i++)
            {
                bool subsequentCall = IsReferenceOrContainsReferences<DeepNestedStructWithRef>();
                Assert.That(subsequentCall, Is.EqualTo(firstCall));
            }
        }
    }
}
