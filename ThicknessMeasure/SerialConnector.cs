using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Ports;

namespace ThicknessMeasure {
	public class SerialConnector {

		public SerialPort _port;
		const int BAUDRATE = 250000;
		const int STX = 0x02;
		const int ETX = 0x03;
		const int DLE = 0x10;

		public String ErrMes { get; private set; }
		public bool IsEnable { get; private set; }
		public UInt16[] GetReceiveData { get; private set; }

		public event EventHandler PacketReceived;

		public SerialConnector( String port ) {
			try {
				_port = new SerialPort(port, BAUDRATE, Parity.None, 8, StopBits.One);
				_port.Open();
				_port.DataReceived += DataReceivedCallback;
				IsEnable = true;
			} catch (Exception e) {
				ErrMes = e.Message;
				IsEnable = false;
			}
		}

		~SerialConnector() {
			Close();
		}

		public void Close() {
			if (_port != null) {
				if (_port.IsOpen) _port.Close();
				_port = null;
			}
		}

		public static String[] GetPortList() => SerialPort.GetPortNames();

		public void OnPacketReceived() => PacketReceived(null, EventArgs.Empty);

		MemoryStream ms = new MemoryStream();
		bool isDle = false;
		private void DataReceivedCallback(object sender, SerialDataReceivedEventArgs e) {
			var s = (SerialPort)sender;

			while (s.BytesToRead > 0) {
				int dat = s.ReadByte();

				if( isDle) {
					isDle = false;
					switch(dat) {
					case STX:
						ms.Dispose();
						ms = new MemoryStream();
						break;
					case ETX:
						ms.Seek(0, SeekOrigin.Begin);
						Byte[] buf = ms.ToArray();
						UInt16[] res = new UInt16[buf.Length/2];
						int j = 0;
						for (int i = 0 ; i<res.Length; i++,j+=2) {
							res[i] = ( BitConverter.ToUInt16(buf, j) );
						}
						GetReceiveData = res;
						if( PacketReceived != null) {
							PacketReceived(GetReceiveData, EventArgs.Empty);
						}

						ms.Dispose();
						ms = new MemoryStream();
						break;
					case DLE:
						ms.WriteByte((byte)dat);
						break;
					}
				} else if( dat == DLE) {
					isDle = true;
				} else {
					ms.WriteByte((byte)dat);
				}

			}
		}
	}

}
