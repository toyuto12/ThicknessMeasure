using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThicknessMeasure {

	public partial class SettingForm :Form {
		public float[] _corr { get; set; }
		public float[][] _rank { get; set; }

		public stBodyType[] _setting;
		TextBox[] tbLvList;
		TextBox[] tbColorList;
		Label[] lLvList;


		public SettingForm( List<stBodyType> bt ) {
			InitializeComponent();

			_setting = new stBodyType[bt.Count];
			for ( int i = 0; i<bt.Count; i++ ) _setting[i] = new stBodyType( bt[i] );

		}

		const string pass = "aaaa";
		private void textBox1_KeyDown( object sender, KeyEventArgs e ) {
			if ( e.KeyCode == Keys.Enter ) {
				if ( textBox1.Text.Equals( pass ) ) {
					panel1.Visible = true;
					panel1.Enabled = true;
					gbPass.Enabled = false;
				}
			}
		}

		private void SettingForm_Load( object sender, EventArgs e ) {
			tbLvList = new TextBox[] { tbThreshLv0, tbThreshLv1, tbThreshLv2, tbThreshLv3 };
			tbColorList = new TextBox[] { tbThreshColorLv0, tbThreshColorLv1, tbThreshColorLv2, tbThreshColorLv3, tbThreshColorLv4 };
			lLvList = new Label[] { lLv0, lLv1, lLv2, lLv3 };
			cbTypeName.Items.Clear();
			foreach ( var set in _setting ) {
				cbTypeName.Items.Add( set._typeName );
			}
			if ( cbTypeName.Items.Count > 0 ) cbTypeName.SelectedIndex = 0;

			for ( int i = 0; i<tbLvList.Length; i++ ) {
				tbLvList[i].Leave += tbThreshLvX_Leave;
			}
		}

		private void cbTypeName_SelectedIndexChanged( object sender, EventArgs e ) {
			int no = cbTypeName.SelectedIndex;

			tbCorr.Text = _setting[no]._correction.ToString("F4");

			if ( _setting[no]._judgeRange.Length == 3 ) {
				tbThreshColorLv3.BackColor = Color.Red;
				tbThreshColorLv4.Visible = false;
				lLv3.Visible = false;
			} else {
				tbThreshColorLv3.BackColor = Color.FromArgb( 153, 102, 255 );
				tbThreshColorLv4.Visible = true;
				lLv3.Visible = true;
			}

			for ( int i = 0; i<tbLvList.Length; i++ ) {
				if ( _setting[no]._judgeRange.Length > i ) {
					tbLvList[i].Text = _setting[no]._judgeRange[i].ToString("F2");
					tbLvList[i].Visible = true;
				} else {
					tbLvList[i].Visible = false;
				}
			}
		}

		private void tbThreshLvX_Leave( object sender, EventArgs e ) {
			TextBox tb = (TextBox)sender;
			int lv = int.Parse( (string)tb.Tag );
			float val;

			if ( float.TryParse( tb.Text, out val ) ) {
				_setting[cbTypeName.SelectedIndex]._judgeRange[lv] = val;
			} else {
				tb.Text = _setting[cbTypeName.SelectedIndex]._judgeRange[lv].ToString("F2");
			}

		}

		private void tbCorr_Leave( object sender, EventArgs e ) {
			TextBox tb = (TextBox)sender;
			float val;

			if ( float.TryParse( tbCorr.Text, out val ) ) {
				_setting[cbTypeName.SelectedIndex]._correction = val;
			} else {
				tb.Text = _setting[cbTypeName.SelectedIndex]._correction.ToString( "F4" );
			}
		}

		private void bOk_Click( object sender, EventArgs e ) {
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button1_Click( object sender, EventArgs e ) {
			this.DialogResult = DialogResult.No;
			this.Close();
		}
	}
}
