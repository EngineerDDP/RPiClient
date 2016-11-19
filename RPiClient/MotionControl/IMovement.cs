using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.MotionControl
{
	/// <summary>
	/// 逻辑运动控制单元
	/// </summary>
	interface IMovementController
	{
		int LeftSpeed
		{
			get;
		}
		int RightSpeed
		{
			get;
		}
		/// <summary>
		/// 以指定速度前进
		/// </summary>
		/// <param name="speed">前进速度百分比,从1-100</param>
		void Forward(int speed);
		/// <summary>
		/// 控制电机立即停止动作
		/// </summary>
		void Stop();
		/// <summary>
		/// 在指定时间内制动
		/// </summary>
		/// <param name="millisecond">指定的时间,以毫秒记</param>
		void Stop(int millisecond);
		/// <summary>
		/// 以指定速度后退
		/// </summary>
		/// <param name="speed">后退速度百分比,从1-100</param>
		void Backward(int speed);
		/// <summary>
		/// 以指定的差速左转弯
		/// </summary>
		/// <param name="speed">差速比,从1-100</param>
		void TurnLeft(int speed);
		/// <summary>
		/// 以指定的差速右转弯
		/// </summary>
		/// <param name="speed">差速比,从1-100</param>
		void TurnRight(int speed);
	}
}
