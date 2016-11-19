using System;
using System.Threading.Tasks;

namespace RPiClient.SensorControl.Devices
{
	interface IGyroscope
	{
		event EventHandler<GyroscopeEventArgs> GyroscopeUpdate;

		Task Initialize();
	}
}