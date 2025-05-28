using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace NativePlugin.Utils
{
    internal sealed class AsyncOperationHandle : AsyncOperationHandleBase
    {
        #region Callback Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnSuccessCallback(
            [MarshalAs(UnmanagedType.I4)] Int32 instanceId
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnErrorCallback(
            [MarshalAs(UnmanagedType.I4)] Int32 instanceId,
            [MarshalAs(UnmanagedType.I4)] Int32 errorCode,
            [MarshalAs(UnmanagedType.LPStr), In] string errorMessage
        );
        #endregion


        private TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Task => _tcs.Task;


        /// ----------------------------------------------------------------------------

        public AsyncOperationHandle(int id) : base(id) { }

        internal (IntPtr successCallbackPtr, IntPtr errorCallbackPtr) GetCallbackPointers()
        {
            lock (_syncRoot)
            {
                if (Status is not AsyncOperationStatus.Stop)
                    throw new InvalidOperationException("Handle already started or completed.");

                Status = AsyncOperationStatus.Running;

                // Callback
                _completionCallback = new OnSuccessCallback(StaticCompletionCallback);
                _errorCallback = new OnErrorCallback(StaticFailedCallback);

                try
                {
                    _completionCallbackHandle = GCHandle.Alloc(_completionCallback);
                    _errorCallbackHandle = GCHandle.Alloc(_errorCallback);

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

        private void SetCompletion()
        {
            lock (base._syncRoot)    
            {
                if (Status is AsyncOperationStatus.Running)
                {
                    Status = AsyncOperationStatus.Succeeded;
                    _tcs.TrySetResult(true);
                }
                Dispose();
            }
        }

        private void SetException(int errorCode, string errorMessage)
        {
            lock (base._syncRoot)
            {
                if (Status is AsyncOperationStatus.Running)
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
            lock (base._syncRoot)
            {
                if (Status.IsDone())
                    return;

                Status = AsyncOperationStatus.Canceled;
                FreeCallbackHandles();
                _tcs.SetCanceled();
                Dispose();
            }
        }

        /// ----------------------------------------------------------------------------
        #region Static

        internal static AsyncOperationHandle CreateHandle()
        {
            return AsyncOperationHandleBase.CreateHandle<AsyncOperationHandle>();
        }

        // [NOTE]
        //  - Native code can only managed static method in AOT platform. (Can not instance method)
        //  - MonoPInvoke attribute must be set

        [AOT.MonoPInvokeCallback(typeof(OnSuccessCallback))]
        internal static void StaticCompletionCallback(int id)
        {
            var handle = GetHandle<AsyncOperationHandle>(id);
            handle.SetCompletion();
        }

        [AOT.MonoPInvokeCallback(typeof(OnErrorCallback))]
        internal static void StaticFailedCallback(int id, int errorCode, string errorMessage)
        {
            var handle = GetHandle<AsyncOperationHandle>(id);
            handle.SetException(errorCode, errorMessage);
        }
        #endregion
    }


    

}
