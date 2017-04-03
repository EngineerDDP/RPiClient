using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient
{
	/// <summary>
	/// 通过已有的功能构造复杂的功能序列
	/// </summary>
	class UGVManager
	{
		/// <summary>
		/// 无人机设备
		/// </summary>
		private UGV Dev;
		public UGVManager()
		{
			Dev = new UGV();
		}

	}
}
