using System;
using System.Linq;

namespace TestUtility
{
    public static class RandomNameGenerator
    {
        private static readonly Random s_rand = new Random();
        private const string AllowedChars = "abcdefghijklmnopqrstuvwxyz0123456789";

        /// <summary>
        /// Generates a random Grafana name with the pattern: ut-{17 random chars}
        /// Total length: 20 characters
        /// Characters used: a-z and 0-9
        /// </summary>
        /// <returns>A 20-character random name starting with 'ut-'</returns>
        public static string GenerateGrafanaName()
        {
            var random = new Random();
            var randomSuffix = new string(Enumerable.Repeat(AllowedChars, 17)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return "ut-" + randomSuffix;
        }

        /// <summary>
        /// Generates a random name with specified prefix and total length
        /// </summary>
        /// <param name="prefix">The prefix for the name</param>
        /// <param name="totalLength">Total length of the generated name</param>
        /// <returns>A random name with the specified prefix and length</returns>
        public static string GenerateRandomName(string prefix, int totalLength)
        {
            Ensure.ArgumentNotNull(prefix, nameof(prefix));

            if(prefix.Length >= totalLength)
            {
                throw new ArgumentException($"Prefix length ({prefix.Length}) must be less than total length ({totalLength}).", nameof(totalLength));
            }

            var suffixLength = totalLength - prefix.Length;

            var randomSuffix = new string(Enumerable.Repeat(AllowedChars, suffixLength)
                .Select(s => s[s_rand.Next(s.Length)]).ToArray());

            return prefix + randomSuffix;
        }
    }
}
