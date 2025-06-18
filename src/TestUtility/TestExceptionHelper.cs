using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TestUtility
{
    /// <summary>
    /// https://github.com/SimonCropp/XunitContext/blob/master/src/XunitContext/XunitContext.cs
    /// </summary>
    internal static class TestExceptionHelper
    {
        private static AsyncLocal<TestContext> s_local = new AsyncLocal<TestContext>();

        private static bool s_enableExceptionCapture;

        /// <summary>
        /// The <see cref="Exception"/> for the current test if it failed.
        /// </summary>
        public static Exception TestException => s_local.Value?.TestException;

        public static void EnableExceptionCapture()
        {
            if (s_enableExceptionCapture)
            {
                return;
            }

            s_enableExceptionCapture = true;
            AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
            {
                if (s_local.Value == null)
                {
                    return;
                }

                s_local.Value._exception = e.Exception;
            };
        }

        public static TestContext Register([CallerFilePath] string sourceFile = "")
        {
            var existingContext = s_local.Value;

            if (existingContext == null)
            {
                var context = new TestContext(sourceFile);
                s_local.Value = context;
                return context;
            }

            existingContext.SourceFile = sourceFile;
            return existingContext;
        }
    }
}
