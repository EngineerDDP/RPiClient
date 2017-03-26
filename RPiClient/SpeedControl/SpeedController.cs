using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.SpeedControl
{
	/// <summary>
	/// 方向
	/// </summary>
	enum Direction
	{
		/// <summary>
		/// 前向
		/// </summary>
		Forward,
		/// <summary>
		/// 后向
		/// </summary>
		Backward,
		/// <summary>
		/// 左向
		/// </summary>
		Left,
		/// <summary>
		/// 右向
		/// </summary>
		Right,
		/// <summary>
		/// 无
		/// </summary>
		None
	}
	/// <summary>
	/// 速度控制器
	/// </summary>
	class SpeedController
	{
		/// <summary>
		/// 电动机
		/// </summary>
		private Motions Device;
		/// <summary>
		/// 速度比例
		/// </summary>
		private double Speed;
		/// <summary>
		/// 当前运动方向
		/// </summary>
		private Direction Dir;
		/// <summary>
		/// 构造函数
		/// </summary>
		public SpeedController()
		{
			Device = new Motions(MotorDriver.Motor_Default);
			Speed = 0;
			Dir = Direction.None;
		}
		/// <summary>
		/// 设定当前速度比率
		/// </summary>
		/// <param name="speed"></param>
		public void SetSpeed(float speed)
		{
			if (speed > 0)
				Speed = speed;
			else
				throw new ArgumentException("数值超出允许的范围", "speed");
		}
		/// <summary>
		/// 依照指定角度立即改变当前运动方向
		/// </summary>
		/// <param name="dir"></param>
		public void Move(Direction dir)
		{
			Dir = dir;
			double l = Speed * 100, r = Speed * 100;
			switch (dir)
			{
				case Direction.Forward:
					break;
				case Direction.Backward:
					l = -l;
					r = -r;
					break;
				case Direction.Left:
					l = -l / 2;
					r = r / 2;
					break;
				case Direction.Right:
					l = l / 2;
					r = -r / 2;
					break;
				case Direction.None:
				default:
					l = r = 0;
					break;
			}
			Device.Power((int)l, (int)r);
		}
		/// <summary>
		/// 立即停止运动
		/// </summary>
		public void Hold()
		{
			Device.Stop();
		}
		/// <summary>
		/// 减速至停止运动
		/// </summary>
		/// <param name="msec">减速过程所需时间，单位为毫秒</param>
		public async Task StopAsync(int msec)
		{
			int i = msec / (int)(100 * Speed);		//每次减速所需间隔时间

			await Task.Run(() =>
			{
				while(Speed != 0)
				{
					Speed -= 0.01;
					Move(Dir);

					Task.Delay(i).Wait();
				}
			});
		}
		/// <summary>
		/// 加速至额定速度
		/// </summary>
		/// <param name="dir">方向</param>
		/// <param name="msec">加速过程所需时间，单位为毫秒</param>
		public async Task StartAsync(Direction dir,int msec)
		{
			int i = msec / (int)(100 * Speed);

			await Task.Run(() =>
			{
				float j = 0;
				while(j < Speed)
				{
					j += 0.01f;
					SetSpeed(j);
					Move(dir);

					Task.Delay(i).Wait();
				}
			});
		}
	}
}
