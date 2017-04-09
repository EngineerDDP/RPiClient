using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.System.Threading;
using Windows.UI.Xaml;
using RPiClient.Interface;

namespace RPiClient.LED
{
	class LEDDriver : IDriver
	{
		#region 红蓝LED组
		private static LEDDriver Device_0;

		/// <summary>
		/// 获取红蓝LED组
		/// </summary>
		public static LEDDriver LEDGroup_BlueRed
		{
			get
			{
				if (Device_0 == null)
				{
					int[] gpios = { 19, 16, 26, 20 };
					Device_0 = new LEDDriver(gpios);
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
		/// 计时器
		/// </summary>
		private ThreadPoolTimer Blink;
		/// <summary>
		/// 当前状态
		/// </summary>
		private bool[] Status;
		/// <summary>
		/// 变化方案
		/// </summary>
		private Func<bool[], bool[]> Change;
		/// <summary>
		/// 变化间隔
		/// </summary>
		private TimeSpan Interval;
		/// <summary>
		/// 创建LED控制器
		/// </summary>
		public LEDDriver(int []gpios)
		{
			LED = new GpioPin[gpios.Length];
			Status = new bool[gpios.Length];
			GpioController controller = GpioController.GetDefault();
			for(int i = 0;i<gpios.Length;++i)
			{
				Status[i] = false;
				LED[i] = controller.OpenPin(gpios[i]);
				LED[i].SetDriveMode(GpioPinDriveMode.Output);
			}

			Change = (bool [] status) => {
				return status;
			};
			Interval = TimeSpan.FromMilliseconds(100);
			Blink = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, Interval);
		}
		/// <summary>
		/// 获取可用LED数量
		/// </summary>
		/// <returns></returns>
		public int GetLEDNumber()
		{
			return LED.Length;
		}
		/// <summary>
		/// 设置LED状态
		/// </summary>
		/// <param name="status">LED状态，0为灭，1为亮</param>
		public void SetStatus(bool []status)
		{
			int i = 0;
			for(i = 0;i < status.Length && i < LED.Length;++i)
			{
				Status[i] = status[i];
				LED[i].Write(Status[i] ? GpioPinValue.High : GpioPinValue.Low);
			}
		}
		/// <summary>
		/// 设置LED变化方案
		/// </summary>
		/// <param name="lambda">用于变化方案的函数</param>
		/// <param name="msec">用于调用变换方案的延时</param>
		public void SetChange(Func<bool [],bool []> lambda, int msec)
		{
			Change = lambda;
			Interval = TimeSpan.FromMilliseconds(msec);
		}
		/// <summary>
		/// 按要求改变LED状态
		/// </summary>
		/// <param name="timer"></param>
		private void Timer_Tick(ThreadPoolTimer timer)
		{
			if(timer.Period != Interval)
			{
				timer.Cancel();
				timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, Interval);
			}
			else
			{
				SetStatus(Change(Status));
			}
		}

		/// <summary>
		/// 快速停止接口
		/// </summary>
		public void QuickStop()
		{
			Change = (bool[] s) =>
			{
				return new bool[] { false, false, false, false };
			};
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
