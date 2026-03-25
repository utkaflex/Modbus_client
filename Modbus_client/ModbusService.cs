using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Modbus.Device;

namespace Modbus_client
{
    public class ModbusService
    {
        private SerialPort _serialPort;
        private IModbusMaster _master;

        /// <summary>
        /// Подключение к порту
        /// </summary>
        /// <param name="portName"></param>
        /// <returns></returns>
        public bool Connect(string portName)
        {
            try
            {
                _serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
                _serialPort.Open();
                _master = ModbusSerialMaster.CreateRtu(_serialPort);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        /// <summary>
        /// Отключение от порта
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _serialPort.Close();
                _serialPort = null;
                _master = null;
            }
            catch(Exception e)
            {

            }
        }
        /// <summary>
        /// Чтение регистров
        /// </summary>
        /// <returns></returns>
        public ushort[] ReadRegister()
        {
            try
            {
                return _master.ReadHoldingRegisters(1, 0, 13);
            }
            catch(Exception e)
            {
                return null;
            }
        }
        /// <summary>
        /// Запись значения в 12 регистр
        /// </summary>
        /// <param name="number"></param>

        public void WriteRegister12(ushort number)
        {
            try
            {
                _master.WriteSingleRegister(1, 12, number);
            }
            catch(Exception e)
            {

            }
        }
    }
}
