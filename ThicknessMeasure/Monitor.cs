﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThicknessMeasure {
	public partial class Monitor :Form {

		SerialConnector _port;
		Form1 _own;

		public Monitor( SerialConnector p, Form1 own ) {
			InitializeComponent();
			_port = p;
			_own = own;
		}

		~Monitor() {
			interval.Enabled = false;
			if( _port != null) {
				_port._port.WriteLine("CP");
			}
		}

		private void Monitor_Load(object sender, EventArgs e) {
			_port._port.WriteLine("CT");
			this.interval.Enabled = true;
		}

		private void interval_Tick(object sender, EventArgs e) {
			stThickness[] _monitor = _own.GetMonitor;

			if ((_monitor[0] == null) || (_monitor[0].adValue > 0xFFF0) || (_monitor[0].adValue == 0)) {
				tbA.Text = "0.000";
			} else {
				tbA.Text = Math.Round(_monitor[0].GetNakedMmValue, 3).ToString("F3");
			}
			if ((_monitor[1] == null) || (_monitor[1].adValue > 0xFFF0) || (_monitor[1].adValue == 0)) {
				tbB.Text = "0.000";
			} else {
				tbB.Text = Math.Round(_monitor[1].GetNakedMmValue, 3).ToString("F3");
			}
			if ((_monitor[2] == null) || (_monitor[2].adValue > 0xFFF0) || (_monitor[2].adValue == 0)) {
				tbC.Text = "0.000";
			} else {
				tbC.Text = Math.Round(_monitor[2].GetNakedMmValue, 3).ToString("F3");
			}
			if ((_monitor[3] == null) || (_monitor[3].adValue > 0xFFF0) || (_monitor[3].adValue == 0)) {
				tbD.Text = "0.000";
			} else {
				tbD.Text = Math.Round(_monitor[3].GetNakedMmValue, 3).ToString("F3");
			}
			if ((_monitor[4] == null) || (_monitor[4].adValue > 0xFFF0) || (_monitor[4].adValue == 0)) {
				tbE.Text = "0.000";
			} else {
				tbE.Text = Math.Round(_monitor[4].GetNakedMmValue, 3).ToString("F3");
			}
			if ((_monitor[5] == null) || (_monitor[5].adValue > 0xFFF0) || (_monitor[5].adValue == 0)) {
				tbF.Text = "0.000";
			} else {
				tbF.Text = Math.Round(_monitor[5].GetNakedMmValue, 3).ToString("F3");
			}
			if ((_monitor[6] == null) || (_monitor[6].adValue > 0xFFF0) || (_monitor[6].adValue == 0)) {
				tbG.Text = "0.000";
			} else {
				tbG.Text = Math.Round(_monitor[6].GetNakedMmValue, 3).ToString("F3");
			}
		}
	}
}
