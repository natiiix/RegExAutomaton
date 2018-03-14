using Microsoft.VisualStudio.TestTools.UnitTesting;
using RegExAutomaton;

namespace UnitTest
{
    [TestClass]
    public class RegExTest
    {
        [TestMethod]
        public void FullTest()
        {
            RegEx regex = new RegEx(@"^hello* world|bye* world$");

            string[] strings = { "hell world", "hello world", "hellooooo world", "by world", "bye world", "byeeeeee world" };

            foreach (string str in strings)
            {
                Match match = regex.Match(str);
                Assert.IsNotNull(match);
            }
        }
    }
}
