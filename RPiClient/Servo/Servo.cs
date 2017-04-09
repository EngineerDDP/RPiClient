using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IoT.Lightning.Providers;
using Windows.Devices;
using Windows.Devices.Pwm;
using RPiClient.Interface;

namespace RPiClient.Servo
{
	/// <summary>
	/// 本地物理舵机设备
	/// </summary>
	class Servo : IDriver
	{
		#region 默认舵机
		private static Servo Device_0;
		public static Servo Default
		{
			get
			{
				if (Device_0 == null)
					Device_0 = new Servo(23, 50, 0.075);
				return Device_0;
			}
		}
		#endregion
		/// <summary>
		/// 信号线
		/// </summary>
		private PwmPin Singal;
		/// <summary>
		/// 频率
		/// </summary>
		private Int32 Frequency;

		/// <summary>
		/// 当前角度
		/// </summary>
		public int Angle { get; private set; }
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="singal"></param>
		/// <param name="freq"></param>
		private Servo(int singal, int freq, double dc)
		{
			Frequency = freq;

			if (LightningProvider.IsLightningEnabled)
			{
				//获取系统Pwm控制器
				Task<IReadOnlyList<PwmController>> t = PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider()).AsTask<IReadOnlyList<PwmController>>();
				t.Wait();
				PwmController controller = t.Result[1];
				//设置频率
				controller.SetDesiredFrequency(Frequency);
				//关联引脚
				Singal = controller.OpenPin(singal);
				Singal.SetActiveDutyCyclePercentage(dc);
				Singal.Start();
			}
		}
		/// <summary>
		/// 设置方波脉冲占空比
		/// </summary>
		/// <param name="dc"></param>
		public void SetDC(double dc)
		{
			if(dc > 0)
				Singal.SetActiveDutyCyclePercentage(dc);
		}

		public void QuickStop()
		{
			Singal.Stop();
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
					Singal.Stop();
					Singal.Dispose();
				}

				// TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
				// TODO: 将大型字段设置为 null。

				disposedValue = true;
			}
		}

		// TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
		// ~Servo() {
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
