#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using SMBLibrary;
using SMBLibrary.Client;
using NativePlugin.Utils;

// [REF]
//  MSDoc: NtCreateFile function (winternl.h) https://learn.microsoft.com/en-us/windows/win32/api/winternl/nf-winternl-ntcreatefile

// [NOTE]
// CreateFile()はリモートファイル操作を行うためのメソッド
//
// e.g.
//NTStatus openStatus = fileStore.CreateFile(
//    out fileHandle,          // ファイルハンドル (結果が格納される)
//    out fileStatus,          // ファイル状態 (結果が格納される)
//    remoteFilePath,          // サーバー上のファイルパス
//    AccessMask.GENERIC_READ, // アクセスマスク (読み取り権限を要求)
//    FileAttributes.Normal,   // ファイル属性
//    ShareAccess.Read,        // 共有モード (読み取りの共有を許可)
//    CreateDisposition.FILE_OPEN, // ファイルが存在する場合にのみ開く
//    CreateOptions.FILE_NON_DIRECTORY_FILE, // ファイルがディレクトリでないことを指定
//    null                     // 拡張オプション (ここでは指定なし)
//);

// [NOTE]
//  エラー"Not enough credits"が発生する場合、1つのクライアントインスタンスに対して同時に複数のコマンドを実行していないか確認する
//  
//  https://github.com/TalAloni/SMBLibrary/issues/105

namespace Project.Networking.SMB.Windows {

	public sealed class SMBClientWrapper {
		private readonly SMB2Client _client;
		public readonly SMBConnectionInfo _config;

		private const SMBTransportType DefaultTransportType = SMBTransportType.DirectTCPTransport; // port 445
		private const int TimeoutMillSeconds = 500;


		/// ----------------------------------------------------------------------------
		// Public Method

		public SMBClientWrapper(SMBConnectionInfo info) {
			_client = new SMB2Client();
			_config = info;
		}

		public Task<string[]> GetRemoteFileNames(string remoteDirectoryPath) {
			return ExecuteWithConnection(fileStore => {
				NTStatus openStatus = fileStore.CreateFile(
					handle: out var directoryHandle,
					fileStatus: out var fileStatus,
					path: remoteDirectoryPath,
					desiredAccess: AccessMask.GENERIC_READ,
					fileAttributes: FileAttributes.Directory,
					shareAccess: ShareAccess.Read | ShareAccess.Write,
					createDisposition: CreateDisposition.FILE_OPEN,
					createOptions: CreateOptions.FILE_DIRECTORY_FILE,
					securityContext: null);

				if (openStatus != NTStatus.STATUS_SUCCESS) {
					var errorCode = ErrorCode.FileOperation;
					var errorMessage = $"Failed to open remote directory: {openStatus}";
					throw new SMBException(errorCode, errorMessage);
				}

				// Get Files
				var queryStatus = fileStore.QueryDirectory(out var fileList, directoryHandle, "*", FileInformationClass.FileDirectoryInformation);
				var closeStatus = fileStore.CloseFile(directoryHandle);

				return fileList
					.OfType<FileDirectoryInformation>()
					.Select(dirInfo => dirInfo.FileName)
					.Where(fileName => fileName != "." && fileName != "..") // Exclude special directory entries
					.ToArray();
			});
		}

		public Task DownloadFile(string remoteFilePath, string localFilePath) {
			return ExecuteWithConnection(fileStore => {
				if (fileStore is SMB1FileStore) {
					remoteFilePath = @"\\" + remoteFilePath;
				}

				// Open an existing file on the server as read-only
				NTStatus openStatus = fileStore.CreateFile(
					handle: out var fileHandle,
					fileStatus: out var fileStatus,
					path: remoteFilePath,
					desiredAccess: AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE,
					fileAttributes: FileAttributes.Normal,
					shareAccess: ShareAccess.Read,
					createDisposition: CreateDisposition.FILE_OPEN,
					createOptions: CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
					securityContext: null);

				if (openStatus != NTStatus.STATUS_SUCCESS) {
					var errorCode = ErrorCode.FileOperation;
					var errorMessage = $"Failed to open remote file: {openStatus}";
					throw new SMBException(errorCode, errorMessage);
				}

				// Copy file
				try {
					using var fileStream = new System.IO.FileStream(localFilePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
					long bytesRead = 0;
					while (true) {
						var status = fileStore.ReadFile(out byte[] data, fileHandle, bytesRead, (int)_client.MaxReadSize);

						if (status != NTStatus.STATUS_SUCCESS && status != NTStatus.STATUS_END_OF_FILE) {
							var errorCode = ErrorMapper.FromNtStatus(status);
							throw new SMBException(errorCode, $"Failed to read remote file: {status}");
						}
						if (status == NTStatus.STATUS_END_OF_FILE || data.Length == 0)
							break;

						bytesRead += data.Length;
						fileStream.Write(data, 0, data.Length);
					}
				}
				finally {
					fileStore.CloseFile(fileHandle);
				}

				return true;
			});

		}

		public Task UploadFile(string localFilePath, string remoteFilePath) {
			if (!System.IO.File.Exists(localFilePath))
				throw new System.IO.FileNotFoundException($"Local file '{localFilePath}' not found.", localFilePath);

			return ExecuteWithConnection(fileStore => {

				if (fileStore is SMB1FileStore)
					remoteFilePath = @"\\" + remoteFilePath;

				using (var localFileStream = new System.IO.FileStream(localFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
					var desiredAccess = AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE;

					NTStatus createStatus = fileStore.CreateFile(
						handle: out var fileHandle,
						fileStatus: out var fileStatus,
						path: remoteFilePath,
						desiredAccess: desiredAccess,
						fileAttributes: FileAttributes.Normal,
						shareAccess: ShareAccess.Write,
						createDisposition: CreateDisposition.FILE_OVERWRITE_IF,  // 上書き設定に変更
						createOptions: CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
						securityContext: null);

					if (createStatus != NTStatus.STATUS_SUCCESS) {
						var errorCode = ErrorCode.FileOperation;
						throw new SMBException(errorCode, $"Failed to create or overwrite remote file: {createStatus}");
					}

					try {
						int writeOffset = 0;
						NTStatus writeStatus;
						while (localFileStream.Position < localFileStream.Length) {
							byte[] buffer = new byte[(int)_client.MaxWriteSize];
							int bytesRead = localFileStream.Read(buffer, 0, buffer.Length);
							if (bytesRead < (int)_client.MaxWriteSize) {
								Array.Resize<byte>(ref buffer, bytesRead);
							}
							writeStatus = fileStore.WriteFile(out int numberOfBytesWritten, fileHandle, writeOffset, buffer);
							if (writeStatus != NTStatus.STATUS_SUCCESS) {
								var errorCode = ErrorCode.FileOperation;
								throw new SMBException(errorCode, $"Failed to write to file: {writeStatus}");
							}
							writeOffset += bytesRead;
						}
					}
					finally {
						fileStore.CloseFile(fileHandle);
					}
				}

				return true;
			});
		}

		public Task DeleteRemoteFile(string remoteFilePath) {
			return ExecuteWithConnection(fileStore => Process_DeleteRemoteFile(fileStore, remoteFilePath));
		}

		public Task<bool> ExistsFile(string remoteFilePath) {
			return ExecuteWithConnection(fileStore => Process_ExistsRemoteFile(fileStore, remoteFilePath));
		}


		/// ----------------------------------------------------------------------------
		// Private Method

		private bool Process_ExistsRemoteFile(ISMBFileStore fileStore, string remoteFilePath) {
			if (fileStore is SMB1FileStore) {
				remoteFilePath = @"\\" + remoteFilePath;
			}

			// Open file
			NTStatus status = fileStore.CreateFile(
				out var fileHandle,
				out var fileStatus,
				remoteFilePath,
				AccessMask.GENERIC_READ,
				FileAttributes.Normal,
				ShareAccess.Read,
				CreateDisposition.FILE_OPEN, // 存在する場合にのみ開く
				CreateOptions.FILE_NON_DIRECTORY_FILE,
				null);

			// ファイルが存在しなければ失敗ステータスになる
			bool exists = status is NTStatus.STATUS_SUCCESS;
			if (exists) {
				fileStore.CloseFile(fileHandle);
			}

			return exists;
		}

		private bool Process_DeleteRemoteFile(ISMBFileStore fileStore, string remoteFilePath) {
			// [REF] https://github.com/TalAloni/SMBLibrary/blob/master/ClientExamples.md#delete-file

			if (fileStore is SMB1FileStore) {
				remoteFilePath = @"\\" + remoteFilePath;
			}

			// Open file
			NTStatus status = fileStore.CreateFile(
				handle: out var fileHandle,
				fileStatus: out var fileStatus,
				path: remoteFilePath,
				desiredAccess: AccessMask.GENERIC_WRITE | AccessMask.DELETE | AccessMask.SYNCHRONIZE,
				fileAttributes: FileAttributes.Normal,
				shareAccess: ShareAccess.None,
				createDisposition: CreateDisposition.FILE_OPEN,
				createOptions: CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
				securityContext: null);

			if (status is not NTStatus.STATUS_SUCCESS)
				return false;

			var fileDispositionInformation = new FileDispositionInformation { DeletePending = true };
			status = fileStore.SetFileInformation(fileHandle, fileDispositionInformation);
			var isSuccess = (status is NTStatus.STATUS_SUCCESS);

			fileStore.CloseFile(fileHandle);
			return isSuccess;
		}


		/// ----------------------------------------------------------------------------
		#region Under developing
		// [REF] github issue: How to create directory #90 https://github.com/TalAloni/SMBLibrary/issues/90

		/// <summary>
		/// Create directory.
		/// </summary>
		public void CreateDirectory(string remoteDirectoryPath) {
			// [NOTE] The directory can only be created one level at a time. For example, "/_csv/2/in" requires calling the method three times.

			ExecuteWithConnection(fileStore => {
				NTStatus status = fileStore.CreateFile(
					out var directoryHandle,
					out var fileStatus,
					remoteDirectoryPath,
					AccessMask.WRITE_OWNER,
					FileAttributes.Directory,
					ShareAccess.Read | ShareAccess.Write | ShareAccess.Delete,
					CreateDisposition.FILE_CREATE,
					CreateOptions.FILE_DIRECTORY_FILE,
					null);

				if (status != NTStatus.STATUS_SUCCESS) {
					throw new Exception($"Failed to open remote file: {status}");
				}

				return true;
			});
		}
		#endregion


		/// ----------------------------------------------------------------------------
		// Private Method

		private Task<T> ExecuteWithConnection<T>(Func<ISMBFileStore, T> operation) {
			return Task.Run(() => {
				// Connect to Server
				ConnectOrThrow();
				using (ConnectionScope()) {
					// Login
					LoginOrThrow();
					using (LoginScope()) {
						// Connect to `Shared Resource`
						ISMBFileStore fileStore = _client.TreeConnect(_config.shareName, out var connectStatus);    // return resource handle
						if (connectStatus != NTStatus.STATUS_SUCCESS)
							throw new SMBException(ErrorCode.FileOperation, $"Tree connection failed with status: {connectStatus}");

						// Action
						try {
							return operation.Invoke(fileStore);
						}
						finally {
							fileStore.Disconnect();
						}
					}
				}
			});
		}

		private void ConnectOrThrow() {
			var isConnected = _client.Connect(_config.ipAddress, DefaultTransportType, TimeoutMillSeconds);
			if (!isConnected) {
				var errorCode = ErrorCode.Connection;
				var errorMessage = "Failed to connect to SMB server.";
				Debug.LogError(errorMessage);
				throw new SMBException(errorCode, errorMessage);
			}
		}

		private void LoginOrThrow() {
			NTStatus loginStatus = _client.Login(string.Empty, _config.userName, _config.password);
			if (loginStatus != NTStatus.STATUS_SUCCESS) {
				var errorCode = ErrorMapper.FromNtStatus(loginStatus);
				var errorMessage = $"SMB login failed with status: {loginStatus}";
				throw new SMBException(errorCode, errorMessage);
			}
		}

		private IDisposable ConnectionScope() {
			return Disposable.Create(() => {
				_client.Disconnect();
			});
		}

		private IDisposable LoginScope() {
			return Disposable.Create(() => {
				_client.Logoff();
			});
		}
	}
}
#endif
