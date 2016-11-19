using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeWifi;

namespace RPiClient.SensorControl.Devices
{
	class WifiScanner : IWifiScanner
	{
		private NativeWifi.WlanClient Client;
		private Dictionary<String, int> List;
		public WifiScanner()
		{
			Client = new WlanClient();
			List = new Dictionary<string, int>();
			RefrashList();
		}
		public int GetLinkQuality(string ssid)
		{
			RefrashList();
			return List[ssid];
		}

		public string[] ListAccessPoint()
		{
			RefrashList();
			String[] results = List.Keys.ToArray();
			return results;
		}
		private void RefrashList()
		{
			Wlan.WlanBssEntry[] lstWlanBss = Client.Interfaces[0].GetNetworkBssList();
			foreach(var ap in lstWlanBss)
			{
				string ssid = System.Text.Encoding.UTF8.GetString(ap.dot11Ssid.SSID);
				int dbm = CalculateSignalQuality(ap.linkQuality);
				List.Add(ssid, dbm);
			}
		}
		int CalculateSignalQuality(uint Percentage)
		{
			int RSSI = (int)Percentage / 2 - 100;

			return RSSI;
		}
	}
}
