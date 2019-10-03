using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Drawing;

namespace ThicknessMeasure {
	class CsvOutput {
		List<List<String>> _cell;
		Point offset;

		public CsvOutput() {
			_cell = new List<List<string>>();
		}

		public void SetOffset(int x, int y) => offset = new Point(x, y);

		public void AddOffset(int x, int y) {
			offset.X += x;
			offset.Y += y;
		}

		public void FileOut( String filePath ) {

			int no = 0;
			String fp = Path.GetFileNameWithoutExtension(filePath);
			String dic = Path.GetDirectoryName(filePath);
			while (File.Exists(filePath)) {
				String ext = Path.GetExtension(filePath);
				String path = Path.GetDirectoryName(filePath);
				if (path != "") path = path + "/";
				filePath = String.Format(@"{3}/{2}{0}_{1}.{3}", fp, ++no, path, ext, dic);
			}

			FileStream fs = File.Open(filePath , FileMode.Create);
			bool Start;
			Encoding enc = Encoding.GetEncoding("shift_jis");

			foreach (List<String> row in _cell) {
				Start = true;
				foreach(String data in row ){
					Byte[] d;
					if (!Start) {
						d = enc.GetBytes(",");
						fs.Write(d , 0 , d.Count());
					}
					Start = false;
					d = enc.GetBytes('"' + data + '"');
					fs.Write( d , 0 , d.Count() );
				}
				Byte[] d2 = enc.GetBytes("\r\n");
				fs.Write( d2 , 0 , d2.Count() );
			}

			fs.Flush();
			fs.Dispose();
		}

		public void DataInput( int col , int row , String data ){

			col = col+offset.X;
			row = row+offset.Y;

			if (row >= _cell.Count()) {
				int cnt = row - _cell.Count() + 1;
				for( int i=0 ; i<cnt ; i++ ) _cell.Add( new List<String>() );
			}

			if( col >= _cell[row].Count() ){
				int cnt = col - _cell[row].Count() + 1;
				for( int i=0 ; i<cnt ; i++ ) _cell[row].Add( "" );
			}

			_cell[row][col] = data;
		}

		public void DataInput(int col, int row, String[] data) {
			for(int i=0; i<data.Length; i++) {
				DataInput(col++, row, data[i]);
			}
		}

	}
}
