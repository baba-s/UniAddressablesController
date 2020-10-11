using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Kogane
{
	/// <summary>
	/// <para>Addressables を操作するインターフェイス</para>
	/// </summary>
	public interface IAddressablesController
	{
		//================================================================================
		// 関数
		//================================================================================
		/// <summary>
		/// Addressable を初期化します
		/// </summary>
		AddressablesControllerHandle InitializeAsync();

		/// <summary>
		/// <para>指定されたアドレスもしくはラベルに紐づくアセットバンドルのサイズを取得します</para>
		/// <para>キャッシュに存在するアセットバンドルのサイズは 0 で返ってくるため</para>
		/// <para>ダウンロードが必要なアセットバンドルのサイズの合計が取得できます</para>
		/// <para>既に読み込んだリモートカタログを使用してサイズの計算を行うため、通信は行いません</para>
		/// <para>存在しないアドレスやラベルを指定すると onFailure が呼び出されます</para>
		/// </summary>
		AddressablesControllerHandle<long> GetDownloadSizeAsync( IList<object> addressesOrLabels );

		/// <summary>
		/// <para>指定されたアドレスもしくはラベルに紐づくアセットバンドルを事前にダウンロードします</para>
		/// <para>autoReleaseHandle を true にすると、ダウンロードしたアセットバンドルをキャッシュに保存してメモリからは解放します</para>
		/// <para>autoReleaseHandle を false にすると、ダウンロードしたアセットバンドルはメモリに残り続けます</para>
		/// <para>ダウンロードしたアセットバンドルに含まれるアセットをすぐに使う場合は false を、</para>
		/// <para>すぐに使わない場合は true を指定すると効率的です</para>
		/// </summary>
		AddressablesControllerHandle DownloadDependenciesAsync
		(
			object addressOrLabel,
			bool   autoReleaseHandle
		);

		/// <summary>
		/// <para>リモートカタログの更新が存在するかどうかを通信して確認します</para>
		/// <para>onSuccess で更新が必要なリモートカタログの名前のリストを取得できます</para>
		/// <para>リストの要素が存在しない場合はリモートカタログの更新は必要ありません</para>
		/// </summary>
		AddressablesControllerHandle<List<string>> CheckForCatalogUpdates();

		/// <summary>
		/// <para>最新のリモートカタログを取得します</para>
		/// <para>catalogs に null を指定すると、すべてのリモートカタログを更新します</para>
		/// <para>特定のリモートカタログのみ更新したい場合は catalogs を指定します</para>
		/// </summary>
		AddressablesControllerHandle UpdateCatalogs( IEnumerable<string> catalogs );

		/// <summary>
		/// <para>指定されたアドレスに紐づくシーンに遷移します</para>
		/// </summary>
		AddressablesControllerHandle LoadSceneAsync
		(
			object        address,
			LoadSceneMode loadMode
		);
	}
}