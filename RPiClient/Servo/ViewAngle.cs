using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.Servo
{
	/// <summary>
	/// 转向设备
	/// </summary>
	class ViewAngle
	{
		/// <summary>
		/// 舵机
		/// </summary>
		private Servo Driver;
		/// <summary>
		/// 当前指向角度
		/// </summary>
		private int Angle;
		public int AngleValue {
			get
			{
				return Angle;
			}
		}
		/// <summary>
		/// 最小占空比
		/// </summary>
		private const double MinimumDC = 0.03;
		/// <summary>
		/// 最大占空比
		/// </summary>
		private const double MaxmumDC = 0.12;
		/// <summary>
		/// 可用角度
		/// </summary>
		private const Int32 AngleRange = 180;
		/// <summary>
		/// 使用舵机单元构造角度控制器
		/// </summary>
		/// <param name="servo"></param>
		public ViewAngle(Servo servo)
		{
			Driver = servo;
		}
		/// <summary>
		/// 转向指定的角度
		/// </summary>
		/// <param name="angle"></param>
		public void TurnToAngle(int angle)
		{
			if (angle <= AngleRange && angle >= 0)
			{
				double dc = MinimumDC + (MaxmumDC - MinimumDC) * angle / AngleRange;
				Driver.SetDC(dc);
				Angle = angle;
			}
		}
	}
}
