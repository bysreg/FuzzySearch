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
            Tuple<bool, int> result = ExhaustiveFuzzyMatch("abc", "abc");
            Assert.AreEqual(result.Item1, true);

            result = ExhaustiveFuzzyMatch("abc", "abcd");
            Assert.AreEqual(result.Item1, true);

            result = ExhaustiveFuzzyMatch("abcde", "abcd");
            Assert.AreEqual(result.Item1, false);

            result = ExhaustiveFuzzyMatch("abc", "axxxbxxxcxxxx");
            Assert.AreEqual(result.Item1, true);
        }

        // the score is only relevant if there's a match
        // if no match is found, then the score may be inaccurate
        [TestMethod]
        public void ScoreTest()
        {
            Tuple<bool, int> result;

            // basic test (regular match letters doesn't affect the score)
            result = ExhaustiveFuzzyMatch("a", "a");
            Assert.AreEqual(result.Item1, true);
            Assert.AreEqual(0, result.Item2);

            // 'b' is a consecutive match
            result = ExhaustiveFuzzyMatch("ab", "ab");
            Assert.AreEqual(result.Item1, true);
            Assert.AreEqual(consecutive_match_score, result.Item2);

            // 'b' and 'c' are consecutive matches
            result = ExhaustiveFuzzyMatch("abc", "abc");
            Assert.AreEqual(result.Item1, true);
            Assert.AreEqual(consecutive_match_score * 2, result.Item2);

            // 'b' is the only consecutive match with one 'x' unmatched letter
            result = ExhaustiveFuzzyMatch("abc", "abxc");
            Assert.AreEqual(result.Item1, true);
            Assert.AreEqual(consecutive_match_score * 1 + unmatched_letter_score, result.Item2);

            // 'b' matches after a separator '_' which worths more.
            // the separator '_' is not matched
            result = ExhaustiveFuzzyMatch("ab", "a_b");
            Assert.AreEqual(result.Item1, true);
            Assert.AreEqual(prev_separator_score + unmatched_letter_score, result.Item2);

            // 'xx' are unmatched leading characters and also unmatched regular characters
            result = ExhaustiveFuzzyMatch("ab", "xxaxb");
            Assert.AreEqual(result.Item1, true);
            Assert.AreEqual(unmatched_letter_score * 3 + unmatched_leading_letters_score * 2, result.Item2);

            // 'x', 'y', 'z', 'p' are unmatched regular characters
            // and also 'xyz' are unmatched leading characters
            result = ExhaustiveFuzzyMatch("ab", "xyzapb");
            Assert.AreEqual(result.Item1, true);
            Assert.AreEqual(unmatched_letter_score * 4 + unmatched_leading_letters_score * 3, result.Item2);

            // no match at all. 'xyzpq' are unmatched regular characters
            // and also 'xyz' are unmatched leading characters (capped to max_unmatch_leading_letters)
            result = ExhaustiveFuzzyMatch("ab", "xyzpqaxb");
            Assert.AreEqual(result.Item1, true);
            Assert.AreEqual(max_unmatch_leading_letters, 3); // we assume in this test that max unmatch leading characters is 3
            Assert.AreEqual(unmatched_letter_score * 6 + unmatched_leading_letters_score * 3, result.Item2);

            // matches 'B' which is camel case (it's right after a lowercase letter 'x')
            // also 'xx' are unmatched regular characters
            result = ExhaustiveFuzzyMatch("ab", "axxB");
            Assert.AreEqual(result.Item1, true);
            Assert.AreEqual(camel_case_score + unmatched_letter_score * 2, result.Item2);
        }

        [TestMethod]
        public void ExhaustiveMatchTest()
        {
            Tuple<bool, int> result;

            // "ab" matches with "*a*xxbxx*B*" (the matches are emphasized by the asterisks)
            // instead of "*a*xx*b*xxB"
            result = ExhaustiveFuzzyMatch("ab", "axxbxxB");
            Assert.AreEqual(result.Item1, true);
            Assert.AreEqual(camel_case_score + unmatched_letter_score * 5, result.Item2);
        }
    }
}
