using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.MotionControl.Devices
{
	enum Direction
	{
		/// <summary>
		/// 前进
		/// </summary>
		Forward,
		/// <summary>
		/// 后退
		/// </summary>
		Backward,
	}
	/// <summary>
	/// 逻辑电机驱动器接口
	/// </summary>
	interface IMotorDevice
	{
		/// <summary>
		/// 以指定占空比驱动左电机
		/// </summary>
		/// <param name="dutyCycle">占空比</param>
		/// <param name="dir">方向</param>
		void PowerLeft(double dutyCycle, Direction dir);
		/// <summary>
		/// 以指定占空比驱动右电机
		/// </summary>
		/// <param name="dutyCycle">占空比</param>
		/// <param name="dir">方向</param>
		void PowerRight(double dutyCycle, Direction dir);
	}
}
