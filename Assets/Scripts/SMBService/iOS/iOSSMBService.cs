#if UNITY_IOS || UNITY_EDITOR
using System;
using System.Threading.Tasks;
using NativePlugin;

namespace Project.Networking.SMB.iOS {

	public sealed class iOSSMBService : INativeSMBService {

		private static SMBConnectionInfo info;

		void INativeSMBService.SetConfig(SMBConnectionInfo info) {
			iOSSMBService.info = info;
		}

		async Task<string[]> INativeSMBService.GetRemoteFileNames(string remoteDirectoryPath) {
			return await NativeMethods.GetFileNames(info, remoteDirectoryPath);
		}

		async Task INativeSMBService.DownloadFile(string remoteFilePath, string localFilePath) {
			await NativeMethods.DownloadFile(info, remoteFilePath, localFilePath);
		}

		async Task INativeSMBService.UploadFile(string localFilePath, string remoteFilePath) {
			await NativeMethods.UploadFile(info, localFilePath, remoteFilePath);
		}

		Task INativeSMBService.DeleteRemoteFile(string remoteFilePath) {
			throw new System.NotImplementedException();
		}
	}

}
#endif
