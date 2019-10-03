using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Timers;

namespace ThicknessMeasure {
	public class Timeout {
		Timer _t;

		public Timeout(double to, ElapsedEventHandler func ) {
			_t = new Timer(to);
			_t.Elapsed += func;
			_t.Start();
		}

		public void Stop() {
			_t.Stop();
		}

	}
}
