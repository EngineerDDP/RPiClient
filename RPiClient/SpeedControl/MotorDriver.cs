using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IoT.Lightning.Providers;
using RPiClient.Interface;
using Windows.Devices;
using Windows.Devices.Gpio;
using Windows.Devices.Pwm;

namespace RPiClient.SpeedControl
{
	/// <summary>
	/// 本地物理电机控制器设备
	/// </summary>
	class MotorDriver : IDriver
	{
		#region 默认电机组
		private static MotorDriver Device_0;

		/// <summary>
		/// 获取默认的电机控制器
		/// </summary>
		/// <returns></returns>
		public static MotorDriver Motor_Default
		{
			get
			{
				if(Device_0 == null)
				{
					int[] gpios = { 17, 18, 27, 22 };
					Device_0 = new MotorDriver(gpios, 50);
				}
				return Device_0;
			}
		}
		#endregion

		/// <summary>
		/// 引脚数
		/// </summary>
		private const int PinNumbers = 4;
		/// <summary>
		/// 引脚
		/// </summary>
		private PwmPin []IN;
		/// <summary>
		/// 频率
		/// </summary>
		private int Frequency;
		
		/// <summary>
		/// 链接到指定的L298N电机控制器
		/// </summary>
		private MotorDriver(int []gpios, int freq)
		{
			IN = new PwmPin[PinNumbers];
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
				for(int i = 0; i < PinNumbers;++i)
				{
					IN[i] = controller.OpenPin(gpios[i]);
					IN[i].SetActiveDutyCyclePercentage(0);
					IN[i].Start();
				}
			}
		}

		/// <summary>
		/// 以指定占空比和电流方向驱动左电机
		/// </summary>
		/// <param name="dutyCycle">占空比</param>
		/// <param name="dir">电流方向</param>
		public void PowerLeft(double dutyCycle, Direction dir)
		{
			switch (dir)
			{
				case Direction.Forward:
					IN[2].SetActiveDutyCyclePercentage(dutyCycle);
					IN[3].SetActiveDutyCyclePercentage(0);
					break;
				case Direction.Backward:
					IN[2].SetActiveDutyCyclePercentage(0);
					IN[3].SetActiveDutyCyclePercentage(dutyCycle);
					break;
				default:
					break;
			}
		}
		/// <summary>
		/// 以指定占空比和电流方向驱动右电机
		/// </summary>
		/// <param name="dutyCycle">占空比</param>
		/// <param name="dir">电流方向</param>
		public void PowerRight(double dutyCycle, Direction dir)
		{
			switch (dir)
			{
				case Direction.Forward:
					IN[0].SetActiveDutyCyclePercentage(dutyCycle);
					IN[1].SetActiveDutyCyclePercentage(0);
					break;
				case Direction.Backward:
					IN[0].SetActiveDutyCyclePercentage(0);
					IN[1].SetActiveDutyCyclePercentage(dutyCycle);
					break;
				default:
					break;
			}
		}
		/// <summary>
		/// 立即停止工作
		/// </summary>
		public void QuickStop()
		{
			PowerLeft(0, Direction.Forward);
			PowerRight(0, Direction.Forward);
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
					int i;
					for (i = 0; i < IN.Length; ++i) 
					{
						IN[i].Stop();
						IN[i].Dispose();
					}
				}

				// TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
				// TODO: 将大型字段设置为 null。

				disposedValue = true;
			}
		}

		// TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
		// ~Motor() {
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
