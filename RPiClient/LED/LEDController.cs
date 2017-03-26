using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.LED
{
	enum LEDMode
	{
		/// <summary>
		/// 开
		/// </summary>
		Open,
		/// <summary>
		/// 关
		/// </summary>
		Close,
		/// <summary>
		/// 交错闪烁
		/// </summary>
		Special_A,
		/// <summary>
		/// 走马灯
		/// </summary>
		Special_B
	}
	/// <summary>
	/// LED控制器
	/// </summary>
	class LEDController
	{
		/// <summary>
		/// LED设备
		/// </summary>
		private Light Device;
		/// <summary>
		/// 创建LED控制器
		/// </summary>
		public LEDController()
		{
			Device = new Light(LEDDriver.LEDGroup_BlueRed);
		}
		/// <summary>
		/// 设置LED模式
		/// </summary>
		/// <param name="mode">模式枚举值</param>
		public void Set(LEDMode mode)
		{
			switch (mode)
			{
				case LEDMode.Open:
					Device.StayOpen();
					break;
				case LEDMode.Close:
					Device.StayClose();
					break;
				case LEDMode.Special_A:
					Device.BlinkA();
					break;
				case LEDMode.Special_B:
					Device.BlinkB();
					break;
				default:
					break;
			}
		}
	}
}
