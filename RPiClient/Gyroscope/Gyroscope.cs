using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPiClient.Interface;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace RPiClient.Gyroscope
{

	/// <summary>
	/// 常量
	/// </summary>
	public class Constants
	{
		public const byte Address = 0x68;
		public const byte PwrMgmt1 = 0x6B;
		public const byte SmplrtDiv = 0x19;
		public const byte Config = 0x1A;
		public const byte GyroConfig = 0x1B;
		public const byte AccelConfig = 0x1C;
		public const byte FifoEn = 0x23;
		public const byte IntEnable = 0x38;
		public const byte IntStatus = 0x3A;
		public const byte UserCtrl = 0x6A;
		public const byte FifoCount = 0x72;
		public const byte FifoRW = 0x74;
		public const int SensorBytes = 12;

		public const byte XGyro_Offset = 0x13;
		public const byte YGyro_Offset = 0x15;
		public const byte ZGyro_Offset = 0x17;
	}

	class Gyroscope : IDriver
	{
		#region 默认设备
		private static Gyroscope Device_0;
		public static Gyroscope Default
		{
			get
			{
				if (Device_0 == null)
					Device_0 = new Gyroscope(4,100);
				return Device_0;
			}
		}
		#endregion

		private GpioPin InterruptPin;
		private I2CReadWrite MPU6050;
		private Int32 SampleRate;

		public event EventHandler<GyroscopeEventArgs> GyroscopeUpdate;

		/// <summary>
		/// 使用指定的Int针脚编号和刷新率创建MPU6050交换控制器
		/// </summary>
		/// <param name="Int"></param>
		/// <param name="sampleRate"></param>
		private Gyroscope(int Int, int sampleRate)
		{
			SampleRate = sampleRate;
			GpioController controller = GpioController.GetDefault();
			InterruptPin = controller.OpenPin(Int);
			InterruptPin.Write(GpioPinValue.Low);
			InterruptPin.SetDriveMode(GpioPinDriveMode.Input);

			InterruptPin.ValueChanged += InterruptPin_ValueChanged;
		}

		/// <summary>
		/// 响应
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void InterruptPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
		{
			if (MPU6050 != null)
			{
				int interruptStatus = MPU6050.ReadByte(Constants.IntStatus);

				if ((interruptStatus & 0x10) != 0)
				{
					MPU6050.WriteByte(Constants.UserCtrl, 0x44); // 重置并设置FIFO
				}
				if ((interruptStatus & 0x01) != 0)
				{
					var values = new List<MpuSensorValue>();

					int count = MPU6050.ReadWord(Constants.FifoCount);

					while (count >= Constants.SensorBytes)
					{
						var data = MPU6050.ReadBytes(Constants.FifoRW, Constants.SensorBytes);
						count -= Constants.SensorBytes;

						var xa = (short)(data[0] << 8 | data[1]);
						var ya = (short)(data[2] << 8 | data[3]);
						var za = (short)(data[4] << 8 | data[5]);

						var xg = (short)(data[6] << 8 | data[7]);
						var yg = (short)(data[8] << 8 | data[9]);
						var zg = (short)(data[10] << 8 | data[11]);

						var sv = new MpuSensorValue
						{
							AccelerationX = xa / (float)16384,
							AccelerationY = ya / (float)16384,
							AccelerationZ = za / (float)16384,
							GyroX = xg / (float)131,
							GyroY = yg / (float)131,
							GyroZ = zg / (float)131
						};
						values.Add(sv);
					}

					GyroscopeEventArgs argv = new GyroscopeEventArgs(interruptStatus, (1.0 / SampleRate), values.ToArray());

					if (argv.Values.Length > 0)
					{
						GyroscopeUpdate?.Invoke(this, argv);
					}
				}
			}
		}

		/// <summary>
		/// 初始化
		/// </summary>
		/// <returns></returns>
		public async Task Initialize()
		{
			I2cController controller = await I2cController.GetDefaultAsync();
			I2cConnectionSettings setting = new I2cConnectionSettings(Constants.Address)
			{
				BusSpeed = I2cBusSpeed.FastMode,
				SharingMode = I2cSharingMode.Exclusive
			};

			MPU6050 = new I2CReadWrite(controller.GetDevice(setting));

			await Task.Delay(3);

			MPU6050.WriteByte(Constants.PwrMgmt1, 0x80); // reset the device
			await Task.Delay(100);
			MPU6050.WriteByte(Constants.PwrMgmt1, 0x2);
			MPU6050.WriteByte(Constants.UserCtrl, 0x04); //reset fifo

			MPU6050.WriteByte(Constants.PwrMgmt1, 1); // clock source = gyro x
			MPU6050.WriteByte(Constants.GyroConfig, 0); // +/- 250 degrees sec
			MPU6050.WriteByte(Constants.AccelConfig, 0); // +/- 2g

			MPU6050.WriteByte(Constants.Config, 1); // 184 Hz, 2ms delay
			MPU6050.WriteByte(Constants.SmplrtDiv, (byte)((1000 / SampleRate) - 1)); // set rate 50Hz
			MPU6050.WriteByte(Constants.FifoEn, 0x78); // enable accel and gyro to read into fifo
			MPU6050.WriteByte(Constants.UserCtrl, 0x40); // reset and enable fifo
			MPU6050.WriteByte(Constants.IntEnable, 0x1);

			MPU6050.WriteWord(Constants.XGyro_Offset, 0x00B5);
			MPU6050.WriteWord(Constants.YGyro_Offset, 0xFFDF);
			MPU6050.WriteWord(Constants.ZGyro_Offset, 0xFFE1);
		}

		public void QuickStop()
		{
			InterruptPin.ValueChanged -= InterruptPin_ValueChanged;
		}

		#region IDisposable Support
		private bool disposedValue = false; // 要检测冗余调用

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: 释放托管状态(托管对象)。
					InterruptPin.Dispose();
				}

				// TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
				// TODO: 将大型字段设置为 null。

				disposedValue = true;
			}
		}

		// TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
		// ~Gyroscope() {
		//   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
		//   Dispose(false);
		// }

		// 添加此代码以正确实现可处置模式。
		public void Dispose()
		{
			// 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
			Dispose(true);
			// TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
