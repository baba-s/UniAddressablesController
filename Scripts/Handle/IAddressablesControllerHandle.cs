using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kogane
{
	/// <summary>
	/// <para>AddressablesController の操作状況を管理するハンドルのインターフェイス</para>
	/// </summary>
	public interface IAddressablesControllerHandle
	{
		//================================================================================
		// プロパティ
		//================================================================================
		/// <summary>
		/// 有効な場合 true を返します
		/// </summary>
		bool IsValid { get; }

		/// <summary>
		/// ハンドルを返します
		/// </summary>
		AsyncOperationHandle Handle { get; }

		/// <summary>
		/// タスクを返します
		/// </summary>
		Task Task { get; }

		/// <summary>
		/// リザルトコードを返します
		/// </summary>
		AddressablesControllerResultCode ResultCode { get; }

		/// <summary>
		/// 成功した場合 true を返します
		/// </summary>
		bool IsSuccess { get; }

		/// <summary>
		/// 失敗した場合 true を返します
		/// </summary>
		bool IsFailure { get; }

		/// <summary>
		/// デバッグ名を返します
		/// </summary>
		string DebugName { get; }

		/// <summary>
		/// 進捗を返します（ 0.0 から 1.0 ）
		/// </summary>
		float Progress { get; }

		/// <summary>
		/// 読み込みが完了した場合 true を返します
		/// </summary>
		bool IsDone { get; }
	}
}