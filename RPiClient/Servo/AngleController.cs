using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.Servo
{
	/// <summary>
	/// 角度控制器
	/// </summary>
	class AngleController
	{
		/// <summary>
		/// 角度控制设备
		/// </summary>
		private ViewAngle Device;
		/// <summary>
		/// 初始化设备
		/// </summary>
		public AngleController()
		{
			Device = new ViewAngle(Servo.Default);
		}
		/// <summary>
		/// 转向指定角度
		/// </summary>
		/// <param name="angle"></param>
		public void SetAngle(int angle)
		{
			Device.TurnToAngle(angle);
		}
		/// <summary>
		/// 转指定角度
		/// </summary>
		/// <param name="angle"></param>
		public void Turn(int angle)
		{
			Device.TurnToAngle(Device.AngleValue + angle);
		}
	}
}
