using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using SharedCore.Models;

namespace SharedCore.Services.Performance
{
    /// <summary>
    /// The performance tracking provider.
    /// </summary>
    public interface IPerformanceProvider
    {
        void Stop(string aReference, string aTag, string aPath, string aMember);

        void Start(string aReference, string aTag, string aPath, string aMember);
    }

    /// <summary>
    /// The static class to measure performance.
    /// </summary>
    public class Performance
    {
        internal static long Reference;

        public static IPerformanceProvider Provider { get; set; }

        public static void Start(out string aReference, string aTag = null, [CallerMemberName] string aMember = null)
        {
            if (Provider == null)
            {
                aReference = string.Empty;
                return;
            }

            aReference = Interlocked.Increment(ref Reference).ToString();
            Provider.Start(aReference, aTag, aMember, null);
        }

        public static void Start(string aReference, string aTag = null, [CallerMemberName] string aMember = null)
        {
            Provider?.Start(aReference, aTag, aMember, null);
        }

        public static void Stop(string aReference, string aTag = null, [CallerMemberName] string aMember = null)
        {
            Provider?.Stop(aReference, aTag, aMember, null);
        }

        public static IDisposable StartNew(string aTag = null, [CallerMemberName] string aMember = null)
        {
            return new DisposablePerformanceReference(aTag, aMember);
        }

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        private class DisposablePerformanceReference : BaseDisposable
        {
            private readonly string iReference;
            private readonly string iTag;
            private readonly string iMember;

            public DisposablePerformanceReference(string aTag, string aMember)
            {
                iTag = aTag;
                iMember = aMember;
                Start(out string reference, iTag, iMember);
                iReference = reference;
            }

            /// <inheritdoc />
            protected override void DisposeManagedObjects()
            {
                Stop(iReference, iTag, iMember);
            }
        }
    }
}