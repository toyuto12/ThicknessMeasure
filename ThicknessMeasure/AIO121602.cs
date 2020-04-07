using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using CaioCs;
using System.Threading;

namespace ThicknessMeasure {

	public struct ThicknessData {
		
		public float GetUpperVolt { get; private set; }
		public float GetUnderVolt { get; private set; }

		public float GetUpperThickness { get { return (2.5f -GetUpperVolt) *2; } }
		public float GetUnderThickness { get { return (2.5f -GetUnderVolt) *2; } }
		public float GetThickness {
			get {
				if ( (GetUpperVolt >= 4.99f) || (GetUnderVolt >= 4.99f) ) return 0;
				return 6.0f -((5.0f -GetUpperVolt -GetUnderVolt)*2);
			}
		}

		public float GetAverage { get; set; }

		public ThicknessData( float upperVolt, float underVolt ) {
			GetUpperVolt = upperVolt;
			GetUnderVolt = underVolt;
			GetAverage = 0;
		}

		public override string ToString() {
			return String.Format( "{0:f2}", GetThickness );
		}

	}

	public class MoveAverage {
		double[] buf;
		double sum;
		int cnt,wp;

		public double GetAverage { get { return sum/cnt; } }

		public MoveAverage( int cyc ) {
			buf = new double[cyc];
			cnt = cyc;
			for ( int i = 0; i<buf.Length; i++ ) buf[i] = 0;
		}

		public void SetValue( double val ) {
			sum -= buf[wp];
			buf[wp] = val;
			sum += val;
			wp = (wp+1) %cnt;
		}
	}

	public class AIO121602 {

		Caio _aio = new Caio();
		System.Timers.Timer _interval;
		public Queue<ThicknessData[]> DebugQue = new Queue<ThicknessData[]>();
		public bool isDebug;

		public int debugNo = 0;
		public bool DataEnable { get; set; }

		public float[] ThicknessOffsets { get; set; }
		public Queue<ThicknessData[][]> ThicknessOneSheetQue { get; private set; }

		public ThicknessData[] NowThicknessData { get; private set; }

		public event EventHandler PacketReceived;
		public event EventHandler PacketErr;

		short _aioId;
		public int LastErrorNo { get; private set; }
		public String LastErrorStr {
			get {
				String errStr;
				_aio.GetErrorString( LastErrorNo, out errStr );
				return errStr;
			}
		}

		const String ITEM_NAME = "AIO000";


		public AIO121602() {
			ThicknessOffsets = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };
			System.Timers.Timer _timeout = new System.Timers.Timer( 5000 );
			bool isTimeout = false, isInitEnd = false;

			for ( int i = 0; i<_avrs.Length; i++ ) _avrs[i] = new MoveAverage( 4 );

			_timeout.Elapsed += ((object sender, System.Timers.ElapsedEventArgs e) => {
				_timeout.Stop();
				isTimeout = true;
			});
			_timeout.Start();

			Task.Run( () => {
				LastErrorNo = _aio.Init( ITEM_NAME, out _aioId );
				isInitEnd = true;
			} );
			
			while( !isTimeout && !isInitEnd ) Thread.Sleep( 100 );
			_timeout.Stop();
			if ( isTimeout ) {
				LastErrorNo = -1;
				return;
			}

			LastErrorNo = _aio.ResetDevice( _aioId );
			if ( LastErrorNo != 0 ) return;

			LastErrorNo = _aio.SetAiStopTrigger( _aioId, 4 );
			if ( LastErrorNo != 0 ) return;

			LastErrorNo = _aio.SetAiChannels( _aioId, 14 );
			if ( LastErrorNo != 0 ) return;
			LastErrorNo = _aio.SetAiRangeAll( _aioId, (short)CaioConst.PM5 );
			if ( LastErrorNo != 0 ) return;
			LastErrorNo = _aio.SetAiTransferMode( _aioId, 0 );			// デバイスバッファモード
			if ( LastErrorNo != 0 ) return;
			LastErrorNo = _aio.SetAiMemoryType( _aioId, 0 );				// MemoryFIFOモード
			if ( LastErrorNo != 0 ) return;
			LastErrorNo = _aio.SetAiClockType( _aioId, 0 );
			if ( LastErrorNo != 0 ) return;
			LastErrorNo = _aio.SetAiSamplingClock( _aioId, 3000 );      // 1msサンプリング
//			LastErrorNo = _aio.SetAiSamplingClock( _aioId, 50000 );      // 1msサンプリング
			if ( LastErrorNo != 0 ) return;

			ThicknessOneSheetQue = new Queue<ThicknessData[][]>();

			_aio.StartAi( _aioId );
			_interval = new System.Timers.Timer();
			_interval.Interval = 100;
			_interval.Elapsed += _interval_Elapsed;
			_interval.Start();
		}

		bool IsCheckTiming = false;
		int ThicknessCheckCount = 0;
		int ThicknessErrCount = 0;
		List<ThicknessData[]> SheetDataTmp = new List<ThicknessData[]>();
		Queue<ThicknessData[]> ThicknessQue = new Queue<ThicknessData[]>();
		MoveAverage[] _avrs = new MoveAverage[7];
		private void _interval_Elapsed( object sender, System.Timers.ElapsedEventArgs e ) {
			_interval.Stop();

			TaskGetThickness();
			if ( !DataEnable ) ThicknessQue.Clear();

			while ( ThicknessQue.Count > 0 ) {
				var lineData = ThicknessQue.Dequeue();
				int detectCnt = 0;
				for ( int i = 0; i<7; i++ ) {
					if ( lineData[i].GetThickness != 0 ) detectCnt++;
					_avrs[i].SetValue( lineData[i].GetThickness );
					lineData[i].GetAverage = (float)_avrs[i].GetAverage;
				}

				if ( detectCnt >= 6 ) {
					if ( ThicknessCheckCount < 10 ) ThicknessCheckCount++;
					else IsCheckTiming = true;
				} else{
					if ( ThicknessCheckCount > 0 ) ThicknessCheckCount--;
					else IsCheckTiming = false;
				}

				if ( IsCheckTiming ) {
					if ( detectCnt == 7 ) SheetDataTmp.Add( lineData );
					else ThicknessErrCount++;
				} else {
					if ( (SheetDataTmp.Count>5) || (ThicknessErrCount>10) ) {
						if ( ThicknessErrCount < SheetDataTmp.Count ) {
							PacketReceived?.Invoke( SheetDataTmp.Take( SheetDataTmp.Count-10 ).ToArray(), EventArgs.Empty );
						} else {
							PacketErr?.Invoke( null, EventArgs.Empty );
						}
						SheetDataTmp.Clear();
						ThicknessErrCount = 0;
					}
				}
				debugNo = ThicknessOneSheetQue.Count();

			}
			_interval.Start();
		}

		public void close() {
			if ( _aio != null ) {
				_aio.StopAi( _aioId );
				_aio.ResetDevice( _aioId );
				_aio.Exit( _aioId );
				_aio = null;
			}
		}

		~AIO121602() {
			close();
		}

		void _setbit( int bitno, ref int val ) => val |= (1<<bitno);
		bool _getbit( int bitno, int val ) => ((val&(1<<bitno)) > 0);

		bool _checkInrange( float target, float baseVal, float range ) {
			return ((target > (baseVal-range)) && (target < (baseVal+range))) ? true : false;
		}

		float[] baseVal = new float[7];
		int[] offsetCnt = new int[7];
		bool CheckOffset( int no, float val ) {

			if ( val == 0 ) return false;

			if ( _checkInrange( val, baseVal[no], 0.1f ) ) {
				offsetCnt[no]++;
			} else {
				baseVal[no] = val;
				offsetCnt[no] = 0;
			}

			return ( offsetCnt[no] >= 50 ) ?true :false ;			
		}



		public bool SetTicknessOffset( float[] target ) {
			int flg = 0;
			bool isTimeout = false,r=true;
			System.Timers.Timer to = new System.Timers.Timer( 5000 );

			to.Elapsed += ( object sender, System.Timers.ElapsedEventArgs e ) => {
				isTimeout = true;
			};
			to.Start();

			_interval.Enabled = false;
			ThicknessQue.Clear();
			while ( flg != 0x7F ) {

				if ( isTimeout ) {
					r = false;
					goto _ST_TIMEOUT;
				}

				TaskGetThickness();
				if ( ThicknessQue.Count > 0 ) {
					var vals = ThicknessQue.Dequeue();
					for ( int i = 0; i<7; i++ ) {
						if ( _getbit( i, flg ) ) continue; 
						if ( target[i] == 0 ) _setbit( i, ref flg );
						else if( CheckOffset(i, vals[i].GetThickness) ){
							ThicknessOffsets[i] = target[i] -vals[i].GetThickness;
							_setbit( i, ref flg );
						}
					}
				}
			}
_ST_TIMEOUT:
			to.Stop();
			to.Dispose();
			_interval.Enabled = true;
			return r;
		}

		void TaskGetThickness() {
			int len, pos = 0;
			_aio.GetAiSamplingCount( _aioId, out len );
			if ( len > 0 ) {
				var buf = new float[len*14];
				_aio.GetAiSamplingDataEx( _aioId, ref len, ref buf );
				while ( len-- > 0 ) {
					ThicknessData[] tmps = new ThicknessData[7];
					for ( int i = 0; i<7; i++ ) tmps[i] = new ThicknessData( buf[pos++], buf[pos++] );
					if( !isDebug ) ThicknessQue.Enqueue( tmps );
					else DebugQue.Enqueue( tmps );

					if ( len == 0 ) NowThicknessData = tmps;
				}
			}
		}

		float[][] GetAdDatas() {
			int len;
			float[][] result = null;
			_aio.GetAiSamplingCount( _aioId, out len );
			if ( len > 0 ) {
				var buf = new float[len*14];
				_aio.GetAiSamplingDataEx( _aioId, ref len, ref buf );

				result = new float[len][];
				int pos = 0;
				for ( int i = 0; i<len; i++ ) {
					result[i] = new float[14];
					for ( int j = 0; j<14; j++ ) result[i][j] = buf[pos++];
				}
			}
			return result;
		}

		static public float GetRange( float volt ) {
			return ( volt >= 4.99f ) ?0 :35.0f -(volt*2) ;
		}

		public float GetThickness( float UpperVolt, float LowerVolt, float offset ) {
			if ( (UpperVolt >= 4.99f) || (LowerVolt >= 4.99f) ) return 0;
			return offset -((5.0f -UpperVolt -LowerVolt)*2);
		}

	}
}
