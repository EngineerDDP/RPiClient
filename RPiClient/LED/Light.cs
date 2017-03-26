using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.LED
{
	/// <summary>
	/// 闪烁控制器，用于实现灯光闪烁，常亮，关闭等效果。
	/// </summary>
	class Light
	{
		/// <summary>
		/// 关联的设备驱动器
		/// </summary>
		private LEDDriver LED;
		/// <summary>
		/// 使用已有的设备驱动器构造虚拟设备控制器
		/// </summary>
		/// <param name="led"></param>
		public Light(LEDDriver led)
		{
			LED = led;
		}
		/// <summary>
		/// 使用方案A闪烁LED，奇数脚与偶数脚交替闪烁
		/// </summary>
		public void BlinkA()
		{
			LED.SetStatus(new bool[] { false, true, false, true });
			LED.SetChange((bool[] status) =>
			{
				int i = 0;
				for (i = 0; i < status.Length; ++i)
				{
					status[i] = !status[i];
				}
				return status;
			}, 200);
		}
		/// <summary>
		/// 使用方案B闪烁LED，逐个亮起
		/// </summary>
		public void BlinkB()
		{
			bool s = true;
			LED.SetStatus(new bool[] { true, false, false, false });
			LED.SetChange((bool[] status) =>
			{
				int i = 0;
				foreach(bool j in status)
				{
					if (j)
						break;
					++i;
				}
				if (i == status.Length)
					s = false;
				if (i == 1)
					s = true;
				status[i + (s ? 0 : -2)] = true;
				status[i - 1] = false;
				return status;
			}, 100);
		}
		/// <summary>
		/// 设置全亮
		/// </summary>
		public void StayOpen()
		{
			LED.SetStatus(new bool[] { true, true, true, true });
			LED.QuickStop();
		}
		/// <summary>
		/// 设置全灭
		/// </summary>
		public void StayClose()
		{
			LED.SetStatus(new bool[] { false, false, false, false });
			LED.QuickStop();
		}
	}
}
