using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using CaioCs;

namespace ThicknessMeasure {
	class AIO121602 {

		Caio _aio = new Caio();
		System.Timers.Timer _interval;

		public float[] ThicknessOffsets { get; private set; }
		public Queue<float[]> ThicknessQue { get; private set; }

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
			ThicknessOffsets = new float[] { 6, 6, 6, 6, 6, 6, 6, 6 };

			LastErrorNo = _aio.Init( ITEM_NAME, out _aioId );
			if ( LastErrorNo != 0 ) return;

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
			LastErrorNo = _aio.SetAiSamplingClock( _aioId, 1000 );      // 1msサンプリング
			if ( LastErrorNo != 0 ) return;

			_aio.StartAi( _aioId );
			ThicknessQue = new Queue<float[]>();
			_interval = new System.Timers.Timer();
			_interval.Interval = 100;
			_interval.Elapsed += _interval_Elapsed;
			_interval.Start();
		}

		private void _interval_Elapsed( object sender, System.Timers.ElapsedEventArgs e ) {
			_interval.Stop();

			float[][] datas = GetAdDatas();
			if ( datas != null ) {
				foreach ( float[] ds in datas ) {
					float[] tns = new float[7];
					for ( int i = 0, j = 0; i<7; i++, j+=2 ) {
						tns[i] = GetThickness( ds[j], ds[j+1], ThicknessOffsets[i] );
					}
					ThicknessQue.Enqueue( tns );
				}
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

			return ( offsetCnt[no] >= 8 ) ?true :false ;			
		}

		public void SetTicknessOffset( float[] target ) {
			int flg = 0;

			ThicknessQue.Clear();
			while ( flg != 0x7F ) {
				if ( ThicknessQue.Count > 0 ) {
					var vals = ThicknessQue.Dequeue();
					for ( int i = 0; i<7; i++ ) {
						if ( _getbit( i, flg ) ) continue; 
						if ( target[i] == 0 ) _setbit( i, ref flg );
						else if( CheckOffset(i, target[i]) ){
							ThicknessOffsets[i] = (ThicknessOffsets[i]-vals[i])+ target[i];
							_setbit( i, ref flg );
						}
					}
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
