using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Project.Networking.SMB;

namespace Demo {

	public class EntryPoint : MonoBehaviour {

		public Button _button;

		[Space]
		public string ipAddress = "192.168.1.100";
		public string userName = "username";
		public string password = "password";
		public string shareName = "WORK";


		// Start is called once before the first execution of Update after the MonoBehaviour is created
		async void Start() {
			var token = this.destroyCancellationToken;

			var info = new SMBConnectionInfo(
				System.Net.IPAddress.Parse(ipAddress),
				userName,
				password,
				shareName);
			Debug.Log(info);
			SMBService.SetConfig(info);
			
			try {
				Debug.Log("---------------------");
				Debug.Log("Click to Enumerate files");
				await _button.OnClickAsync(cancellationToken: token);

				var files = await SMBService.GetRemoteFileNames("");
				Debug.Log(string.Join("\n", files));

				Debug.Log("---------------------");
				Debug.Log("Click to Download file");
				await _button.OnClickAsync(cancellationToken: token);

				// await SMBService.DownloadFile("");
			}
			catch (OperationCanceledException) {
				Debug.LogWarning("処理がキャンセルされました（GameObjectがDestroyされた可能性があります）");
			}
			catch (Exception ex) {
				Debug.LogError($"予期せぬエラーが発生しました: {ex}");
			}
		}

	}
}
