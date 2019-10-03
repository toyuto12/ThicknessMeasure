using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ThicknessMeasure {
	public class ObjectSerializer {
		public static void SaveFile( object obj , String path ) {
			if(File.Exists(path)) {
				File.Delete(path);
			}
			var fs = new FileStream( path , FileMode.Create , FileAccess.Write );
			new BinaryFormatter().Serialize( fs , obj );
			fs.Flush();
			fs.Dispose();
		}
		public static void Serialize( Stream stm , object obj ) {
			new BinaryFormatter().Serialize( stm , obj );
		}
		public static object Deserialize( Stream stm ) {
			return new BinaryFormatter().Deserialize( stm );
		}
		public static object LoadFile( String path ) {
			if( File.Exists( path ) ) {
				var fs = new FileStream( path , FileMode.Open , FileAccess.Read );
				object dat = new BinaryFormatter().Deserialize( fs );
				fs.Dispose();
				return dat;
			} else {
				return null;
			}
		}
	}
}
