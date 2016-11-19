using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Windows.Devices.Gpio;
using Windows.Devices.Pwm;
using System.Threading.Tasks;
using Microsoft.IoT.Lightning.Providers;
using Windows.Devices;
using ESocketModule.ClientEntry;
using Windows.Networking;
using RPiClient.MotionControl;
using RPiClient.MotionControl.Devices;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace RPiClient
{
	/// <summary>
	/// 程序入口点
	/// </summary>
    public sealed class StartupTask : IBackgroundTask
    {
		BackgroundTaskDeferral deferral;
		IUGVManager manager;

		public async void Run(IBackgroundTaskInstance taskInstance)
		{
			deferral = taskInstance.GetDeferral();
			manager = new NetworkUGVManager();
			//manager = new AutoPilot();
			await manager.Start(UGV.UGV_Default);
		}
	}
}
