using System;
using System.Runtime.CompilerServices;

namespace SharedCore.Models
{
    /// <summary>
    /// Base async operation result.
    /// Thiss class calculates how it log
    /// from start async operation (create instance) to finish asyncOperation (SetResult)
    /// </summary>
    public class AOResult
    {
        private readonly DateTime _creationUtcTime;

        public AOResult([CallerMemberName]string callerName = null, [CallerFilePath]string callerFile = null, [CallerLineNumber]int callerLineNumber = 0)
        {
            _creationUtcTime = DateTime.UtcNow;
            CallerName = callerName;
            CallerFile = callerFile;
            CallerLineNumber = callerLineNumber;
        }

        #region -- Public properties --

        public TimeSpan OperationTime { get; private set; }

        public bool IsSuccess { get; private set; }

        public Exception Exception { get; private set; }

        public string ErrorId { get; private set; }

        public string Message { get; private set; }

        public string CallerName { get; private set; }

        public string CallerFile { get; private set; }

        public int CallerLineNumber { get; private set; }

        #endregion

        #region -- Public methods --

        public void SetSuccess()
        {
            SetResult(true, null, null, null);
        }

        public void SetFailure()
        {
            SetResult(false, null, null, null);
        }

        public void SetFailure(string message)
        {
            SetResult(false, null, message, null);
        }

        public void ArgumentException(string argumentName, string message)
        {
            SetError("ArgumentException", $"argumentName: {argumentName}, message: {message}");
        }

        public void ArgumentNullException(string argumentName)
        {
            SetError("ArgumentNullException", $"argumentName: {argumentName}");
        }

        public void SetError(string errorId, string message, Exception ex = null)
        {
            SetResult(false, errorId, message, ex);
        }

        public void SetResult(bool isSuccess, string errorId, string message, Exception ex)
        {
            var finishTime = DateTime.UtcNow;
            OperationTime = finishTime - _creationUtcTime;
            IsSuccess = isSuccess;
            ErrorId = errorId;
            Exception = ex;
            Message = message;
        }

        #endregion

    }

    /// <summary>
    /// Async operation result with result value
    /// </summary>
    public class AOResult<T> : AOResult
    {
        public AOResult([CallerMemberNameAttribute]string callerName = null, [CallerFilePath]string callerFile = null, [CallerLineNumber]int callerLineNumber = 0) : base(callerName, callerFile, callerLineNumber)
        {

        }

        #region -- Public properties --

        public T Result { get; private set; }

        #endregion

        #region -- Public methods --

        public void SetSuccess(T result)
        {
            Result = result;
            SetSuccess();
        }

        public void SetResult(T result, bool isSuccess, string errorId, string message, Exception ex = null)
        {
            Result = result;
            SetResult(isSuccess, errorId, message, ex);
        }

        #endregion
    }
}
