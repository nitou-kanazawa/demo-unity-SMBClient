using System;
using System.Linq;
using Cysharp.Threading.Tasks;
//using SMBLibrary;
//using SMBLibrary.Client;

namespace Project.Networking.SMB.Win {
    
    public sealed class WinSMBClient : INativeSMBClient {

        //private readonly SMB2Client _client;


        void INativeSMBClient.SetConfig(SMBConnectionInfo info) {
            throw new System.NotImplementedException();
        }



        UniTask INativeSMBClient.DeleteRemoteFile(string remoteFilePath) {
            throw new System.NotImplementedException();
        }

        UniTask INativeSMBClient.DownloadFile(string remoteFilePath, string localFilePath) {
            throw new System.NotImplementedException();
        }

        UniTask<bool> INativeSMBClient.ExsitsDirectory(string remoteFilePath) {
            throw new System.NotImplementedException();
        }

        UniTask<bool> INativeSMBClient.ExsitsFile(string remoteFilePath) {
            throw new System.NotImplementedException();
        }

        UniTask<string[]> INativeSMBClient.GetRemoteFileNames(string remoteDirectoryPath) {
            throw new System.NotImplementedException();
        }


        UniTask INativeSMBClient.UploadFile(string localFilePath, string remoteFilePath) {
            throw new System.NotImplementedException();
        }


        /// ----------------------------------------------------------------------------
        // Private Method



    }
}
