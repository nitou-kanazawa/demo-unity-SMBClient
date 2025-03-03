using System;
using Cysharp.Threading.Tasks;

namespace Project.Networking.SMB {

    public interface INativeSMBClient {

        void SetConfig(SMBConnectionInfo info);

        UniTask<bool> ExsitsFile(string remoteFilePath);
        UniTask<bool> ExsitsDirectory(string remoteFilePath);
        UniTask<string[]> GetRemoteFileNames(string remoteDirectoryPath);
        UniTask DownloadFile(string remoteFilePath, string localFilePath);
        
        UniTask UploadFile(string localFilePath, string remoteFilePath);
        UniTask DeleteRemoteFile(string remoteFilePath);
    }
}
