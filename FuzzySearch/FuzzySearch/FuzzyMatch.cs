using System;
using System.Collections.Generic;

namespace FuzzySearch
{
    public static class Algorithm
    {
        public const int consecutive_match_score = 5;
        public const int prev_separator_score = 20;
        public const int unmatched_leading_letters_score = -3;
        public const int max_unmatch_leading_letters = 3; // this is character count
        public const int unmatched_letter_score = -1;
        public const int camel_case_score = 20;

        private static bool IsSeparator(char c)
        {
            switch (c)
            {
                case ' ':
                case '_':
                case '.':
                case '-':
                    return true;
            }

            return false;
        }

        private struct SearchInfo
        {
            public int search_i;
            public int str_i;
            public int score;

            public SearchInfo(int search_i, int str_i, int score)
            {
                this.search_i = search_i;
                this.str_i = str_i;
                this.score = score;
            }
        }

        private struct SearchResult
        {
            public int str_match_i;
            public int score;

            public SearchResult(int str_match_i, int score)
            {
                this.str_match_i = str_match_i;
                this.score = score;
            }
        }

        public static Tuple<bool, int> ExhaustiveFuzzyMatch(string search, string str)
        {
            int search_len = search.Length;
            int str_len = str.Length;

            if (search_len == 0 || str_len == 0)
                return Tuple.Create(false, 0);

            Stack<SearchInfo> stack = new Stack<SearchInfo>();
            stack.Push(new SearchInfo(0, 0, 0));
            bool matched_before = false;
            int max_score = 0;

            while (stack.Count != 0)
            {
                SearchInfo si = stack.Pop();

                SearchResult result = ExhaustiveFuzzyMatchIter(search, si.search_i, str, si.str_i, si.str_i - 1);
                if (result.str_match_i != -1)
                {
                    if (si.search_i == search_len - 1)
                    {
                        // only do the score comparison if we just matched the last character from the search string
                        if (matched_before)
                        {
                            max_score = Math.Max(max_score, si.score + result.score);
                        }
                        else
                        {
                            matched_before = true;
                            max_score = si.score + result.score;
                        }
                    }

                    if (result.str_match_i + 1 < str_len)
                    {
                        if (si.search_i + 1 < search_len)
                        {
                            stack.Push(new SearchInfo(si.search_i + 1, result.str_match_i + 1, si.score + result.score));
                        }
                        stack.Push(new SearchInfo(si.search_i, result.str_match_i + 1, si.score));
                    }
                }

                // if this is the last comparison, 
            }

            return Tuple.Create(matched_before, max_score);
        }

        // First element of the tuple is the index of the first search_i-th character match, it'd be -1 if it doesn't find match
        // Second element of the tuple would be the score of the "matchness"
        // This function searches for ONE match of character search[search_start] in str[str_start:str_len - 1] range
        // This function is meant to be called multiple times by ExhaustiveFuzzyMatch
        private static SearchResult ExhaustiveFuzzyMatchIter(string search, int search_start, string str, int str_start, int prev_match_i)
        {
            int search_i = search_start;
            int str_i = str_start;
            int search_len = search.Length;
            int str_len = str.Length;

            int first_str_match = -1;

            int score = 0;

            if (search_len > str_len)
                return new SearchResult(-1, score);

            while (search_i < search_len && str_i < str_len)
            {
                if (search[search_i] == Char.ToLower(str[str_i]))
                {
                    // the character matches!

                    ++search_i;

                    // score : consecutive matches worth more
                    if (prev_match_i != -1 && prev_match_i == str_i - 1)
                        score += consecutive_match_score;

                    if (first_str_match == -1)
                        first_str_match = str_i;

                    // score : if the previous character is separator
                    // then it's worth more
                    if (str_i > 0 && IsSeparator(str[str_i - 1]))
                        score += prev_separator_score;

                    // score : if the previous character is lowercase
                    // and the current matched character case is uppercase
                    // then it's worth more
                    if (str_i > 0 && Char.IsUpper(str[str_i]) && Char.IsLower(str[str_i - 1]))
                        score += camel_case_score;

                    break; //  as soon as we find a match, then we break out of the loop
                }

                ++str_i;
            }

            // score : unmatched letter scores negatively
            // only add this score if this is the last character to match 
            if (search_start == search_len - 1)
                score += (str_len - search_i) * unmatched_letter_score;

            // score : unmatched leading letter affect negatively (capped to 'max_unmatch_leading_letters' leading letters)
            // only add this score for the first character match
            if (prev_match_i == -1)
                if (first_str_match != -1)
                    score += Math.Min(first_str_match, max_unmatch_leading_letters) * unmatched_leading_letters_score;
                else
                    score += Math.Min(str_len, max_unmatch_leading_letters) * unmatched_leading_letters_score;

            return new SearchResult(first_str_match, score);
        }
    }

}