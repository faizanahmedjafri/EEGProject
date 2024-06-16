using System;
using System.Collections.Generic;

namespace SharedCore.Models
{
    public abstract class BaseDisposable : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BaseDisposable()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                DisposeManagedObjects();
                // free other managed objects that implement
                // IDisposable only
            }

            DisposeUnmanagedObjects();

            _disposed = true;
        }

        protected abstract void DisposeManagedObjects();

        protected virtual void DisposeUnmanagedObjects()
        {
            // do nothing
        }

        public static void SafeDispose(IDisposable disposable)
        {
            disposable?.Dispose();
        }

        public static void SafeDisposeObject(object maybeDisposable)
        {
            SafeDispose(maybeDisposable as IDisposable);
        }

        public static void SafeDispose<T>(IEnumerable<T> disposables) where T : IDisposable
        {
            if (disposables != null)
            {
                foreach (var disposable in disposables)
                {
                    SafeDispose(disposable);
                }
            }
        }
    }
}