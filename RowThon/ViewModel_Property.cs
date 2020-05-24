using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Windows;
using System.Net;						//for Dns
using System.IO;                        //for Stream
using System.Xml;                       //for xml reader
using System.Xml.Serialization;			//for
using System.ComponentModel;            //for INotifyPropertyChanged
using System.Windows.Input;				//for ICommand;

namespace RowThon
{
	public partial class ViewModel : ViewModelBase
	{
		/////////////////////////////////////////////////////////////
		//
		//ﾌﾟﾛﾊﾟﾃｨ
		//
		public PythonConsoleControl.IronPythonConsoleControl PythonConsole
		{
			get { return (_pythonConsole); }
			set { _pythonConsole = value; }
		}
		/// <summary>
		///  ｱﾌﾟﾘ設定
		/// </summary>
		public Settings Setting
		{
			//get { return (_appSettings); }
			//set { _appSettings = value; }
			get { return (App.Setting); }
			set { App.Setting = value; }
		}
		/// <summary>
		///  ｻｰﾎﾞの電源状態
		/// </summary>
		public bool SvPowStat
		{
			//get { return (((_ioOutput & 3) == 3) ? true : false); }
			get { return (((_ioInput & 4) == 4) ? true : false); }
			/*
			get {
				if(SvControl == null) {
					return (false);
				}
				int pow = 0;
				for(int i = 0; i < 5; i++) {
					pow += SvControl.GetAxisStatus(i, (int)SvStatType.POWER_INFO);
				}
				return ((pow == 5) ? true : false);
			}
			*/
			set { /*IoOutputValue = (value) ? (IoOutputValue |= 3) : (IoOutputValue & ~(uint)3);*/ }
		}
		/// <summary>
		/// I/O 入力
		/// </summary>
		public UInt32 IoInputValue
		{
			get { return (_ioInput); }
			set
			{
				//変化のあった
				UInt32 diff = value ^ _ioInput;

				_ioInput = value;

				if (diff != 0) {
					//RaisePropertyChanged("IoInputValue");
					this.RaisePropertyChanged(o => o.IoInputValue);
				}
				for (int bit = 0; bit < 32; bit++) {
					if ((diff & (1 << bit)) != 0) {

						string propName = "IsChecked" + (bit + 1).ToString("00");
						RaisePropertyChanged(propName);
					}
				}
				this.RaisePropertyChanged(o => o.SvPowStat);
			}
		}
		/// <summary>
		/// I/O 出力
		/// </summary>
		public UInt32 IoOutputValue
		{
			get { return (_ioOutput); }
			set
			{
				UInt32 diff = _ioOutput ^ value;

				if( diff != 0 ) {
					_ioOutput = value;

					for( int bit = 0; bit < 32; bit++ ) {
						if( (diff & (1 << bit)) != 0 ) {

							string propName = "IsOut" + (bit + 1).ToString("00");
							RaisePropertyChanged(propName);
						}
					}
					//RaisePropertyChanged("SvPowStat");
					this.RaisePropertyChanged(o => o.SvPowStat);

					IoControl.Write(0, IoOutputValue);
				}
			}
		}
		/// <summary>
		///  接続状況の背景色
		/// </summary>
		public System.Windows.Media.SolidColorBrush ConnectionBackGround
		{
			get
			{
				if( _connectMsg == "接続中" ) {
					return (System.Windows.Media.Brushes.GreenYellow);
				}
				else {
					return (System.Windows.Media.Brushes.White);
				}
			}
		}
		/// <summary>
		///  接続状況ﾒｯｾｰｼﾞ
		/// </summary>
		public string ConnectionMsg
		{
			get { return (_connectMsg); }
			set {
				_connectMsg = value;
//				RaisePropertyChanged("ConnectionMsg");
//				RaisePropertyChanged("ConnectionBackGround");

				//拡張ﾒｿｯﾄﾞでﾌﾟﾛﾊﾟﾃｨ更新
				this.RaisePropertyChanged(o => o.ConnectionMsg);
				this.RaisePropertyChanged(o => o.ConnectionBackGround);
			}
		}
		private System.Windows.Media.SolidColorBrush AxisBackGround(int axisNo)
		{
			if( _svStatus[axisNo] != 0 ) {
				return (System.Windows.Media.Brushes.Red);
			}
			else {
				return (System.Windows.Media.Brushes.White);
			}
		}
		/// <summary>
		///  軸現在値表示の背景色
		/// </summary>
		public System.Windows.Media.SolidColorBrush AxisBackGround0 { get { return (AxisBackGround(0)); } }
		public System.Windows.Media.SolidColorBrush AxisBackGround1 { get { return (AxisBackGround(1)); } }
		public System.Windows.Media.SolidColorBrush AxisBackGround2 { get { return (AxisBackGround(2)); } }
		public System.Windows.Media.SolidColorBrush AxisBackGround3 { get { return (AxisBackGround(3)); } }
		public System.Windows.Media.SolidColorBrush AxisBackGround4 { get { return (AxisBackGround(4)); } }

		static public string SvAlamMsg(int code)
		{
			string msg = _svCtrl.GetAlamMsg(code);
/*
			string msg = "";
			string multi = "";

			if( (code & 0x0001) != 0)	{ msg += "＋方向偏差過大";					multi = "\n";	}
			if( (code & 0x0002) != 0)	{ msg += (multi + "－方向偏差過大");		multi = "\n";	}
			if( (code & 0x0004) != 0)	{ msg += (multi + "ｻｰﾎﾞｱﾝﾌﾟｱﾗｰﾑ");			multi = "\n";	}
			if( (code & 0x0008) != 0)	{ msg += (multi + "＋方向ｿﾌﾄﾘﾐｯﾄ");			multi = "\n";	}
			if( (code & 0x0010) != 0)	{ msg += (multi + "－方向ｿﾌﾄﾘﾐｯﾄ");			multi = "\n";	}
			if( (code & 0x0020) != 0)	{ msg += (multi + "＋方向ﾊｰﾄﾞﾘﾐｯﾄ");		multi = "\n";	}
			if( (code & 0x0040) != 0)	{ msg += (multi + "－方向ﾊｰﾄﾞﾘﾐｯﾄ");		multi = "\n";	}
			if( (code & 0x0080) != 0)	{ msg += (multi + "＋方向ﾊﾟﾙｽ発生過大");	multi = "\n";	}
			if( (code & 0x0100) != 0)	{ msg += (multi + "－方向ﾊﾟﾙｽ発生過大");	multi = "\n";	}
			if( (code & 0x0200) != 0)	{ msg += (multi + "ｻｰﾎﾞ主電源OFF");			multi = "\n";	}
			if( (code & 0x0400) != 0)	{ msg += (multi + "ﾒｶﾄﾛﾘﾝｸ通信ｴﾗｰ");		multi = "\n";	}
			if( (code & 0x0800) != 0)	{ msg += (multi + "ﾒｶﾄﾛﾘﾝｸ多重ｺﾏﾝﾄﾞ");		multi = "\n";	}
*/

			return (msg);
		}
		private string AxisErrMsg(int axisNo)
		{
			int st = (int)_svStatus[axisNo];

			if( st == -2 ) {
				return ("ｻｰﾎﾞ未生成");
			}
			else if( st != 0 ){
				return (SvAlamMsg(st));
			}
			return ("正常");
		}
		/// <summary>
		///  軸ｴﾗｰﾒｯｾｰｼﾞ(ToolTip)
		/// </summary>
		public string AxisErrMsg0 { get { return ( AxisErrMsg(0)); } }
		public string AxisErrMsg1 { get { return ( AxisErrMsg(1)); } }
		public string AxisErrMsg2 { get { return ( AxisErrMsg(2)); } }
		public string AxisErrMsg3 { get { return ( AxisErrMsg(3)); } }
		public string AxisErrMsg4 { get { return ( AxisErrMsg(4)); } }

		public bool GrabStatus
		{
			get { return ( _visionCtrl.GrabStatus() ); }
			set { }
		}
		public void UpdateProperty(string propName)
		{
			RaisePropertyChanged(propName);
		}
	}
}
