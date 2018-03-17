using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegExAutomaton;

namespace UnitTest
{
    [TestClass]
    public class RegExTest
    {
        [TestMethod]
        public void BasicMatchingTest()
        {
            RegEx regex = new RegEx(@"^hello* world|bye* world$");

            string[] strings = { "hell world", "hello world", "hellooooo world", "by world", "bye world", "byeeeeee world" };

            foreach (string str in strings)
            {
                Match match = regex.Match(str);
                Assert.IsNotNull(match);
            }
        }

        [TestMethod]
        public void EmptyExpressionTest()
        {
            RegEx regex1 = new RegEx("");
            RegEx regex2 = new RegEx("|test");
            RegEx regex3 = new RegEx("|test||1234|");

            string[] strings = { "", "abc", "hello world" };

            foreach (string str in strings)
            {
                Match match1 = regex1.Match(str);
                Assert.IsNotNull(match1);

                Match match2 = regex2.Match(str);
                Assert.IsNotNull(match2);

                Match match3 = regex3.Match(str);
                Assert.IsNotNull(match3);
            }
        }

        [TestMethod]
        public void ZeroOrMoreBugTest()
        {
            RegEx regex = new RegEx("^a*|hello$");

            Match match1 = regex.Match("aaaahello");
            Assert.IsNull(match1);

            Match match2 = regex.Match("");
            Assert.IsNotNull(match2);

            Match match3 = regex.Match("a");
            Assert.IsNotNull(match3);

            Match match4 = regex.Match("aaaa");
            Assert.IsNotNull(match4);

            Match match5 = regex.Match("hello");
            Assert.IsNotNull(match5);
        }

        [TestMethod]
        public void BasicGroupTest()
        {
            RegEx regex = new RegEx("^hello (world|you) yay$");

            {
                string[] strings = { "hello world yay", "hello you yay" };

                foreach (string str in strings)
                {
                    Match match = regex.Match(str);
                    Assert.IsNotNull(match);
                }
            }

            {
                string[] strings = { "", "hello ", "world", "you", "yay", "hello  yay" };

                foreach (string str in strings)
                {
                    Match match = regex.Match(str);
                    Assert.IsNull(match);
                }
            }
        }

        [TestMethod]
        public void QuantifiedGroupTest()
        {
            {
                RegEx regex = new RegEx("^hello( |_)*world$");

                string[] strings = { "helloworld", "hello world", "hello  world", "hello   world", "hello_world", "hello__world", "hello___world" };

                foreach (string str in strings)
                {
                    Match match = regex.Match(str);
                    Assert.IsNotNull(match);
                }
            }

            {
                RegEx regex = new RegEx("^x (abc*|de*f)* y$");

                string[] strings = { "x  y", "x ab y", "x abc y", "x abccccc y", "x df y", "x def y", "x deeeeeef y", "x abdfabcccdeeeef y" };

                foreach (string str in strings)
                {
                    Match match = regex.Match(str);
                    Assert.IsNotNull(match);
                }
            }

            {
                RegEx regex = new RegEx("^x (a*b|(cd|ef)*|hello) y$");

                string[] strings = { "x b y", "x ab y", "x aaaab y", "x  y", "x cd y", "x ef y", "x cdefcdef y", "x hello y" };

                foreach (string str in strings)
                {
                    Match match = regex.Match(str);
                    Assert.IsNotNull(match);
                }
            }
        }

        [TestMethod]
        public void CaptureGroupTest()
        {
            RegEx regex = new RegEx("^a(b(?:c(d)))$");
            Match match = regex.Match("abcd");

            Assert.IsNotNull(match);

            Assert.AreEqual("abcd", match.Value);

            Assert.AreEqual(2, match.Captures.Length);

            Assert.AreEqual("bcd", match.Captures[0]);
            Assert.AreEqual("d", match.Captures[1]);
        }
    }
}
