using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NAudio.Wave;
using System.Xml.Serialization;
using System.IO;
using System.Timers;

namespace ThicknessMeasure {
	public partial class Form1 :Form {


		void _writeCsvResultAndGraph( CsvOutput csv, int page ) {
			var d = ResultMaster.ReadResult( page );
			csv.DataInput( 0, 0, String.Format( "{0}枚目", page+1 ) );
			csv.DataInput( 0, 1, "検査日" );
			csv.DataInput( 1, 1, d.now.ToString( "yyyy/MM/dd HH:mm:ss" ) );
			csv.DataInput( 0, 2, "検査品番" );
			csv.DataInput( 1, 2, d._typeName );
			csv.DataInput( 0, 3, "公称厚み" );
			csv.DataInput( 1, 3, d.type._stdThickness + "mm" );
			csv.DataInput( 3, 1, "総合判定" );
			csv.DataInput( 4, 1, d._allResult );
			csv.DataInput( 3, 2, "総合平均" );
			csv.DataInput( 4, 2, d._allAverage );
			csv.DataInput( 3, 3, "基準値" );
			csv.DataInput( 4, 3, d.type.GetRange );
			csv.DataInput( 7, 1, "検査担当者" );
			csv.DataInput( 8, 1, d._humanName );

			csv.DataInput( 0, 5, "項目" );
			csv.DataInput( 1, 5, "Ａ列" );
			csv.DataInput( 2, 5, "Ｂ列" );
			csv.DataInput( 3, 5, "Ｃ列" );
			csv.DataInput( 4, 5, "Ｄ列" );
			csv.DataInput( 5, 5, "Ｅ列" );
			csv.DataInput( 6, 5, "Ｆ列" );
			csv.DataInput( 7, 5, "Ｇ列" );
			csv.DataInput( 0, 6, "最小値" );
			csv.DataInput( 0, 7, "最大値" );
			csv.DataInput( 0, 8, "平均値" );
			csv.DataInput( 0, 9, "判定" );

			for ( int row = 0; row<4; row++ ) {
				for ( int col = 0; col<7; col++ ) {
					csv.DataInput( col+1, row+6, d._results[row][col] );
				}
			}

			csv.DataInput( 10, 0, new String[] { "CNT", "A", "B", "C", "D", "E", "F", "G" } );

			int x = 11, y = 1;
			foreach ( var val in d.GetResultMM() ) {
				if ( !float.IsNaN( val ) ) {
					csv.DataInput( x, y, val.ToString() );
					x++;
				} else {
					csv.DataInput( 10, y, y.ToString() );
					y++;
					x = 11;
				}
			}
			csv.AddOffset( 0, y+2 );
		}

		// 現在確定した測定データのタイトルをCSVファイルに出力する
		void CSVTitleOutput( String filePath ) {
			CsvOutput csv = new CsvOutput();
			var datas = ResultMaster.ReadResult( 0 );

			csv.DataInput( 2, 3, "母材" );
			csv.DataInput( 2, 4, datas.type._typeName );

			csv.DataInput( 2, 6, "総数" );
			csv.DataInput( 2, 7, ResultMaster.Qty.ToString() + " 枚" );

			csv.DataInput( 2, 9, "原板ロット" );
			csv.DataInput( 2, 10, datas._lotName );


			csv.DataInput( 6, 3, "検査日" );
			csv.DataInput( 6, 4, datas.now.ToString( "yyyy年" ) );
			csv.DataInput( 7, 4, datas.now.ToString( "MM月dd日" ) );

			//			int[] res = GetResult2JudgeData(ref datas);
			int[] res = ResultMaster.ResultCount4Rank;
			csv.DataInput( 6, 6, "合格（紫）" );
			csv.DataInput( 7, 6, String.Format( "{0}枚", res[3] ) );
			csv.DataInput( 6, 7, "合格（緑）" );
			csv.DataInput( 7, 7, String.Format( "{0}枚", res[2] ) );
			csv.DataInput( 6, 8, "合格（黄）" );
			csv.DataInput( 7, 8, String.Format( "{0}枚", res[1] ) );
			csv.DataInput( 6, 9, "不合格" );
			csv.DataInput( 7, 9, String.Format( "{0}枚", res[0] ) );
			//			csv.DataInput(6, 7, GetResult2NGCount(ref datas).ToString() + " 枚");

			csv.FileOut( filePath );

		}


		void CSVResultAndGraphOutput( string filePath = null ) {
			CsvOutput csv = new CsvOutput();
			int pageMax = ResultMaster.Qty;
			if ( ResultMaster.Qty == 0 ) return;

			if ( filePath == null ) {
				if ( !Directory.Exists( "CSV" ) ) Directory.CreateDirectory( "CSV" );

			}


			for ( int i = 0; i<pageMax; i++ ) {
				_writeCsvResultAndGraph( csv, i );
			}
			csv.FileOut( filePath );
		}

		// 現在確定した測定データ一覧をCSVファイルに出力する
		void CSVOutput( String filePath ) {
			CsvOutput csv = new CsvOutput();
			//var datas = ResultMaster.ReadResult( 0 );
			int pageMax = ResultMaster.Qty;
			int page = 1;

			if ( ResultMaster.Qty == 0 ) return;

			for ( int i = 0; i<pageMax; i++ ) {
				_writeCsvResultAndGraph( csv, i );
			}

			for ( int i = 0; i<pageMax; i++ ) {
				var d = ResultMaster.ReadResult( i );
				csv.DataInput( 0, 0, String.Format( "{0}枚中{1}枚目", pageMax, page ) );
				csv.DataInput( 0, 1, "検査日" );
				csv.DataInput( 1, 1, d.now.ToString( "yyyy/MM/dd HH:mm:ss" ) );
				csv.DataInput( 0, 2, "検査品番" );
				csv.DataInput( 1, 2, d._typeName );
				csv.DataInput( 0, 3, "公称厚み" );
				csv.DataInput( 1, 3, d.type._stdThickness + "mm" );
				csv.DataInput( 3, 1, "総合判定" );
				csv.DataInput( 4, 1, d._allResult );
				csv.DataInput( 3, 2, "総合平均" );
				csv.DataInput( 4, 2, d._allAverage );
				csv.DataInput( 3, 3, "基準値" );
				csv.DataInput( 4, 3, d.type.GetRange );
				csv.DataInput( 7, 1, "検査担当者" );
				csv.DataInput( 8, 1, d._humanName );

				csv.DataInput( 0, 5, "項目" );
				csv.DataInput( 1, 5, "Ａ列" );
				csv.DataInput( 2, 5, "Ｂ列" );
				csv.DataInput( 3, 5, "Ｃ列" );
				csv.DataInput( 4, 5, "Ｄ列" );
				csv.DataInput( 5, 5, "Ｅ列" );
				csv.DataInput( 6, 5, "Ｆ列" );
				csv.DataInput( 7, 5, "Ｇ列" );
				csv.DataInput( 0, 6, "最小値" );
				csv.DataInput( 0, 7, "最大値" );
				csv.DataInput( 0, 8, "平均値" );
				csv.DataInput( 0, 9, "判定" );

				for ( int row = 0; row<4; row++ ) {
					for ( int col = 0; col<7; col++ ) {
						csv.DataInput( col+1, row+6, d._results[row][col] );
					}
				}

				csv.AddOffset( 0, 13 );
				page++;

			}
			csv.FileOut( filePath );
		}

		// 測定データ全てをCSVファイルに出力する
		void CSVgraphOutput( String filePath, stThicknessResult data ) {
			CsvOutput csv = new CsvOutput();

			csv.DataInput( 0, 0, new String[] { "CNT", "A", "B", "C", "D", "E", "F", "G" } );

			int x = 1, y = 1;
			foreach ( var val in data.GetResultMM() ) {
				if ( !float.IsNaN( val ) ) {
					csv.DataInput( x, y, val.ToString() );
					x++;
				} else {
					csv.DataInput( 0, y, y.ToString() );
					y++;
					x = 1;
				}
			}

			csv.FileOut( filePath );
		}

		// 測定データ全てをCSVファイルに出力する
		void CSVgraphOutput2AD( String filePath, stThicknessResult data ) {
			CsvOutput csv = new CsvOutput();

			csv.DataInput( 0, 0, new String[] { "CNT", "A", "B", "C", "D", "E", "F", "G" } );

			int x = 1, y = 1;
			foreach ( var val in data.GetResultVoltage() ) {
				if ( !float.IsNaN( val ) ) {
					csv.DataInput( x, y, val.ToString() );
					x++;
				} else {
					csv.DataInput( 0, y, y.ToString() );
					y++;
					x = 1;
				}
			}

			csv.FileOut( filePath );
		}
	}
}
