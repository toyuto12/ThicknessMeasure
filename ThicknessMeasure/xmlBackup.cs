using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThicknessMeasure {
	class xmlBackup {
		String _path;

		public xmlBackup( String path) {
			_path = path;
		}

		public void Write( object data) {
			//出力先XMLのストリーム
			System.IO.FileStream stream = new System.IO.FileStream(_path, System.IO.FileMode.Create);
			System.IO.StreamWriter writer = new System.IO.StreamWriter(stream, Encoding.UTF8);
			try {
				//シリアライズ
				System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(data.GetType());
				serializer.Serialize(writer, data);
			} catch {

			} finally {
				writer.Flush();
				writer.Close();
			}
		}

	}
}
