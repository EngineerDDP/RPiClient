using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using RPiClient.SensorControl.Devices;
using System.IO;
using Windows.Storage.Streams;

namespace RPiClient.SuperSonic
{
	class Arduino
	{
		public const UInt16 Vid = 0x2341;
		public const UInt16 Pid = 0x0001;

		private SerialComDeviceWatcher watcher;
		private SerialDevice Device;

		public Arduino()
		{
			watcher = new SerialComDeviceWatcher(Vid, Pid);
			watcher.OnEnumerationComplete += Watcher_OnEnumerationComplete;
		}

		private async void Watcher_OnEnumerationComplete(SerialComDeviceWatcher sender, List<DeviceListEntry> args)
		{
			Device = await SerialDevice.FromIdAsync(args[0].DeviceInformation.Id);
			Device.BaudRate = 9600;
			Device.Parity = SerialParity.None;
			Device.StopBits = SerialStopBitCount.One;
			Device.Handshake = SerialHandshake.None;
			Device.DataBits = 8;
		}
		/// <summary>
		/// 写入数据
		/// </summary>
		/// <param name="str"></param>
		public void WriteString(String str)
		{
			DataWriter w = new DataWriter(Device.OutputStream);
			w.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
			w.WriteString(str);
			w.FlushAsync().AsTask().Wait();
			w.DetachStream();
		}
		public String ReadString()
		{
			Stream s = Device.InputStream.AsStreamForRead();
			StreamReader rs = new StreamReader(s);
			return rs.ReadLine();
		}
	}
}
