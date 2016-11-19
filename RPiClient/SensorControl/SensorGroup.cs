using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPiClient.SensorControl.Devices;

namespace RPiClient.SensorControl
{
	/// <summary>
	/// 虚拟信息采集单元
	/// </summary>
	class SensorGroup : IInfoCollector
	{
		private const float K1 = 0.05f;
		/// <summary>
		/// 逻辑超声波单元
		/// </summary>
		private ISuperSonic Sonic;
		/// <summary>
		/// 逻辑陀螺仪单元
		/// </summary>
		private IGyroscope Gyroscope;
		/// <summary>
		/// X轴向速度
		/// </summary>
		private double Xspeed;

		public double Direction
		{
			get;private set;
		}

		public double Displacement
		{
			get;private set;
		}
		public double Pitch
		{
			get;private set;
		}
		public double Roll
		{
			get;private set;
		}
		public double AccelerationX
		{
			get;private set;
		}

		public double AccelerationY
		{
			get;private set;
		}

		public double AccelerationZ
		{
			get;private set;
		}
		

		public SensorGroup(ISuperSonic sonic, IGyroscope gyrescope)
		{
			Sonic = sonic;

			Gyroscope = gyrescope;
			Gyroscope.Initialize();
			Gyroscope.GyroscopeUpdate += Gyroscope_OnInterrupt;
		}

		private void Gyroscope_OnInterrupt(object sender, GyroscopeEventArgs e)
		{
			foreach (MpuSensorValue v in e.Values)
			{
				if (Math.Abs(v.GyroZ) > 0.5)
					Direction = Direction + v.GyroZ * e.SamplePeriod;
				if (Direction >= 360)
					Direction = Direction - 360;
				else if (Direction < 0)
					Direction = Direction + 360;
			}

			AccelerationX = -e.Values[0].AccelerationX;
			AccelerationY = e.Values[0].AccelerationY;
			AccelerationZ = e.Values[0].AccelerationZ;

			if (Math.Abs(AccelerationX) > 0.02)
			{
				Xspeed = Xspeed + AccelerationX * e.SamplePeriod;
				Displacement = Displacement + Xspeed * e.SamplePeriod;
			}
		}

		public double GetDistance()
		{
			double dis = Sonic.GetDistance();
			return dis;
		}

		public void ResetAngle()
		{
			Direction = 0;
		}
	}
}
