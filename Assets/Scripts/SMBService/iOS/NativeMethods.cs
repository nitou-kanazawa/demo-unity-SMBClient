using UnityEngine;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.IO;
using Project.Networking.SMB;
using NativePlugin.Utils;

namespace NativePlugin {
	internal static class NativeMethods {
#if UNITY_IOS && !UNITY_EDITOR
        private const string DLL_NAME = "__Internal";
#else
		private const string DLL_NAME = "SMBWrapper";
#endif

		/// ----------------------------------------------------------------------------
		#region Native Function

		// Get file names
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		private static extern void SwiftPlugin_GetFileNames(
			string configJson,
			string path,
			int instanceId,
			IntPtr onSuccess,
			IntPtr onError
		);

		// Downloads
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		private static extern void SwiftPlugin_DownloadFile(
			string configJson,
			string remotePath,
			string localPath,
			int instanceId,     // instance handle
			IntPtr onSuccess,
			IntPtr onError
		);

		// Upload
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		private static extern void SwiftPlugin_UploadFile(
			string configJson,
			string localPath,
			string remotePath,
			int instanceId,
			IntPtr onSuccess,
			IntPtr onError
		);

		// Delete
		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		private static extern void SwiftPlugin_DeleteFile(
			string configJson,
			string remotePath,
			int instanceId,
			IntPtr onSuccess,
			IntPtr onError
		);
		#endregion


		/// ----------------------------------------------------------------------------
		// Public Method

		public async static Task<string[]> GetFileNames(
			SMBConnectionInfo config,
			string remotePath,
			CancellationToken token = default) {
			string configJson = JsonUtility.ToJson(config);
			using (var handle = StringAsyncOperationHandle.CreateHandle()) {
				var (successPtr, errorPtr) = handle.GetCallbackPointers();
				SwiftPlugin_GetFileNames(
					configJson,
					remotePath,
					handle.ControlId,
					successPtr, errorPtr);

				var result = await handle.Task;
				return result.Split(',');
			}
		}


		public async static Task DownloadFile(
			SMBConnectionInfo config,
			string remotePath, string localPath,
			CancellationToken token = default) {
			string configJson = JsonUtility.ToJson(config);
			using (var handle = AsyncOperationHandle.CreateHandle()) {
				var (successPtr, errorPtr) = handle.GetCallbackPointers();
				SwiftPlugin_DownloadFile(
					configJson,
					remotePath, localPath,
					handle.ControlId,
					successPtr, errorPtr);

				await handle.Task;
				Debug.Log($"Download file: {localPath}");
			}
		}

		public async static Task UploadFile(
			SMBConnectionInfo config,
			string localPath, string remotePath,
			CancellationToken token = default) {
			string configJson = JsonUtility.ToJson(config);
			using (var handle = AsyncOperationHandle.CreateHandle()) {
				var (successPtr, errorPtr) = handle.GetCallbackPointers();
				SwiftPlugin_UploadFile(
					configJson,
					localPath, remotePath,
					handle.ControlId,
					successPtr, errorPtr);

				await handle.Task;
				Debug.Log($"Upload file: {remotePath}");
			}
		}

	}
}
