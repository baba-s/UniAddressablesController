using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;

namespace Kogane
{
	/// <summary>
	/// <para>Addressables を操作するクラス</para>
	/// <para>処理に失敗した時になぜ失敗したのかを列挙型に変換します</para>
	/// <para>Addressables.InitializeAsync 内でリモートカタログのダウンロードに失敗して進行不能になる不具合を回避できます</para>
	/// </summary>
	public sealed class AddressablesController : IAddressablesController
	{
		//================================================================================
		// 変数
		//================================================================================
		private bool              m_isInitialized;    // 初期化済みの場合 true
		private Action<Exception> m_exceptionHandler; // Addressable の例外を検知するためのデリゲート

		//================================================================================
		// デリゲート
		//================================================================================
		/// <summary>
		/// <para>InternalId を変換する時に呼び出されます</para>
		/// <para>アセットバンドルのダウンロード元の URL を動的に切り替えるために使用できます</para>
		/// </summary>
		public Func<IResourceLocation, string> OnInternalIdTransform { private get; set; }

		//================================================================================
		// イベント(static)
		//================================================================================
		/// <summary>
		/// Addressable 内部から Exception が渡された時に呼び出されます
		/// </summary>
		public event Action<AsyncOperationHandle, Exception> OnExceptionHandler;

		//================================================================================
		// 関数
		//================================================================================
		/// <summary>
		/// Addressable 内部から Exception が渡された時に呼び出されます
		/// </summary>
		private void CatchException( AsyncOperationHandle handle, Exception exception )
		{
			m_exceptionHandler?.Invoke( exception );
			OnExceptionHandler?.Invoke( handle, exception );
		}

		/// <summary>
		/// Addressable を初期化します
		/// </summary>
		public AddressablesControllerHandle InitializeAsync()
		{
			var source    = new TaskCompletionSource<AddressablesControllerResultCode>();
			var isFailure = false;

			m_isInitialized = false;
			AddressablesUtils.ResetInitializationFlag();

			void OnFailure( AddressablesControllerResultCode resultCode )
			{
				AddressablesUtils.ResetInitializationFlag();
				source.TrySetResult( resultCode );
			}

			void OnComplete( AsyncOperationHandle<IResourceLocator> handle )
			{
				m_exceptionHandler -= ExceptionHandler;

				if ( isFailure )
				{
					OnFailure( AddressablesControllerResultCode.FAILURE_CANNOT_CONNECTION );
					return;
				}

				if ( handle.Status != AsyncOperationStatus.Succeeded )
				{
					OnFailure( AddressablesControllerResultCode.FAILURE_UNKNOWN );
					return;
				}

				m_isInitialized = true;
				source.TrySetResult( AddressablesControllerResultCode.SUCCESS );
			}

			// InternalId を変換する処理を登録します
			Addressables.InternalIdTransformFunc = location => InternalIdTransformFunc( location );

			var result = Addressables.InitializeAsync();
			result.Completed += handle => OnComplete( handle );

			// Addressables.InitializeAsync の呼び出し前にコールバックを設定すると
			// Addressables.InitializeAsync の中で
			// ResourceManager.ExceptionHandler が上書きされてしまうため
			// InitializeAsync の呼び出し後にコールバックを設定しています
			ResourceManager.ExceptionHandler = CatchException;

			// リモートカタログのダウンロードに失敗した場合などに呼び出されます
			void ExceptionHandler( Exception exception )
			{
				isFailure          =  true;
				m_exceptionHandler -= ExceptionHandler;
			}

			m_exceptionHandler += ExceptionHandler;

			var controllerHandle = new AddressablesControllerHandle( result, source.Task );
			return controllerHandle;
		}

		/// <summary>
		/// InternalId を変換する時に呼び出されます
		/// </summary>
		private string InternalIdTransformFunc( IResourceLocation location )
		{
			return OnInternalIdTransform == null
					? location.InternalId
					: OnInternalIdTransform( location )
				;
		}

		/// <summary>
		/// <para>指定されたアドレスもしくはラベルに紐づくアセットバンドルのサイズを取得します</para>
		/// <para>キャッシュに存在するアセットバンドルのサイズは 0 で返ってくるため</para>
		/// <para>ダウンロードが必要なアセットバンドルのサイズの合計が取得できます</para>
		/// <para>既に読み込んだリモートカタログを使用してサイズの計算を行うため、通信は行いません</para>
		/// <para>存在しないアドレスやラベルを指定すると onFailure が呼び出されます</para>
		/// </summary>
		public AddressablesControllerHandle<long> GetDownloadSizeAsync( IList<object> addressesOrLabels )
		{
			var source = new TaskCompletionSource<(long, AddressablesControllerResultCode)>();

			void OnFailure( AddressablesControllerResultCode resultCode )
			{
				source.TrySetResult( ( 0, resultCode ) );
			}

			if ( !m_isInitialized )
			{
				OnFailure( AddressablesControllerResultCode.FAILURE_NOT_INITIALIZED );
				return new AddressablesControllerHandle<long>( source.Task );
			}

			var isFailure     = false;
			var isNotExistKey = false;

			void OnComplete( AsyncOperationHandle<long> handle )
			{
				m_exceptionHandler -= ExceptionHandler;

				if ( isFailure )
				{
					var resultCode = isNotExistKey
							? AddressablesControllerResultCode.FAILURE_KEY_NOT_EXIST
							: AddressablesControllerResultCode.FAILURE_CANNOT_CONNECTION
						;

					OnFailure( resultCode );
					return;
				}

				if ( handle.Status != AsyncOperationStatus.Succeeded )
				{
					OnFailure( AddressablesControllerResultCode.FAILURE_UNKNOWN );
					return;
				}

				source.TrySetResult( ( handle.Result, AddressablesControllerResultCode.SUCCESS ) );
			}

			void ExceptionHandler( Exception exception )
			{
				isFailure     = true;
				isNotExistKey = exception.Message.Contains( nameof( InvalidKeyException ) );

				m_exceptionHandler -= ExceptionHandler;
			}

			m_exceptionHandler += ExceptionHandler;

			// アドレスもしくはラベルは IList<object> 型で受け取る必要があります
			// IList<string> 型で渡すとサイズの取得に失敗します
			var result = Addressables.GetDownloadSizeAsync( ( IEnumerable ) addressesOrLabels );
			result.Completed += handle => OnComplete( handle );

			return new AddressablesControllerHandle<long>( source.Task );
		}

		/// <summary>
		/// <para>指定されたアドレスもしくはラベルに紐づくアセットバンドルを事前にダウンロードします</para>
		/// <para>autoReleaseHandle を true にすると、ダウンロードしたアセットバンドルをキャッシュに保存してメモリからは解放します</para>
		/// <para>autoReleaseHandle を false にすると、ダウンロードしたアセットバンドルはメモリに残り続けます</para>
		/// <para>ダウンロードしたアセットバンドルに含まれるアセットをすぐに使う場合は false を、</para>
		/// <para>すぐに使わない場合は true を指定すると効率的です</para>
		/// </summary>
		public AddressablesControllerHandle DownloadDependenciesAsync
		(
			object addressOrLabel,
			bool   autoReleaseHandle
		)
		{
			var source = new TaskCompletionSource<AddressablesControllerResultCode>();

			if ( !m_isInitialized )
			{
				source.TrySetResult( AddressablesControllerResultCode.FAILURE_NOT_INITIALIZED );
				return new AddressablesControllerHandle( source.Task );
			}

			var isFailure     = false;
			var isNotExistKey = false;

			void OnComplete( AsyncOperationHandle handle )
			{
				m_exceptionHandler -= ExceptionHandler;

				if ( isFailure )
				{
					var resultCode = isNotExistKey
							? AddressablesControllerResultCode.FAILURE_KEY_NOT_EXIST
							: AddressablesControllerResultCode.FAILURE_CANNOT_CONNECTION
						;

					source.TrySetResult( resultCode );
					return;
				}

				if ( handle.Status != AsyncOperationStatus.Succeeded )
				{
					source.TrySetResult( AddressablesControllerResultCode.FAILURE_UNKNOWN );
					return;
				}

				source.TrySetResult( AddressablesControllerResultCode.SUCCESS );
			}

			void ExceptionHandler( Exception exception )
			{
				isFailure     = true;
				isNotExistKey = exception.Message.Contains( nameof( InvalidKeyException ) );

				m_exceptionHandler -= ExceptionHandler;
			}

			m_exceptionHandler += ExceptionHandler;

			var result = Addressables.DownloadDependenciesAsync( addressOrLabel, autoReleaseHandle );
			result.Completed += handle => OnComplete( handle );

			var controllerHandle = new AddressablesControllerHandle( result, source.Task );
			return controllerHandle;
		}

		/// <summary>
		/// <para>リモートカタログの更新が存在するかどうかを通信して確認します</para>
		/// <para>onSuccess で更新が必要なリモートカタログの名前のリストを取得できます</para>
		/// <para>リストの要素が存在しない場合はリモートカタログの更新は必要ありません</para>
		/// </summary>
		public AddressablesControllerHandle<List<string>> CheckForCatalogUpdates()
		{
			var source = new TaskCompletionSource<(List<string>, AddressablesControllerResultCode)>();

			void OnFailure( AddressablesControllerResultCode resultCode )
			{
				source.TrySetResult( ( new List<string>(), resultCode ) );
			}

			if ( !m_isInitialized )
			{
				OnFailure( AddressablesControllerResultCode.FAILURE_NOT_INITIALIZED );
				return new AddressablesControllerHandle<List<string>>( source.Task );
			}

			var isFailure = false;

			void OnComplete( AsyncOperationHandle<List<string>> handle )
			{
				m_exceptionHandler -= ExceptionHandler;

				if ( isFailure )
				{
					OnFailure( AddressablesControllerResultCode.FAILURE_CANNOT_CONNECTION );
					return;
				}

				if ( handle.Status != AsyncOperationStatus.Succeeded )
				{
					OnFailure( AddressablesControllerResultCode.FAILURE_UNKNOWN );
					return;
				}

				var list = handle.Result ?? new List<string>();

				source.TrySetResult( ( list, AddressablesControllerResultCode.SUCCESS ) );
			}

			void ExceptionHandler( Exception exception )
			{
				isFailure          =  true;
				m_exceptionHandler -= ExceptionHandler;
			}

			m_exceptionHandler += ExceptionHandler;

			var result = Addressables.CheckForCatalogUpdates();
			result.Completed += handle => OnComplete( handle );

			return new AddressablesControllerHandle<List<string>>( source.Task );
		}

		/// <summary>
		/// <para>最新のリモートカタログを取得します</para>
		/// <para>catalogs に null を指定すると、すべてのリモートカタログを更新します</para>
		/// <para>特定のリモートカタログのみ更新したい場合は catalogs を指定します</para>
		/// </summary>
		public AddressablesControllerHandle UpdateCatalogs( IEnumerable<string> catalogs )
		{
			var source = new TaskCompletionSource<AddressablesControllerResultCode>();

			if ( !m_isInitialized )
			{
				source.TrySetResult( AddressablesControllerResultCode.FAILURE_NOT_INITIALIZED );
				return new AddressablesControllerHandle( source.Task );
			}

			var isFailure = false;

			void OnComplete( AsyncOperationHandle handle )
			{
				m_exceptionHandler -= ExceptionHandler;

				if ( isFailure )
				{
					source.TrySetResult( AddressablesControllerResultCode.FAILURE_CANNOT_CONNECTION );
					return;
				}

				if ( handle.Status != AsyncOperationStatus.Succeeded )
				{
					source.TrySetResult( AddressablesControllerResultCode.FAILURE_UNKNOWN );
					return;
				}

				source.TrySetResult( AddressablesControllerResultCode.SUCCESS );
			}

			void ExceptionHandler( Exception exception )
			{
				isFailure          =  true;
				m_exceptionHandler -= ExceptionHandler;
			}

			m_exceptionHandler += ExceptionHandler;

			var result = Addressables.UpdateCatalogs( catalogs );
			result.Completed += handle => OnComplete( handle );

			var controllerHandle = new AddressablesControllerHandle( result, source.Task );
			return controllerHandle;
		}

		/// <summary>
		/// <para>指定されたアドレスに紐づくシーンに遷移します</para>
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

			var isFailure     = false;
			var isNotExistKey = false;

			void OnComplete( AsyncOperationHandle handle )
			{
				m_exceptionHandler -= ExceptionHandler;

				if ( isFailure )
				{
					var resultCode = isNotExistKey
							? AddressablesControllerResultCode.FAILURE_KEY_NOT_EXIST
							: AddressablesControllerResultCode.FAILURE_CANNOT_CONNECTION
						;

					source.TrySetResult( resultCode );
					return;
				}

				if ( handle.Status != AsyncOperationStatus.Succeeded )
				{
					source.TrySetResult( AddressablesControllerResultCode.FAILURE_UNKNOWN );
					return;
				}

				source.TrySetResult( AddressablesControllerResultCode.SUCCESS );
			}

			void ExceptionHandler( Exception exception )
			{
				isFailure     = true;
				isNotExistKey = exception.Message.Contains( nameof( InvalidKeyException ) );

				m_exceptionHandler -= ExceptionHandler;
			}

			m_exceptionHandler += ExceptionHandler;

			var result = Addressables.LoadSceneAsync( address, loadMode );
			result.Completed += handle => OnComplete( handle );

			return new AddressablesControllerHandle( source.Task );
		}
	}
}