using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPiClient.LED;
using RPiClient.SensorControl;
using RPiClient.SensorControl.Devices;
using RPiClient.Interface;

namespace RPiClient
{
	/// <summary>
	/// 本地UGV设备门面类
	/// </summary>
	class UGV
	{

		/// <summary>
		/// 运动控制单元
		/// </summary>
		public SpeedControl.SpeedController Speed
		{
			get; private set;
		}
		/// <summary>
		/// 方向控制单元
		/// </summary>
		public Servo.AngleController Angle
		{
			get; private set;
		}
		/// <summary>
		/// 方向信息单元
		/// </summary>
		public Gyroscope.Orientation Orientate
		{
			get; private set;
		}
		/// <summary>
		/// LED控制单元
		/// </summary>
		public LED.LEDController LED
		{
			get; private set;
		}
		/// <summary>
		/// 设备列表
		/// </summary>
		public DeviceList Devs
		{
			get; private set;
		}

		#region 常量
		/// <summary>
		/// LED控制单元数量
		/// </summary>
		private const int NumberofLEDs = 1;
		#endregion
		/// <summary>
		/// 创建默认的UGV控制器
		/// </summary>
		public UGV()
		{
			Devs = new DeviceList();
			Speed = new SpeedControl.SpeedController();
			Angle = new Servo.AngleController();
			Orientate = null;
			LED = new LEDController();
		}
	}
}
