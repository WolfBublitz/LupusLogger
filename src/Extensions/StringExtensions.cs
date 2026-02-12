using System;
using System.Collections.Generic;

namespace WB.Logging;

internal static class StringExtensions
{
    private const string Space = " ";

    public static IEnumerable<string> WrapLines(this string @this, int width)
    {
        if (string.IsNullOrEmpty(@this) || width < 1)
        {
            yield break;
        }

        if (width <= 0)
        {
            yield return @this;
            yield break;
        }

        // Normalize newlines and split paragraphs
        string normalized = @this.Replace("\r\n", Environment.NewLine);
        string[] paragraphs = normalized.Split(Environment.NewLine);

        for (int p = 0; p < paragraphs.Length; p++)
        {
            string paragraph = paragraphs[p].Trim();

            // Empty paragraph → blank line
            if (paragraph.Length == 0)
            {
                yield return string.Empty;
                continue;
            }

            // Split into words
            string[] words = paragraph.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            List<string> line = [];
            int lineLength = 0;

            foreach (string word in words)
            {
                int wordLength = word.Length;

                // If the word itself is longer than width → hard wrap it
                if (wordLength > width)
                {
                    // Flush current line first
                    if (line.Count > 0)
                    {
                        yield return string.Join(Space, line);

                        line.Clear();
                        lineLength = 0;
                    }

                    int index = 0;

                    while (index < wordLength)
                    {
                        int take = Math.Min(width, wordLength - index);

                        yield return word.Substring(index, take);

                        index += take;
                    }

                    continue;
                }

                // Normal wrapping
                if (lineLength == 0)
                {
                    line.Add(word);
                    lineLength = wordLength;
                }
                else if (lineLength + 1 + wordLength > width)
                {
                    yield return string.Join(Space, line);
                    line.Clear();
                    line.Add(word);
                    lineLength = wordLength;
                }
                else
                {
                    line.Add(word);
                    lineLength += 1 + wordLength;
                }
            }

            // Emit final line of paragraph
            if (line.Count > 0)
            {
                yield return string.Join(Space, line);
            }
        }
    }
}