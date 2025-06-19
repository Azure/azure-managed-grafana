using System;

namespace TestUtility
{
    public static class Ensure
    {
        /// <summary>
        /// Ensures that the specified argument is not null.
        /// </summary>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="argument">The argument.</param>
        public static void ArgumentNotNull(object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Ensures that the specified argument is not null nor empty nor white space.
        /// </summary>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="argument">The argument.</param>
        /// <exception cref="ArgumentNullException">argument is null</exception>
        /// /// <exception cref="ArgumentException">argument is empty or only has white space</exception>
        public static void StringNotNullNorWhiteSpace(string argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }

            if (argument.Length == 0 || argument.Trim().Length == 0)
            {
                throw new ArgumentException($"Argument {argumentName} is empty or only has white space.", argumentName);
            }
        }
    }
}
