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

		const String VerMes = "Ver 0.91";
		const String OFFSET_FILE_NAME = "ofs.dat";							// オフセットデータ保存用ファイル名
		const String DATA_FILE_NAME = "result";								// データ保存用ファイル名プレフィックス

		String GetTodaySubfolder { get { return "./" + DateTime.Now.ToString("yyyyMMdd"); } }	// 日付フォルダ生成

		SerialConnector _port;												// シリアルポートとの接続管理
		static stThickness[] _monitor = new stThickness[7];					// リアルタイムデータ保存用（別窓で参照したいのでstatic指定
		Queue<stThickness[][]> Result = new Queue<stThickness[][]>();       // 測定データ取得整理後の蓄積用キュー（１シート単位）
		Queue<Char[]> SensorErrQ = new Queue<char[]>();                     // センサーエラー検知時のデータ保持キュー

		TicknessResultAdapter ResultMaster; // = new TicknessResultAdapter(DateTime.Now);

		stThicknessResult nowDisplayData;									// 表示中データ
		int nowLot = 0;
		int maxLot = 0;

		List<stBodyType> BodyType = new List<stBodyType>();					// 母材データ一覧
		stBodyType SelectedType;											// 上記から現在選択されているものへのセレクタ

		tkMeasureRange D111_40180_14, B131_40181_14, PH05_CH18_04_1, PH05_CH18_04_2;    // くり貫き位置算出用データ一覧
		tkMeasureRange Base;
		tkMeasureRange SelectedItem;                                                    // 上記から現在選択されているものへのセレクタ
		
		ThicknessOffset[] Offset = new ThicknessOffset[7];

		int cmdValue = 0;                                                   // マイコンへの指令を定期的に送る為の変数

		Timeout _to;                                                        // オフセット処理タイムアウトカウント用

		System.Threading.Mutex mutex;                                       // 二重起動禁止処理用

		// センサ測定位置（Ｘ方向）（割合データ）
		float[] _sensXpos = { 0.074286f, 0.217143f, 0.36f, 0.502857f, 0.645714f, 0.788571f, 0.931429f };

		// リアルタイムデータ取得
		public stThickness[] GetMonitor { get { return _monitor; } }

		//--------------------------------------------------------------------------------------------------------------
		// イベント

		public Form1() {
			InitializeComponent();

			// 二重起動禁止
			bool createdNow;
			mutex = new System.Threading.Mutex(true, "ThicknessMeasure", out createdNow);
			if (!createdNow) {
				// ミューテックすの初期所有権が付与されなかったときは
				// すでに起動されていると判断する
				MessageBox.Show("二重起動できません");
				Environment.Exit(0);
			}
		}

		private void Form1_Load(object sender, EventArgs e) {

			this.Text += VerMes;

			ExchangeComPortList();
			int l = Offset.Length;

			dgvResult.EnableHeadersVisualStyles = false;

			// 母材データ
			Color[] c = new Color[] { Color.FromArgb(255, 255, 0), Color.FromArgb(226, 239, 218), Color.FromArgb(153, 102, 255) };
			BodyType.Add(new stBodyType("4130 ネオサーム(INF)6t", 0.989f, 6, 5.5f, 6.6f, new double[] { 5.4, 5.56, 6.61 }, c));
			BodyType.Add(new stBodyType("5130-TN FFﾍﾟｰﾊﾟｰ4t", 1.045f, 4, 3.75f, 4.4f, new double[] { 3.6, 3.76, 4.41 }, c));
			BodyType.Add(new stBodyType("5130-TN FFﾍﾟｰﾊﾟｰ6t", 1.037f, 6, 5.4f, 6.6f, new double[] { 5.5, 5.61, 6.60, 6.90 }, c));


			// 加工型番データ
			D111_40180_14 = new tkMeasureRange(350, 300, 50.6f/2);
			D111_40180_14.addBaseX(new float[] { 28.3f, 81.7f, 135.1f, 188.5f, 241.9f, 295.3f });
			D111_40180_14.addBaseX(new float[] { 54.7f, 108.1f, 161.5f, 214.9f, 268.3f, 321.7f, });
			D111_40180_14.addCol(new float[] { 29.3f, 77.8f, 126.3f, 174.8f, 223.3f, 271.8f });

			B131_40181_14 = new tkMeasureRange(350, 300, 43.1f/2);
			B131_40181_14.addBaseX(new float[] { 81.6f, 129.1f, 176.6f, 224.1f, 271.6f, 319.1f });
			B131_40181_14.addCol(new float[] { 27.9f, 75.4f, 122.9f, 170.4f, 217.9f, 265.4f });

			PH05_CH18_04_1 = new tkMeasureRange(350, 300, 45f/2);
			PH05_CH18_04_1.addBaseX(new float[] { 29f, 77f, 125f, 173f, 221f, 269f, 317f });
			PH05_CH18_04_1.addBaseX(new float[] { 36f, 84f, 132f, 180f, 228f, 276f, 324f });
			PH05_CH18_04_1.addCol(new float[] { 27.5f, 76.5f, 125.5f, 174.5f, 223.5f, 272.5f });

			PH05_CH18_04_2 = new tkMeasureRange(350, 300, 49.4f/2);
			PH05_CH18_04_2.addBaseX(new float[] { 56.3f, 108.7f, 161.1f, 213.5f, 265.9f, 318.3f });
			PH05_CH18_04_2.addBaseX(new float[] { 31.7f, 84.1f, 136.5f, 188.9f, 241.3f, 293.7f });
			PH05_CH18_04_2.addCol(new float[] { 30f, 78f, 126f, 174f, 222f, 270 });

			Base = new tkMeasureRange(350, 300, 30f/2);
			Base.addBaseX(new float[] { 18f+22, 62f+22, 106f+22, 150f+22, 194f+22, 238f+22 });
			Base.addCol(new float[] { 54f, 102f, 150f, 198f, 246f, 294f });

			// 校正データバックアップ読み出し
			if ( File.Exists(OFFSET_FILE_NAME)) {
				Offset = (ThicknessOffset[])ObjectSerializer.LoadFile(OFFSET_FILE_NAME);
				nudOffsetA.Value = (decimal)Offset[0].mm;
				nudOffsetB.Value = (decimal)Offset[1].mm;
				nudOffsetC.Value = (decimal)Offset[2].mm;
				nudOffsetD.Value = (decimal)Offset[3].mm;
				nudOffsetE.Value = (decimal)Offset[4].mm;
				nudOffsetF.Value = (decimal)Offset[5].mm;
				nudOffsetG.Value = (decimal)Offset[6].mm;
			} else {
				for (int i = 0; i<l; i++) {
					// 2.5V 2.5Vの値 = 6mm
					Offset[i] = new ThicknessOffset(32760, 6.0f);
				}
			}

			// ロット処理とバックアップデータ読み出し
			int loadNo = GetLatestLotNo(GetTodaySubfolder);
			LotControl(loadNo);

			// 厚さチェックポイント計算用アイテムの設定
			cbBodyType.SelectedIndex = 0;
			this.SelectedItem = D111_40180_14;

			// ネオサーム色
			Color co = Color.FromArgb(83, 141, 213);
			ChangeReportColor(co, co);

			// 名前一覧の読み出し
			LoadMemberList("member.txt");

			// 印刷時のメソッド登録
			pd.PrintPage += Pd_PrintPage;
			pd.DefaultPageSettings.Landscape = true;

		}

		// 印刷処理用イベントハンドラ
		private void Pd_PrintPage(object sender, PrintPageEventArgs e) {
			var size = e.PageBounds;

			// タイトル
			Font f = new Font("MSUI Gothic", 30, FontStyle.Underline | FontStyle.Bold);
			e.Graphics.DrawString("原板移動票", f, Brushes.Black, 90, 10);

			// 日付
			f = new Font("MSUI Gothic", 25, FontStyle.Bold);
			String date = nowDisplayData.now.ToString("yyyy年MM月dd日");
			e.Graphics.DrawString(date, f, Brushes.Black, 800, 10);

			// 原板種類
			Brush titleColor = new SolidBrush(Color.FromArgb(230, 184, 183));
			var baseRect = new RectangleF(0, 100, 580, 150);
			String body = nowDisplayData.type._typeName;
			DrawTextBox(ref e, body, 0, baseRect, titleColor, Brushes.Black ) ;

			// 原版ロット
			var lotRect = new RectangleF(baseRect.Right, baseRect.Y, 350, 60);
			String lot = nowDisplayData._lotName;
			DrawTextBox(ref e, "原板ロット", 20, lotRect, Brushes.White, Brushes.Black);
			lotRect = new RectangleF(lotRect.X, lotRect.Bottom, lotRect.Width, baseRect.Height-lotRect.Height);
			DrawTextBox(ref e, lot, 50, lotRect, Brushes.White, Brushes.Black);

			// 総数
			var pageRect = new RectangleF(lotRect.Right, baseRect.Y, 190, 60);
//			int allVal = ResultData.Count();
			int allVal = ResultMaster.Qty;
			DrawTextBox(ref e, "総　数", 20, pageRect, Brushes.White, Brushes.Black);
			pageRect = new RectangleF(pageRect.X, pageRect.Bottom, pageRect.Width, baseRect.Height-pageRect.Height);
			DrawQtyBox(ref e, allVal, "枚", 30, pageRect);

			// 判子
			RectangleF sRect;
			sRect = new RectangleF(700, 300, 100, 150);

			Font f2 = new Font("MSUI Gothic", 20);
			e.Graphics.DrawString("打 抜", f2, Brushes.Black, sRect.X+12, sRect.Y+40);
			e.Graphics.DrawString("担当者", f2, Brushes.Black, sRect.X+2, sRect.Y+80);

			for (int i=0; i<4; i++) {
				e.Graphics.DrawRectangle(Pens.Black, sRect.X, sRect.Y, sRect.Width, sRect.Height);
				sRect.X = sRect.Right;
			}


			sRect.X = 700;
			sRect.Y = sRect.Bottom;
			sRect.Height = 50;
			String[] post = new String[] { "品質課長", "製造課長", "現場責任者", "検査者" };
			for(int i=0; i<4; i++) {
				DrawTextBox(ref e, post[i], 0, sRect, Brushes.White, Brushes.Black);
				sRect.X = sRect.Right;
			}

			sRect.X = 700;
			sRect.Height = 150 + 50;
			for (int i = 0; i<4; i++) {
				e.Graphics.DrawRectangle(Pens.Black, sRect.X, sRect.Y, sRect.Width, sRect.Height);
				sRect.X = sRect.Right;
			}


			// result
			RectangleF rrect = new RectangleF(20, 300, 400, 60);
			DrawTextBox(ref e, "測定結果", 30, rrect, Brushes.White, Brushes.Black);

			RectangleF rrect2 = new RectangleF(rrect.X, rrect.Bottom, 180, 90);
			DrawTextBox(ref e, "合格", 30, rrect2, Brushes.White, Brushes.Black);

			rrect2.Y = rrect2.Bottom;
			DrawTextBox(ref e, "合格（灰）", 25, rrect2, Brushes.Gray, Brushes.Black);

			rrect2.Y = rrect2.Bottom;
			DrawTextBox(ref e, "合格（黄）", 25, rrect2, Brushes.Yellow, Brushes.Black);

			rrect2.Y = rrect2.Bottom;
			DrawTextBox(ref e, "不合格", 30, rrect2, Brushes.White, Brushes.Red);

			var res = ResultMaster.ResultCount4Rank;

			rrect2.Y = rrect.Bottom;
			rrect2.X = rrect2.Right;
			rrect2.Width = rrect.Width-rrect2.Width;
			DrawQtyBox(ref e, res[2], "枚", 30, rrect2);

			rrect2.Y = rrect2.Bottom;
			DrawQtyBox(ref e, res[3], "枚", 30, rrect2);

			rrect2.Y = rrect2.Bottom;
			DrawQtyBox(ref e, res[1], "枚", 30, rrect2);

			rrect2.Y = rrect2.Bottom;
			DrawQtyBox(ref e, res[0], "枚", 30, rrect2);

			rrect.X = rrect.Right;
			rrect.Width = 220;
			DrawTextBox(ref e, "枚数確認", 30, rrect, Brushes.White, Brushes.Black);

			rrect2.X = rrect.X;
			rrect2.Y = rrect.Bottom;
			rrect2.Width = rrect.Width;
			DrawTextBox(ref e, "　", 30, rrect2, Brushes.White, Brushes.Black);

			rrect2.Y = rrect2.Bottom;
			DrawTextBox(ref e, "　", 30, rrect2, Brushes.White, Brushes.Black);

			rrect2.Y = rrect2.Bottom;
			DrawTextBox(ref e, "　", 30, rrect2, Brushes.White, Brushes.Black);

			rrect2.Y = rrect2.Bottom;
			DrawTextBox(ref e, "　", 30, rrect2, Brushes.White, Brushes.Black);


			e.PageSettings.Color = true;
			e.HasMorePages = false;

		}

		//// 接続部
		// ＣＯＭスキャンボタン押下
		private void bCommScan_Click(object sender, EventArgs e) {
			ExchangeComPortList();
		}

		// ＣＯＭ接続ボタン押下
		private void bConnect_Click(object sender, EventArgs e) {
			if (cbPortList.SelectedIndex>=0) {
				_port = new SerialConnector((String)cbPortList.SelectedItem);
				if (_port.IsEnable) {
					gbConnect.Enabled = false;
					gbCont.Enabled = true;
					公正有効化ToolStripMenuItem.Enabled = true;
					menuThicknessMonitor.Enabled = true;
					_port.PacketReceived += DataExchangeCallback;
				} else {
					_port.Close();
				}
			}
		}


		//// 操作部
		// 測定ボタン押下
		bool isMeasure = false;
		private void bMeasure_Click(object sender, EventArgs e) {
			bMeasureControl(!isMeasure);
		}

		// イニシャルボタン押下
		bool isInitial = false;
		private void bInitial_Click(object sender, EventArgs e) {
			if (!isMeasure && !isInitial) {
				byte[] buf = new byte[4];
				cmdValue = 3;

				buf[0] = (byte)'C';
				buf[1] = (byte)'I';
				buf[3] = (byte)'\n';

				byte msk = 0;
				if (cbOffsetA.Checked) msk |= 0x01;
				if (cbOffsetB.Checked) msk |= 0x02;
				if (cbOffsetC.Checked) msk |= 0x04;
				if (cbOffsetD.Checked) msk |= 0x08;
				if (cbOffsetE.Checked) msk |= 0x10;
				if (cbOffsetF.Checked) msk |= 0x20;
				if (cbOffsetG.Checked) msk |= 0x40;

				if (msk > 0) {
					msk |= 0x80;
					buf[2] = msk;
					isInitial = true;
					bMeasureControl(false);
					bInitial.BackColor = Color.Yellow;
					_port._port.Write(buf, 0, 4);
					if (_to != null) _to.Stop();
					_to = new Timeout(6000, TimeoutEvent);
				}
			}
		}

		// グラフ出力ボタン押下
		private void bGraph_Click(object sender, EventArgs e) {
			SaveFileDialog fd = new SaveFileDialog();
			fd.Title = "保存するファイル";
			fd.Filter = "CSVファイル(*.csv)|*.csv|すべてのファイル(*.+)|*.*";
			fd.RestoreDirectory = true;

			var r = fd.ShowDialog();
			if (r == DialogResult.OK) {
				CSVgraphOutput(fd.FileName, nowDisplayData);
			}
		}

		// 母材データ更新時
		private void cbBodyType_SelectedIndexChanged(object sender, EventArgs e) {
			SelectedType = BodyType[cbBodyType.SelectedIndex];
			Color c;
			switch (cbBodyType.SelectedIndex) {
			case 0:         // ネオサーム
				c = Color.FromArgb(83, 141, 213);
				break;
			case 1:         // TN4t
				c = Color.FromArgb(255, 255, 0);
				break;
			default:         // TN6t
				c = Color.FromArgb(255, 102, 153);
				break;
			}
			cbBodyType.BackColor = c;
			bMeasure.Focus();
		}

		// 検査者選択変更時
		private void cbName_SelectedIndexChanged(object sender, EventArgs e) {
		}

		// ＣＳＶ出力ボタン押下
		private void button5_Click(object sender, EventArgs e) {
			SaveFileDialog fd = new SaveFileDialog();
			fd.Title = "保存するファイル";
			fd.Filter = "CSVファイル(*.csv)|*.csv|すべてのファイル(*.+)|*.*";
			fd.RestoreDirectory = true;

			var r = fd.ShowDialog();
			if (r == DialogResult.OK) {
				//List<stThicknessResult> d = null;
				String file = Path.GetFileNameWithoutExtension(fd.FileName);
				String dic = Path.GetDirectoryName(fd.FileName);
				for (int i = 0; i<=maxLot; i++) {
					//					LoadResultFile(ref d, GetTodaySubfolder, i);
					//					LoadResultFile(GetTodaySubfolder, i);
					//					if (d != null) {
					var d = ResultMaster.ReadResult(0);
					CSVOutput(dic + "/" + file + d.now.ToString("yyyyMMdd") + i.ToString("00") + ".csv" );
					CSVTitleOutput(dic + "/" + file + d.now.ToString("yyyyMMdd") + i.ToString("00") + "Title.csv");
//					}
				}
			}
		}

		// 電圧表出力ボタン押下
		private void button2_Click(object sender, EventArgs e) {
			SaveFileDialog fd = new SaveFileDialog();
			fd.Title = "保存するファイル";
			fd.Filter = "CSVファイル(*.csv)|*.csv|すべてのファイル(*.+)|*.*";
			fd.RestoreDirectory = true;

			var r = fd.ShowDialog();
			if (r == DialogResult.OK) {
				CSVgraphOutput2AD(fd.FileName, nowDisplayData);
			}
		}

		// 印刷ボタン押下
		private void bPrintout_Click(object sender, EventArgs e) {
			if( nowDisplayData != null) {
				ResultPrintout();
			}
		}

		// Lot加算ボタン押下
		private void bNext_Click(object sender, EventArgs e) {
			LotControl(true);
		}

		// Lot減算ボタン押下
		private void bPrev_Click(object sender, EventArgs e) {
			LotControl(false);
		}

		//// 校正
		// Allチェック操作
		private void cbAll_CheckedChanged(object sender, EventArgs e) {
			cbOffsetA.Checked = cbAll.Checked;
			cbOffsetB.Checked = cbAll.Checked;
			cbOffsetC.Checked = cbAll.Checked;
			cbOffsetD.Checked = cbAll.Checked;
			cbOffsetE.Checked = cbAll.Checked;
			cbOffsetF.Checked = cbAll.Checked;
			cbOffsetG.Checked = cbAll.Checked;
		}


		//// 測定結果
		// 測定結果ページ切り替え時
		private void nudResultPage_ValueChanged(object sender, EventArgs e) {
			DisplayResult((int)nudResultPage.Value-1);
		}


		//// メニュー
		// 厚みモニターメニューセレクト
		private void menuThicknessMonitor_Click(object sender, EventArgs e) {
			var f = new Monitor(_port, this);
			gbOffset.Enabled = false;
			bMeasureControl(false);
			cmdValue = 2;
			f.ShowDialog();
			cmdValue = 0;
		}

		// 校正有効化メニューセレクト
		private void 公正有効化ToolStripMenuItem_Click(object sender, EventArgs e) {
			gbOffset.Enabled = true;
		}


		// 未使用
		private void bClear_Click(object sender, EventArgs e) {
			var f = new Monitor(_port, this);
			f.ShowDialog();
		}


		// インターバル処理
		int dly = 0;
		private void timer1_Tick(object sender, EventArgs e) {

			if (dly < 10) dly++;
			else {
				dly = 0;

				if (_port != null) {
					switch (cmdValue) {
					case 0:
						_port._port.WriteLine("CP");
						break;
					case 1:
						_port._port.WriteLine("CS");
						break;
					case 2:
						_port._port.WriteLine("CT");
						break;
					}
				}
			}

			while(SensorErrQ.Count() > 0) {
				var r = SensorErrQ.Dequeue();
				StringBuilder sb = new StringBuilder();

				PlaySE(eMelodyType.ERROR);
				foreach( Char c in r) {
					sb.AppendFormat("センサー{0}の検知が異常です。\r\n", c);
				}
				MessageBox.Show(sb.ToString(), "センサー異常検知");
			}

			while (Result.Count() > 0) {
				var r = Result.Dequeue();
				if (r.Count() > 2) {
					CheckThicknessResult(r);
				}
			}
		}


		//-----------------------------------------------------------------------------------------------------------
		// ファンクション

		// COMポート一覧を更新
		void ExchangeComPortList() {
			cbPortList.Items.Clear();
			cbPortList.Text = "";
			cbPortList.Items.AddRange(SerialConnector.GetPortList());
			if (cbPortList.Items.Count > 0) {
				cbPortList.SelectedIndex = 0;
			}

		}

		/// ロットデータ管理用メソッド郡
		String[] GetResultFileNames( String subFolder ) {
			return Directory.GetFiles(subFolder, "result???_0000");
		}

		// ロットデータファイルを見に行って、最終番号を取得する
		int GetLatestLotNo( String subFolder) {
			int max = -1;
			if (Directory.Exists(subFolder)) {
				foreach (String f in GetResultFileNames(subFolder)) {
					int no;
					if (int.TryParse(f.Substring(f.Length-8, 3), out no)) {
						if (max < no) max = no;
					}
				}
			}
			return max;
		}


		// Lotボタン関係の処理
		void LotControl(bool isUp) {
			if (isUp) {
				LotControl( nowLot+1 );
			} else {
				LotControl((nowLot>0) ? nowLot-1 : 0);
			}
		}

		void LotControl(int lotNo) {
			if (lotNo >= 0) nowLot = lotNo;
			else lotNo = 0;
			if (maxLot < lotNo) {
				maxLot = lotNo;
				bMeasureControl(false);
			}
			// 現在有効データを更新
			ResultMaster = new TicknessResultAdapter(DateTime.Now, lotNo);
//			LoadResultFile(GetTodaySubfolder, lotNo);
			DisplayResult();

		}


		// ボディタイプデータの更新（過去からのVerUp)
		void ExchangeBodytype(ref stThicknessResult data) {
			if( data != null) {
				foreach (var b in BodyType) {
					if (data.type._typeName == b._typeName) {
						data.type = b;
						break;
					}
				}
			}
		}

		// member.txtからメンバーリスト取得
		void LoadMemberList( String path) {
			if( File.Exists(path)) {
				String[] nameList = File.ReadAllLines(path);
				cbName.Items.Clear();
				cbName.Items.AddRange(nameList);
				cbName.Items.Add("その他");
			}
		}

		// 四角形の真ん中に文字を描画する
		void DrawTextCenter(ref PrintPageEventArgs e, String str, int fontsize, RectangleF rect, Brush fontColor) {
			Font f;
			SizeF size;
			Graphics g = e.Graphics;

			if (fontsize <= 0) {
				fontsize = 8;
				do {
					fontsize++;
					f = new Font("MSUI Gothic", fontsize, FontStyle.Bold);
					size = g.MeasureString(str, f);
				} while (rect.Width > size.Width);
				f = new Font("MSUI Gothic", fontsize-1, FontStyle.Bold);
			} else {
				f = new Font("MSUI Gothic", fontsize, FontStyle.Bold);
			}
			size = g.MeasureString(str, f);

			// 文字を中央に持ってくる計算
			float ypos = (rect.Height-size.Height) /2 +rect.Y;
			float xpos = (rect.Width-size.Width)/2 +rect.X;

			g.DrawString(str, f, fontColor, xpos, ypos);

		}

		// 個数単位を含めた箱を描画する
		void DrawQtyBox(ref PrintPageEventArgs e, int value, String unit, int fontsize, RectangleF rect) {
			Font f = new Font("MSUI Gothic", 30, FontStyle.Bold);
			Graphics g = e.Graphics;

			var uFontSize = g.MeasureString(unit, f);
			g.DrawRectangle(Pens.Black, rect.X, rect.Y, rect.Width, rect.Height);

			RectangleF valRect = new RectangleF(rect.X, rect.Y, rect.Width-uFontSize.Width, rect.Height);
			DrawTextCenter(ref e, value.ToString(), fontsize, valRect, Brushes.Black);

			valRect = new RectangleF(rect.X+valRect.Width, rect.Y, uFontSize.Width, rect.Height);
			DrawTextCenter(ref e, unit, fontsize, valRect, Brushes.Black);

		}

		// 背景色を指定して箱を描画する
		void DrawTextBox(ref PrintPageEventArgs e, String str, int fontsize, RectangleF rect, Brush boxColor, Brush fontColor) {
			Graphics g = e.Graphics;

			g.FillRectangle(boxColor, rect);
			g.DrawRectangle(Pens.Black, rect.X, rect.Y, rect.Width, rect.Height);

			DrawTextCenter(ref e, str, fontsize, rect, fontColor);
		}


		// プリントアウト処理を開始する
		void ResultPrintout() {
			pd.DefaultPageSettings.Landscape = true;
			pd.Print();
		}


		// 不良検出用のブザーを鳴らす
		WaveOut wo = new WaveOut();
		public enum eMelodyType {
			TRUE, Y_TRUE, FALSE, ERROR, SP_ADPO, MISS
		}
		void PlaySE( eMelodyType t) {
			String file = "";

			switch(t) {
			case eMelodyType.TRUE:		file = "true.mp3";	break;
			case eMelodyType.Y_TRUE:	file = "yellowtrue.mp3"; break;
			case eMelodyType.FALSE:		file = "false.mp3"; break;
			case eMelodyType.MISS:		file = "inputmiss.mp3"; break;
			case eMelodyType.SP_ADPO:	file = "spadop.mp3"; break;
			case eMelodyType.ERROR:		file = "error.mp3"; break;
			}

			if(File.Exists(file)) {
				if (wo.PlaybackState == PlaybackState.Playing) {
					wo.Stop();
				}

				var reader = new AudioFileReader(file);
				wo.Init(reader);
				wo.Play();
			}
		}

		// 測定結果を画面に反映させる
		void DisplayResult() => DisplayResult(ResultMaster.Qty-1);
		void DisplayResult( int no ) {
			stThicknessResult data;

			//			if ((ResultData.Count > 0) && (no < ResultData.Count())) data = ResultData[no];
			//			else data = null;
			data = ResultMaster.ReadResult(no);

			nowDisplayData = data;

			ExchangeBodytype(ref data);			// 判定をランク別にする以前のデータに判定値を押し込める

			if ( data != null) {
				Color c;
				switch (data.type._typeName) {
				case "4130 ネオサーム(INF)6t":	// ネオサーム
					c = Color.FromArgb(83, 141, 213);
					break;
				case "5130-TN FFﾍﾟｰﾊﾟｰ4t":		// TN4t
					c = Color.FromArgb(255, 255, 0);
					break;
				default :						// TN6t
					c = Color.FromArgb(255, 102, 153);
					break;
				}
				ChangeReportColor(c, c);
			}

//			PageValueExchange(no+1, ResultData.Count());
			PageValueExchange(no+1, ResultMaster.pageMax);
			lLotDisp.Text = String.Format("ロット {0} / {1}", nowLot+1, maxLot+1);

			dgvResult.Rows.Clear();
			dgvResult.Rows.Add(4);
			dgvResult.Rows[0].DefaultCellStyle.BackColor = Color.DarkGray;
			dgvResult.Rows[1].DefaultCellStyle.BackColor = Color.LightGray;
			dgvResult.Rows[2].DefaultCellStyle.BackColor = Color.DarkGray;
			dgvResult.Rows[3].DefaultCellStyle.BackColor = Color.LightGray;

			dgvResult[0, 0].Value = "最小値";
			dgvResult[0, 1].Value = "最大値";
			dgvResult[0, 2].Value = "平均値";
			dgvResult[0, 3].Value = "判定";

			//dgvResult.Rows[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

			if (data != null) {

				tbResultDate.Text = String.Format("検査日：{0}", data.now.ToString("yy/MM/dd HH:mm:ss"));
				tbResultType.Text = String.Format("検査品番：{0}", data._partName);
				tbResultTN.Text = String.Format("公称厚み：{0}mm", data.type._stdThickness);
				tbResultRange.Text = String.Format("基準値：{0}", data.type.GetRange);
				tbResultLotName.Text = String.Format("原板ロット：{0}", data._lotName);
				if( data._humanName != null )
					tbResultMember.Text = String.Format("検査担当者：{0}", data._humanName);

				int[] result = new int[] { 1, 1, 1, 1, 1, 1, 1 };		// 複数判定用処理
//				int allResult = 1;										// 複数判定用処理
				for (int i = 0; i<4; i++) {
					for (int j = 0; j<7; j++) {
						DataGridViewCell cell = dgvResult[j+1, i];
						DataGridViewColumn col = dgvResult.Columns[j+1];
						String s = data._results[i][j];

						switch(i) {
						case 0:
						case 1:
						case 2:
							float tmp = GetEndValue(s);

							result[j] = GetMeasureLvMax( result[j], data.type.GetJudgeTypeNo(tmp) );
							Color c = data.type.GetJudgeColor(tmp);
							if( c == data.type.GetTrueColor) {
								cell.Style.BackColor = dgvResult.RowsDefaultCellStyle.BackColor;
							} else {
								cell.Style.BackColor = c;
							}

							cell.Value = data._results[i][j];
							break;
						case 3:
							if( result[j] == 1) {
								cell.Style.BackColor = dgvResult.RowsDefaultCellStyle.BackColor;
								col.HeaderCell.Style.BackColor = col.DefaultCellStyle.BackColor;
							} else {
								cell.Style.BackColor = data.type.GetJudgeColorFromTypeNo( result[j] );
								col.HeaderCell.Style.BackColor = data.type.GetJudgeColorFromTypeNo(result[j]);
							}

							if( result[j] == -1) {
								cell.Value = "不良";
							} else {
								cell.Value = "良";
							}

							//allResult = GetMeasureLvMax(allResult, result[j]);
							break;
						}

					}
				}
				dgvResult.ClearSelection();


				tbAllAverage.Text = data._allAverage;
				int allResult = data.GetRankValue();
				bAllResult.BackColor = data.type.GetJudgeColorFromTypeNo(allResult);
				switch ( allResult) {
				case 0:
					bAllResult.Text = "合格";
					break;
				case 1:
					bAllResult.Text = "合格";
					break;
				case 2:
					bAllResult.Text = "Ｂ１３１用";
					break;
				default:
					bAllResult.Text = "不合格";
					break;
				}

				lNGValue.Text = String.Format("不良数： {0}枚", ResultMaster.ResultCount4Rank[0]);
			} else {
				tbResultDate.Text = String.Format("検査日：");
				tbResultType.Text = String.Format("検査品番：");
				tbResultTN.Text = String.Format("公称厚み：");
				tbResultRange.Text = String.Format("基準値：");
				for (int i = 0; i<4; i++) {
					for (int j = 0; j<7; j++) {
						dgvResult[j+1, i].Value = "-";
					}
				}
				tbAllAverage.Text = "";
				bAllResult.BackColor = SystemColors.Control;
				bAllResult.Text = "";
				lNGValue.Text = String.Format("不良数： 0枚");
			}
		}

		// 文字列の後ろ側にある数値を取り出す
		float GetEndValue(String str) {
			int i,e=0;
			for(i=str.Length-1; i>0; i--) {
				if ((str[i]>='0' && str[i]<='9') || str[i]=='.') {
					e = i+1;
					break;
				}
			}
			for (; i>0; i--) {
				if ((str[i]>='0' && str[i]<='9') || str[i]=='.') {
				} else {
					i++;
					break;
				}
			}
			return float.Parse(str.Substring(i, e-i));
		}

		// 測定データを測定結果に変換し、最新のものを画面に反映させる
		void CheckThicknessResult( stThickness[][] data) {
			stThickness[] maxs = new stThickness[7];
			stThickness[] mins = new stThickness[7];
			float[] sums = new float[7] { 0, 0, 0, 0, 0, 0, 0 };
			int[] sumcnts = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
			bool[] results = new bool[7] { true, true, true, true, true, true, true };


			// データ整理
			for(int i = 0; i<data.Length; i++) {
				for(int j = 0; j<data[i].Length; j++) {
					stThickness d = data[i][j];
					stThickness max = maxs[j];
					stThickness min = mins[j];
					float sum = sums[j];
					int cnt = sumcnts[j];
					bool result = results[j];

					var pos = SelectedItem.CheckMeasureRange2Rect(new PointF(_sensXpos[d.Group-'A'], d.PosPer));
					if( pos.X != -1) {
						if( (maxs[j] == null) || (maxs[j].mmValue < d.mmValue) ) {
							d.Pos = pos.Y;
							maxs[j] = d;
						}
						if ( (mins[j] == null) || (mins[j].mmValue > d.mmValue) ) {
							d.Pos = pos.Y;
							mins[j] = d;
						}
						sums[j] += d.mmValue;
						sumcnts[j] ++;

						if (d.Result == false) results[j] = false;
					}
				}
			}

			String[][] r = new String[4][];
			r[0] = new string[7];
			r[1] = new string[7];
			r[2] = new string[7];
			r[3] = new string[7];
			int allTypeResult = 1;

			float allSums = 0, allSumcnt = 0;
			for (int i = 0; i<7; i++) {
				if (maxs[i] != null) {
					r[0][i] = mins[i].GetResultStr();
					r[1][i] = maxs[i].GetResultStr();
					r[2][i] = (Math.Round(sums[i] /sumcnts[i],2)).ToString("F2") + "mm";

					r[3][i] = (results[i]) ? "良" : "不良";

					allTypeResult = GetMeasureLvMax(allTypeResult, mins[i].GetResultRank());
					allTypeResult = GetMeasureLvMax(allTypeResult, maxs[i].GetResultRank());

					allSums += sums[i];
					allSumcnt += sumcnts[i];
				} else {
					r[0][i] = "-";	r[1][i] = "-";	r[2][i] = "-";	r[3][i] = "-";
				}
			}

			String part = cbBodyType.Text;
			String allAvr = (allSums/allSumcnt).ToString("F2") + "mm";


			var tmp = new stThicknessResult(data, tbLotName.Text, part, SelectedType, "", (String)cbName.SelectedItem, r, allAvr, null);
			switch (allTypeResult) {
			case 0:
				PlaySE(eMelodyType.Y_TRUE);
				tmp._allResult = "合格,0";
				break;
			case 1:
				PlaySE(eMelodyType.TRUE);
				tmp._allResult = "合格,1";
				break;
			case 2:
				PlaySE(eMelodyType.SP_ADPO);
				tmp._allResult = "合格,2";
				break;
			default:
				PlaySE(eMelodyType.FALSE);
				tmp._allResult = "不合格.-1";
				break;
			}

			ResultMaster.AddResult(tmp);

			PageValueExchange(ResultMaster.Qty, ResultMaster.Qty);

		}

		// 測定結果ページ表示の一括管理
		void PageValueExchange( int page, int maxPage) {
			if ( page > maxPage ) page = maxPage;
			lResultPageMax.Text = String.Format("/{0}", maxPage);
			nudResultPage.Maximum = maxPage;
			if (maxPage > 0) nudResultPage.Minimum = 1;
			nudResultPage.Value = page;
		}

		// 現在確定した測定データのタイトルをCSVファイルに出力する
		void CSVTitleOutput(String filePath){
			CsvOutput csv = new CsvOutput();
			var datas = ResultMaster.ReadResult(0);

			csv.DataInput(2, 3, "母材");
			csv.DataInput(2, 4,  datas.type._typeName);

			csv.DataInput(2, 6,  "総数");
			csv.DataInput(2, 7,  ResultMaster.Qty.ToString() + " 枚");

			csv.DataInput(2, 9,  "原板ロット");
			csv.DataInput(2, 10, datas._lotName);


			csv.DataInput(6, 3, "検査日");
			csv.DataInput(6, 4, datas.now.ToString("yyyy年"));
			csv.DataInput(7, 4, datas.now.ToString("MM月dd日"));

//			int[] res = GetResult2JudgeData(ref datas);
			int[] res = ResultMaster.ResultCount4Rank;
			csv.DataInput(6, 6, "合格（紫）");
			csv.DataInput(7, 6, String.Format("{0}枚", res[3]));
			csv.DataInput(6, 7, "合格（緑）");
			csv.DataInput(7, 7, String.Format("{0}枚", res[2]));
			csv.DataInput(6, 8, "合格（黄）");
			csv.DataInput(7, 8, String.Format("{0}枚", res[1]));
			csv.DataInput(6, 9, "不合格" );
			csv.DataInput(7, 9, String.Format("{0}枚", res[0]));
//			csv.DataInput(6, 7, GetResult2NGCount(ref datas).ToString() + " 枚");

			csv.FileOut(filePath);

		}

		// 現在確定した測定データ一覧をCSVファイルに出力する
		void CSVOutput(String filePath){
			CsvOutput csv = new CsvOutput();
			var datas = ResultMaster.ReadResult(0);
			int pageMax = ResultMaster.Qty;
			int page = 1;

			if (ResultMaster.Qty == 0) return;

			for(int i=0; i<pageMax; i++) {
				var d = ResultMaster.ReadResult(i);
				csv.DataInput(0, 0, String.Format("{0}枚中{1}枚目", pageMax, page));
				csv.DataInput(0, 1, "検査日");
				csv.DataInput(1, 1, d.now.ToString("yyyy/MM/dd HH:mm:ss"));
				csv.DataInput(0, 2, "検査品番");
				csv.DataInput(1, 2, d._typeName);
				csv.DataInput(0, 3, "公称厚み");
				csv.DataInput(1, 3, d.type._stdThickness + "mm");
				csv.DataInput(3, 1, "総合判定");
				csv.DataInput(4, 1, d._allResult);
				csv.DataInput(3, 2, "総合平均");
				csv.DataInput(4, 2, d._allAverage);
				csv.DataInput(3, 3, "基準値");
				csv.DataInput(4, 3, d.type.GetRange);
				csv.DataInput(7, 1, "検査担当者");
				csv.DataInput(8, 1, d._humanName);

				csv.DataInput(0, 5, "項目");
				csv.DataInput(1, 5, "Ａ列");
				csv.DataInput(2, 5, "Ｂ列");
				csv.DataInput(3, 5, "Ｃ列");
				csv.DataInput(4, 5, "Ｄ列");
				csv.DataInput(5, 5, "Ｅ列");
				csv.DataInput(6, 5, "Ｆ列");
				csv.DataInput(7, 5, "Ｇ列");
				csv.DataInput(0, 6, "最小値");
				csv.DataInput(0, 7, "最大値");
				csv.DataInput(0, 8, "平均値");
				csv.DataInput(0, 9, "判定");

				for (int row = 0; row<4; row++) {
					for (int col = 0; col<7; col++) {
						csv.DataInput(col+1, row+6, d._results[row][col]);
					}
				}

				csv.AddOffset(0, 13);
				page++;

			}
			csv.FileOut(filePath);
		}

		// 測定データ全てをCSVファイルに出力する
		void CSVgraphOutput(String filePath, stThicknessResult data) {
			CsvOutput csv = new CsvOutput();

			csv.DataInput(0,0,new String[] { "CNT","A","B","C","D","E","F","G"});

			int x = 1, y = 1;
			foreach( var val in data.GetResultMM() ) {
				if( !float.IsNaN(val) ) {
					csv.DataInput(x, y, val.ToString());
					x++;
				} else {
					csv.DataInput(0, y, y.ToString());
					y++;
					x = 1;
				}
			}

			csv.FileOut(filePath);
		}

		// 測定データ全てをCSVファイルに出力する
		void CSVgraphOutput2AD(String filePath, stThicknessResult data) {
			CsvOutput csv = new CsvOutput();

			csv.DataInput(0, 0, new String[] { "CNT", "A", "B", "C", "D", "E", "F", "G" });

			int x = 1, y = 1;
			foreach (var val in data.GetResultVoltage()) {
				if (!float.IsNaN(val) ) {
					csv.DataInput(x, y, val.ToString());
					x++;
				} else {
					csv.DataInput(0, y, y.ToString());
					y++;
					x = 1;
				}
			}

			csv.FileOut(filePath);
		}

		// 測定データ（１シート）を受信したら呼び出されるコールバック
		void DataExchangeCallback( object sender, EventArgs e) {
			UInt16[] data = (UInt16[])sender;

			if (isInitial) {
				isInitial = false;

				if (_to != null) {
					_to.Stop();
					_to = null;
				}

				if (cbOffsetA.Checked) {
					Offset[0].ad = data[0];
					Offset[0].mm = (float)nudOffsetA.Value;
				}
				if (cbOffsetB.Checked) {
					Offset[1].ad = data[1];
					Offset[1].mm = (float)nudOffsetB.Value;
				}
				if (cbOffsetC.Checked) {
					Offset[2].ad = data[2];
					Offset[2].mm = (float)nudOffsetC.Value;
				}
				if (cbOffsetD.Checked) {
					Offset[3].ad = data[3];
					Offset[3].mm = (float)nudOffsetD.Value;
				}
				if (cbOffsetE.Checked) {
					Offset[4].ad = data[4];
					Offset[4].mm = (float)nudOffsetE.Value;
				}
				if (cbOffsetF.Checked) {
					Offset[5].ad = data[5];
					Offset[5].mm = (float)nudOffsetF.Value;
				}
				if (cbOffsetG.Checked) {
					Offset[6].ad = data[6];
					Offset[6].mm = (float)nudOffsetG.Value;
				}

				ObjectSerializer.SaveFile(Offset, OFFSET_FILE_NAME);

				this.Invoke((MethodInvoker)(() => {
					cbOffsetA.Checked = false;
					cbOffsetB.Checked = false;
					cbOffsetC.Checked = false;
					cbOffsetD.Checked = false;
					cbOffsetE.Checked = false;
					cbOffsetF.Checked = false;
					cbOffsetG.Checked = false;
					cbAll.Checked = false;
					bInitial.BackColor = SystemColors.Control;
				}));

				cmdValue = 0;
				// this.Invoke((MethodInvoker)(() => bInitial.BackColor = SystemColors.Control));

			} else {
				List<stThickness> row = new List<stThickness>();
				List<stThickness[]> col = new List<stThickness[]>();
				char gr = 'A';
				int cnt = 0;
				bool dataEn = true;
				int[] errCnt = new int[] { 0, 0, 0, 0, 0, 0, 0 };

				for (int i = 0; i<data.Length; i++) {
					if (data[i] != 0xFFFF) {
						if (data[i] == 0) dataEn = false;
						else if (data[i] == 0xFFFE) errCnt[gr-'A']++;
						row.Add(new stThickness(SelectedType, data[i], Offset[gr-'A'].ad, Offset[gr-'A'].mm, gr, i/8, data.Length/8));
						gr++;
					} else {
						if( !dataEn) cnt++;
						col.Add(row.ToArray());
						dataEn = true;
						_monitor = row.ToArray();
						row = new List<stThickness>();

						gr = 'A';
					}
				}

				List<Char> sErr = new List<char>();
				for( int i=0; i<7; i++) {
					if (errCnt[i] > 50) sErr.Add((Char)(i+'A'));
				}

				if( col.Count() > 2) {
					if (sErr.Count() > 0) {
						SensorErrQ.Enqueue(sErr.ToArray());
					} else if (cnt > (data.Length/80)) {
						PlaySE(eMelodyType.MISS);
					} else {
						Result.Enqueue(col.ToArray());
					}
				} else {
					Result.Enqueue(col.ToArray());
				}
			}
		}

		// 割合から位置を計算する
		float _Per2Pos( float per, float max) => max/100+per;

		// 測定ボタンの挙動を制御する
		void bMeasureControl(bool en) {
			if (!en) {
				cbName.Enabled = true;
				tbLotName.ReadOnly = false;
				isMeasure = false;
				bMeasure.BackColor = SystemColors.Control;
				if( _port != null ) _port._port.WriteLine("CP");
				cmdValue = 0;
			} else {
				if( cbName.Text != "" && tbLotName.Text !=  "" ) {
					cbName.Enabled = false;
					tbLotName.ReadOnly = true;
					isMeasure = true;
					gbOffset.Enabled = false;
					bMeasure.BackColor = Color.LawnGreen;
					if( _port != null) _port._port.WriteLine("CS");
					cmdValue = 1;
				}
			}
		}

		private void bResultDelete_Click( object sender, EventArgs e ) {
			if ( ResultMaster.pageMax > 0 ) {
				bResultDelete.Enabled = false;
				ResultMaster.DeleteResult( (int)nudResultPage.Value-1 );
				DisplayResult( (int)nudResultPage.Value-1 );
				bResultDelete.Enabled = true;
			}
		}

		// 検査結果表示の背景色を変更する
		void ChangeReportColor(Color title, Color items) {
			tbResultDate.BackColor = items;
			tbResultMember.BackColor = items;
			tbResultRange.BackColor = items;
			tbResultRange.BackColor = items;
			tbResultTN.BackColor = items;
			tbResultLotName.BackColor = items;
			tbResultType.BackColor = items;
			dgvResult.ColumnHeadersDefaultCellStyle.BackColor = title;
		}


		// 判定結果（数値）の数値更新
		int GetMeasureLvMax( int d1, int d2) {
			int[] sort = new int[] { 3, 1, 0, 2 };
			if( sort[d1+1] > sort[d2+1]) {
				return d1;
			} else {
				return d2;
			}
		}

		// イベント
		void TimeoutEvent(object sender, ElapsedEventArgs e) {
			this.Invoke((MethodInvoker)(() => bInitial.BackColor = SystemColors.Control));
			cmdValue = 0;
			isInitial = false;
			_to.Stop();
			_to = null;
			MessageBox.Show("校正に失敗しました");
		}

	}

	public class TicknessResultAdapter {

		public int pageMax { get; private set; }
		int nowGroupNo;
		List<stThicknessResult> nowPageData;

		String DATA_FILE_NAME;
		String filePattern;
		String folderName;

		const int PAGE_SIZE = 50;

		public int[] ResultCount4Rank { get; private set; }             // 合格判定別のロット毎の数を記録
		public int Qty { get { return ResultCount4Rank.Sum(); } }		// 判定結果の合計＝総数

		public String[] GetFileList {
			get {
				var files = Directory.GetFiles("./" + folderName, filePattern + "_*");
				Array.Sort(files);
				return files;
			}
		}

		public TicknessResultAdapter(DateTime dateFolder, int lotNo = 0) {
			DATA_FILE_NAME = "Result";
			folderName = dateFolder.ToString("yyyyMMdd");
			filePattern = String.Format("Result{0:000}", lotNo);
			InitData();
		}


		public void DivisionFile( string fileName) {
			var dataList = LoadFile(fileName);
			int divNo = 0;
			while( dataList.Count() > 0) {
				if (dataList.Count() > 50) {
					var save = dataList.Take(50).ToList();
					SaveFile(save, divNo++);
					dataList = dataList.Skip(50).ToList();
				} else {
					SaveFile(dataList, divNo);
					break;
				}
			}
		}

		/// <summary>分割ファイルから現在データを吸い取る</summary>
		void InitData() {
			List<stThicknessResult> dataList;
			int[] rank = { 0, 0, 0, 0 };
			int pageCnt = 0, grCnt = 0;

			if (!Directory.Exists("./"+folderName)) Directory.CreateDirectory("./"+folderName);

			foreach (string file in GetFileList) {
				dataList = LoadFile(Path.GetFileName(file));
				int[] r = dataList.GetResultRank();
				for (int i = 0; i<rank.Length; i++) rank[i] += r[i];
				pageCnt += dataList.Count();
				grCnt++;
				nowPageData = dataList;		// 最新版を捕獲
			}
			pageMax = pageCnt;
			nowGroupNo = grCnt-1;
			ResultCount4Rank = rank;
		}

		/// <summary>リザルトデータを追加する</summary>
		/// <param name="data">追加対象</param>
		public void AddResult( stThicknessResult data) {
			int grPage = pageMax/PAGE_SIZE;
			if (nowGroupNo != grPage) nowPageData = LoadFile(grPage);
//			if (nowPageData.Count() == PAGE_SIZE) AddGroup();

			ResultCount4Rank[data.GetRankValue()+1]++;		// 合否カウント更新

			nowPageData.Add(data);
			SaveFile(nowPageData, grPage);
			pageMax++;

		}

		/// <summary>最新の分割番号でグループを取得する</summary>
		//void AddGroup() {
		//	SaveFile(nowPageData, pageMax/PAGE_SIZE +1);
		//	nowPageData = new List<stThicknessResult>();
		//}

		/// <summary>ページ番号から指定されたリザルトデータを取得する</summary>
		/// <param name="page">ページ番号</param>
		/// <returns>リザルトデータ</returns>
		public stThicknessResult ReadResult(int page) {
			if (pageMax == 0) return null;
			if (page >= pageMax) page = pageMax-1;
			var divPage = page % PAGE_SIZE;
			var grPage = page / PAGE_SIZE;

			if (nowGroupNo != grPage) nowPageData = LoadFile(grPage);
			return nowPageData[divPage];
		}

		public void DeleteResult(int page) {
			if ( page >= pageMax ) page = pageMax-1;
			var divPage = page %PAGE_SIZE;
			var grPage = page /PAGE_SIZE;
			var grMax = (pageMax-1) /PAGE_SIZE;


			List<stThicknessResult> delPage = LoadFile( grPage );
			ResultCount4Rank[delPage[divPage].GetRankValue()+1]--;
			pageMax--;
			delPage.RemoveAt( divPage );

			List<stThicknessResult> dest, src=null;
			dest = delPage;
			for ( ; grPage<grMax; grPage++ ) {
				src = LoadFile( grPage+1 );

				dest.Add( src[0] );
				src.RemoveAt( 0 );
				SaveFile( dest, grPage );
				dest = src;
			}
			if ( dest.Count > 0 ) SaveFile( dest, grPage );
			else DeleteFile( grPage );
			nowGroupNo = -1;
		}

		/// <summary>ファイルからリザルトデータをロードする</summary>
		/// <param name="divisionNo">分割ファイルの番号</param>
		/// <returns></returns>
		List<stThicknessResult> LoadFile(int divisionNo) {
			string file = String.Format("{0}_{1:0000}", filePattern, divisionNo);
			nowGroupNo = divisionNo;
			return LoadFile(file);
		}
		List<stThicknessResult> LoadFile(string fileName) {
			var data = ObjectSerializer.LoadFile(String.Format("./{0}/{1}", folderName, fileName)) as List<stThicknessResult>;
			if (data == null) data = new List<stThicknessResult>();
			return data;
		}

		/// <summary>分割ファイルにデータを保存する</summary>
		/// <param name="data">保存対象</param>
		/// <param name="divisionNo">保存用の分割番号</param>
		void SaveFile(object data, int divisionNo) {
			string file = String.Format("{0}_{1:0000}", filePattern, divisionNo);
			SaveFile(data, file);

		}
		void SaveFile(object data, string fileName) {
			ObjectSerializer.SaveFile( data, String.Format("./{0}/{1}", folderName, fileName));
			return;
		}

		void DeleteFile( int divisionNo ) {
			string file = String.Format( "./{0}/{1}_{2:0000}",folderName, filePattern, divisionNo );
			if ( File.Exists( file ) ) {
				File.Delete( file );
			}
		}
	}

	// オフセットデータ保存用クラス
	[Serializable]
	public class ThicknessOffset {
		public int ad;			// センサー値のオフセット基準距離データ
		public float mm;		// センサー値のオフセット実データ

		public ThicknessOffset( int ad, float mm) {
			this.ad = ad;
			this.mm = mm;
		}
	}

	// 母材の設定値クラス
	[Serializable]
	public class stBodyType {
		public String _typeName;			// 母材の型名
		public double _correction;			// 
		public double _stdThickness;		// 基準厚さ
		public double _min;					// 厚さ下限
		public double _max;					// 厚さ上限
		public double _allAverage;			// 厚み全体平均
		public double[] _judgeRange;		// 段階的判定範囲
		public Color[] _judgeRangeColor;	// 段階的判定範囲の背景色分け

		public Color GetTrueColor { get { return _judgeRangeColor[1]; } }		// 確実な正常判定色取得

		// シリアル化したデータをロードするためのダミー
		public stBodyType() {
		}

		// コンストラクタ
		public stBodyType(String typeName, float corr, float stdThickness, float min, float max, double[] judgeRange, Color[] judgeRangeColor) {
			_typeName = typeName;
			_correction = corr;
			_stdThickness = stdThickness;
//			double errRange = Math.Round(stdThickness *errRate /100, 2);
			_min = min;
			_max = max;
			_judgeRange = judgeRange;
			_judgeRangeColor = judgeRangeColor;
		}

		/// <summary>判定レベルを返却する
		/// -1:不合格 / 0:ぎりぎり合格 / 1:標準合格 / 2特殊採用
		/// </summary>
		/// <param name="val">検査データ</param>
		/// <returns></returns>
		public int GetJudgeTypeNo(double val) {
			int i;
			int cnt = _judgeRange.Length;

			for (i=0; i<cnt; i++) {
				if (_judgeRange[i] > val) break;
			}
			if (i==0 || i==cnt) return -1;
			return i-1;
		}

		// 判定レベルの色を返却する
		public Color GetJudgeColor(double val) {
			int i;
			int cnt = _judgeRange.Length;

			for (i=0; i<cnt; i++) {
				if (_judgeRange[i] > val) break;
			}
			if (i==0 || i==cnt) return Color.Red;
			return _judgeRangeColor[i-1];
		}

		// 判定レベルから、判定レベルの色を返却する
		public Color GetJudgeColorFromTypeNo(int no) {
			int cnt = _judgeRangeColor.Length;
			if (no==-1 || no==cnt) return Color.Red;
			return _judgeRangeColor[no];
		}

		// 判定範囲を示す文字列を返却する
		public String GetRange { get { return String.Format("{0}mm ～ {1}mm", Math.Round(_max, 2), Math.Round(_min, 2)); } }

	}

	// 測定データを保存用に加工するクラス
	[Serializable]
	public class stThicknessResult {
		public stThickness[][] _result;		// 測定データ本体
		public String _lotName;				// 対象ロット名	
		public String _partName;			// 対象型名
		public String _typeName;			// 対象母材名
		public String _allAverage;			// 総合平均値
		public String _allResult;			// 総合判定
		public String[][] _results;			// 測定データ結果
		public DateTime now;				// 計測日時
		public stBodyType type;				// 母材の詳細データ
		public String _humanName;			// 検査者

		public stThicknessResult( stThickness[][] result, String lotName, String partName, stBodyType type, String typeName, String humanName, String[][] results, String allAverage, String allResult) {
			this._result = result;
			this._lotName = lotName;
			this._partName = partName;
			this.type = type;
			this.now = DateTime.Now;
			this._typeName = typeName;
			this._humanName = humanName;
			this._results = results;
			this._allAverage = allAverage;
			this._allResult = allResult;
		}

		/// <summary>
		/// 文字列の最後尾にある数値データを取得する
		/// </summary>
		/// <param name="str">数値が含まれる文字列</param>
		/// <returns>変換後の実数</returns>
		double _getEndValue(String str) {
			int i, e = 0;
			for (i=str.Length-1; i>0; i--) {
				if ((str[i]>='0' && str[i]<='9') || str[i]=='.') {
					break;
				}
			}
			e = i+1;
			for (; i>0; i--) {
				if ((str[i]>='0' && str[i]<='9') || str[i]=='.') {
				} else {
					i++;
					break;
				}
			}
			return double.Parse(str.Substring(i, e-i));
		}

		/// <summary>内部保持のresults[][]からランクを取得する。内部allResultも更新する</summary>
		public int GetRankValue() {
			double min=1000, max=0,mintmp,maxtmp;
			string[] vals = _allResult.Split(new char[] { ',' });
			int r;
			if( vals.Length >= 2) {
				r = int.Parse(vals[1]);
			} else {
				for( int i=0; i<7; i++) {
					mintmp = _getEndValue(_results[0][i]);
					maxtmp = _getEndValue(_results[1][i]);
					min = (min<mintmp) ? min : mintmp;
					max = (max>maxtmp) ? max : maxtmp;
				}
				r = ExpansionClass.GetMeasureLvMax(type.GetJudgeTypeNo(min),type.GetJudgeTypeNo(max));
				switch (r) {
				case 0:
					_allResult = "合格,0";
					break;
				case 1:
					_allResult = "合格,1";
					break;
				case 2:
					_allResult = "合格,2";
					break;
				default:
					_allResult = "不合格,-1";
					break;
				}
			}
			return r;
		}

		/// <summary>取得AD値から電圧に変換して吐き出す。列データ・-1････で繰り返す</summary>
		public IEnumerable<float> GetResultVoltage() {

			if (_result == null) yield break;

			int i_size = _result.Length-3;
			int j_size = _result[0].Length;

			for (int i = 0; i<i_size; i++) {
				for (int j = 0; j<j_size; j++) {
					if (_result[i][j] != null) {
						yield return _result[i][j].adValue*10.0f/65536f;
					} else {
						yield return 0;
					}
				}
				yield return float.NaN;
			}
			yield break;
		}

		// Foreach用のyield関数
		public IEnumerable<float> GetResultMM() {

			if (_result == null) yield break;

			int i_size = _result.Length-3;
			int j_size = _result[0].Length;

			for(int i = 0; i<i_size; i++) {
				for(int j = 0; j<j_size; j++) {
					if( _result[i][j] != null) {
						yield return _result[i][j].mmValue;
					} else {
						yield return 0f;
					}
				}
				yield return float.NaN;
			}
			yield break;
		}

	}

	// 測定データクラス
	[Serializable]
	public class stThickness {
		public stBodyType type;		// 母材の詳細データ
		public int adValue;			// 受信したアナログデータ
		public float mmValue;		// アナログデータから算出した厚さデータ
		public char Group;			// 位置グループ
		public int Count;			// データの位置カウント
		public int Pos;				// 未使用
		public float PosPer;		// 位置割合
		public bool Result;			// 判定

		public int OffsetAd;		// ＡＤデータのオフセット値
		public float OffsetMm;		// 厚さデータのオフセット

		// オフセットを含まない厚さデータ
		public float GetNakedMmValue {
			get {
				return (float)((float)(adValue-OffsetAd)*10/0xFFF/8  +OffsetMm);
			}
		}

		// コンストラクター
		public stThickness(stBodyType type, int ad, int offsetAd, float offsetMm, char gr, int cnt, int maxCnt) {
			this.type = type;
			OffsetAd = offsetAd;
			OffsetMm = offsetMm;
			adValue = ad;
			mmValue = (float)(((float)(ad-offsetAd)*10/0xFFF/8  +offsetMm) *type._correction);
			Group = gr;
			Count = cnt;
			PosPer = (float)cnt/maxCnt;
			Result = ((mmValue < type._max) && (mmValue > type._min)) ? true : false;
		}

		// 結果を表の文字列に変換する関数
		public String GetResultStr() {
			return String.Format("{0}{1} / {2}", Group, Pos, Math.Round(mmValue, 2).ToString("F2"));
		}

		/// <summary>判定地取得 -1:不合格 / 0:ぎりぎり合格 / 1:標準合格 / 2特殊採用</summary>
		public int GetResultRank() => type.GetJudgeTypeNo(mmValue);

		public void SetPosPer( int pos, int length ) {
			PosPer = (float)pos /length;
		}

		// デバッグ表示用
		override public string ToString() {
			return String.Format("{0}{1}:{2},{3},{4}", Group, Count, adValue, Math.Round(mmValue, 2), Result);
		}

	};

	// 板の測定位置計測用クラス
	public class tkMeasureRange {
		PointF baseSize;
		float _radius;
		int _xpos = 0;

		List<PointF[]> _posList = new List<PointF[]>();
		List<PointF[]> _perList = new List<PointF[]>();
		List<float[]> _baseXList = new List<float[]>();


		public tkMeasureRange(float sizex, float sizey, float itemRadius) {
			baseSize = new PointF(sizex, sizey);
			_radius = itemRadius / sizex;
		}

		public void addBaseX(float[] list) {
			_baseXList.Add(list);
		}

		public void addCol(float[] list) {
			if (_baseXList.Count() > 0) {

				for(int y = 0; y<list.Length; y++) {
					float[] xp = _baseXList[_xpos];
					PointF[] d = new PointF[xp.Length];
					PointF[] p = new PointF[xp.Length];

					for (int x = 0; x<xp.Length; x++) {
						d[x] = new PointF(xp[x], list[y]);
						p[x] = new PointF(d[x].X /baseSize.X, d[x].Y /baseSize.Y);
					}
					_posList.Add(d);
					_perList.Add(p);

					_xpos++;
					if (_xpos == _baseXList.Count()) _xpos = 0;

				}
			}
		}

		bool IsRange(float a, float target, int range) {
			return ((target-range) <= a && a <= (target +range));
		}

		public Point CheckMeasureRange2Rect(PointF chkPos) {
			int ypos = 0, xpos = 0;

			// Y方向の決定
			for (ypos=0; ypos<_perList.Count(); ypos++) {
				PointF p = _perList[ypos][0];
				if (chkPos.Y < (p.Y + _radius)) {
					if (!(chkPos.Y >= (p.Y -_radius))) return new Point(-1, -1);
					break;
				}
			}

			if (ypos == _perList.Count()) return new Point(-1, -1);

			for(xpos=0; xpos<_perList[ypos].Length; xpos++) {
				PointF p = _perList[ypos][xpos];
				if (IsRange(chkPos.X, p.X, 1)) {
					break;
				}
			}

			return new Point(xpos, ypos);
		}

		public Point CheckMeasureRange2Cirle(PointF chkPos) {
			int ypos = 0, xpos = 0;

			// 指定座標をY方向に超えた位置を検出
			for (ypos=0; ypos<_perList.Count(); ypos++) {
				if (chkPos.Y < _perList[ypos][0].Y) break;
			}
			if (ypos >= _perList.Count()) ypos--;

			// 上記Y時のX方向に超えた位置を検出
			for (xpos=0; xpos<_perList[ypos].Length; xpos++) {
				if (chkPos.X < _perList[ypos][xpos].X) break;
			}
			if (xpos >= _perList[ypos].Length) xpos--;

			// XY共に超えたデータでの範囲比較
			float len = GetMeasureFrontPoint(_perList[ypos][xpos], _radius, chkPos.X);
			if ((chkPos.Y > _perList[ypos][xpos].Y-len) && (chkPos.Y < _perList[ypos][xpos].Y+len)) return new Point(xpos, ypos);

			if (xpos > 0) {
				xpos--;

				// Xが超える手前のデータでの範囲比較
				len = GetMeasureFrontPoint(_perList[ypos][xpos], _radius, chkPos.X);
				if ((chkPos.Y > _perList[ypos][xpos].Y-len) && (chkPos.Y < _perList[ypos][xpos].Y+len)) return new Point(xpos, ypos);
			}

			if (ypos > 0) {
				ypos--;

				for (xpos=0; xpos<_perList[ypos].Count(); xpos++) {
					if (chkPos.X < _perList[ypos][xpos].X) break;
				}
				if (xpos >= _perList[ypos].Length) xpos--;

				// Yが超える手前のデータで範囲比較
				len = GetMeasureFrontPoint(_perList[ypos][xpos], _radius, chkPos.X);
				if ((chkPos.Y > _perList[ypos][xpos].Y-len) && (chkPos.Y < _perList[ypos][xpos].Y+len)) return new Point(xpos, ypos);

				if (xpos > 0) {
					xpos--;

					// XYが超える手前のデータでの範囲比較
					len = GetMeasureFrontPoint(_perList[ypos][xpos], _radius, chkPos.X);
					if ((chkPos.Y > _perList[ypos][xpos].Y-len) && (chkPos.Y < _perList[ypos][xpos].Y+len)) return new Point(xpos, ypos);
				}
			}
			return new Point(-1, -1);
		}

		public float GetMeasureFrontPoint(PointF cirBase, float radius, float targetX) {
			return radius;
//			double xLen = targetX -cirBase.X;
//			return (float)Math.Sqrt(radius*radius - xLen*xLen);
		}


	}

	public class DummyData {
		//public static stThickness CreatDummyData() {

		//}

		decimal GetRandamData(decimal baseValue, decimal range) {
			var rnd = new System.Random();
			decimal rngVal = ((decimal)(rnd.NextDouble() -0.5) *range * 2) ;
			return rngVal + baseValue;
		}

	}

	public static class ExpansionClass {

		/// <summary>
		/// リストを捜査し、判定結果の分類を各々の数で取得する
		/// </summary>
		/// <param name="my"></param>
		/// <returns></returns>
		public static int[] GetResultRank(this List<stThicknessResult> my) {
			int[] r = { 0, 0, 0, 0 };
			int lv;
			foreach (var d in my) {
				lv = d.GetRankValue();
				r[lv+1]++;
			}
			return r;
		}

		/// <summary>判定結果の悪いほうを返す  -1:不合格 / 0:ぎりぎり合格 / 1:標準合格 / 2特殊採用</summary>
		public static int GetMeasureLvMax(int d1, int d2) {
			int[] sort = new int[] { 3, 2, 0, 1 };      // 優先順位(不合格 > 特殊合格 > ギリギリ合格 > 合格）
			return (sort[d1+1] > sort[d2+1]) ? d1 : d2;
		}


	}

}
