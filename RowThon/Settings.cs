using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.IO;                        //for Stream
using System.Xml;                       //for xml reader
using System.Xml.Serialization;			//for
using Microsoft.Windows.Controls.Ribbon;

namespace RowThon
{
	public class Settings
	{
		private string _ioBoardName;
		private string _svCtrlName;
		private int _cmdPort;		// ｺﾏﾝﾄﾞ受信用ﾎﾟｰﾄ
		private int _statusPort;	// ｽﾃｰﾀｽ通知用ﾎﾟｰﾄ
		private int _eventPort;		// 未使用
		private int _waitPort;		// wait ｺﾏﾝﾄﾞ用のｿｹｯﾄﾎﾟｰﾄ番号
		private string _ipAddress;	// Rowthon IPｱﾄﾞﾚｽ
		private int _saftyPosZ;		// Z軸安全高さ
		private int _UpperLimitZ;
		private int _LowerLimitZ;
		private int _UpperLimitL;
		private int _LowerLimitL;
		private int _UpperLimitQ;
		private int _LowerLimitQ;
		private int _MaxJogSpd;		//JOG最大設定速度
		private	int _MinJogSpd;		//
		private int _MaxXYZMoveSpd;	//XYZ方向の最大移動速度
		private int _MinXYZMoveSpd;	//XYZ方向の最小移動速度
		private int _MaxLQMoveSpd;	//LQ(回転系)方向の最大移動速度
		private int _MinLQMoveSpd;	//LQ(回転系)方向の最小移動速度
		private double _pixelRatioX;//X方向解像度
		private double _pixelRatioY;//Y方向解像度
		private string _imagingPath;	//imaging.exe の実行ﾊﾟｽ
		private int _modeMoveLQ;	//LQ(回転系)の移動モード(0:XY同時移動、1:別々移動）

		private SerializableDictionary<string, string> _areaInfo;

		[System.Xml.Serialization.XmlArray("StartUpScript")]
		[System.Xml.Serialization.XmlArrayItem("Command",typeof(string))]
		public System.Collections.ArrayList Items;

		private IniFileUtils _iniFile;

		//Property
		public string	IoBoardName	{ get { return (_ioBoardName);	}	set { _ioBoardName = value;	}	}
		public string	SvCtrlName	{ get { return (_svCtrlName);	}	set { _svCtrlName = value;	}	}
		public int		CmdPort		{ get { return (_cmdPort);		}	set { _cmdPort = value;		}	}
		public int		StatusPort	{ get { return (_statusPort);	}	set { _statusPort = value;	}	}
		public int		EventPort	{ get { return (_eventPort);	}	set { _eventPort = value;	}	}
		public string	IPAddress	{ get { return (_ipAddress);	}	set { _ipAddress = value;	}	}
		public int		WaitPort	{ get { return (_waitPort);		}	set { _waitPort = value;	}	}
		public int		SaftyPosZ	{ get { return (_saftyPosZ);	}	set { _saftyPosZ = value;	} 	}
		public int		UpperLimitZ { get { return (_UpperLimitZ);	}	set { _UpperLimitZ = value; }	}
		public int		LowerLimitZ { get { return (_LowerLimitZ);	}	set { _LowerLimitZ = value; }	}
		public int	  	UpperLimitL { get { return (_UpperLimitL);	}	set { _UpperLimitL = value; }	}
		public int		LowerLimitL { get { return (_LowerLimitL);	}	set { _LowerLimitL = value; }	}
		public int		UpperLimitQ { get { return (_UpperLimitQ);	}	set { _UpperLimitQ = value; }	}
		public int		LowerLimitQ { get { return (_LowerLimitQ);	}	set { _LowerLimitQ = value; }	}
		public int		MaxJogSpd	{ get { return (_MaxJogSpd);	}	set { _MaxJogSpd = value;	}	}
		public int		MinJogSpd	{ get { return (_MinJogSpd);	}	set { _MinJogSpd = value;	}	}
		public int		MaxXYZMoveSpd{get { return (_MaxXYZMoveSpd);}	set { _MaxXYZMoveSpd = value; } }
		public int		MinXYZMoveSpd{get { return (_MinXYZMoveSpd);}	set { _MinXYZMoveSpd = value; } }
		public int		MaxLQMoveSpd{ get { return (_MaxLQMoveSpd);	}	set { _MaxLQMoveSpd = value; }	}
		public int		MinLQMoveSpd{ get { return (_MinLQMoveSpd);	}	set { _MinLQMoveSpd = value; }	}
		public double	PixelRatioX { get { return (_pixelRatioX);	}	set { _pixelRatioX = value; }	}
		public double	PixelRatioY { get { return (_pixelRatioY);	}	set { _pixelRatioY = value; }	}
		public string	ImagingPath
		{
			get { return (_imagingPath);}
			set {
				_imagingPath = value;
				_iniFile = new IniFileUtils(_imagingPath + "Imaging.ini");
			}
		}
		public int		ModeMoveLQ { get { return (_modeMoveLQ);	}	set { _modeMoveLQ = value; } }
		public IniFileUtils IniFile
		{
			get { return(_iniFile); }
		}

		public SerializableDictionary<string, string> AreaInfo
		{
			get { return _areaInfo; }
			set { _areaInfo = value; }
		}
		public Settings()
		{
			_ioBoardName = "DIO000";
			_svCtrlName = "LINSEY";
			_cmdPort = 10010;
			_statusPort = 10011;
			_eventPort = 10012;
			_waitPort = 10013;
			_ipAddress = "127.0.0.1";
			_saftyPosZ = 300000;
			_UpperLimitZ = 1000000;
			_LowerLimitZ = -900000;

			_UpperLimitL =  1800000;
			_LowerLimitL = -1800000;

			_UpperLimitQ =  1800000;
			_LowerLimitQ = -1800000;

			_MaxJogSpd = 600000;
			_MinJogSpd = 0;

			_MaxXYZMoveSpd = 10000000;
			_MinXYZMoveSpd = 0;
			_MaxLQMoveSpd = 10000000;
			_MinLQMoveSpd = 0;

			_pixelRatioX = 12.0 / 640.0;
			_pixelRatioY = 12.0 / 640.0;

			_modeMoveLQ = 0;	// XY同時移動

			_imagingPath = "C:\\ProgramFiles\\imaging\\";
			_areaInfo = new SerializableDictionary<string, string>();
//			_areaInfo.Add("areaName", "10.1,20.2,33.3,44.4,1.5");

			Items = new System.Collections.ArrayList();
		}
		public IList<double> GetAreaInfo(string areaName)
		{
			IList<double> pos;

			if( _areaInfo.ContainsKey(areaName) == true ) {
				string[] area = _areaInfo[areaName].Split(',');

				pos = new double[ area.Count() ];
				int idx = 0;
				foreach( string ss in area ) {
					pos[idx++] = double.Parse(ss);
				}
			}
			else {
				pos = new double[1];
				pos[0] = 0.0;
			}
			return (pos);
		}
	}
	public partial class ViewModel : ViewModelBase
	{
//		private static Settings _appSettings = new Settings();		//ｱﾌﾟﾘｹｰｼｮﾝ・設定ﾌｧｲﾙ

		/// <summary>
		///  Rowthon 設定ﾌｧｲﾙの読み込み
		/// </summary>
		/// <returns>-1:読み込み失敗(ﾃﾞﾌｫﾙﾄが設定されます)</returns>
		public int ReadSettings()
		{
			string path = System.Environment.CurrentDirectory + "\\Rowthon.ini";

			XmlSerializer serial = new XmlSerializer(typeof(Settings));
			try {

				StreamReader sr = new StreamReader(path, new UTF8Encoding(false));
				//ﾌｧｲﾙが無い場合は例外が発生する

				//_appSettings = (Settings)serial.Deserialize(sr);
				App.Setting = (Settings)serial.Deserialize(sr);
			}
			catch ( Exception ee ) {
				//何故か下記の例外が拾えないので Exception で
				//System.Windows.Markup.XamlParseException e){
				MessageBox.Show(ee.Message, "設定ﾌｧｲﾙ読み込みｴﾗｰ", MessageBoxButton.OK, MessageBoxImage.Error);

				//新規に生成する場合はﾃﾞﾌｫﾙﾄを書き込む
				App.Setting.Items.Add("import IronPythonConsole");
				App.Setting.Items.Add("import debugLib as debug");
				App.Setting.Items.Add("import moveLib");
				App.Setting.Items.Add("import ioLib as io");
				App.Setting.Items.Add("import servoLib as servo");
				App.Setting.Items.Add("import traceLib");
				App.Setting.Items.Add("import ctrlLib as ctrl");
				App.Setting.Items.Add("import visionLib as vision");

				return (-1);
			}
			return (0);
		}

		/// <summary>
		/// Rowthon設定ﾌｧｲﾙの書き込み
		/// </summary>
		/// <returns>-1:書き込み失敗</returns>
		public int WriteSettings()
		{
			string path = System.Environment.CurrentDirectory + "\\Rowthon.ini";
			XmlSerializer serial = new XmlSerializer(typeof(Settings));

			try {
				StreamWriter sw = new StreamWriter(path, false, new UTF8Encoding(false));
				//serial.Serialize(sw, _appSettings);
				serial.Serialize(sw, App.Setting);
			}
			catch ( Exception ee ) {
				MessageBox.Show(ee.Message, "設定ﾌｧｲﾙ書き込みｴﾗｰ", MessageBoxButton.OK, MessageBoxImage.Error);
				return (-1);
			}
			return (0);
		}
	}
}
