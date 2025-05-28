using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace NativePlugin.Utils
{

    internal abstract class AsyncOperationHandleBase : IDisposable
    {
        private bool _disposed;
        protected readonly object _syncRoot = new();

        protected Delegate _completionCallback;
        protected Delegate _errorCallback;
        protected GCHandle _completionCallbackHandle;
        protected GCHandle _errorCallbackHandle;


        public int ControlId { get; } = -1;

        public bool IsValid => ControlId >= 0 && !_disposed;

        public AsyncOperationStatus Status { get; protected set; }

        public Exception OperationException { get; protected set; }


        /// ----------------------------------------------------------------------------

        protected AsyncOperationHandleBase(int id)
        {
            ControlId = id;
            Status = AsyncOperationStatus.Stop;
        }

        ~AsyncOperationHandleBase() => Dispose();

        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (_disposed)
                    return;
                _disposed = true;

                Status = AsyncOperationStatus.Canceled;
                FreeCallbackHandles();
                UnRegisterHandle(ControlId);
                GC.SuppressFinalize(this);
            }
        }

        protected void FreeCallbackHandles()
        {
            _completionCallback = null;
            _errorCallback = null;

            if (_completionCallbackHandle.IsAllocated)
                _completionCallbackHandle.Free();
            if (_errorCallbackHandle.IsAllocated)
                _errorCallbackHandle.Free();
        }


        /// ----------------------------------------------------------------------------
        #region Static

        private static int _nextControlId = -1;
        private readonly static ConcurrentDictionary<int, AsyncOperationHandleBase> _handleDict = new();

        /// <summary>
        /// 
        /// </summary>
        public static IReadOnlyDictionary<int, AsyncOperationHandleBase> Handles => _handleDict;

        protected static T CreateHandle<T>() where T : AsyncOperationHandleBase
        {
            int id = GetNextControlId();
            var handle = (T)Activator.CreateInstance(typeof(T), id);
            RegisterHandle(handle);
            return handle;
        }

        protected static int GetNextControlId()
        {
            return Interlocked.Increment(ref _nextControlId);
        }

        private static void RegisterHandle(AsyncOperationHandleBase handle)
        {
            if (!handle.IsValid)
                return;

            _handleDict.TryAdd(handle.ControlId, handle);
        }

        private static void UnRegisterHandle(int id)
        {
            _handleDict.TryRemove(id, out _);
        }

        public static T GetHandle<T>(int id)
            where T : AsyncOperationHandleBase
        {
            return (T)_handleDict[id];
        }
        #endregion
    }

}