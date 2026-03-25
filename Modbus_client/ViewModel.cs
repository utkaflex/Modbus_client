using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Modbus_client
{
    public  class ViewModel : INotifyPropertyChanged
    {
        private ModbusService _service;
        private System.Timers.Timer _timer;

        public event PropertyChangedEventHandler PropertyChanged;
        public string PortName { get; set; }
        private string _connectedPort;
        public string WriteValue { get; set; }

        private string _status = "Отключено";
        private bool _isConnected = false;
        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); }
        }

        private string _error;
        public string Error
        {
            get { return _error; }
            set { _error = value; OnPropertyChanged("Error"); }
        }

        public ObservableCollection<Register> Registers { get; set; }
        public ObservableCollection<string> Log { get; set; }

        public Command ConnectCommand { get; set; }
        public Command WriteRegister12Command { get; set; }
        public Command DisconnectCommand { get; set; }

        public ViewModel()
        {
            _service = new ModbusService();
            Registers = new ObservableCollection<Register>();
            Log = new ObservableCollection<string>();
            ConnectCommand = new Command(Connect);
            WriteRegister12Command = new Command(WriteRegister12);
            DisconnectCommand = new Command(Disconnect);
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Логирование для журнала событий
        /// </summary>
        /// <param name="message"></param>
        private void AddLog(string message)
        {
            Log.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
        /// <summary>
        /// Подключение с обработкой ошибок
        /// </summary>
        private void Connect()
        {
            if (string.IsNullOrEmpty(PortName))
            {
                Status = "Введите имя порта";
                return;
            }
            if (_isConnected)
            {
                Status = "Уже подключено к " + _connectedPort;
                return;
            }
            else if (_service.Connect(PortName))
            {
                _isConnected = true;
                _connectedPort = PortName;
                Status = "Подключено к " + _connectedPort;
                AddLog("Подключено к " + _connectedPort);
                StartPolling();
            }
            else
            {
                Status = "Ошибка подключения";
                AddLog("Не удалось подключиться к " + PortName);
            }
        }
        /// <summary>
        /// Запуск таймера, для чтения регистров каждую секунду
        /// </summary>
        private void StartPolling()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) => Poll();
            _timer.Start();
        }
        /// <summary>
        /// Чтение регистров
        /// </summary>
        private void Poll()
        {
            var values = _service.ReadRegister();
            App.Current.Dispatcher.Invoke(() =>
            {
                if (values == null)
                {
                    AddLog("Не удалось прочитать регистры");
                    return;
                }

                Registers.Clear();
                for (int i = 0; i < values.Length; i++)
                {
                    Registers.Add(new Register { IndexName = i, Value = values[i] });
                }
            });
        }
        /// <summary>
        /// Запись значения в 12 регистр с проверкой значения
        /// </summary>
        private void WriteRegister12()
        {
            if (!_isConnected)
            {
                Error = "Сначала подключитесь к устройству";
                return;
            }
            if (ushort.TryParse(WriteValue, out ushort value))
            {
                Error = "";
                _service.WriteRegister12(value);
                AddLog("Записано значение " + value + " в регистр 12");
            }
            else
            {
                Error = "Введите целое число от 0 до 65535";
            }
        }
        /// <summary>
        /// Конец работы приложения
        /// </summary>
        public void Disconnect()
        {
            _timer?.Stop();
            _service.Disconnect();
            _isConnected = false;
            AddLog("Отключено от " + PortName);
            Status = "";
        }
    }
}
