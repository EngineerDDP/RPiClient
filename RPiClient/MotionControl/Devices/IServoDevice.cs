using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.MotionControl.Devices
{
	/// <summary>
	/// 逻辑舵机设备接口
	/// </summary>
	interface IServoDevice
	{
		/// <summary>
		/// 获取当前方向
		/// </summary>
		int Angle { get;}
		/// <summary>
		/// 转向指定方向
		/// </summary>
		/// <param name="angle"></param>
		void TurnToAngle(int angle);
	}
}
