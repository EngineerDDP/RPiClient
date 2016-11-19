using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPU6050.ValueType;

namespace MPU6050
{
	public class GyroscopeSensorEventArgs : EventArgs
	{
		public GyroscopeSensorEventArgs(YawPitchRoll value, GravityVector gravity, Quaternion _Quaternion, int simpleRate)
		{
			Value = value;
			Gravity = gravity;
			this._Quaternion = _Quaternion;
			SimpleRate = simpleRate;
		}

		/// <summary>
		/// YPW数据
		/// </summary>
		public YawPitchRoll Value { get; private set; }
		public GravityVector Gravity { get; private set; }
		public Quaternion _Quaternion { get; private set; }
		/// <summary>
		/// 采样率
		/// </summary>
		public int SimpleRate { get; private set; }
	}
}
