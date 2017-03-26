using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.SensorControl
{
	/// <summary>
	/// 逻辑信息采集单元
	/// </summary>
	interface IInfoCollector
	{
		/// <summary>
		/// 获取距离
		/// </summary>
		/// <returns></returns>
		double GetDistance();
		/// <summary>
		/// 获取偏向角
		/// </summary>
		double Direction { get; }
		/// <summary>
		/// 获取X轴向总位移
		/// </summary>
		double Displacement { get; }
		/// <summary>
		/// 获取X轴向加速度
		/// </summary>
		/// <returns></returns>
		double AccelerationX { get; }
		/// <summary>
		/// 获取Y轴向加速度
		/// </summary>
		/// <returns></returns>
		double AccelerationY { get; }
		/// <summary>
		/// 获取Z轴向加速度
		/// </summary>
		/// <returns></returns>
		double AccelerationZ { get; }
	}
}
