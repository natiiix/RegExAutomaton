using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegExAutomaton;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class ExtensionMethodsTest
    {
        [TestMethod]
        public void ContainsUnescapedTest()
        {
            string strABC = "abc";
            string strWithBackslash = "a\\bc";

            // Single value
            Assert.IsTrue(strABC.ContainsUnescaped("a"));
            Assert.IsTrue(strABC.ContainsUnescaped("b"));
            Assert.IsTrue(strABC.ContainsUnescaped("c"));
            Assert.IsTrue(strABC.ContainsUnescaped(""));
            Assert.IsTrue(strABC.ContainsUnescaped("ab"));
            Assert.IsTrue(strABC.ContainsUnescaped("bc"));
            Assert.IsTrue(strABC.ContainsUnescaped("abc"));

            Assert.IsFalse(strABC.ContainsUnescaped("A"));
            Assert.IsFalse(strABC.ContainsUnescaped("1"));
            Assert.IsFalse(strABC.ContainsUnescaped("\\"));

            // Single value on a string containing an backslash
            Assert.IsTrue(strWithBackslash.ContainsUnescaped("a"));
            Assert.IsTrue(strWithBackslash.ContainsUnescaped("c"));
            Assert.IsTrue(strWithBackslash.ContainsUnescaped("\\"));

            Assert.IsFalse(strWithBackslash.ContainsUnescaped("b"));

            // Multiple values
            Assert.IsTrue(strABC.ContainsUnescaped(true, "a", "b"));
            Assert.IsTrue(strABC.ContainsUnescaped(false, "a", "x"));
            Assert.IsTrue(strABC.ContainsUnescaped("a", "x"));

            Assert.IsFalse(strABC.ContainsUnescaped(true, "a", "x"));
            Assert.IsFalse(strABC.ContainsUnescaped(false, "x"));
            Assert.IsFalse(strABC.ContainsUnescaped("x"));
        }

        [TestMethod]
        public void FindUnescapedTest()
        {
            string strHello = "hello";
            string strGroupsBackslashes = @"\(hello|bye\)|(hello|bye)|hello world";

            {
                Dictionary<string, List<int>> result = strHello.FindUnescaped("l", "e");

                Assert.AreEqual(2, result.Count);

                Assert.IsTrue(result.ContainsKey("l"));
                Assert.IsTrue(result.ContainsKey("e"));

                Assert.AreEqual(2, result["l"].Count);
                Assert.AreEqual(1, result["e"].Count);

                Assert.AreEqual(2, result["l"][0]);
                Assert.AreEqual(3, result["l"][1]);
                Assert.AreEqual(1, result["e"][0]);
            }

            {
                Dictionary<string, List<int>> result = strHello.FindUnescaped("x", "y", "z");

                Assert.AreEqual(3, result.Count);

                Assert.IsTrue(result.ContainsKey("x"));
                Assert.IsTrue(result.ContainsKey("y"));
                Assert.IsTrue(result.ContainsKey("z"));

                Assert.AreEqual(0, result["x"].Count);
                Assert.AreEqual(0, result["y"].Count);
                Assert.AreEqual(0, result["z"].Count);
            }

            {
                List<int> result1 = strGroupsBackslashes.FindUnescaped("hello", true);
                List<int> result2 = strGroupsBackslashes.FindUnescaped("bye", true);
                List<int> result3 = strGroupsBackslashes.FindUnescaped("|", true);

                Assert.AreEqual(2, result1.Count);
                Assert.AreEqual(1, result2.Count);
                Assert.AreEqual(3, result3.Count);

                Assert.AreEqual(2, result1[0]);
                Assert.AreEqual(26, result1[1]);

                Assert.AreEqual(8, result2[0]);

                Assert.AreEqual(7, result3[0]);
                Assert.AreEqual(13, result3[1]);
                Assert.AreEqual(25, result3[2]);
            }
        }

        [TestMethod]
        public void ContainsAtTest()
        {
            string strHello = "hello";

            Assert.IsTrue(strHello.ContainsAt(0, "hel"));
            Assert.IsTrue(strHello.ContainsAt(1, "ell"));
            Assert.IsTrue(strHello.ContainsAt(2, "llo"));

            Assert.IsTrue(strHello.ContainsAt(0, ""));
            Assert.IsTrue(strHello.ContainsAt(3, ""));
            Assert.IsTrue(strHello.ContainsAt(5, ""));

            Assert.IsFalse(strHello.ContainsAt(1, "xy"));
            Assert.IsFalse(strHello.ContainsAt(4, "abcdef"));
            Assert.IsFalse(strHello.ContainsAt(0, "furniture"));

            Assert.IsFalse(strHello.ContainsAt(6, ""));
            Assert.IsFalse(strHello.ContainsAt(-1, ""));

            Assert.IsTrue(strHello.ContainsAt(0, "a", "bqwe", "c", "he", "x", "ycvqwex", "z"));
            Assert.IsTrue(strHello.ContainsAt(4, "aaxzcsd", "b", "chffg", "w", "xwesdfsv", "y", "o"));

            Assert.IsFalse(strHello.ContainsAt(3, "a", "bbc", "c", "h", "x", "ygfd", "zbvbwrsdfs"));
            Assert.IsFalse(strHello.ContainsAt(2, "a", "b", "c", "wcvb", "xdsq", "y", "o"));
        }

        [TestMethod]
        public void ContainsAtUnescapedTest()
        {
            string strHello = @"\he\llo\";

            Assert.IsTrue(strHello.ContainsAtUnescaped(0, @"\"));
            Assert.IsTrue(strHello.ContainsAtUnescaped(0, @"\he"));
            Assert.IsTrue(strHello.ContainsAtUnescaped(2, "e"));
            Assert.IsTrue(strHello.ContainsAtUnescaped(2, @"e\l"));
            Assert.IsTrue(strHello.ContainsAtUnescaped(5, @"lo"));
            Assert.IsTrue(strHello.ContainsAtUnescaped(7, @"\"));

            Assert.IsFalse(strHello.ContainsAtUnescaped(1, "h"));
            Assert.IsFalse(strHello.ContainsAtUnescaped(1, "he"));
            Assert.IsFalse(strHello.ContainsAtUnescaped(4, "l"));
            Assert.IsFalse(strHello.ContainsAtUnescaped(4, "llo"));
        }

        [TestMethod]
        public void IsEscapedTest()
        {
            string str = @"\a\\bc";

            Assert.IsTrue(str.IsEscaped(1));
            Assert.IsTrue(str.IsEscaped(3));

            Assert.IsFalse(str.IsEscaped(0));
            Assert.IsFalse(str.IsEscaped(2));
            Assert.IsFalse(str.IsEscaped(4));
            Assert.IsFalse(str.IsEscaped(5));
        }

        [TestMethod]
        public void IndexInRangeTest()
        {
            string str = "0123456789";

            for (int i = 0, len = str.Length; i < len; i++)
            {
                Assert.IsTrue(str.IndexInRange(i));
            }

            Assert.IsFalse(str.IndexInRange(str.Length));
            Assert.IsFalse(str.IndexInRange(str.Length + 123));

            Assert.IsFalse(str.IndexInRange(-1));
            Assert.IsFalse(str.IndexInRange(-123));
        }

        [TestMethod]
        public void SplitOnIndicesTest()
        {
            string str = @"|hello|a\|b|world|";
            List<int> splitIndices = str.FindUnescaped(Meta.Or);
            string[] parts = str.SplitOnIndices(splitIndices);

            Assert.AreEqual(4, splitIndices.Count);

            Assert.AreEqual(5, parts.Length);

            Assert.AreEqual(string.Empty, parts[0]);
            Assert.AreEqual("hello", parts[1]);
            Assert.AreEqual(@"a\|b", parts[2]);
            Assert.AreEqual("world", parts[3]);
            Assert.AreEqual(string.Empty, parts[4]);
        }

        [TestMethod]
        public void SplitOnUnescapedTest()
        {
            string str = @"|hello|a\|b|world|";
            string[] parts = str.SplitOnUnescaped(Meta.Or);

            Assert.AreEqual(5, parts.Length);

            Assert.AreEqual(string.Empty, parts[0]);
            Assert.AreEqual("hello", parts[1]);
            Assert.AreEqual(@"a\|b", parts[2]);
            Assert.AreEqual("world", parts[3]);
            Assert.AreEqual(string.Empty, parts[4]);
        }

        [TestMethod]
        public void SubstringBetweenTest()
        {
            string str = "abcdef";

            Assert.AreEqual(str, str.SubstringBetween(0, 5));
            Assert.AreEqual("cde", str.SubstringBetween(2, 4));
        }

        [TestMethod]
        public void IndexOfTest()
        {
            int[] arr = new int[] { 10, 20, 30, 40, 50, 60, 70 };

            for (int i = 0, len = arr.Length; i < len; i++)
            {
                Assert.AreEqual(i, arr.IndexOf(arr[i]));
            }
        }
    }
}
