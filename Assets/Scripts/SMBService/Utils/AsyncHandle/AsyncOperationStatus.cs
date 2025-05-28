using System;

namespace NativePlugin.Utils
{

    public enum AsyncOperationStatus
    {
        Stop = 0,
        Running = 1,
        Succeeded = 2,
        Failed = 3,
        Canceled = 4,
    }


    public static class NativeOperationStatusExtensions
    {

        /// <summary>
        /// 非同期処理が終了しているかどうか．
        /// </summary>
        public static bool IsDone(this AsyncOperationStatus self) => self is
            AsyncOperationStatus.Succeeded or
            AsyncOperationStatus.Failed or
            AsyncOperationStatus.Canceled;

        /// <summary>
        /// 非同期処理がキャンセル済みかどうか．
        /// </summary>
        public static bool IsCanceled(this AsyncOperationStatus self) =>
            self is AsyncOperationStatus.Canceled;

    }
}
