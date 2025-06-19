using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TestUtility
{
    public static class StringExtensions
    {
        public static bool StrictEquals(this string self, string input)
            => string.Equals(self, input, StringComparison.Ordinal);

        public static bool OrdinalEquals(this string self, string input)
            => string.Equals(self, input, StringComparison.OrdinalIgnoreCase);

        public static bool OrdinalContains(this string self, string value)
        {
            Ensure.ArgumentNotNull(self, nameof(self));

            return self.OrdinalIndexOf(value) != -1;
        }

        public static bool OrdinalStartsWith(this string self, string value)
        {
            Ensure.ArgumentNotNull(self, nameof(self));

            return self.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        public static bool OrdinalEndsWith(this string self, string value)
        {
            Ensure.ArgumentNotNull(self, nameof(self));

            return self.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        public static int OrdinalIndexOf(this string self, string value)
        {
            Ensure.ArgumentNotNull(self, nameof(self));

            return self.IndexOf(value, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsBase64(this string base64String)
        {
            if (string.IsNullOrEmpty(base64String)
                || base64String.Length % 4 != 0
                || base64String.Contains("\"")
                || base64String.Contains(" ")
                || base64String.Contains("\t")
                || base64String.Contains("\r")
                || base64String.Contains("\n"))
            {
                return false;
            }

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ToBase64(this string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        public static string FromBase64(this string input)
        {
            var bytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string ToUrlSafeBase64(this string input)
        {
            var b64Str = input.ToBase64();

            b64Str = b64Str.Split('=')[0];      // Remove any trailing '='s
            b64Str = b64Str.Replace('+', '-');  // 62nd char of encoding
            b64Str = b64Str.Replace('/', '_');  // 63rd char of encoding

            return b64Str;
        }

        public static string FromUrlSafeBase64(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            var b64Str = input.Replace('-', '+');   // 62nd char of encoding
            b64Str = b64Str.Replace('_', '/');      // 63rd char of encoding

            // Pad with trailing '='s
            switch (b64Str.Length % 4)
            {
                case 0: break;                      // No pad chars in this case
                case 2: b64Str += "=="; break;      // Two pad chars
                case 3: b64Str += "="; break;       // One pad char
                default: throw new InvalidOperationException("Illegal base64url string!");
            }

            return b64Str.FromBase64();
        }

        public static string NormalizedAzRegion(this string input)
            => input?.Replace(" ", string.Empty)?.ToLowerInvariant(); // https://github.com/Azure/azure-libraries-for-net/blob/f5298f4f9c257dcf113b76ac86bcad25f050af8b/src/ResourceManagement/ResourceManager/Region.cs#L135

        public static string RemoveWhitespace(this string input)
        {
            Ensure.ArgumentNotNull(input, nameof(input));

            return new string(input.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }

        /// <summary>
        /// Get string in hexadecimal format from bytes array
        /// </summary>
        /// <param name="input">Byte array</param>
        /// <returns>Hexadecimal string representation</returns>
        public static string GetStringFromBytes(byte[] input)
        {
            Ensure.ArgumentNotNull(input, nameof(input));

            // Merge all bytes into a string of bytes
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                builder.Append(input[i].ToString("X", CultureInfo.InvariantCulture));
            }

            var output = builder.ToString();
            return output;
        }

        public static int OrdinalSubstringCount(this string text, string pattern)
        {
            Ensure.ArgumentNotNull(text, nameof(text));
            if (string.IsNullOrEmpty(pattern))
            {
                return 0;
            }

            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                i += pattern.Length;
                count++;
            }

            return count;
        }
    }
}
