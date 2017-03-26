using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.Interface
{
	/// <summary>
	/// 定义逻辑驱动器接口，仅包含必要功能
	/// </summary>
	interface IDriver : IDisposable
	{
		/// <summary>
		/// 快速停止,设备立即停止运行
		/// </summary>
		void QuickStop();
	}
}
