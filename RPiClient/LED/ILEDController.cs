using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.LED
{
	interface ILEDController
	{
		/// <summary>
		/// 打开LED
		/// </summary>
		void Open();
		/// <summary>
		/// 关闭LED
		/// </summary>
		void Close();
	}
}
