using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPU6050.ValueType;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.Foundation;

namespace MPU6050
{
	public class MPU6050 : IDisposable
	{
		#region 默认MPU6050设备
		private static MPU6050 Device_0;
		public static MPU6050 Default
		{
			get
			{
				if (Device_0 == null)
					Device_0 = new MPU6050(4);
				return Device_0;
			}
		}
		#endregion

		/// <summary>
		/// I2C控制器
		/// </summary>
		private I2CReadWrite Device;
		/// <summary>
		/// Int
		/// </summary>
		private GpioPin InterruptPin;
		/// <summary>
		/// Int上升沿
		/// </summary>
		public event TypedEventHandler<GpioPin,GpioPinValueChangedEventArgs> OnInterrupt
		{
			add
			{
				InterruptPin.ValueChanged += value;
			}
			remove
			{
				InterruptPin.ValueChanged -= value;
			}
		}
		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="Int"></param>
		private MPU6050(int Int)
		{
			GpioController controller = GpioController.GetDefault();
			InterruptPin = controller.OpenPin(Int);
			InterruptPin.Write(GpioPinValue.Low);
			InterruptPin.SetDriveMode(GpioPinDriveMode.Input);
		}
		/// <summary>
		/// 初始化I2C总线,初始化设备
		/// </summary>
		/// <returns></returns>
		public async Task Initialize()
		{
			I2cController controller = await I2cController.GetDefaultAsync();
			I2cConnectionSettings setting = new I2cConnectionSettings(Constants.Address)
			{
				BusSpeed = I2cBusSpeed.FastMode,
				SharingMode = I2cSharingMode.Exclusive
			};

			Device = new I2CReadWrite(controller.GetDevice(setting));

			await Task.Delay(3);

			SetClockSource(Constants.MPU6050_CLOCK_PLL_XGYRO);
			SetFullScaleGyroRange(Constants.MPU6050_GYRO_FS_250);
			SetFullScaleAccelRange(Constants.MPU6050_ACCEL_FS_2);
			SetSleepEnabled(0x00);
		}
		public void DmpInitialize()
		{
			//重设设备
			Reset();
			Task.Delay(30).Wait();

			//关闭睡眠
			SetSleepEnabled(0x00);
			//获取硬件版本
			SetMemoryBank(0x10, true, true);
			SetMemoryStartAddress(0x06);
			ReadMemoryByte();

			//重设
			SetMemoryBank(0x00, false, false);

			getOTPBankValid();

			//获取XYZ gyro基准值
			byte xgOffsetTC = getXGyroOffsetTC();
			byte ygOffsetTC = getYGyroOffsetTC();
			byte zgOffsetTC = getZGyroOffsetTC();

			//未知
			SetSlaveAddress(0, 0x7F);
			SetI2CMasterModeEnabled(0x00);
			SetSlaveAddress(0, 0x68);
			ResetI2CMaster();

			Task.Delay(30).Wait();

			//加载DMP内核
			WriteProgMemoryBlock(DMPMemory.dmpMemory, DMPMemory.MPU6050_DMP_CODE_SIZE, 0, 0, false);

			//写入DMP配置
			WriteProgDMPConfigurationSet(DMPMemory.dmpConfig, DMPMemory.MPU6050_DMP_CONFIG_SIZE);
			//Setting clock source to Z Gyro
			SetClockSource(Constants.MPU6050_CLOCK_PLL_ZGYRO);
			//Setting DMP and FIFO_OFLOW interrupts enabled
			SetIntEnabled(0x12);
			//Setting sample rate to 200Hz
			SetRate(4);
			//Setting external frame sync to TEMP_OUT_L[0]
			SetExternalFrameSync(Constants.MPU6050_EXT_SYNC_TEMP_OUT_L);
			//Setting DLPF bandwidth to 42Hz
			SetDLPFMode(Constants.MPU6050_DLPF_BW_42);
			//Setting gyro sensitivity to +/- 2000 deg/sec
			SetDMPConfig1(0x03);
			SetDMPConfig2(0x00);

			SetOTPBankValid(0x00);

			//Setting X/Y/Z gyro offset TCs to previous values
			setXGyroOffsetTC(xgOffsetTC);
			setYGyroOffsetTC(ygOffsetTC);
			setZGyroOffsetTC(zgOffsetTC);

			int pos = 0;
			//Writing final memory update 1/7 (function unknown)
			WriteProgMemoryBlock(DMPMemory.dmpUpdates[pos].Skip(3).ToArray(), DMPMemory.dmpUpdates[pos][2], DMPMemory.dmpUpdates[pos][0], DMPMemory.dmpUpdates[pos][1], true);
			pos++;
			//Writing final memory update 2/7 (function unknown)
			WriteProgMemoryBlock(DMPMemory.dmpUpdates[pos].Skip(3).ToArray(), DMPMemory.dmpUpdates[pos][2], DMPMemory.dmpUpdates[pos][0], DMPMemory.dmpUpdates[pos][1], true);
			pos++;

			resetFIFO();

			int count = 0;
			count = getFIFOCount();
			if(count > 0)
				getFIFOBytes(count);

			//Setting motion detection threshold to 2
			setMotionDetectionThreshold(2);

			//Setting zero-motion detection threshold to 156
			setZeroMotionDetectionThreshold(156);

			//Setting motion detection duration to 80
			setMotionDetectionDuration(80);

			//Setting zero-motion detection duration to 0
			setZeroMotionDetectionDuration(0);

			//Resetting FIFO
			resetFIFO();

			setFIFOEnabled(0x01);

			setDMPEnabled(0x01);

			resetDMP();

			//Writing final memory update 3/7 (function unknown)
			WriteProgMemoryBlock(DMPMemory.dmpUpdates[pos].Skip(3).ToArray(), DMPMemory.dmpUpdates[pos][2], DMPMemory.dmpUpdates[pos][0], DMPMemory.dmpUpdates[pos][1], true);
			pos++;

			//Writing final memory update 4/7 (function unknown)
			WriteProgMemoryBlock(DMPMemory.dmpUpdates[pos].Skip(3).ToArray(), DMPMemory.dmpUpdates[pos][2], DMPMemory.dmpUpdates[pos][0], DMPMemory.dmpUpdates[pos][1], true);
			pos++;

			//Writing final memory update 5/7 (function unknown)
			WriteProgMemoryBlock(DMPMemory.dmpUpdates[pos].Skip(3).ToArray(), DMPMemory.dmpUpdates[pos][2], DMPMemory.dmpUpdates[pos][0], DMPMemory.dmpUpdates[pos][1], true);
			pos++;

			do
			{
				count = getFIFOCount();
			} while (count < 3);
			if (count < 1024)
				getFIFOBytes(count);
			else
				resetFIFO();

			//Reading interrupt status
			getIntStatus();

			//Reading final memory update 6/7 (function unknown)
			ReadMemoryBlock(DMPMemory.dmpUpdates[pos].Skip(3).ToArray(), DMPMemory.dmpUpdates[pos][2], DMPMemory.dmpUpdates[pos][0], DMPMemory.dmpUpdates[pos][1]);
			pos++;

			while (getFIFOCount() < 3)
				continue;
			getFIFOBytes(getFIFOCount());

			//Reading interrupt status
			getIntStatus();

			//Writing final memory update 7/7 (function unknown)
			WriteProgMemoryBlock(DMPMemory.dmpUpdates[pos].Skip(3).ToArray(), DMPMemory.dmpUpdates[pos][2], DMPMemory.dmpUpdates[pos][0], DMPMemory.dmpUpdates[pos][1], true);
			pos++;

			setDMPEnabled(0x00);

			resetFIFO();
			getIntStatus();

			
		}
		/// <summary>
		/// Set clock source setting.
		/// An internal 8MHz oscillator, gyroscope based clock, or external sources can
		/// be selected as the MPU-60X0 clock source.When the internal 8 MHz oscillator
		/// or an external source is chosen as the clock source, the MPU-60X0 can operate
		/// in low power modes with the gyroscopes disabled.
		///
		/// Upon power up, the MPU-60X0 clock source defaults to the internal oscillator.
		/// However, it is highly recommended that the device be configured to use one of
		/// the gyroscopes(or an external clock source) as the clock reference for
		/// improved stability.The clock source can be selected according to the following table:
		/// </summary>
		/// <example>
		/// CLK_SEL | Clock Source
		/// --------+--------------------------------------
		/// 0       | Internal oscillator
		/// 1       | PLL with X Gyro reference
		/// 2       | PLL with Y Gyro reference
		/// 3       | PLL with Z Gyro reference
		/// 4       | PLL with external 32.768kHz reference
		/// 5       | PLL with external 19.2MHz reference
		/// 6       | Reserved
		/// 7       | Stops the clock and keeps the timing generator in reset
		/// </example>
		/// <param name="source"></param>
		public void SetClockSource(byte source)
		{
			Device.WriteBits(Constants.MPU6050_RA_PWR_MGMT_1, Constants.MPU6050_PWR1_CLKSEL_BIT, Constants.MPU6050_PWR1_CLKSEL_LENGTH, source);
		}
		/// <summary>
		/// Set external FSYNC configuration.
		/// </summary>
		/// <param name="sync"> New FSYNC configuration value</param>
		public void SetExternalFrameSync(byte sync)
		{
			Device.WriteBits(Constants.MPU6050_RA_CONFIG, Constants.MPU6050_CFG_EXT_SYNC_SET_BIT, Constants.MPU6050_CFG_EXT_SYNC_SET_LENGTH, sync);
		}
		/// <summary>
		/// Set digital low-pass filter configuration.
		/// </summary>
		/// <param name="mode"></param>
		public void SetDLPFMode(byte mode)
		{
			Device.WriteBits(Constants.MPU6050_RA_CONFIG, Constants.MPU6050_CFG_DLPF_CFG_BIT, Constants.MPU6050_CFG_DLPF_CFG_LENGTH, mode);
		}
		/// <summary>
		/// Set full-scale gyroscope range.
		/// </summary>
		/// <param name="range">New full-scale gyroscope range value</param>
		public void SetFullScaleGyroRange(byte range)
		{
			Device.WriteBits(Constants.MPU6050_RA_GYRO_CONFIG, Constants.MPU6050_GCONFIG_FS_SEL_BIT, Constants.MPU6050_GCONFIG_FS_SEL_LENGTH, range);
		}
		/// <summary>
		/// Set full-scale accelerometer range.
		/// </summary>
		/// <param name="range">New full-scale accelerometer range setting</param>
		public void SetFullScaleAccelRange(byte range)
		{
			Device.WriteBits(Constants.MPU6050_RA_ACCEL_CONFIG, Constants.MPU6050_ACONFIG_AFS_SEL_BIT, Constants.MPU6050_ACONFIG_AFS_SEL_LENGTH, range);
		}
		/// <summary>
		/// Set sleep mode status.
		/// </summary>
		/// <param name="enabled">New sleep mode enabled status</param>
		public void SetSleepEnabled(byte enabled)
		{
			Device.WriteBit(Constants.MPU6050_RA_PWR_MGMT_1, Constants.MPU6050_PWR1_SLEEP_BIT, enabled);
		}
		/// <summary>
		/// Trigger a full device reset.
		/// </summary>
		public void Reset()
		{
			Device.WriteBit(Constants.MPU6050_RA_PWR_MGMT_1, Constants.MPU6050_PWR1_DEVICE_RESET_BIT, 0x01);
		}
		/// <summary>
		/// Set full interrupt enabled status.
		/// </summary>
		/// <param name="enabled">Full register byte for all interrupts, for quick reading. Each bit should be set 0 for disabled, 1 for enabled.</param>
		public void SetIntEnabled(byte enabled)
		{
			Device.WriteByte(Constants.MPU6050_RA_INT_ENABLE, enabled);
		}
		/// <summary>
		/// BANK_SEL register
		/// </summary>
		/// <param name="bank"></param>
		/// <param name="prefetchEnabled"></param>
		/// <param name="userBank"></param>
		public void SetMemoryBank(byte bank, bool prefetchEnabled, bool userBank)
		{
			int val = bank & 0x1F;
			if (userBank)
				val = bank | 0x20;
			if (prefetchEnabled)
				val = bank | 0x40;
			Device.WriteByte(Constants.MPU6050_RA_BANK_SEL, bank);
		}
		/// <summary>
		/// MEM_START_ADDR register
		/// </summary>
		/// <param name="addr"></param>
		public void SetMemoryStartAddress(byte addr)
		{
			Device.WriteByte(Constants.MPU6050_RA_MEM_START_ADDR, addr);
		}
		/// <summary>
		/// MEM_R_W register
		/// </summary>
		/// <returns></returns>
		public byte ReadMemoryByte()
		{
			return Device.ReadByte(Constants.MPU6050_RA_MEM_R_W);
		}
		/// <summary>
		/// Set the active internal register for the specified slave (0-3).
		/// </summary>
		/// <param name="index"></param>
		/// <param name="addr"></param>
		public void SetSlaveAddress(byte index, byte addr)
		{
			if (index < 3)
				Device.WriteByte((byte)(Constants.MPU6050_RA_I2C_SLV0_ADDR + index * 3), addr);
		}
		/// <summary>
		/// Set I2C Master Mode enabled status.
		/// </summary>
		/// <param name="enabled"></param>
		public void SetI2CMasterModeEnabled(byte enabled)
		{
			Device.WriteBit(Constants.MPU6050_RA_USER_CTRL, Constants.MPU6050_USERCTRL_I2C_MST_EN_BIT, enabled);
		}
		/// <summary>
		/// Reset the I2C Master.
		/// </summary>
		public void ResetI2CMaster()
		{
			Device.WriteBit(Constants.MPU6050_RA_USER_CTRL, Constants.MPU6050_USERCTRL_I2C_MST_RESET_BIT, 0x01);
		}
		/// <summary>
		/// Set gyroscope sample rate divider.
		/// </summary>
		/// <param name="rate">New sample rate divider</param>
		public void SetRate(byte rate)
		{
			Device.WriteByte(Constants.MPU6050_RA_SMPLRT_DIV, rate);
		}
		/// <summary>
		/// 写内存储器
		/// </summary>
		/// <param name="data"></param>
		/// <param name="size"></param>
		/// <param name="bank"></param>
		/// <param name="addr"></param>
		/// <param name="verify"></param>
		public void WriteProgMemoryBlock(byte[] data, short size, byte bank, byte addr, bool verify)
		{
			SetMemoryBank(bank, false, false);
			SetMemoryStartAddress(addr);

			byte chunkSize;
			byte[] verifyBuffer;
			byte[] targetBuffer;
			short i = 0;

			if (verify)
				verifyBuffer = new byte[Constants.MPU6050_DMP_MEMORY_CHUNK_SIZE];


			while (i < size)
			{
				chunkSize = Constants.MPU6050_DMP_MEMORY_CHUNK_SIZE;
				

				if (i + chunkSize > size)
					chunkSize = (byte)(size - i);
				if (chunkSize > 256 - addr)
					chunkSize = (byte)(256 - addr);
				targetBuffer = data.Skip(i).Take(chunkSize).ToArray();
				Device.WriteBytes(Constants.MPU6050_RA_MEM_R_W, targetBuffer);

				//verify data if needed
				if(verify)
				{
					SetMemoryBank(bank, false, false);
					SetMemoryStartAddress(addr);
					verifyBuffer = Device.ReadBytes(Constants.MPU6050_RA_MEM_R_W, chunkSize);

					for (int k = 0; k < verifyBuffer.Length; ++k)
						if (verifyBuffer[k] != targetBuffer[k])
							throw new InvalidOperationException("写入存储器错误，数据不匹配");
				}

				i += chunkSize;
				addr += chunkSize;

				if (i < size)
				{
					if (addr == 0)
						bank++;
					SetMemoryBank(bank, false, false);
					SetMemoryStartAddress(addr);
				}
			}

		}
		public void ReadMemoryBlock(byte[] data, short size, byte bank, byte addr)
		{
			SetMemoryBank(bank, false, false);
			SetMemoryStartAddress(addr);

			byte chunkSize;
			int i = 0;

			while(i < size)
			{
				chunkSize = Constants.MPU6050_DMP_MEMORY_CHUNK_SIZE;

				if (i + chunkSize > size)
					chunkSize = (byte)(size - i);
				if (chunkSize > 256 - addr)
					chunkSize = (byte)(256 - addr);

				Device.ReadBytes(Constants.MPU6050_RA_MEM_R_W, chunkSize);

				i += chunkSize;

				addr += chunkSize;

				if(i <size)
				{
					if (addr == 0)
						bank++;
					SetMemoryBank(bank, false, false);
					SetMemoryStartAddress(addr);
				}
			}
			
		}
		/// <summary>
		/// 写DMP配置到内存储器
		/// </summary>
		/// <param name="data"></param>
		/// <param name="size"></param>
		public void WriteProgDMPConfigurationSet(byte[] data,short size)
		{
			byte special;
			short i = 0;
			byte bank, offset, length;

			while(i < size)
			{
				bank = data[i++];
				offset = data[i++];
				length = data[i++];
				if (length > 0)
				{
					WriteProgMemoryBlock(data.Skip(i).ToArray(), length, bank, offset, true);
					i += length;
				}
				else
				{
					special = data[i++];

					if (special == 0x01)
						Device.WriteByte(Constants.MPU6050_RA_INT_ENABLE, 0x32);
					else
						throw new Exception("unknown special command");
				}
			}
		}

		void SetDMPConfig1(byte config)
		{
			Device.WriteByte(Constants.MPU6050_RA_DMP_CFG_1, config);
		}

		void SetDMPConfig2(byte config)
		{
			Device.WriteByte(Constants.MPU6050_RA_DMP_CFG_2, config);
		}
		void SetOTPBankValid(byte enabled)
		{
			Device.WriteBit(Constants.MPU6050_RA_XG_OFFS_TC, Constants.MPU6050_TC_OTP_BNK_VLD_BIT, enabled);
		}
		public void setXGyroOffsetTC(byte offset)
		{
			Device.WriteBits( Constants.MPU6050_RA_XG_OFFS_TC, Constants.MPU6050_TC_OFFSET_BIT, Constants.MPU6050_TC_OFFSET_LENGTH, offset);
		}

		public void setYGyroOffsetTC(byte offset)
		{
			Device.WriteBits( Constants.MPU6050_RA_YG_OFFS_TC, Constants.MPU6050_TC_OFFSET_BIT, Constants.MPU6050_TC_OFFSET_LENGTH, offset);
		}

		public void setZGyroOffsetTC(byte offset)
		{
			Device.WriteBits( Constants.MPU6050_RA_ZG_OFFS_TC, Constants.MPU6050_TC_OFFSET_BIT, Constants.MPU6050_TC_OFFSET_LENGTH, offset);
		}
		public byte getXGyroOffsetTC()
		{
			return Device.ReadBits(Constants.MPU6050_RA_XG_OFFS_TC, Constants.MPU6050_TC_OFFSET_BIT, Constants.MPU6050_TC_OFFSET_LENGTH);
		}
		public byte getYGyroOffsetTC()
		{
			return Device.ReadBits(Constants.MPU6050_RA_YG_OFFS_TC, Constants.MPU6050_TC_OFFSET_BIT, Constants.MPU6050_TC_OFFSET_LENGTH);
		}
		public byte getZGyroOffsetTC()
		{
			return Device.ReadBits(Constants.MPU6050_RA_ZG_OFFS_TC, Constants.MPU6050_TC_OFFSET_BIT, Constants.MPU6050_TC_OFFSET_LENGTH);
		}
		public void resetFIFO()
		{
			Device.WriteBit(Constants.MPU6050_RA_USER_CTRL, Constants.MPU6050_USERCTRL_FIFO_RESET_BIT, 0x01);
		}
		public int getFIFOCount()
		{
			var result = Device.ReadBytes(Constants.MPU6050_RA_FIFO_COUNTH, 2);
			return (result[0] << 8) | result[1];
		}
		public byte[] getFIFOBytes(int length)
		{
			return Device.ReadBytes(Constants.MPU6050_RA_FIFO_R_W, length);
		}
		public void setMotionDetectionThreshold(byte threshold)
		{
			Device.WriteByte(Constants.MPU6050_RA_MOT_THR, threshold);
		}
		public void setZeroMotionDetectionThreshold(byte threshold)
		{
			Device.WriteByte(Constants.MPU6050_RA_ZRMOT_THR, threshold);
		}
		public void setMotionDetectionDuration(byte duration)
		{
			Device.WriteByte(Constants.MPU6050_RA_MOT_DUR, duration);
		}
		public void setZeroMotionDetectionDuration(byte duration)
		{
			Device.WriteByte(Constants.MPU6050_RA_ZRMOT_DUR, duration);
		}
		public void setFIFOEnabled(byte enabled)
		{
			Device.WriteBit(Constants.MPU6050_RA_USER_CTRL, Constants.MPU6050_USERCTRL_FIFO_EN_BIT, enabled);
		}
		public void setDMPEnabled(byte enabled)
		{
			Device.WriteBit(Constants.MPU6050_RA_USER_CTRL, Constants.MPU6050_USERCTRL_DMP_EN_BIT, enabled);
		}
		public byte getIntStatus()
		{
			return Device.ReadByte(Constants.MPU6050_RA_INT_STATUS);
		}
		public void resetDMP()
		{
			Device.WriteBit(Constants.MPU6050_RA_USER_CTRL, Constants.MPU6050_USERCTRL_DMP_RESET_BIT, 0x01);
		}

		public void setXGyroOffset(ushort offset)
		{
			Device.WriteWord(Constants.MPU6050_RA_XG_OFFS_USRH, offset);
		}
		public void setYGyroOffset(ushort offset)
		{
			Device.WriteWord(Constants.MPU6050_RA_YG_OFFS_USRH, offset);
		}
		public void setZGyroOffset(ushort offset)
		{
			Device.WriteWord(Constants.MPU6050_RA_ZG_OFFS_USRH, offset);
		}
		public void setZAccelOffset(ushort offset)
		{
			Device.WriteWord(Constants.MPU6050_RA_ZA_OFFS_H, offset);
		}
		public byte getRate()
		{
			return Device.ReadByte(Constants.MPU6050_RA_SMPLRT_DIV);
		}
		public Quaternion dmpGetQuaternion(byte[] packet)
		{
			int[] data = new int[4];
			data[0] = ((packet[0] << 8) + packet[1]);
			data[1] = ((packet[4] << 8) + packet[5]);
			data[2] = ((packet[8] << 8) + packet[9]);
			data[3] = ((packet[12] << 8) + packet[13]);
			Quaternion q = new Quaternion();
			q.W = data[0] / 16384;
			q.X = data[1] / 16384;
			q.Y = data[2] / 16384;
			q.Z = data[3] / 16384;

			return q;
		}
		public GravityVector dmpGetGravity(Quaternion q)
		{
			GravityVector v = new GravityVector();
			v.X = 2 * (q.X * q.Z - q.W * q.Y);
			v.Y = 2 * (q.W * q.X + q.Y * q.Z);
			v.Z = q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z;
			return v;
		}
		public YawPitchRoll dmpGetYawPitchRoll(Quaternion q,GravityVector v)
		{
			YawPitchRoll ypw = new YawPitchRoll()
			{
				Yaw = Math.Atan2(2 * q.X * q.Y - 2 * q.W * q.Z, 2 * q.W * q.W + 2 * q.X * q.X - 1),
				Pitch = Math.Atan(v.X / Math.Sqrt(v.Y * v.Y + v.Z * v.Z)),
				Roll = Math.Atan(v.Y / Math.Sqrt(v.X * v.X + v.Z * v.Z))
			};
			return ypw;
		}
		public byte getOTPBankValid()
		{
			return Device.ReadBit(Constants.MPU6050_RA_XG_OFFS_TC, Constants.MPU6050_TC_OTP_BNK_VLD_BIT);
		}
		public void Dispose()
		{
			((IDisposable)Device).Dispose();
			InterruptPin.Dispose();
		}
	}
}
