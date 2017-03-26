using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.SpeedControl
{
	/// <summary>
	/// 虚拟电动机设备
	/// </summary>
	class Motions
	{
		/// <summary>
		/// 逻辑电机驱动器
		/// </summary>
		private MotorDriver Devices;
		public int LeftSpeed
		{
			get;private set;
		}
		public int RightSpeed
		{
			get;private set;
		}
		/// <summary>
		/// 从指定的逻辑电动机驱动器创建运动控制单元
		/// </summary>
		/// <param name="device">设备</param>
		public Motions(MotorDriver device)
		{
			Devices = device;
			LeftSpeed = 0;
			RightSpeed = 0;
		}
		/// <summary>
		/// 分别设定左右驱动器速度
		/// </summary>
		/// <param name="speedLeft">左驱动器速度</param>
		/// <param name="speedRight">右驱动器速度</param>
		public void Power(int speedLeft,int speedRight)
		{
			if(speedLeft > 100 && speedLeft < -100)
			{
				throw new ArgumentOutOfRangeException("speedLeft", "速度不在允许的范围内");
			}
			if(speedRight > 100 && speedRight < -100)
			{
				throw new ArgumentOutOfRangeException("speedRight", "速度不在允许的范围内");
			}
			LeftSpeed = speedLeft;
			RightSpeed = speedRight;
			Devices.PowerLeft(1.0 * Math.Abs(speedLeft) / 100, speedLeft >= 0 ? Direction.Forward : Direction.Backward);
			Devices.PowerLeft(1.0 * Math.Abs(speedRight) / 100, speedRight >= 0 ? Direction.Forward : Direction.Backward);
		}
		/// <summary>
		/// 立即停止运动
		/// </summary>
		public void Stop()
		{
			Devices.QuickStop();
		}
	}
}
