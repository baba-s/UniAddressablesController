# UniAddressablesController

Addressables をラップしてエラーハンドリングを充実化したパッケージ

## 依存するパッケージ

```
https://github.com/baba-s/UniAddressablesUtils.git
```

## 使用例

```cs
using Kogane;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Addressable のラッパークラス
/// </summary>
public static class MyAddressables
{
    //================================================================================
    // 変数
    //================================================================================
    private static IAddressablesController m_controller;

    //================================================================================
    // 関数(static)
    //================================================================================
    /// <summary>
    /// コントローラを設定します
    /// </summary>
    public static void SetController( IAddressablesController controller )
    {
        m_controller = controller;
    }

    /// <summary>
    /// Addressable を初期化します
    /// </summary>
    public static AddressablesControllerHandle InitializeAsync()
    {
        return m_controller.InitializeAsync();
    }

    /// <summary>
    /// <para>指定されたアドレスもしくはラベルに紐づくアセットバンドルのサイズを取得します</para>
    /// <para>キャッシュに存在するアセットバンドルのサイズは 0 で返ってくるため</para>
    /// <para>ダウンロードが必要なアセットバンドルのサイズの合計が取得できます</para>
    /// <para>既に読み込んだリモートカタログを使用してサイズの計算を行うため、通信は行いません</para>
    /// <para>存在しないアドレスやラベルを指定すると onFailure が呼び出されます</para>
    /// </summary>
    public static AddressablesControllerHandle<long> GetDownloadSizeAsync( object addressOrLabel )
    {
        return m_controller.GetDownloadSizeAsync( addressOrLabel );
    }

    /// <summary>
    /// <para>指定されたアドレスもしくはラベルに紐づくアセットバンドルのサイズを取得します</para>
    /// <para>キャッシュに存在するアセットバンドルのサイズは 0 で返ってくるため</para>
    /// <para>ダウンロードが必要なアセットバンドルのサイズの合計が取得できます</para>
    /// <para>既に読み込んだリモートカタログを使用してサイズの計算を行うため、通信は行いません</para>
    /// <para>存在しないアドレスやラベルを指定すると onFailure が呼び出されます</para>
    /// </summary>
    public static AddressablesControllerHandle<long> GetDownloadSizeAsync( IList<object> addressesOrLabels )
    {
        return m_controller.GetDownloadSizeAsync( addressesOrLabels );
    }

    /// <summary>
    /// <para>指定されたアドレスもしくはラベルに紐づくアセットバンドルを事前にダウンロードします</para>
    /// <para>autoReleaseHandle を true にすると、ダウンロードしたアセットバンドルをキャッシュに保存してメモリからは解放します</para>
    /// <para>autoReleaseHandle を false にすると、ダウンロードしたアセットバンドルはメモリに残り続けます</para>
    /// <para>ダウンロードしたアセットバンドルに含まれるアセットをすぐに使う場合は false を、</para>
    /// <para>すぐに使わない場合は true を指定すると効率的です</para>
    /// </summary>
    public static AddressablesControllerHandle DownloadDependenciesAsync
    (
        object addressOrLabel,
        bool   autoReleaseHandle
    )
    {
        return m_controller.DownloadDependenciesAsync( addressOrLabel, autoReleaseHandle );
    }

    /// <summary>
    /// <para>リモートカタログの更新が存在するかどうかを通信して確認します</para>
    /// <para>onSuccess で更新が必要なリモートカタログの名前のリストを取得できます</para>
    /// <para>リストの要素が存在しない場合はリモートカタログの更新は必要ありません</para>
    /// </summary>
    public static AddressablesControllerHandle<List<string>> CheckForCatalogUpdates()
    {
        return m_controller.CheckForCatalogUpdates();
    }

    /// <summary>
    /// <para>最新のリモートカタログを取得します</para>
    /// <para>catalogs に null を指定すると、すべてのリモートカタログを更新します</para>
    /// <para>特定のリモートカタログのみ更新したい場合は catalogs を指定します</para>
    /// </summary>
    public static AddressablesControllerHandle UpdateCatalogs( IEnumerable<string> catalogs )
    {
        return m_controller.UpdateCatalogs( catalogs );
    }

    /// <summary>
    /// <para>指定されたアドレスに紐づくシーンに遷移します</para>
    /// </summary>
    public static AddressablesControllerHandle LoadSceneAsync( object address )
    {
        return m_controller.LoadSceneAsync( address );
    }

    /// <summary>
    /// <para>指定されたアドレスに紐づくシーンに遷移します</para>
    /// </summary>
    public static AddressablesControllerHandle LoadSceneAsync
    (
        object        address,
        LoadSceneMode loadMode
    )
    {
        return m_controller.LoadSceneAsync( address, loadMode );
    }
}
```

```cs
using Kogane;
using UnityEngine;

public class Example : MonoBehaviour
{
    private static IAddressablesController CreateController( bool isVirtual )
    {
        if ( isVirtual )
        {
            var controller = new VirtualAddressablesController();
            return controller;
        }
        else
        {
            var controller = new AddressablesController
            {
                OnInternalIdTransform = location => location.InternalId
            };
            return controller;
        }
    }

    private void Awake()
    {
        MyAddressables.SetController( CreateController( false ) );
    }

    private void OnGUI()
    {
        Hoge();
    }

    private async void Hoge()
    {
        if ( GUILayout.Button( nameof( MyAddressables.InitializeAsync ) ) )
        {
            var handle = MyAddressables.InitializeAsync();
            await handle.Task;
            Debug.Log( handle.ResultCode );
        }

        if ( GUILayout.Button( nameof( MyAddressables.GetDownloadSizeAsync ) ) )
        {
            var handle = MyAddressables.GetDownloadSizeAsync( "hoge" );
            await handle.Task;
            var size = handle.Result;
            Debug.Log( size );
            Debug.Log( handle.ResultCode );
        }

        if ( GUILayout.Button( nameof( MyAddressables.DownloadDependenciesAsync ) ) )
        {
            var handle = MyAddressables.DownloadDependenciesAsync( "hoge", true );
            await handle.Task;
            Debug.Log( handle.ResultCode );
        }
    }
}
```
