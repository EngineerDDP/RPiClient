using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPiClient.MotionControl.Devices;

namespace RPiClient.MotionControl
{
	/// <summary>
	/// 虚拟运动控制单元
	/// </summary>
	class Motions : IMovementController
	{
		/// <summary>
		/// 逻辑电机驱动器
		/// </summary>
		private IMotorDevice Devices;
		public int LeftSpeed
		{
			get;private set;
		}
		public int RightSpeed
		{
			get;private set;
		}
		/// <summary>
		/// 从指定的逻辑电动机驱动器创建运动控制单元
		/// </summary>
		/// <param name="device">设备</param>
		public Motions(IMotorDevice device)
		{
			Devices = device;
			LeftSpeed = 0;
			RightSpeed = 0;
		}
		public void Backward(int speed)
		{
			if(speed <= 100 && speed >= 1)
			{
				Devices.PowerLeft(1.0 * speed / 100, Direction.Backward);
				Devices.PowerRight(1.0 * speed / 100, Direction.Backward);
				RightSpeed = -speed;
				LeftSpeed = -speed;
			}
			else
			{
				throw new ArgumentOutOfRangeException("speed", "速度不在允许的范围内");
			}
		}

		public void Forward(int speed)
		{
			if (speed <= 100 && speed >= 1)
			{
				Devices.PowerLeft(1.0 * speed / 100, Direction.Forward);
				Devices.PowerRight(1.0 * speed / 100, Direction.Forward);
				RightSpeed = speed;
				LeftSpeed = speed;
			}
			else
			{
				throw new ArgumentOutOfRangeException("speed", "速度不在允许的范围内");
			}
		}

		public void Stop()
		{
			Devices.PowerLeft(0, Direction.Forward);
			Devices.PowerRight(0, Direction.Forward);
			RightSpeed = 0;
			LeftSpeed = 0;
		}

		public void Stop(int millisecond)
		{
			throw new NotImplementedException();
		}

		public void TurnLeft(int speed)
		{
			if (speed <= 100 && speed >= 1)
			{
				RightSpeed = LeftSpeed;
				if (speed * 2 > 100 - RightSpeed)
				{
					LeftSpeed = LeftSpeed - (speed * 2 - 100 + RightSpeed);
					RightSpeed = 100;
				}
				else
				{
					RightSpeed = RightSpeed + speed * 2;
				}
				if (LeftSpeed >= 0)
					Devices.PowerLeft(1.0 * LeftSpeed / 100, Direction.Forward);
				else
					Devices.PowerLeft(-1.0 * LeftSpeed / 100, Direction.Backward);
				if (RightSpeed >= 0)
					Devices.PowerRight(1.0 * RightSpeed / 100, Direction.Forward);
				else
					Devices.PowerRight(-1.0 * RightSpeed / 100, Direction.Backward);
			}
			else
			{
				throw new ArgumentOutOfRangeException("speed", "速度不在允许的范围内");
			}
		}

		public void TurnRight(int speed)
		{
			if (speed <= 100 && speed >= 1)
			{
				LeftSpeed = RightSpeed;
				if (speed * 2 > 100 - LeftSpeed)
				{
					RightSpeed = RightSpeed - (speed * 2 - 100 + LeftSpeed);
					LeftSpeed = 100;
				}
				else
				{
					LeftSpeed = LeftSpeed + speed * 2;
				}
				if (RightSpeed >= 0)
					Devices.PowerRight(1.0 * RightSpeed / 100, Direction.Forward);
				else
					Devices.PowerRight(-1.0 * RightSpeed / 100, Direction.Backward);
				if (LeftSpeed >= 0)
					Devices.PowerLeft(1.0 * LeftSpeed / 100, Direction.Forward);
				else
					Devices.PowerLeft(-1.0 * LeftSpeed / 100, Direction.Backward);
			}
			else
			{
				throw new ArgumentOutOfRangeException("speed", "速度不在允许的范围内");
			}
		}
	}
}
