using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.System.Threading;

namespace RPiClient.SensorControl.Devices
{
	class WifiScanner
	{
		/// <summary>
		/// Wifi适配器
		/// </summary>
		private WiFiAdapter Client;
		/// <summary>
		/// RSSI列表，保存了所有的SSID和更新后的RSSI信息
		/// </summary>
		private Dictionary<String, int> RSSIList;
		/// <summary>
		/// 获取指定SSID的RSSI信息
		/// </summary>
		public int LookUpForRSSI(String ssid)
		{
			int rssi = -100;
			RSSIList.TryGetValue(ssid, out rssi);
			return rssi;
		}
		public WifiScanner()
		{
			//等待初始化并开始扫描更新Wifi
			ThreadPoolTimer.CreateTimer((ThreadPoolTimer t) => { Init().Wait(); }, TimeSpan.FromMilliseconds(100));
			RSSIList = new Dictionary<string, int>();
		}
		/// <summary>
		/// 初始化WifiAdapter
		/// </summary>
		private async System.Threading.Tasks.Task Init()
		{
			Client = (await WiFiAdapter.FindAllAdaptersAsync())[0];

			Client.AvailableNetworksChanged += Client_AvailableNetworksChanged;
			await Client.ScanAsync();
		}
		/// <summary>
		/// 更新RSSI信息列表
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void Client_AvailableNetworksChanged(WiFiAdapter sender, object args)
		{
			RSSIList.Clear();
			foreach(var i in sender.NetworkReport.AvailableNetworks)
			{
				RSSIList.Add(i.Ssid, (int)i.NetworkRssiInDecibelMilliwatts);
			}
			Client.ScanAsync().AsTask().Wait();
		}
	}
}
