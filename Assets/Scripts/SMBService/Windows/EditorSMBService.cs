#if UNITY_EDITOR
using UnityEngine;
using System.Threading.Tasks;

namespace Project.Networking.SMB.Windows {

	public class WindowsSMBService : INativeSMBService {

		public static SMBConnectionInfo ConnectionInfo;


		/// ----------------------------------------------------------------------------
		#region Interface Method

		void INativeSMBService.SetConfig(SMBConnectionInfo info) {
			WindowsSMBService.ConnectionInfo = info;
		}

		Task<string[]> INativeSMBService.GetRemoteFileNames(string remoteDirectoryPath) {
			var client = new SMBClientWrapper(ConnectionInfo);
			return client.GetRemoteFileNames(remoteDirectoryPath);
		}

		Task INativeSMBService.DownloadFile(string remoteFilePath, string localFilePath) {
			var client = new SMBClientWrapper(ConnectionInfo);
			return client.DownloadFile(remoteFilePath, localFilePath);
		}

		Task INativeSMBService.UploadFile(string localFilePath, string remoteFilePath) {
			var client = new SMBClientWrapper(ConnectionInfo);
			return client.UploadFile(localFilePath, remoteFilePath);
		}

		Task INativeSMBService.DeleteRemoteFile(string remoteFilePath) {
			var client = new SMBClientWrapper(ConnectionInfo);
			return client.DeleteRemoteFile(remoteFilePath);
		}
		#endregion
	}

}
#endif
