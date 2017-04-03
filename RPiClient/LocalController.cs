using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient
{
	/// <summary>
	/// 本地控制中心
	/// </summary>
	class LocalController
	{
		private UGV ugv;
		public LocalController()
		{
			ugv = new UGV();
		}
		/// <summary>
		/// 初始化
		/// </summary>
		public void Init()
		{

		}
		/// <summary>
		/// 启动
		/// </summary>
		public void Run()
		{
			ugv.LED.Set(LED.LEDMode.Special_A);
			Task.Delay(10000).Wait();
		}
		/// <summary>
		/// 退出
		/// </summary>
		public void Exit()
		{
			ugv.Devs.ExitProgram();
		}
	}
}
