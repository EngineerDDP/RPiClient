using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.Gyroscope
{
	class GyroscopeEventArgs : EventArgs
	{
		public GyroscopeEventArgs(int status, double samplePeriod, MpuSensorValue[] values)
		{
			Status = status;
			SamplePeriod = samplePeriod;
			Values = values;
		}

		public int Status { get; private set; }
		public double SamplePeriod { get; private set; }
		public MpuSensorValue[] Values { get; private set; }
	}
	public sealed class MpuSensorValue
	{
		public float AccelerationX { get; set; }
		public float AccelerationY { get; set; }
		public float AccelerationZ { get; set; }
		public float GyroX { get; set; }
		public float GyroY { get; set; }
		public float GyroZ { get; set; }
	}
}
