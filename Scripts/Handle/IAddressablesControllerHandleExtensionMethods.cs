using UnityEngine.AddressableAssets;

namespace Kogane
{
	/// <summary>
	/// IAddressablesControllerHandle 型の拡張メソッドを管理するクラス
	/// </summary>
	public static class IAddressablesControllerHandleExtensionMethods
	{
		//================================================================================
		// 関数(static)
		//================================================================================
		/// <summary>
		/// <para>参照カウンタを増やします</para>
		/// <para>シーンを読み込んだ後にシーン内のゲームオブジェクトに DontDestroyOnLoad する場合</para>
		/// <para>シーンがアンロードされるとそのゲームオブジェクトが参照している</para>
		/// <para>すべてのスクリプトやアセットが Missing になってしまうため</para>
		/// <para>それを防ぐためにシーン内のゲームオブジェクトを DontDestroyOnLoad する場合は</para>
		/// <para>シーンの読み込みのリクエスト後にこの関数を呼び出す必要があります</para>
		/// </summary>
		public static void Acquire( this IAddressablesControllerHandle self )
		{
			if ( !self.IsValid ) return;
			Addressables.ResourceManager.Acquire( self.Handle );
		}

		/// <summary>
		/// 参照カウンタを減らします
		/// </summary>
		public static void Release( this IAddressablesControllerHandle self )
		{
			if ( !self.IsValid ) return;
			Addressables.ResourceManager.Release( self.Handle );
		}
	}
}