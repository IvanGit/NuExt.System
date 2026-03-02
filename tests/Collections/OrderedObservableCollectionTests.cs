using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NuExt.System.Tests.Collections
{
    public sealed class TestItem(string id, int order = 0) : IOrdered
    {
        public string Id { get; } = id;
        public int Order { get; set; } = order;

        public override string ToString() => $"{Id}:{Order}";
    }

    [TestFixture]
    public class OrderedObservableCollectionTests
    {
        private static OrderedObservableCollection<TestItem> Create(params int[] orders)
        {
            var list = new List<TestItem>(orders.Length);
            for (int i = 0; i < orders.Length; i++)
                list.Add(new TestItem($"i{i}", orders[i]));
            return new OrderedObservableCollection<TestItem>(list);
        }

        private static void AssertStrictlyIncreasingAndNonNegative(OrderedObservableCollection<TestItem> c, int startIndex = 0)
        {
            Assert.That(c, Has.Count.GreaterThanOrEqualTo(0));
            if (c.Count == 0) return;

            int prev = startIndex > 0 ? c[startIndex - 1].Order : int.MinValue;
            for (int i = startIndex; i < c.Count; i++)
            {
                int cur = c[i].Order;
                Assert.That(cur, Is.GreaterThan(prev), $"Not strictly increasing at index {i}");
                Assert.That(cur, Is.GreaterThanOrEqualTo(0), $"Negative order at index {i}");
                prev = cur;
            }
        }

        // -------------------------
        // InsertItem tests
        // -------------------------

        [Test]
        public void Insert_ToEnd_Assigns_PrettyTailAnchor()
        {
            // chunk=10, step=100 for small counts
            var c = Create(120, 350);
            // Tail insert: should align left to chunk and then +step
            var item = new TestItem("x");
            c.Insert(c.Count, item);

            Assert.That(item.Order, Is.EqualTo(10300));
            AssertStrictlyIncreasingAndNonNegative(c);
        }

        [Test]
        public void Insert_ToStart_PrettyUnderRight_WhenRightIsGrown()
        {
            var c = Create(300, 315); // already grown
            var x = new TestItem("x");
            c.Insert(0, x);

            Assert.That(x.Order, Is.EqualTo(200));
            AssertStrictlyIncreasingAndNonNegative(c);
        }

        [Test]
        public void Insert_ToStart_MinZero_WhenRightIsSmall()
        {
            var c = Create(7); // right=7
            var x = new TestItem("x");
            c.Insert(0, x);

            // EnsureNonNegative(..., 0) => 0 < right
            Assert.That(x.Order, Is.Zero);
            AssertStrictlyIncreasingAndNonNegative(c);
        }

        [Test]
        public void Insert_Inside_WideGap_Picks_Pretty()
        {
            var c = Create(100, 180);
            var x = new TestItem("x");
            c.Insert(1, x);

            // mid=140, pretty=floor(mid,10)=140
            Assert.That(x.Order, Is.EqualTo(140));
            AssertStrictlyIncreasingAndNonNegative(c);
        }

        [Test]
        public void Insert_ToEnd_Assigns_TailAnchor_IgnoresProvidedOrder()
        {
            var c = Create(7);
            var x = new TestItem("x", 123456); // provided 'Order' is irrelevant for InsertItem

            c.Add(x);

            // For Count<1000: tail anchor = left - left%chunk + step = 7 - 7%100 + 10000 = 10000
            Assert.That(x.Order, Is.EqualTo(10000));
            AssertStrictlyIncreasingAndNonNegative(c);
        }

        [Test]
        public void Insert_Inside_NarrowGap_NormalizesSuffix()
        {
            var c = Create(100, 101, 300);
            var x = new TestItem("x");
            c.Insert(1, x); // gap<=1 => NormalizeOrders(index)

            // Suffix normalization will make the sequence strictly increasing;
            // exact numbers depend on "grown" status, but must be valid.
            AssertStrictlyIncreasingAndNonNegative(c);
        }

        // -------------------------
        // MoveItem tests
        // -------------------------

        [Test]
        public void Move_Adjacent_SwapsOrders()
        {
            var c = Create(100, 120, 140, 160);
            var a = c[1];   // 120
            var b = c[2];   // 140

            c.Move(1, 2);   // adjacent -> swap

            using (Assert.EnterMultipleScope())
            {
                Assert.That(a.Order, Is.EqualTo(140));
                Assert.That(b.Order, Is.EqualTo(120));
            }
            AssertStrictlyIncreasingAndNonNegative(c);
        }

        [Test]
        public void Move_FirstToLast_Assigns_TailAnchor()
        {
            var c = Create(100, 130, 170, 220);
            var moved = c[0]; // 100

            c.Move(0, 3);

            // tail anchor: left=220 -> 220 - 20 + 10000 = 10200
            Assert.That(moved.Order, Is.EqualTo(10200));
            AssertStrictlyIncreasingAndNonNegative(c);
        }

        [Test]
        public void Move_ToEnd_Assigns_PrettyTailAnchor()
        {
            var c = Create(120, 350);
            var moved = c[0];

            c.Move(0, 1); // move to end (no right neighbor after move)

            Assert.That(moved.Order, Is.EqualTo(10300));
            AssertStrictlyIncreasingAndNonNegative(c);
        }

        [Test]
        public void Move_ToStart_PrettyUnderRight_WhenRightIsGrown()
        {
            var c = Create(300, 315);
            var moved = c[1]; // 315 -> move to start

            c.Move(1, 0);

            Assert.That(moved.Order, Is.EqualTo(200));
            AssertStrictlyIncreasingAndNonNegative(c);
        }

        [Test]
        public void Move_ToStart_MinZero_WhenRightIsSmall()
        {
            // Prepare exact orders without InsertItem recomputations:
            // right = 100 (small relative to step=10000 for Count<1000)
            var c = Create(100, 7);     // index 0: 100 (right neighbor), index 1: 7 (to be moved)
            var moved = c[1];           // the one with Order=7

            c.Move(1, 0);               // move '7' to start -> !hasLeft branch

            // preferred = right - right%chunk - chunk = 100 - 0 - 100 = 0
            Assert.That(moved.Order, Is.Zero);
            AssertStrictlyIncreasingAndNonNegative(c);
        }

        [Test]
        public void Move_Inside_Adjacent_AfterAdd_PerformsSwap()
        {
            var c = Create(139, 149);
            var moved = new TestItem("x", 0);
            c.Add(moved);
            c.Move(2, 1);

            Assert.That(moved.Order, Is.EqualTo(149));
            AssertStrictlyIncreasingAndNonNegative(c);
        }

        [Test]
        public void Move_Inside_NarrowGap_NormalizesSuffix()
        {
            var c = Create(100, 101, 300);
            var moved = new TestItem("x", 0);
            c.Add(moved);
            // Move inside where gap<=1 (between 100 and 101)
            c.Move(3, 1);

            AssertStrictlyIncreasingAndNonNegative(c, startIndex: 1);
        }

        // -------------------------
        // NormalizeOrders tests
        // -------------------------

        [Test]
        public void NormalizeOrders_Full_MinimalAdjustments_And_Pretty_WhenGrown()
        {
            // Crafted unsorted chunk with duplicates and negatives (if any appear)
            // Use List ctor to bypass InsertItem logic and set exact initial orders.
            var list = new List<TestItem>
            {
                new("a", 0),
                new("b", 140),
                new("c", 139), // non-monotone
                new("d", 300)
            };
            var c = new OrderedObservableCollection<TestItem>(list);

            c.NormalizeOrders();

            AssertStrictlyIncreasingAndNonNegative(c);
        }

        [Test]
        public void NormalizeOrders_FromIndex_DoesNotTouchPrefix()
        {
            var list = new List<TestItem>
            {
                new("a", 100),
                new("b", 120),
                new("c", 110), // will be fixed
                new("d", 300)
            };
            var c = new OrderedObservableCollection<TestItem>(list);

            c.NormalizeOrders(2);

            using (Assert.EnterMultipleScope())
            {
                // Prefix untouched
                Assert.That(c[0].Order, Is.EqualTo(100));
                Assert.That(c[1].Order, Is.EqualTo(120));
            }

            AssertStrictlyIncreasingAndNonNegative(c, startIndex: 2);
        }

        // -------------------------
        // Edge cases
        // -------------------------

        [Test]
        public void Insert_ToStart_WhenRightIsZero_CallsNormalization_AndEndsValid()
        {
            var c = Create(0); // right = 0
            var x = new TestItem("x");
            c.Insert(0, x);    // triggers NormalizeOrders()

            AssertStrictlyIncreasingAndNonNegative(c);
        }

        [Test]
        public void Move_Inside_WithZeroLeft_And_TinyGap_NormalizesProperly()
        {
            var c = Create(0, 1, 100);
            var x = new TestItem("x", 0);
            c.Add(x);
            // Move 'x' between 0 and 1 (gap=1 -> NormalizeOrders(newIndex))
            c.Move(3, 1);

            AssertStrictlyIncreasingAndNonNegative(c, startIndex: 1);
        }

        [Test]
        public void LongTail_Insert_And_Move_StayValid()
        {
            // Build longer tail to ensure step/chunk scale changes don't break invariants
            var orders = new List<int>();
            for (int i = 0; i < 200; i++)
                orders.Add(i * 10); // already strictly increasing, many items

            var c = Create([.. orders]);

            // Insert at start, middle, end; then move elements around
            c.Insert(0, new TestItem("s"));
            c.Insert(100, new TestItem("m"));
            c.Insert(c.Count, new TestItem("e"));

            c.Move(c.Count - 1, 0);
            c.Move(50, 51);
            c.Move(1, c.Count - 1);

            AssertStrictlyIncreasingAndNonNegative(c);
        }
    }
}
