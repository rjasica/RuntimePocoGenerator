using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RJ.RuntimePocoGenerator.Extensions;

namespace RJ.RuntimePocoGenerator.Tests.Extensions
{
    [TestFixture]
    public class StringExtentionsTests
    {
        [Test]
        public void UppercaseFirst_should_upercase_first_char_when_char_is_lower()
        {
            string source = "source";

            string target = source.UppercaseFirst();

            Assert.AreEqual("Source", target);
        }

        [Test]
        public void UppercaseFirst_should_not_change_first_char_when_char_is_uper()
        {
            string source = "Source";

            string target = source.UppercaseFirst();

            Assert.AreEqual("Source", target);
        }

        [Test]
        public void UppercaseFirst_should_empty_string_when_source_is_empty()
        {
            string source = "";

            string target = source.UppercaseFirst();

            Assert.AreEqual("", target);
        }
    }
}
