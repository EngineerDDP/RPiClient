using System;

namespace MPU6050
{
	public interface IGyroscope
	{
		event EventHandler<GyroscopeSensorEventArgs> OnInterrupt;

		void Initialize();
	}
}