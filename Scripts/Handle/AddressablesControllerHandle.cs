using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Kogane
{
	/// <summary>
	/// <para>AddressablesController の操作状況を管理するハンドル</para>
	/// <para>非同期タスクの待機や進捗やリザルトコードの取得が可能です</para>
	/// </summary>
	public sealed class AddressablesControllerHandle : IAddressablesControllerHandle
	{
		//================================================================================
		// 変数(readonly)
		//================================================================================
		private readonly bool                                   m_isValid;
		private readonly AsyncOperationHandle                   m_handle;
		private readonly Task<AddressablesControllerResultCode> m_task;

		//================================================================================
		// プロパティ
		//================================================================================
		/// <summary>
		/// 有効な場合 true を返します
		/// </summary>
		bool IAddressablesControllerHandle.IsValid => m_isValid;

		/// <summary>
		/// ハンドルを返します
		/// </summary>
		AsyncOperationHandle IAddressablesControllerHandle.Handle => m_handle;

		/// <summary>
		/// タスクを返します
		/// </summary>
		public Task Task => m_task;

		/// <summary>
		/// リザルトコードを返します
		/// </summary>
		public AddressablesControllerResultCode ResultCode => m_task.Result;

		/// <summary>
		/// 成功した場合 true を返します
		/// </summary>
		public bool IsSuccess => ResultCode == AddressablesControllerResultCode.SUCCESS;

		/// <summary>
		/// 失敗した場合 true を返します
		/// </summary>
		public bool IsFailure => !IsSuccess;

		/// <summary>
		/// デバッグ名を返します
		/// </summary>
		public string DebugName => m_isValid ? m_handle.DebugName : string.Empty;

		/// <summary>
		/// 進捗を返します（ 0.0 から 1.0 ）
		/// </summary>
		public float Progress => m_isValid ? IsDone ? 1 : m_handle.PercentComplete : 1;

		/// <summary>
		/// 読み込みが完了した場合 true を返します
		/// </summary>
		public bool IsDone => !m_isValid || m_handle.IsDone;

		//================================================================================
		// 関数
		//================================================================================
		/// <summary>
		/// <para>コンストラクタ</para>
		/// <para>処理に成功した場合はこちらのコンストラクタを使用します</para>
		/// </summary>
		public AddressablesControllerHandle
		(
			AsyncOperationHandle                   handle,
			Task<AddressablesControllerResultCode> task
		)
		{
			m_isValid = true;
			m_handle  = handle;
			m_task    = task;
		}

		/// <summary>
		/// <para>コンストラクタ</para>
		/// <para>処理に失敗した場合はこちらのコンストラクタを使用します</para>
		/// </summary>
		public AddressablesControllerHandle( Task<AddressablesControllerResultCode> task )
		{
			m_task = task;
		}
	}
}