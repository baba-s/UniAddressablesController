using UnityEngine.SceneManagement;

namespace Kogane
{
	/// <summary>
	/// IAddressablesController 型の拡張メソッドを管理するクラス
	/// </summary>
	public static class IAddressablesControllerExtensionMethods
	{
		//================================================================================
		// 関数(static)
		//================================================================================
		/// <summary>
		/// <para>指定されたアドレスもしくはラベルに紐づくアセットバンドルのサイズを取得します</para>
		/// <para>キャッシュに存在するアセットバンドルのサイズは 0 で返ってくるため</para>
		/// <para>ダウンロードが必要なアセットバンドルのサイズの合計が取得できます</para>
		/// <para>既に読み込んだリモートカタログを使用してサイズの計算を行うため、通信は行いません</para>
		/// <para>存在しないアドレスやラベルを指定すると onFailure が呼び出されます</para>
		/// </summary>
		public static AddressablesControllerHandle<long> GetDownloadSizeAsync
		(
			this IAddressablesController self,
			object                       addressOrLabel
		)
		{
			return self.GetDownloadSizeAsync( new[] { addressOrLabel } );
		}

		/// <summary>
		/// <para>指定されたアドレスに紐づくシーンに遷移します</para>
		/// </summary>
		public static AddressablesControllerHandle LoadSceneAsync
		(
			this IAddressablesController self,
			object                       address
		)
		{
			return self.LoadSceneAsync
			(
				address: address,
				loadMode: LoadSceneMode.Single
			);
		}
	}
}