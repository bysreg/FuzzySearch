using System;

namespace FuzzySearch
{
    public static class Algorithm
    {
        public const int consecutive_match_score = 5;
        public const int prev_separator_score = 10;
        public const int unmatched_leading_letters_score = -3;
        public const int max_unmatch_leading_letters = 3; // this is character count
        public const int unmatched_letter_score = -1;

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

        // First element of the tuple would be true if search "matches" the str string
        // Second element of the tuple would be the score of the "matchness"
        public static Tuple<bool, int> FuzzyMatch(string search, string str)
        {
            int search_i = 0, search_len = search.Length;
            int str_i = 0, str_len = str.Length;

            bool prev_matched = false;
            int first_str_match = -1;

            int score = 0;

            if (search_len > str_len)
                return Tuple.Create(false, score);

            while (search_i < search_len && str_i < str_len)
            {
                if (search[search_i] == Char.ToLower(str[str_i]))
                {
                    // the character matches!

                    ++search_i;

                    // score : consecutive matches worth more
                    if (prev_matched)
                        score += consecutive_match_score;

                    prev_matched = true;

                    if (first_str_match == -1)
                        first_str_match = str_i;

                    // score : if the previous character is separator
                    // then it's worth more
                    if (str_i > 0 && IsSeparator(str[str_i - 1]))
                        score += prev_separator_score;
                }
                else
                {
                    prev_matched = false;
                }
                ++str_i;
            }

            // score : unmatched letter scores negatively
            score += (str_len - search_i) * unmatched_letter_score;

            // score : unmatched leading letter affect negatively (capped to 'max_unmatch_leading_letters' leading letters)
            if (first_str_match != -1)
                score += Math.Min(first_str_match, max_unmatch_leading_letters) * unmatched_leading_letters_score;
            else
                score += Math.Min(str_len, max_unmatch_leading_letters) * unmatched_leading_letters_score;

            return Tuple.Create(search_i == search_len, score);
        }
    }

}