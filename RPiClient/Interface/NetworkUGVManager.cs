using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESocketModule.ClientEntry;

namespace RPiClient
{
	/// <summary>
	/// 网络UGV控制器
	/// </summary>
	class NetworkUGVManager : IUGVManager
	{
		private IESocketClient NetClient;
		private UGV Device;
		public NetworkUGVManager()
		{
			NetClient = new ESocketClient();
			NetClient.OnBufferReceived += NetClient_OnBufferReceived;
			NetClient.OnConnectionLost += NetClient_OnConnectionLost;

		}

		private async void NetClient_OnConnectionLost(object sender, ConnectionLostEventArgs e)
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine("连接丢失");
#endif
			await Connect();
		}
		/// <summary>
		/// 接收服务器消息并处理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NetClient_OnBufferReceived(object sender, ESocketModule.BufferSendManage.Delegates.BufferReceivedEventArgs e)
		{
			switch (e.Buffer.BufferName)
			{
				case "Move":
					int speed = Convert.ToInt32(e.Buffer.UserParameters["Speed"]);
					Device?.Move(e.Buffer.UserParameters["Direction"], speed);
					break;
				case "Light":
					int id = Convert.ToInt32(e.Buffer.UserParameters["ID"]);
					Device?.Light(e.Buffer.UserParameters["State"], id);
					break;
				case "Servo":
					int angle = Convert.ToInt32(e.Buffer.UserParameters["Angle"]);
					Device?.Steering(angle);
					break;
				case "Sensor":
					var para = Device?.GetSensorInfo(e.Buffer.UserParameters["Type"]);
					para.Add("Type", e.Buffer.UserParameters["Type"]);
					NetClient.SendBuffer(null, e.Buffer.BufferName, para, ESocketModule.BufferSendManage.Buffer.SendPriority.Normal);
					break;
			}
		}
		/// <summary>
		/// 接管控制
		/// </summary>
		/// <param name="ugv"></param>
		public async Task Start(UGV ugv)
		{
			Device = ugv;
			await Connect();
		}
		/// <summary>
		/// 尝试连接服务器
		/// </summary>
		private async Task Connect()
		{
			var Connected = false;
			while(!Connected)
			{
				try
				{
					await NetClient.SetRemoteAddress(new Windows.Networking.HostName("192.168.137.1"), "13897", "", ESocketModule.TransmissionProtocol.TransmissionControlProtocol);
					Connected = true;
				}
				catch
				{
					System.Diagnostics.Debug.WriteLine("连接失败，两秒后重试");
					await Task.Delay(2000);
				}
			}
			System.Diagnostics.Debug.WriteLine("连接成功");
		}
		/// <summary>
		/// 放弃控制权
		/// </summary>
		public void Quit()
		{
			NetClient.RemoveAllRemoteAddress();
			Device = null;
		}
	}
}
