using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace RPiClient.Gyroscope
{
	class I2CReadWrite
	{
		/// <summary>
		/// I2C总线设备句柄
		/// </summary>
		I2cDevice Device;
		/// <summary>
		/// 默认构造方法
		/// </summary>
		/// <param name="dev"></param>
		public I2CReadWrite(I2cDevice dev)
		{
			Device = dev;
		}

		public void Dispose()
		{
			Device.Dispose();
		}

		/// <summary>
		/// 读取指定寄存器的指定位
		/// </summary>
		/// <param name="regAddr"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public byte ReadBit(byte regAddr, byte index)
		{
			byte b = ReadByte(regAddr);
			b = (byte)(b & 1 << index);
			b = (byte)(b >> index);
			return b;
		}
		/// <summary>
		/// 读取指定寄存器的指定位序列
		/// </summary>
		/// <param name="regAddr"></param>
		/// <param name="index"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public byte ReadBits(byte regAddr, byte index, byte length)
		{
			byte b = ReadByte(regAddr);
			b = (byte)(b & ((1 << length) - 1) << (index - length + 1));
			b = (byte)(b >> (index - length + 1));
			return b;
		}
		/// <summary>
		/// 读出指定的字节
		/// </summary>
		/// <param name="regAddr">寄存器地址</param>
		/// <returns></returns>
		public byte ReadByte(byte regAddr)
		{
			byte[] buffer = new byte[1];
			buffer[0] = regAddr;
			byte[] value = new byte[1];
			Device.WriteRead(buffer, value);
			return value[0];
		}
		/// <summary>
		/// 读出指定长度的字节序列
		/// </summary>
		/// <param name="regAddr">寄存器地址</param>
		/// <param name="length">长度</param>
		/// <returns></returns>
		public byte[] ReadBytes(byte regAddr, int length)
		{
			byte[] values = new byte[length];
			byte[] buffer = new byte[1];
			buffer[0] = regAddr;
			Device.WriteRead(buffer, values);
			return values;
		}

		/// <summary>
		/// 从FiFo中读出一个字(2字节)
		/// </summary>
		/// <param name="address">寄存器地址</param>
		/// <returns></returns>
		public ushort ReadWord(byte address)
		{
			byte[] buffer = ReadBytes(Constants.FifoCount, 2);
			return (ushort)(((int)buffer[0] << 8) | (int)buffer[1]);
		}

		/// <summary>
		/// 将指定值写入指定位
		/// </summary>
		/// <param name="regAddr">寄存器地址</param>
		/// <param name="index">指定位的序号</param>
		/// <param name="value">指定的值</param>
		public void WriteBit(byte regAddr, byte index, byte value)
		{
			byte b = ReadByte(regAddr); //取出寄存器值
			b = (byte)(b & ~(1 << index));  //将要设置的位留空
			b = (byte)(b | value << index); //写入位
			WriteByte(regAddr, b);  //写回寄存器值
		}
		/// <summary>
		/// 将指定值写入指定位
		/// </summary>
		/// <param name="regAddr">寄存器地址</param>
		/// <param name="index">指定位的序号</param>
		/// <param name="value">指定的值</param>
		public void WriteBits(byte regAddr, byte index, byte length, byte value)
		{
			byte b = ReadByte(regAddr); //取出寄存器值
			b = (byte)(b & ~(((1 << length) - 1) << (index - length + 1))); //将要设置的位留空
			b = (byte)(b | value << (index - length + 1));
			WriteByte(regAddr, b);
		}
		/// <summary>
		/// 将指定的字节写入
		/// </summary>
		/// <param name="regAddr">寄存器地址</param>
		/// <param name="data">数据</param>
		public void WriteByte(byte regAddr, byte data)
		{
			byte[] buffer = new byte[2];
			buffer[0] = regAddr;
			buffer[1] = data;
			Device.Write(buffer);
		}
		/// <summary>
		/// 将指定的字串写入
		/// </summary>
		/// <param name="regAddr">寄存器地址</param>
		/// <param name="values">数据</param>
		public void WriteBytes(byte regAddr, byte[] values)
		{
			byte[] buffer = new byte[1 + values.Length];
			buffer[0] = regAddr;
			Array.Copy(values, 0, buffer, 1, values.Length);
			Device.Write(buffer);
		}
		/// <summary>
		/// 写入一个字（2字节）
		/// </summary>
		/// <param name="regAddr"></param>
		/// <param name="value"></param>
		public void WriteWord(byte regAddr, ushort value)
		{
			byte[] buffer = new byte[2];
			buffer[0] = (byte)(value >> 8);
			buffer[1] = (byte)(value);
			WriteBytes(regAddr, buffer);
		}
	}
}
