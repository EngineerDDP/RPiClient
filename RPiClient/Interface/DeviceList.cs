using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.Interface
{
	/// <summary>
	/// 设备列表
	/// </summary>
	class DeviceList
	{
		/// <summary>
		/// 驱动列表
		/// </summary>
		private List<IDriver> Drivers;
		public DeviceList()
		{
			//红蓝LED灯组，关联在GPIO 19，16，26，20
			Drivers.Add(LED.LEDDriver.LEDGroup_BlueRed);
			//电机驱动器组，关联在GPIO 17，18，27，22
			Drivers.Add(SpeedControl.MotorDriver.Motor_Default);
			//舵机控制器，关联在GPIO 23
			Drivers.Add(Servo.Servo.Default);
			//陀螺仪装置，关联在GPIO 4，挂载于IIC总线
			Drivers.Add(Gyroscope.Gyroscope.Default);
		}
		/// <summary>
		/// 标准退出程序，执行本方法释放资源并重置设备状态
		/// </summary>
		public void ExitProgram()
		{
			int i;
			for (i = 0; i < Drivers.Count; ++i) 
			{
				Drivers[i].QuickStop();
				Drivers[i].Dispose();
			}
		}
	}
}
