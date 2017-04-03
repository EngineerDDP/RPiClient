using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.Gyroscope
{
	class Orientation
	{
		/// <summary>
		/// 陀螺仪驱动
		/// </summary>
		private Gyroscope Driver;
		/// <summary>
		/// 三元欧拉角
		/// </summary>
		private double[] EulerAngles;
		/// <summary>
		/// 默认构造方法
		/// </summary>
		/// <param name="gyro"></param>
		public Orientation(Gyroscope gyro)
		{
			Driver = gyro;
			Driver.GyroscopeUpdate += Driver_GyroscopeUpdate;
		}

		private void Driver_GyroscopeUpdate(object sender, GyroscopeEventArgs e)
		{
			
		}
	}
}
