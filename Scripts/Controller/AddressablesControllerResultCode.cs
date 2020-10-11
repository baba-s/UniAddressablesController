namespace Kogane
{
	/// <summary>
	/// Addressables の操作のリザルトコード
	/// </summary>
	public enum AddressablesControllerResultCode
	{
		SUCCESS,                   // 成功
		FAILURE_UNKNOWN,           // 不明
		FAILURE_NOT_INITIALIZED,   // 初期化していない
		FAILURE_CANNOT_CONNECTION, // 通信失敗
		FAILURE_KEY_NOT_EXIST,     // キーが存在しない
		//FAILURE_CANNOT_LOAD_ASSET, // アセットの読み込み失敗
	}
}