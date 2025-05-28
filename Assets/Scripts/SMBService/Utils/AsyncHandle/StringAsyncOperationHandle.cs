using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace NativePlugin.Utils
{
    
    /// <summary>
    /// string 型結果を返す非同期ハンドル。
    /// </summary>
    internal sealed class StringAsyncOperationHandle : AsyncOperationHandleBase
    {
        #region Callback Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnSuccessCallback(
            [MarshalAs(UnmanagedType.I4)] int instanceId,
            [MarshalAs(UnmanagedType.LPStr), In] string result
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnErrorCallback(
            [MarshalAs(UnmanagedType.I4)] int instanceId,
            [MarshalAs(UnmanagedType.I4)] int errorCode,
            [MarshalAs(UnmanagedType.LPStr), In] string errorMessage
        );
        #endregion

        private readonly TaskCompletionSource<string> _tcs =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// 完了時に返されるタスク
        /// </summary>
        public Task<string> Task => _tcs.Task;

        public StringAsyncOperationHandle(int id) : base(id) { }

        /// <summary>
        /// ネイティブ呼び出し用のコールバック関数ポインターを取得します。
        /// </summary>
        internal (IntPtr successCallbackPtr, IntPtr errorCallbackPtr) GetCallbackPointers()
        {
            lock (_syncRoot)
            {
                if (Status != AsyncOperationStatus.Stop)
                    throw new InvalidOperationException("Handle already started or completed.");

                Status = AsyncOperationStatus.Running;

                _completionCallback = new OnSuccessCallback(StaticSuccessCallback);
                _errorCallback      = new OnErrorCallback(StaticErrorCallback);

                try
                {
                    _completionCallbackHandle = GCHandle.Alloc(_completionCallback);
                    _errorCallbackHandle      = GCHandle.Alloc(_errorCallback);

                    var sPtr = Marshal.GetFunctionPointerForDelegate(_completionCallback);
                    var ePtr = Marshal.GetFunctionPointerForDelegate(_errorCallback);
                    return (sPtr, ePtr);
                }
                catch
                {
                    Cancel();
                    throw;
                }
            }
        }

        private void SetSuccess(string result)
        {
            lock (_syncRoot)
            {
                if (Status == AsyncOperationStatus.Running)
                {
                    Status = AsyncOperationStatus.Succeeded;
                    _tcs.TrySetResult(result);
                }
                Dispose();
            }
        }

        private void SetError(int errorCode, string errorMessage)
        {
            lock (_syncRoot)
            {
                if (Status == AsyncOperationStatus.Running)
                {
                    Status = AsyncOperationStatus.Failed;
                    OperationException = new InvalidOperationException(errorMessage);
                    _tcs.SetException(OperationException);
                }
                Dispose();
            }
        }

        private void Cancel()
        {
            lock (_syncRoot)
            {
                if (Status.IsDone())
                    return;

                Status = AsyncOperationStatus.Canceled;
                FreeCallbackHandles();
                _tcs.SetCanceled();
                Dispose();
            }
        }

        #region Static

        internal static StringAsyncOperationHandle CreateHandle()
        {
            return AsyncOperationHandleBase.CreateHandle<StringAsyncOperationHandle>();
        }

        [AOT.MonoPInvokeCallback(typeof(OnSuccessCallback))]
        private static void StaticSuccessCallback(int id, string result)
        {
            var handle = GetHandle<StringAsyncOperationHandle>(id);
            handle.SetSuccess(result);
        }

        [AOT.MonoPInvokeCallback(typeof(OnErrorCallback))]
        private static void StaticErrorCallback(int id, int errorCode, string errorMessage)
        {
            var handle = GetHandle<StringAsyncOperationHandle>(id);
            handle.SetError(errorCode, errorMessage);
        }

        #endregion
    }
    
    
}
