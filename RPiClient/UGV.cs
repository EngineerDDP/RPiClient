using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPiClient.LED;
using RPiClient.MotionControl;
using RPiClient.MotionControl.Devices;
using RPiClient.SensorControl;
using RPiClient.SensorControl.Devices;

namespace RPiClient
{
	/// <summary>
	/// 本地UGV设备门面类
	/// </summary>
	class UGV
	{
		#region 默认UGV控制器
		private static UGV Device_0;


		public static UGV UGV_Default
		{
			get
			{
				if(Device_0 == null)
				{
					Device_0 = new UGV();
				}
				return Device_0;
			}
		}
		#endregion

		/// <summary>
		/// 运动控制单元
		/// </summary>
		private IMovementController MovementControl;
		/// <summary>
		/// 方向控制单元
		/// </summary>
		private IServoDevice ServoControl;
		/// <summary>
		/// LED控制单元
		/// </summary>
		private ILEDController []LEDControl;
		/// <summary>
		/// 传感器控制单元
		/// </summary>
		private IInfoCollector Sensor;

		/// <summary>
		/// 操作超时时间
		/// </summary>
		public Int32 OperationTimeout { get; set; }

		#region 常量
		/// <summary>
		/// LED控制单元数量
		/// </summary>
		private const int NumberofLEDs = 1;
		#endregion
		/// <summary>
		/// 创建默认的UGV控制器
		/// </summary>
		private UGV()
		{
			MovementControl = new Motions(Motor.Motor_Default);
			LEDControl = new ILEDController[NumberofLEDs];
			ServoControl = Servo.Default;
			LEDControl[0] = LEDController.LEDGroup_BlueRed;
			Sensor = new SensorGroup(SuperSonic.Default, Gyroscope.Default);

			OperationTimeout = 1000;
		}
		/// <summary>
		/// 执行运动单元
		/// </summary>
		/// <param name="description">动作说明(Forward,Backward,TurnLeft,TurnRight,Stop)</param>
		/// <param name="speed">执行参数</param>
		public void Move(String description, int speed)
		{
			switch (description)
			{
				case "Forward":
					MovementControl.Forward(speed);
					break;
				case "Backward":
					MovementControl.Backward(speed);
					break;
				case "TurnLeft":
					MovementControl.TurnLeft(speed);
					break;
				case "TurnRight":
					MovementControl.TurnRight(speed);
					break;
				case "Stop":
					MovementControl.Stop();
					break;
			}
		}
		/// <summary>
		/// 调度方向控制单元
		/// </summary>
		/// <param name="angle"></param>
		public void Steering(int angle)
		{
			ServoControl.TurnToAngle(angle);
		}
		/// <summary>
		/// 调度LED控制单元
		/// </summary>
		/// <param name="id"></param>
		public void Light(String description, int id)
		{
			if (id < NumberofLEDs)
			{
				switch (description)
				{
					case "Open":
						LEDControl[id].Open();
						break;
					case "Close":
						LEDControl[id].Close();
						break;
				}
			}
		}
		/// <summary>
		/// 调度传感器控制单元,返回值为Double型
		/// </summary>
		/// <param name="description"></param>
		/// <returns></returns>
		public Dictionary<string, string> GetSensorInfo(String description)
		{
			Dictionary<string, string> paras = new Dictionary<string, string>();
			switch (description)
			{
				case "SuperSonic":
					double dis = -1;
					do
					{
						dis = Sensor.GetDistance();
					} while (dis == -1);
					paras.Add("Distance", Convert.ToString(dis));
					break;
				case "Acceleration":
					paras.Add("X", Convert.ToString(Sensor.AccelerationX));
					paras.Add("Y", Convert.ToString(Sensor.AccelerationY));
					paras.Add("Z", Convert.ToString(Sensor.AccelerationZ));
					break;
				case "GetDirection":
					paras.Add("Value", Convert.ToString(Sensor.Direction));
					break;
				case "Shift":
					paras.Add("Value", Convert.ToString(Sensor.Displacement));
					break;
				case "PowerSpeed":
					paras.Add("Left", Convert.ToString(MovementControl.LeftSpeed));
					paras.Add("Right", Convert.ToString(MovementControl.RightSpeed));
					break;
				case "ServoAngle":
					paras.Add("Value", Convert.ToString(ServoControl.Angle));
					break;
			}
			return paras;
		}
		/// <summary>
		/// 转向指定的角度或行动指定的距离
		/// </summary>
		/// <param name="description">动作说明(Forward,Backward,TurnLeft,TurnRight)</param>
		/// <param name="parameter">行动参数</param>
		public void MoveAccurately(String description, int parameter)
		{
			//打开超时计时器
			Stopwatch clock = new Stopwatch();
			clock.Start();
			//角度基准值
			double dir = Sensor.Direction;

			switch (description)
			{
				case "Forward":
					//MovementControl.Forward(100);
					break;
				case "Backward":
					//MovementControl.Backward(100);
					break;
				case "TurnLeft":
					parameter = (parameter + 360) % 360;
					MovementControl.TurnLeft(30);
					while (Math.Abs(Sensor.Direction - dir) < parameter)
					{
						if (clock.ElapsedMilliseconds > OperationTimeout)
							break;
						System.Diagnostics.Debug.WriteLine(Sensor.Direction);
					}
					MovementControl.Stop();
					break;
				case "TurnRight":
					parameter = (parameter + 360) % 360;
					MovementControl.TurnRight(30);
					while (Math.Abs(Sensor.Direction - dir) < parameter)
					{
						if (clock.ElapsedMilliseconds > OperationTimeout)
							break;
						System.Diagnostics.Debug.WriteLine(Sensor.Direction);
					}
					MovementControl.Stop();
					break;
			}
		}
	}
}
