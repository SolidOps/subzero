using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SolidOps.SubZero
{
    public static class TextHelper
    {
        public static string GenerateSlug(string name)
        {
            var pascalCase = ConvertToPascalCase(name);
            string convertedName = string.Empty;
            bool previousLettreIsUpperCase = false;
            for (var i = 0; i < pascalCase.Length; i++)
            {
                var letter = pascalCase[i].ToString();
                string nextLetter = default;
                if (i + 1 < pascalCase.Length)
                {
                    nextLetter = pascalCase[i + 1].ToString();
                }
                if (letter != letter.ToLower())
                {
                    if (!previousLettreIsUpperCase)
                    {
                        if (convertedName == string.Empty)
                            convertedName = letter.ToLower();
                        else
                            convertedName += "-" + letter.ToLower();
                    }
                    else
                    {
                        if (nextLetter != default)
                        {
                            if (Char.IsLetter(nextLetter[0]) && nextLetter == nextLetter.ToLower())
                            {
                                convertedName += "-" + letter.ToLower();
                            }
                            else
                            {
                                convertedName += letter.ToLower();
                            }
                        }
                        else
                        {
                            convertedName += letter.ToLower();
                        }
                    }
                    previousLettreIsUpperCase = true;
                }
                else
                {
                    convertedName += letter.ToLower();
                    previousLettreIsUpperCase = false;
                }
            }

            return convertedName;
        }

        public static string ConvertToPascalCase(string text)
        {
            if (text == null)
                return null;

            if (text.Length > 0)
            {
                string returnedString = string.Empty;
                foreach (var part in text.Split(new char[] { '|' }))
                {
                    if (!string.IsNullOrEmpty(returnedString))
                    {
                        returnedString += "|";
                    }
                    string[] arr = part.Split(new char[] { '_' });

                    for (int i = 0; i < arr.Length; i++)
                    {
                        returnedString += arr[i].Substring(0, 1).ToUpper(CultureInfo.InvariantCulture) + arr[i].Substring(1);
                    }
                }
                return returnedString;
            }
            else
                return string.Empty;
        }

        //private static string SeparateWords(string text)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    foreach (char c in text)
        //    {
        //        if (Char.IsUpper(c))
        //        {
        //            sb.Append(" ");
        //        }
        //        sb.Append(c);
        //    }

        //    if (text != null && text.Length > 0 && Char.IsUpper(text[0]))
        //    {
        //        sb.Remove(0, 1);
        //    }

        //    String result = sb.ToString();
        //    return result;
        //}

        public static Tuple<int, int> Contains(string[] words, string searchedText)
        {
            var searchedWords = searchedText.Split(searchedText.Where(c => !Char.IsLetterOrDigit(c)).Distinct().ToArray(), StringSplitOptions.RemoveEmptyEntries);

            int firstIndex = -1;
            for (var i = 0; i < words.Length; i++)
            {
                if (RemoveDiacritics(words[i]) == RemoveDiacritics(searchedWords[0]))
                {
                    firstIndex = i;
                    break;
                }
            }
            if (firstIndex < 0)
            {
                return new Tuple<int, int>(-1, -1);
            }

            for (var ii = 1; ii < searchedWords.Length; ii++)
            {
                if (words.Length <= ii + firstIndex)
                {
                    return new Tuple<int, int>(-1, -1);
                }
                if (RemoveDiacritics(searchedWords[ii]) != RemoveDiacritics(words[ii + firstIndex]))
                {
                    return new Tuple<int, int>(-1, -1);
                }
            }
            return new Tuple<int, int>(firstIndex, searchedWords.Length);
        }

        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD).ToLower();
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
