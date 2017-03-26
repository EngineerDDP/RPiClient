using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiClient.SensorControl.Devices
{
	interface IWifiScanner
	{
		int GetLinkQuality(string ssid);
		string[] ListAccessPoint();
	}
}
