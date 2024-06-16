using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace biopot.Helpers
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

        /// <summary>
        /// Disposes the element and its binding context, if any is set.
        /// Element's parent is unset too.
        /// </summary>
        /// <param name="element">The element to dispose. It can be <c>null</c>.</param>
        public static void SafeDisposeElement(Element element)
        {
            if (element != null)
            {
                element.Parent = null;

                var bindingContext = element.BindingContext;
                element.BindingContext = null;
                SafeDisposeObject(element);

                if (bindingContext != null)
                {
                    SafeDisposeObject(bindingContext);
                }
            }
        }
    }
}