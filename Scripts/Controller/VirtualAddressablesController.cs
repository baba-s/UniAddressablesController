using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Kogane
{
	/// <summary>
	/// <para>Addressables を操作するクラス</para>
	/// <para>Unity エディタ上で Addressables を使用しない時（いわゆるバーチャルモードの時）に活用できます</para>
	/// <para>EditorSceneManager を使用しているためビルド後のアプリでは使用できません</para>
	/// </summary>
	public sealed class VirtualAddressablesController : IAddressablesController
	{
		//================================================================================
		// 変数
		//================================================================================
		private bool m_isInitialized; // 初期化済みの場合 true

		//================================================================================
		// デリゲート
		//================================================================================
		/// <summary>
		/// シーンのアドレスをシーンのアセットパスに変換する時に呼び出されます
		/// </summary>
		public Func<object, string> OnSceneAddressToSceneAssetPath { private get; set; }

		//================================================================================
		// 関数
		//================================================================================
		/// <summary>
		/// <para>Addressable を初期化します</para>
		/// <para>実際には何もしません</para>
		/// </summary>
		public AddressablesControllerHandle InitializeAsync()
		{
			m_isInitialized = true;
			var source = new TaskCompletionSource<AddressablesControllerResultCode>();
			source.TrySetResult( AddressablesControllerResultCode.SUCCESS );
			return new AddressablesControllerHandle( source.Task );
		}

		/// <summary>
		/// <para>指定されたアドレスもしくはラベルに紐づくアセットバンドルのサイズを取得します</para>
		/// <para>実際には何もしないためサイズは 0 を返します</para>
		/// </summary>
		public AddressablesControllerHandle<long> GetDownloadSizeAsync( IList<object> addressesOrLabels )
		{
			var source = new TaskCompletionSource<(long, AddressablesControllerResultCode)>();

			var resultCode = m_isInitialized
					? AddressablesControllerResultCode.SUCCESS
					: AddressablesControllerResultCode.FAILURE_NOT_INITIALIZED
				;

			source.TrySetResult( ( 0, resultCode ) );

			return new AddressablesControllerHandle<long>( source.Task );
		}

		/// <summary>
		/// <para>指定されたアドレスもしくはラベルに紐づくアセットバンドルを事前にダウンロードします</para>
		/// <para>実際には何もしません</para>
		/// </summary>
		public AddressablesControllerHandle DownloadDependenciesAsync
		(
			object addressOrLabel,
			bool   autoReleaseHandle
		)
		{
			var source = new TaskCompletionSource<AddressablesControllerResultCode>();

			var resultCode = m_isInitialized
					? AddressablesControllerResultCode.SUCCESS
					: AddressablesControllerResultCode.FAILURE_NOT_INITIALIZED
				;

			source.TrySetResult( resultCode );

			return new AddressablesControllerHandle( source.Task );
		}

		/// <summary>
		/// <para>リモートカタログの更新が存在するかどうかを通信して確認します</para>
		/// <para>実際には何もしないため空のリストを返します</para>
		/// </summary>
		public AddressablesControllerHandle<List<string>> CheckForCatalogUpdates()
		{
			var source = new TaskCompletionSource<(List<string>, AddressablesControllerResultCode)>();

			var resultCode = m_isInitialized
					? AddressablesControllerResultCode.SUCCESS
					: AddressablesControllerResultCode.FAILURE_NOT_INITIALIZED
				;

			source.TrySetResult( ( new List<string>(), resultCode ) );

			return new AddressablesControllerHandle<List<string>>( source.Task );
		}

		/// <summary>
		/// <para>最新のリモートカタログを取得します</para>
		/// <para>実際には何もしません</para>
		/// </summary>
		public AddressablesControllerHandle UpdateCatalogs( IEnumerable<string> catalogs )
		{
			var source = new TaskCompletionSource<AddressablesControllerResultCode>();

			var resultCode = m_isInitialized
					? AddressablesControllerResultCode.SUCCESS
					: AddressablesControllerResultCode.FAILURE_NOT_INITIALIZED
				;

			source.TrySetResult( resultCode );

			return new AddressablesControllerHandle( source.Task );
		}

		/// <summary>
		/// <para>指定されたアドレスに紐づくシーンに遷移します</para>
		/// <para>EditorSceneManager.LoadSceneInPlayMode で遷移します</para>
		/// <para>アドレスとシーンのアセットパスに変換したい場合は</para>
		/// <para>OnSceneAddressToSceneAssetPath デリゲートを使用してください</para>
		/// </summary>
		public AddressablesControllerHandle LoadSceneAsync
		(
			object        address,
			LoadSceneMode loadMode
		)
		{
			var source = new TaskCompletionSource<AddressablesControllerResultCode>();

			if ( !m_isInitialized )
			{
				source.TrySetResult( AddressablesControllerResultCode.FAILURE_NOT_INITIALIZED );
				return new AddressablesControllerHandle( source.Task );
			}

			var path = OnSceneAddressToSceneAssetPath != null
					? OnSceneAddressToSceneAssetPath( address )
					: address.ToString()
				;

			var parameters = new LoadSceneParameters( loadMode );

#if UNITY_EDITOR
			UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode( path, parameters );
#endif

			source.TrySetResult( AddressablesControllerResultCode.SUCCESS );

			return new AddressablesControllerHandle( source.Task );
		}
	}
}