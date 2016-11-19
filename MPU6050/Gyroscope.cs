using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MPU6050.ValueType;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace MPU6050
{
    public class Gyroscope : IDisposable, IGyroscope
	{
		public event EventHandler<GyroscopeSensorEventArgs> OnInterrupt;

		private MPU6050 Device;
		private int SimpleRate;

		public Gyroscope(MPU6050 device)
		{
			Device = device;
		}

		public async void Initialize()
		{
			await Device.Initialize();
			Device.DmpInitialize();

			Device.setXGyroOffset(0x00B5);
			Device.setYGyroOffset(0xFFDF);
			Device.setZGyroOffset(0xFFE1);
			Device.setZAccelOffset(0);

			Device.setDMPEnabled(0x01);

			SimpleRate = 1000 / (Device.getRate() + 1);

			Device.OnInterrupt += Device_OnInterrupt;
		}

		private void Device_OnInterrupt(GpioPin sender, GpioPinValueChangedEventArgs args)
		{
			if (Device == null)
				return;
			int interruptStatus = Device.getIntStatus();

			if ((interruptStatus & 0x10) != 0)
				Device.resetFIFO();
			else if((interruptStatus & 0x02) != 0)
			{ 
				int count = Device.getFIFOCount();
				while(count >= Constants.DMPPacketSize)
				{
					byte[] buffer = Device.getFIFOBytes(Constants.DMPPacketSize);
					count -= Constants.DMPPacketSize;

					Quaternion q = Device.dmpGetQuaternion(buffer);
					GravityVector g = Device.dmpGetGravity(q);
					YawPitchRoll y = Device.dmpGetYawPitchRoll(q, g);
					GyroscopeSensorEventArgs argv = new GyroscopeSensorEventArgs(y, g, q, SimpleRate);

					OnInterrupt?.Invoke(this, argv);
				}
			}
		}

		public void Dispose()
		{
			Device.Dispose();
		}
	}
}
