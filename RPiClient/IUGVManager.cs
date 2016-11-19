using System.Threading.Tasks;

namespace RPiClient
{
	/// <summary>
	/// 逻辑UGV控制器
	/// </summary>
	internal interface IUGVManager
	{
		/// <summary>
		/// 接管UGV
		/// </summary>
		Task Start(UGV ugv);
		/// <summary>
		/// 放弃控制权
		/// </summary>
		void Quit();
	}
}