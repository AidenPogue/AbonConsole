using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Terasievert.AbonConsole
{
    /// <summary>
    /// Methods for formatting and logging tabular data
    /// </summary>
    public static class TabularLogTools
    {
        /// <summary>
        /// Calls TabularLogTools.Format() and logs each resulting row.
        /// </summary>
        public static void Log(IList<IList<string>> rows, string separator)
        {
            var logs = Format(rows, separator);
            for (int i = 0; i < logs.Count; i++)
            {
                Debug.Log(logs[i]);
            }
        }

        /// <summary>
        /// Returns a list of strings where each element is constructed from the corresponding row in <paramref name="rows"/>.
        /// The columns in each row are separated by <paramref name="separator"/>, and all the separators are aligned, assuming a monospace font is used.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="rows"/> is null</exception>
        public static IList<string> Format(IList<IList<string>> rows, string separator)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            separator ??= " ";

            List<string> result = new List<string>(rows.Count);
            List<int> maxColumnWidths = new List<int>(5);

            //Find max column count and max column widths
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row is null)
                {
                    continue;
                }

                if (maxColumnWidths.Count < row.Count)
                {
                    var currentCount = maxColumnWidths.Count;
                    for (int j = 0; j < row.Count - currentCount; j++)
                    {
                        maxColumnWidths.Add(0);
                    }
                }

                for (int c = 0; c < row.Count; c++)
                {
                    maxColumnWidths[c] = Mathf.Max(maxColumnWidths[c], row[c]?.Length ?? 0);
                }
            }

            var sb = new StringBuilder(100);

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];

                for (int c = 0; c < maxColumnWidths.Count; c++)
                {
                    //TODO: Right/center align?
                    var str = row[c] ?? string.Empty;
                    var pad = maxColumnWidths[c] - str.Length;
                    sb.Append(str);
                    sb.Append(' ', pad);
                    
                    if (c != maxColumnWidths.Count - 1)
                    {
                        sb.Append(separator);
                    }
                }

                result.Add(sb.ToString());
                sb.Clear();
            }

            return result;
        }
    }
}
