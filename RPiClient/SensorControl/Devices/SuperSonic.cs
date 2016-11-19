using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace RPiClient.SensorControl.Devices
{
	/// <summary>
	/// 本地物理超声波设备
	/// </summary>
	class SuperSonic : ISuperSonic, IDisposable
	{
		#region 默认超声波
		private static SuperSonic Device_0;

		public static SuperSonic Default
		{
			get
			{
				if(Device_0 == null)
				{
					Device_0 = new SuperSonic(24, 25);
				}
				return Device_0;
			}
		}
		#endregion
		private const long MeasuringTimeOut = 10000000;
		/// <summary>
		/// Trig引脚
		/// </summary>
		private GpioPin Trig;
		/// <summary>
		/// Echo引脚
		/// </summary>
		private GpioPin Echo;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="trig"></param>
		/// <param name="echo"></param>
		private SuperSonic(int trig, int echo)
		{
			GpioController controller = GpioController.GetDefault();
			Trig = controller.OpenPin(trig);
			Trig.SetDriveMode(GpioPinDriveMode.Output);
			Trig.Write(GpioPinValue.Low);
			Echo = controller.OpenPin(echo);
			Echo.SetDriveMode(GpioPinDriveMode.Input);
		}
		/// <summary>
		/// 测距
		/// </summary>
		/// <returns></returns>
		public double GetDistance()
		{
			//计时器
			Stopwatch timeclock = new Stopwatch();
			Stopwatch disclock = new Stopwatch();

			//超时计算开始
			timeclock.Start();

			//写高电平
			Trig.Write(GpioPinValue.High);
			while (timeclock.ElapsedTicks < 100)
				;
			Trig.Write(GpioPinValue.Low);

			//等待上升沿反馈
			while (Echo.Read() == GpioPinValue.Low)
			{
				if (timeclock.ElapsedTicks > MeasuringTimeOut)
					return -1;
			}
			//开始计时
			disclock.Start();
			while (Echo.Read() == GpioPinValue.High)
			{
				if (timeclock.ElapsedTicks > MeasuringTimeOut)
					return -1;
			}
			//结束计时
			disclock.Stop();
			timeclock.Stop();
			//返回结果
			return (17.0 * disclock.ElapsedTicks / 1000000);
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
					Trig.Dispose();
					Echo.Dispose();
					Trig = null;
					Echo = null;
				}

				// TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
				// TODO: 将大型字段设置为 null。

				disposedValue = true;
			}
		}

		// TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
		// ~SuperSonic() {
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
