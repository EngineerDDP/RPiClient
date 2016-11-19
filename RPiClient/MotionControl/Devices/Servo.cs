using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IoT.Lightning.Providers;
using Windows.Devices;
using Windows.Devices.Pwm;

namespace RPiClient.MotionControl.Devices
{
	/// <summary>
	/// 本地物理舵机设备
	/// </summary>
	class Servo : IServoDevice, IDisposable
	{
		#region 默认舵机
		private static Servo Device_0;
		public static Servo Default
		{
			get
			{
				if (Device_0 == null)
					Device_0 = new Servo(23, 50);
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
		/// 最小占空比
		/// </summary>
		private const double MinimumDC = 0.03;
		/// <summary>
		/// 最大占空比
		/// </summary>
		private const double MaxmumDC = 0.12;
		/// <summary>
		/// 可用角度
		/// </summary>
		private const Int32 AngleRange = 180;
		/// <summary>
		/// 当前角度
		/// </summary>
		public int Angle { get; private set; }
		private Servo(int singal, int freq)
		{
			Frequency = freq;

			if (LightningProvider.IsLightningEnabled)
			{
				//获取控制器
				LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
				//获取系统Pwm控制器
				Task<IReadOnlyList<PwmController>> t = PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider()).AsTask<IReadOnlyList<PwmController>>();
				t.Wait();
				PwmController controller = t.Result[1];
				//设置频率
				controller.SetDesiredFrequency(Frequency);
				//关联引脚
				Singal = controller.OpenPin(singal);
				Singal.SetActiveDutyCyclePercentage((MaxmumDC + MinimumDC) / 2);
				Singal.Start();
			}
		}
		public void TurnToAngle(int angle)
		{
			if (angle <= AngleRange && angle >= 0)
			{
				double dc = MinimumDC + (MaxmumDC - MinimumDC) * angle / AngleRange;
				if (dc > 0)
				{
					Singal.SetActiveDutyCyclePercentage(dc);
					Angle = angle;
				}
			}
		}

		public void Dispose()
		{
			Singal.Stop();
			Singal.Dispose();
		}
	}
}
