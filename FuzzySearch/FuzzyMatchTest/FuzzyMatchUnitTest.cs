using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static FuzzySearch.Algorithm;

namespace FuzzyMatchTest
{
    [TestClass]
    public class FuzzyMatch
    {
        [TestMethod]
        public void MatchTest()
        {
            Tuple<bool, int> result = FuzzyMatch("abc", "abc");
            Assert.AreEqual(result.Item1, true);

            result = FuzzyMatch("abc", "abcd");
            Assert.AreEqual(result.Item1, true);

            result = FuzzyMatch("abcde", "abcd");
            Assert.AreEqual(result.Item1, false);

            result = FuzzyMatch("abc", "axxxbxxxcxxxx");
            Assert.AreEqual(result.Item1, true);
        }

        [TestMethod]
        public void ScoreTest()
        {
            Tuple<bool, int> result;

            // basic test
            result = FuzzyMatch("a", "a");
            Assert.AreEqual(0, result.Item2);

            // 'b' is a consecutive match
            result = FuzzyMatch("ab", "ab");
            Assert.AreEqual(consecutive_match_score, result.Item2);

            // 'b' and 'c' are consecutive matches
            result = FuzzyMatch("abc", "abc");
            Assert.AreEqual(consecutive_match_score * 2, result.Item2);

            // 'b' is the only consecutive match with one 'x' unmatched letter
            result = FuzzyMatch("abc", "abxc");
            Assert.AreEqual(consecutive_match_score * 1 + unmatched_letter_score, result.Item2);

            // 'b' matches after a separator '_' which worths more.
            // the separator '_' is not matched
            result = FuzzyMatch("ab", "a_b");
            Assert.AreEqual(prev_separator_score + unmatched_letter_score, result.Item2);

            // 'xx' are unmatched leading characters and also unmatched regular characters
            result = FuzzyMatch("ab", "xx");
            Assert.AreEqual(unmatched_letter_score * 2 + unmatched_leading_letters_score * 2, result.Item2);

            // 'x', 'y', 'z', 'p' are unmatched regular characters
            // and also 'xyz' are unmatched leading characters
            result = FuzzyMatch("ab", "xyzapb");
            Assert.AreEqual(unmatched_letter_score * 4 + unmatched_leading_letters_score * 3, result.Item2);

            // no match at all. 'xyzpq' are unmatched regular characters
            // and also 'xyz' are unmatched leading characters (capped to max_unmatch_leading_letters)
            result = FuzzyMatch("ab", "xyzpq");
            Assert.AreEqual(max_unmatch_leading_letters, 3); // we assume in this test that max unmatch leading characters is 3
            Assert.AreEqual(unmatched_letter_score * 5 + unmatched_leading_letters_score * 3, result.Item2);
        }
    }
}
