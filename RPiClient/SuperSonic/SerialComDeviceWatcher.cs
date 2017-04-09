using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using System.Collections.ObjectModel;
using Windows.Storage.Streams;

namespace RPiClient.SensorControl.Devices
{
	class DeviceProperties
	{
		public const String DeviceInstanceId = "System.Devices.DeviceInstanceId";
	}
	class SerialComDeviceWatcher
	{
		/// <summary>
		/// 请求队列
		/// </summary>
		private Queue<String> QuaryList;
		/// <summary>
		/// 设备监视器
		/// </summary>
		DeviceWatcher Watcher;
		/// <summary>
		/// 高级查询字符串
		/// </summary>
		String AQS;
		/// <summary>
		/// 设备列表
		/// </summary>
		private List<DeviceListEntry> ListOfDevices;
		public List<DeviceListEntry> Devices
		{
			get
			{
				return ListOfDevices;
			}
		}
		public event TypedEventHandler<SerialComDeviceWatcher, List<DeviceListEntry>> OnEnumerationComplete;
		/// <summary>
		/// 使用Vid和Pid构造设备枚举监视器
		/// </summary>
		/// <param name="vid"></param>
		/// <param name="pid"></param>
		public SerialComDeviceWatcher(UInt16 vid, UInt16 pid)
		{
			//获取查询字符串
			AQS = SerialDevice.GetDeviceSelectorFromUsbVidPid(vid, pid);
			//获取设备监视器
			Watcher = DeviceInformation.CreateWatcher(AQS);

			Watcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>(this.OnDeviceAdded);
			Watcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(this.OnDeviceRemoved);
			Watcher.EnumerationCompleted += Watcher_EnumerationCompleted;
			Watcher.Start();
		}

		private void Watcher_EnumerationCompleted(DeviceWatcher sender, object args)
		{
			OnEnumerationComplete?.Invoke(this, Devices);
		}

		public void WriteCommand(string s)
		{
			DataWriter DataWriterObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
			DataWriterObject.WriteString(s);
			DataWriterObject.DetachStream();
			DataWriterObject = null;
		}

		/// <summary>
		/// Searches through the existing list of devices for the first DeviceListEntry that has
		/// the specified device Id.
		/// </summary>
		/// <param name="deviceId">Id of the device that is being searched for</param>
		/// <returns>DeviceListEntry that has the provided Id; else a nullptr</returns>
		private DeviceListEntry FindDevice(String deviceId)
		{
			if (deviceId != null)
			{
				foreach (DeviceListEntry entry in ListOfDevices)
				{
					if (entry.DeviceInformation.Id == deviceId)
					{
						return entry;
					}
				}
			}

			return null;
		}
		/// <summary>
		/// Creates a DeviceListEntry for a device and adds it to the list of devices in the UI
		/// </summary>
		/// <param name="deviceInformation">DeviceInformation on the device to be added to the list</param>
		/// <param name="deviceSelector">The AQS used to find this device</param>
		private void AddDeviceToList(DeviceInformation deviceInformation, String deviceSelector)
		{
			// search the device list for a device with a matching interface ID
			var match = FindDevice(deviceInformation.Id);

			// Add the device if it's new
			if (match == null)
			{
				// Create a new element for this device interface, and queue up the query of its
				// device information
				match = new DeviceListEntry(deviceInformation, deviceSelector);

				// Add the new element to the end of the list of devices
				ListOfDevices.Add(match);
			}
		}

		private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
		{
			var deviceEntry = FindDevice(args.Id);

			ListOfDevices.Remove(deviceEntry);
		}

		private void OnDeviceAdded(DeviceWatcher sender, DeviceInformation args)
		{
			AddDeviceToList(args, AQS);
		}
	}
}
