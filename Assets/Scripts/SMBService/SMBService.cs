using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Project.Networking.SMB {

	/// <summary>
	/// Static utility class designed for easily invoking methods related to SMB file sharing. 
	/// It dynamically switches dependency libraries based on the platform.
	/// </summary>
	public static class SMBService {
		private static INativeSMBService _client;

		static SMBService() {
#if UNITY_IOS
    		// iOS用の処理
            _client = new iOS.iOSSMBService();   // 改修版
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			// Windows用の処理（Editor含む）
			_client = new Windows.WindowsSMBService();
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    		// Mac用の処理（Editor含む）
#else
    		// その他
#endif
			SetDefaultConfig();
		}


		/// ----------------------------------------------------------------------------
		// Public Method

		public static void SetConfig(SMBConnectionInfo info) {
			if (info == null)
				throw new ArgumentNullException(nameof(info));

			// Call native method
			_client.SetConfig(info);
		}

		public static Task<string[]> GetRemoteFileNames(string directoryPath,
			CancellationToken cancellationToken = default) {
			if (directoryPath == null)
				throw new ArgumentNullException(nameof(directoryPath));

			// Call native method
			return _client.GetRemoteFileNames(directoryPath);
		}

		public static Task DownloadFile(string remoteFilePath, string localFilePath, bool overwrite = true,
			int maxRetryCount = 3, TimeSpan? retryDelay = null,
			CancellationToken cancellationToken = default) {
			if (string.IsNullOrWhiteSpace(remoteFilePath))
				throw new ArgumentException("Remote file path cannot be null or empty.", nameof(remoteFilePath));
			if (string.IsNullOrWhiteSpace(localFilePath))
				throw new ArgumentException("Local file path cannot be null or empty.", nameof(localFilePath));

			if (remoteFilePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
				throw new ArgumentException("Remote file path contains invalid characters.", nameof(remoteFilePath));
			if (localFilePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
				throw new ArgumentException("Local file path contains invalid characters.", nameof(localFilePath));

			if (File.Exists(localFilePath)) {
				if (!overwrite)
					throw new IOException($"The file '{localFilePath}' already exists.");
				File.Delete(localFilePath);
			}

			return RetryHelper.RetryAsync(
				operation: () => _client.DownloadFile(remoteFilePath, localFilePath),
				maxRetryCount: maxRetryCount,
				retryDelay: retryDelay,
				retryCondition: ex => ex is SMBException, // SMBException のみリトライ
				cancellationToken: cancellationToken
			);
		}

		public static Task UploadFile(string localFilePath, string remoteFilePath, bool overwrite = true,
			int maxRetryCount = 3, TimeSpan? retryDelay = null,
			CancellationToken cancellationToken = default) {
			if (string.IsNullOrWhiteSpace(remoteFilePath))
				throw new ArgumentException("Remote file path cannot be null or empty.", nameof(remoteFilePath));
			if (string.IsNullOrWhiteSpace(localFilePath))
				throw new ArgumentException("Local file path cannot be null or empty.", nameof(localFilePath));
			if (!File.Exists(localFilePath))
				throw new FileNotFoundException($"Local file '{localFilePath}' not found.", localFilePath);

			return RetryHelper.RetryAsync(
				operation: () => _client.UploadFile(localFilePath, remoteFilePath),
				maxRetryCount: maxRetryCount,
				retryDelay: retryDelay,
				retryCondition: ex => ex is SMBException,
				cancellationToken: cancellationToken
			);
		}

		public static Task DeleteRemoteFile(string remoteFilePath,
			int maxRetryCount = 3, TimeSpan? retryDelay = null,
			CancellationToken cancellationToken = default) {
			if (string.IsNullOrWhiteSpace(remoteFilePath))
				throw new ArgumentException("Remote file path cannot be null or empty.", nameof(remoteFilePath));

			return RetryHelper.RetryAsync(
				operation: () => _client.DeleteRemoteFile(remoteFilePath),
				maxRetryCount: maxRetryCount,
				retryDelay: retryDelay,
				retryCondition: ex => ex is SMBException,
				cancellationToken: cancellationToken
			);
		}


		/// ----------------------------------------------------------------------------
		// Debug

		private static void SetDefaultConfig() {
			var config = new SMBConnectionInfo(
				ipAddress: IPAddress.Parse("192.168.1.100"),
				username: "username",
				password: "password",
				shareName: "WORK");
			SetConfig(config);
		}
		
	}


	public static class RetryHelper {
		public static async Task RetryAsync(
			Func<Task> operation,
			int maxRetryCount = 3,
			TimeSpan? retryDelay = null,
			Func<Exception, bool>? retryCondition = null,
			CancellationToken cancellationToken = default) {
			maxRetryCount = Math.Min(maxRetryCount, 5);
			retryDelay ??= TimeSpan.FromMilliseconds(500);

			for (int attempt = 1; ; attempt++) {
				try {
					await operation();
					return;
				}
				catch (Exception ex) when (attempt <= maxRetryCount && (retryCondition?.Invoke(ex) ?? true)) {
					Debug.LogWarning($"Retry attempt {attempt} failed: {ex.Message}");
					await Task.Delay(retryDelay.Value, cancellationToken: cancellationToken);
				}
				catch {
					throw;
				}
			}
		}
	}

}

