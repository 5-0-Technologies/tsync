using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace tSync
{
    public static class Helper
    {
        /// <summary>
        /// Converts Spin timestamp into Twinzo
        /// </summary>
        /// <param name="TimeStampMobile">Spin integer seconds</param>
        /// <returns>Twinzo long miliseconds</returns>
        public static long ParseTimeStamp(int TimeStampMobile)
        {
            return (long)TimeStampMobile * 1000;
        }

        /// <summary>
        /// Deques multiple records from concurrent queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<T> DequeueChunk<T>(this ConcurrentQueue<T> queue, int chunkSize)
        {
            for (int i = 0; i < chunkSize && queue.Count > 0; i++)
            {
                T result;
                queue.TryDequeue(out result);
                yield return result;
            }
        }

        public static string RemoveSpecCharacters(string input)
        {
            if (input == null) return string.Empty;
            input = RemoveAccents(input);
            input = Regex.Replace(input, @"[^0-9a-zA-Z:.\-_]+", "_")
                .ToLowerInvariant();

            // Remove dot due to Twinzo API restrictions
            // Cannot parse url parameter with dot at the end
            if (input.EndsWith("."))
            {
                input = input.TrimEnd('.');
            }

            return input;
        }

        public static string RemoveAccents(string input)
        {
            string normalized = input.Normalize(NormalizationForm.FormKD);
            Encoding removal = Encoding.GetEncoding(Encoding.ASCII.CodePage,
                                                    new EncoderReplacementFallback(""),
                                                    new DecoderReplacementFallback(""));
            byte[] bytes = removal.GetBytes(normalized);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
