using System;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Networking.SMB {
	public interface INativeSMBService {
		void SetConfig(SMBConnectionInfo info);
		Task<string[]> GetRemoteFileNames(string remoteDirectoryPath);
		Task DownloadFile(string remoteFilePath, string localFilePath);
		Task UploadFile(string localFilePath, string remoteFilePath);
		Task DeleteRemoteFile(string remoteFilePath);
	}
}
