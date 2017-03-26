using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.SensorControl.Devices
{
	/// <summary>
	/// 逻辑超声波传感器
	/// </summary>
	interface ISuperSonic
	{
		/// <summary>
		/// 获取距离
		/// </summary>
		/// <returns></returns>
		double GetDistance();
	}
}
