using System;
using Windows.Devices.I2c;

namespace MPU6050
{
    public class I2CReadWrite : IDisposable
    {
		/// <summary>
		/// I2C�����豸���
		/// </summary>
		I2cDevice Device;
		/// <summary>
		/// Ĭ�Ϲ��췽��
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
		/// ��ȡָ���Ĵ�����ָ��λ
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
		/// ��ȡָ���Ĵ�����ָ��λ����
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
		/// ����ָ�����ֽ�
		/// </summary>
		/// <param name="regAddr">�Ĵ�����ַ</param>
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
		/// ����ָ�����ȵ��ֽ�����
		/// </summary>
		/// <param name="regAddr">�Ĵ�����ַ</param>
		/// <param name="length">����</param>
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
		/// ��FiFo�ж���һ����(2�ֽ�)
		/// </summary>
		/// <param name="address">�Ĵ�����ַ</param>
		/// <returns></returns>
        public ushort ReadWord(byte address)
        {
            byte[] buffer = ReadBytes(Constants.FifoCount, 2);
            return (ushort)(((int)buffer[0] << 8) | (int)buffer[1]);
        }

		/// <summary>
		/// ��ָ��ֵд��ָ��λ
		/// </summary>
		/// <param name="regAddr">�Ĵ�����ַ</param>
		/// <param name="index">ָ��λ�����</param>
		/// <param name="value">ָ����ֵ</param>
		public void WriteBit(byte regAddr, byte index, byte value)
		{
			byte b = ReadByte(regAddr);	//ȡ���Ĵ���ֵ
			b = (byte)(b & ~(1 << index));  //��Ҫ���õ�λ����
			b = (byte)(b | value << index); //д��λ
			WriteByte(regAddr, b);	//д�ؼĴ���ֵ
		}
		/// <summary>
		/// ��ָ��ֵд��ָ��λ
		/// </summary>
		/// <param name="regAddr">�Ĵ�����ַ</param>
		/// <param name="index">ָ��λ�����</param>
		/// <param name="value">ָ����ֵ</param>
		public void WriteBits(byte regAddr, byte index, byte length, byte value)
		{
			byte b = ReadByte(regAddr);	//ȡ���Ĵ���ֵ
			b = (byte)(b & ~(((1 << length) - 1) << (index - length + 1))); //��Ҫ���õ�λ����
			b = (byte)(b | value << (index - length + 1));
			WriteByte(regAddr, b);
		}
		/// <summary>
		/// ��ָ�����ֽ�д��
		/// </summary>
		/// <param name="regAddr">�Ĵ�����ַ</param>
		/// <param name="data">����</param>
        public void WriteByte(byte regAddr, byte data)
        {
            byte[] buffer = new byte[2];
            buffer[0] = regAddr;
            buffer[1] = data;
            Device.Write(buffer);
        }
		/// <summary>
		/// ��ָ�����ִ�д��
		/// </summary>
		/// <param name="regAddr">�Ĵ�����ַ</param>
		/// <param name="values">����</param>
        public void WriteBytes(byte regAddr, byte[] values)
        {
            byte[] buffer = new byte[1 + values.Length];
            buffer[0] = regAddr;
            Array.Copy(values, 0, buffer, 1, values.Length);
            Device.Write(buffer);
        }
		public void WriteWord(byte regAddr, ushort value)
		{
			byte[] buffer = new byte[2];
			buffer[0] = (byte)(value >> 8);
			buffer[1] = (byte)(value);
			WriteBytes(regAddr, buffer);
		}
    }
}