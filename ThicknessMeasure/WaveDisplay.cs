using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NPlot;

namespace ThicknessMeasure {
	public partial class WaveDisplay :Form {

		AIO121602 _aio;
		PlotLine _line;
		int pos = 0;
		public WaveDisplay( AIO121602 data ) {
			InitializeComponent();
			_aio = data;
		}

		private void WaveDisplay_Load( object sender, EventArgs e ) {
			_line = new PlotLine(this.plot);
			resize();
			timer1.Enabled = true;
		}

		int ddd = 0;
		private void timer1_Tick( object sender, EventArgs e ) {
			//System.Random rnd = new Random( (int)DateTime.Now.Ticks );
			//_line.SetData( "test", (double)pos++, rnd.Next( -100, 100 ));
			while ( _aio.DebugQue.Count > 0 ) {
				var lines = _aio.DebugQue.Dequeue();
				if ( ddd < 50 ) ddd++;
				else {
					ddd = 0;
					_line.SetData( "0", pos, lines[0].GetThickness );
					_line.SetData( "1", pos, lines[1].GetThickness );
					_line.SetData( "2", pos, lines[2].GetThickness );
					_line.SetData( "3", pos, lines[3].GetThickness );
					_line.SetData( "4", pos, lines[4].GetThickness );
					_line.SetData( "5", pos, lines[5].GetThickness );
					_line.SetData( "6", pos++, lines[6].GetThickness );
				}
			}

		}

		private void bClear_Click( object sender, EventArgs e ) {
			_line.ClearAllData();
			pos = 0;
		}

		private void WaveDisplay_ResizeEnd( object sender, EventArgs e ) {
			resize();
		}

		void resize() {
			plot.Location = new Point( 0, 30 );
			plot.Width = this.Width -30;
			plot.Height = this.Height -80;
		}

	}

	public class PlotLine {

		readonly Color[] colorSample = new Color[] { Color.Brown, Color.Red, Color.Orange, Color.Yellow, Color.Green,Color.Blue, Color.Violet,Color.Gray };
		NPlot.Windows.PlotSurface2D _base;
		Dictionary<String, NPlot.LinePlot> _lines = new Dictionary<string, LinePlot>();

		public PlotLine( NPlot.Windows.PlotSurface2D b ) {
			_base = b;
			_init();
		}

		void _init() {
//			_base.XAxis2.Hidden = true;
//			_base.YAxis2.Hidden = true;
//			_base.XAxis1.WorldMin = 0;
		}

		public void SetData( string name, double pos, double value ) {
			var line = GetLine( name );
			List<double> x = (List<double>)line.AbscissaData;
			List<double> y = (List<double>)line.OrdinateData;

			if ( _base.XAxis1.WorldMax < pos ) _base.XAxis1.WorldMax = pos;
			if ( _base.YAxis1.WorldMax < value ) _base.YAxis1.WorldMax = value;
			if ( _base.YAxis1.WorldMin > value ) _base.YAxis1.WorldMin = value;
			_base.XAxis1.WorldMin = 0;
			x.Add( pos );
			y.Add( value );

			_base.Refresh();
		}

		public void ClearAllData() {
			_base.XAxis1.WorldMax = 0;
			_base.XAxis1.WorldMin = 0;
			_base.YAxis1.WorldMax = 0;
			_base.YAxis1.WorldMin = 0;
			_base.Clear();
			_lines.Clear();
		}

		LinePlot GetLine( string name ) {
			LinePlot r;
			if ( !_lines.ContainsKey( name ) ) {
				var tmp = new LinePlot();
				tmp.Color = colorSample[_lines.Count];
				tmp.Pen.Width = 2;
				tmp.AbscissaData = new List<double>();
				tmp.OrdinateData = new List<double>();
				_lines.Add( name, tmp );
				_base.Add( tmp );
				r = tmp;
			} else {
				r = _lines[name];
			}
			return r;
		}

	}
}
