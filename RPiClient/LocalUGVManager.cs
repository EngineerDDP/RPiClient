using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient
{
	/// <summary>
	/// 本地UGV控制器
	/// </summary>
	class AutoPilot : IUGVManager
	{
		/// <summary>
		/// 设备引用
		/// </summary>
		private UGV Device;
		public async Task Start(UGV ugv)
		{
			Device = ugv;
			Device.Light("Open", 0);

			await Task.Delay(10000);

			Device.Light("Close", 0);
		}

		public void Quit()
		{
			Device = null;
		}
	}
}
