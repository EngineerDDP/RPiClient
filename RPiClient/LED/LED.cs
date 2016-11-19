using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.System.Threading;
using Windows.UI.Xaml;

namespace RPiClient.LED
{
	class LEDController : ILEDController, IDisposable
	{
		#region 红蓝LED组
		private static LEDController Device_0;

		/// <summary>
		/// 获取红蓝LED组
		/// </summary>
		public static LEDController LEDGroup_BlueRed
		{
			get
			{
				if (Device_0 == null)
				{
					int[] gpios = { 19, 16, 26, 20 };
					Device_0 = new LEDController(gpios);
				}
				return Device_0;
			}
		}
		#endregion

		/// <summary>
		/// LED引脚控制器
		/// </summary>
		private GpioPin []LED;
		/// <summary>
		/// 
		/// </summary>
		private int Index;
		/// <summary>
		/// 计时器
		/// </summary>
		private ThreadPoolTimer Blink;
		/// <summary>
		/// 创建默认LED控制器
		/// </summary>
		private LEDController(int []gpios)
		{
			LED = new GpioPin[gpios.Length];
			GpioController controller = GpioController.GetDefault();
			for(int i = 0;i<gpios.Length;++i)
			{
				LED[i] = controller.OpenPin(gpios[i]);
				LED[i].SetDriveMode(GpioPinDriveMode.Output);
			}
			Blink = null;
			Index = 0;
		}
		/// <summary>
		/// 按数组下标顺序逐个闪烁LED
		/// </summary>
		/// <param name="timer"></param>
		private void Timer_Tick(ThreadPoolTimer timer)
		{
			LED[Index].Write(GpioPinValue.High);
			Task.Delay(50).Wait();
			LED[Index++].Write(GpioPinValue.Low);
			if (Index == LED.Length)
				Index = 0;
			LED[Index].Write(GpioPinValue.High);
			Task.Delay(50).Wait();
			LED[Index].Write(GpioPinValue.Low);
		}

		public void Open()
		{
			Close();
			Blink = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(Timer_Tick), TimeSpan.FromMilliseconds(150));
		}

		public void Close()
		{
			if (Blink != null)
				Blink.Cancel();
			foreach(GpioPin pin in LED)
				pin.Write(GpioPinValue.Low);
		}

		#region IDisposable Support
		private bool disposedValue = false; // 要检测冗余调用

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach(GpioPin p in LED)
					{
						p.Dispose();
					}
				}

				// TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
				// TODO: 将大型字段设置为 null。

				disposedValue = true;
			}
		}

		// TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
		// ~LEDController() {
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
