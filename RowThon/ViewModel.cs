using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;

using System.Runtime.InteropServices;   //for DllImport
using System.Windows;
using System.Net;						//for Dns
using System.IO;                        //for Stream
using System.Xml;                       //for xml reader
using System.Xml.Serialization;			//for
using System.ComponentModel;            //for INotifyPropertyChanged
using System.Windows.Input;				//for ICommand

using Microsoft.Windows.Controls.Ribbon;
using Microsoft.Scripting.Hosting;

using System.Linq.Expressions;

// 動作ﾌﾟﾛｸﾞﾗﾑから呼び出すことができる ViewModelｸﾗｽのC#側APIを定義する
//  
//  呼び出し用のｵﾌﾞｼﾞｪｸﾄとして Vm を設定している
//     void Console_ConsoleInitialized() 内
//         pythonConsole.Pad.Console.ScriptScope.SetVariable("Vm", _viewModel)
//
namespace RowThon
{
	public delegate void GrabEventHandler(object sender, DrawCameraViewEventArgs e);

	//
	// ﾌﾟﾛﾊﾟﾃｨ更新用の拡張ﾒｿｯﾄﾞ
	//
	#region ** Class : ViewModelEx
	public static class ViewModelEx
	{
		/// <summary>
		///  ﾗﾑﾀﾞ式からｼﾝﾎﾞﾙ名を取得する
		/// </summary>
		/// <typeparam name="TObj"></typeparam>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="self"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		public static string GetPropertySymbol<TObj, TProp>(this TObj self, System.Linq.Expressions.Expression<Func<TObj, TProp>> e)
		{
			var outerMostMemberExpression = (MemberExpression)(((System.Linq.Expressions.LambdaExpression)(e)).Body);

			var memberExpressions = 
				Iterate<MemberExpression>(memberExpression => memberExpression.Expression as MemberExpression,outerMostMemberExpression)
				.TakeWhile(x => x != null)
				.Aggregate<MemberExpression,string>(string.Empty,(left,right) => right.Member.Name + "." + left);

			return memberExpressions.Substring(0,memberExpressions.Length -1 );
		}
		public static IEnumerable<T> Iterate<T>(Func<T,T> func, T initialValue)
		{
			var value = initialValue;

			while( true ){
				yield return value;

				value = func(value);
			}
		}
		public static void RaisePropertyChanged<TObj, TProp>(this TObj self, System.Linq.Expressions.Expression<Func<TObj, TProp>> e)
			where TObj : ViewModelBase
		{
//			var name = ((MemberExpression)e.Body).Member.Name;
			var name = GetPropertySymbol(self, e);
			self.RaisePropertyChanged(name);
		}
	}
	#endregion

	
	public class ScriptEventArgs : EventArgs
	{
		public string cmd;
		public ScriptEventArgs(string mm)
		{ cmd = mm; }
	}
	public class DrawCameraViewEventArgs : EventArgs
	{
		public int _draw;
		public int _x1;
		public int _y1;
		public int _x2;
		public int _y2;
		public DrawCameraViewEventArgs(int dw, int x1, int y1, int x2, int y2)
		{
			_draw = dw;
			_x1 = x1; _y1 = y1; _x2 = x2; _y2 = y2;
		}
	}
	#region ** Class : ViewModel
	public partial class ViewModel : ViewModelBase
	{
		public int _data = 0;
		private UInt32 _ioInput = 0;	// I/O Input
		private UInt32 _ioOutput = 0;	// I/O Output
		private UInt32[] _svStatus = new UInt32[5];
		public static PostSystem _postSys = new PostSystem();			//ﾒｯｾｰｼﾞ送信
		private static PythonConsoleControl.IronPythonConsoleControl _pythonConsole;//Viewにあるものの参照用

		public static PEventManager.EventPool _eventCtrl = new PEventManager.EventPool();	//ｲﾍﾞﾝﾄﾌﾟｰﾙ

		private static IoModel		_ioCtrl;		//I/Oﾓﾃﾞﾙ
		private static ServoModel	_svCtrl;		//ｻｰﾎﾞﾓﾃﾞﾙ
		private static VisionModel	_visionCtrl;	//ﾋﾞｼﾞｮﾝﾓﾃﾞﾙ

		private static string		_connectMsg;	//ﾈｯﾄﾜｰｸ接続ﾒｯｾｰｼﾞ
		public CtrlMode _appCtrlMode = CtrlMode.Mode_Manual;	//操作権限

		public event EventHandler<ScriptEventArgs>			_ExecuteScript;		//viewで実行するためのｲﾍﾞﾝﾄ(ﾃﾞﾘｹﾞｰﾄ)
		public event EventHandler<DrawCameraViewEventArgs>	_DrawCameraBitmap;	//viewでﾋﾞｯﾄﾏｯﾌﾟを表示するためのｲﾍﾞﾝﾄ(ﾃﾞﾘｹﾞｰﾄ)

		public event GrabEventHandler _GrabEvent;	//visionﾓﾃﾞﾙから画像取り込み毎に呼ばれるﾊﾝﾄﾞﾗ

		private bool _drawCenter = false;

		private static PEventManager.PEvent _msgEvent;
		private string[] _cv_tplList;

		public bool CenterLine { get { return _drawCenter; } }
		/// <summary>
		///  I/O 操作用ﾌﾟﾛﾊﾟﾃｨ
		/// </summary>
		public IoModel IoControl { get { return _ioCtrl; } }
		/// <summary>
		///  ｻｰﾎﾞ操作用ﾌﾟﾛﾊﾟﾃｨ
		/// </summary>
		public ServoModel SvControl { get { return _svCtrl; } }
		/// <summary>
		///  ﾋﾞｼﾞｮﾝ操作用ﾌﾟﾛﾊﾟﾃｨ
		/// </summary>
		public VisionModel VisionControl { get { return _visionCtrl; } }

		/// <summary>
		///  python ｽｸﾘﾌﾟﾄ用のﾄﾚｰｽ出力
		/// </summary>
		/// <param name="func"></param>
		/// <param name="msg"></param>
		public void PyTrace(string func, string msg)
		{
			Trace.TraceText(msg,"[PY]:"+func);
		}
		public PEventManager.PEvent GetEvent(int id)
		{
			return (_eventCtrl.GetEvent(id));
		}
		public PEventManager.PEvent GetEvent()
		{
			return( _eventCtrl.GetEvent() );
		}
		public UInt32 GetSvStat(int axisNo)
		{
			return (_svStatus[axisNo]);
		}
		public void QuitView()
		{
			//ｻｰﾎﾞ終了処理
			SvControl.Close();

			//I/O終了処理
			IoControl.Close();
			//生成したｽﾚｯﾄﾞの終了(ﾌﾗｸﾞを立てる)
			_threadStop = 1;
		}
		/// <summary>
		///  画像取り込み完了時に VisionModel 側から呼び出されるﾊﾝﾄﾞﾗ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void grab_event_func(object sender, DrawCameraViewEventArgs args)
		{
			_DrawCameraBitmap(sender, args);
		}

		/// <summary>
		///  I/O の入力を更新する
		/// </summary>
		/// <returns></returns>
		public uint UpdateIo()
		{
			IoInputValue = (uint)IoControl.Read(0);

			return (IoInputValue);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		private void IoEventProc(uint oldValue, uint newValue)
		{
			uint diff = oldValue ^ newValue;
			bool emg = false;

			for( int i = 0; i < 32; i++ ) {
				if( (diff & (1 << i)) != 0 ) {
					//I/O変化
					if( (i == 1 && (newValue & (1 << 1)) == 0) ||
						(i == 2 && (newValue & (1 << 2)) == 0) ) {
						//非常停止(1ﾋﾞｯﾄ目)On & ｾｰﾌﾃｨﾘﾚｰ(2ﾋﾞｯﾄ目)On

							if(emg == false) {
								exec_cmd_script(string.Format("ctrl.exec_emg({0})", newValue));

								Trace.TraceFormat("Emg or Safty Error");
								SendIoMessage(0, MessageCode.SYS_IO_EMG);
								emg = true;
							}
					}
				}
			}
		}
		/// <summary>
		///  Windowﾒｯｾｰｼﾞの拡張用
		///  　I/O の更新はWindowﾒｯｾｰｼﾞとして通知されるのでWindowのﾒｯｾｰｼﾞ処理部を拡張して処理する
		/// </summary>
		/// <param name="hwnd"></param>
		/// <param name="msg"></param>
		/// <param name="wParam"></param>
		/// <param name="lParam"></param>
		unsafe public void wnd_proc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
		{
			if( msg == 0x1300 ) {
				// I/O の更新が発生
				IoControl.wnd_proc(hwnd, msg, wParam, lParam);	//更新

				IoEventProc(IoInputValue, IoControl._ioInput);

				IoInputValue = IoControl._ioInput;	//ﾌﾟﾛﾊﾟﾃｨ更新

				//ﾃﾞｰﾀ送信
				SendIoMessage(0, MessageCode.SYS_IO_UPDATE);
			}
			else if( msg == 0x4A ) { //WM_COPYDATA(0x4A)
				//Imaging.exe からはWindowsﾒｯｾｰｼﾞ(WM_COPYDATA)で結果通知
				VisionControl.wnd_proc(hwnd, msg, wParam, lParam);
				if( _msgEvent != null ) {
					_msgEvent.Set(0);
				}
			}
		}
		/// <summary>
		///  I/O状態を上位に通知する
		/// </summary>
		/// <param name="st"></param>
		public void SendIoMessage(int st, MessageCode code)
		{
			if( /*st == 0 ||*/ _statusSock == null ) {
				return;
			}
			MessageData dd = new MessageData(MessageCode.SYS_IO_UPDATE);

			dd._code = code;
			dd._ioInput = IoInputValue;
			dd._ioOutput = IoOutputValue;

			_postSys.Post(dd);
		}
		/// <summary>
		/// 下位ﾕﾆｯﾄの状態監視ｽﾚｯﾄﾞ
		/// </summary>
		private static PEventManager.PEvent[] UnitEvent = new PEventManager.PEvent[1];
		unsafe private void UnitEventThread()
		{
			UnitEvent[0] = GetEvent();	//ｻｰﾎﾞﾕﾆｯﾄ用通知ｲﾍﾞﾝﾄ

			WaitHandle[] hdl = new WaitHandle[] { UnitEvent[0].GetEvent() as AutoResetEvent };

			SvControl.SetUnitEvent(UnitEvent[0]);

			while(true) {
				int idx = WaitHandle.WaitAny(hdl, 500);
				if(idx != WaitHandle.WaitTimeout) {
					if(idx == 0) {
						//ｻｰﾎﾞﾕﾆｯﾄ状態通知
						Trace.TraceFormat("Send Servo Error");
						SendIoMessage(0, MessageCode.SYS_SV_ALM);
					}
					UnitEvent[idx].Reset();
				}
				if(_threadStop != 0) {
					break;
				}
			}
		}
		private static Thread _unitStatusThread;
		public void CreateUnitEventThread()
		{
			if(_unitStatusThread == null) {
				_unitStatusThread = new Thread(new ThreadStart(UnitEventThread));
				_unitStatusThread.Start();
			}
		}
		public int camera_init()
		{
			if( _cameraInit == false ) {
				if( _visionCtrl.InitCamera(_GrabEvent) != 0 ) {
					return (-1);
				}
				else {
					_cameraInit = true;
				}
			}
			return ((_cameraInit == true) ? 0 : -1);
		}

		/// <summary>
		/// ｽｸﾘﾌﾟﾄの実行
		/// </summary>
		/// <param name="cmd">ｽｸﾘﾌﾟﾄ文字列</param>
		/// <returns>ｽｸﾘﾌﾟﾄ実行結果</returns>
		public int exec_cmd_script(string cmd)
		{
			_ExecuteScript(this, new ScriptEventArgs(cmd));

			return (0);
		}
		public void WriteConsole(string msg)
		{
			string cmd_msg = String.Format("print '{0}'\n",msg);
			_ExecuteScript(this, new ScriptEventArgs(cmd_msg));
		}
		//////////////////////////////////////////////// 
		//
		// (ICommand)ｺﾏﾝﾄﾞ実装部
		//
//		public ICommand Add1Command { get; private set; }
		public ICommand CmdServoReset { get; private set; }
		public ICommand CmdServoPower { get; private set; }
		public ICommand CmdServoErrClr { get; private set; }
		public ICommand CmdExecScript { get; private set; }
		public ICommand CmdServoErrInfo { get; private set; }
		public ICommand CmdInitCamera { get; private set; }
		public ICommand CmdGrabCamera { get; private set; }
//		public ICommand CmdVisionProc { get; private set; }
		public ICommand CmdDrawCenter { get; private set; }

		/// <summary>
		///  ｻｰﾎﾞ・ﾘｾｯﾄｺﾏﾝﾄﾞ
		/// </summary>
		/// <param name="obj"></param>
		public void SvReset(object obj)
		{
			SvControl.Reset();
		}
		public bool canSvReset(object obj)
		{
			if( _svCtrl == null ) return false;

			//if ((IoOutputValue & 3) == 3) return true;
			//return (false);
			return (true);
		}

		/// <summary>
		/// ｻｰﾎﾞ電源・ｺﾏﾝﾄﾞ
		/// </summary>
		/// <param name="obj"></param>
		public void SvPower(object obj)
		{
			bool onoff = (bool)obj;
			int ret = 0;

			Mouse.OverrideCursor = Cursors.Wait;

			if( onoff == false){
				ret = exec_cmd_script("servo.power(1)");
			}
			else{
				ret = exec_cmd_script("servo.power(0)");
			}

			Mouse.OverrideCursor = null;
		}
		public bool canSvPow(object obj)
		{
			return ((SvControl != null) ? true : false);
		}

		/// <summary>
		///  ｻｰﾎﾞｴﾗｰｸﾘｱ・ｺﾏﾝﾄﾞ
		/// </summary>
		/// <param name="obj"></param>
		public void SvErrorClear(object obj)
		{
			SvControl.ClrError();
		}
		public bool canSvErrorClear(object obj)
		{
			return ((SvControl != null) ? true : false);
		}
		/// <summary>
		///  ｻｰﾎﾞｴﾗｰ情報
		/// </summary>
		/// <param name="obj"></param>
		public void SvErrorInfo(object obj)
		{
			ServoErrInfoWnd wnd = new ServoErrInfoWnd(this);

			wnd.ShowDialog();
		}
		public bool canSvErrorInfo(object obj)
		{
			return (true);
		}

		private bool _cameraInit = false;
		/// <summary>
		///  ｶﾒﾗ初期化
		/// </summary>
		/// <param name="obj"></param>
		public void CameraInit(object obj)
		{
			if( camera_init() != 0 ) {
				MessageBox.Show("ｶﾒﾗの初期化に失敗しました", "ｴﾗｰ", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else {
				MessageBox.Show("ｶﾒﾗを初期化しました", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}
		public bool canCameraInit(object obj)
		{
			return (true);
		}

		/// <summary>
		///  画像取り込み
		/// </summary>
		/// <param name="obj">RibbonButtonｵﾌﾞｼﾞｪｸﾄ</param>
		public void GrabCamera(object obj)
		{
			RibbonToggleButton btn = obj as RibbonToggleButton;

			if( btn.IsChecked == false ) {
				_visionCtrl.GrabCamera(-1);
			}
			else {
				_visionCtrl.GrabCamera(0);
			}
			UpdateProperty("GrabStatus");
		}
		public bool canGrabCamera(object obj)
		{
			return ( _cameraInit );
		}
/*
		/// <summary>
		///  画像処理
		/// </summary>
		/// <param name="obj"></param>
		public void VisionProc(object obj)
		{
			Model.GalleryData<TplImageData> data = obj as Model.GalleryData<TplImageData>;

			string path = System.Environment.CurrentDirectory + "\\Cv_template\\" + data.SelectedItem.Label;
			int idx = 0;
			int type = 0;
			foreach( Model.GalleryCategoryData<TplImageData> c in data.CategoryDataCollection ) {
				foreach( TplImageData ss in c.GalleryItemDataCollection ) {
					if( ss.Label == data.SelectedItem.Label ) {
						string cmd = "print 'error'";
						switch(type) {
						case 0: cmd = string.Format("vision.img_mark_proc({0})", idx); break;
						case 1: cmd = string.Format("vision.img_part_proc({0})", idx); break;
						case 2: cmd = string.Format("vision.cv_mark_proc({0})", idx); break;
						}
						_ExecuteScript(this, new ScriptEventArgs(cmd));
						return;
					}
					idx++;
				}
				idx = 0;
				type++;
			}
		}
		public bool canVisionProc(object obj)
		{
			return (_cameraInit);
		}
*/
		public void DrawCenter(object obj)
		{
			RibbonToggleButton btn = obj as RibbonToggleButton;

			if(btn.IsChecked == false) {
				_drawCenter = false;
				_DrawCameraBitmap(this, new DrawCameraViewEventArgs(0,320,320,640,640));
			}
			else {
				_drawCenter = true;
				_DrawCameraBitmap(this, new DrawCameraViewEventArgs(2,320,320,640,640));
			}
		}
		public bool canDrawCenter(object obj)
		{
			return (true);
		}
		/// <summary>
		///  OpenCv用の画像処理ｲﾝﾃﾞｯｸｽを取得する
		/// </summary>
		/// <param name="tplName">ﾃﾝﾌﾟﾚｰﾄﾌｧｲﾙ名(拡張子含む)</param>
		/// <returns>ｴﾗｰ:-1 正常:0~</returns>
		public int GetCvTplIdx(string tplName)
		{
			int idx = 0;
			foreach(string dir in _cv_tplList) {
				if(Path.GetFileName(dir) == tplName) {
					return idx;
				}
				idx++;
			}
			return (-1);
		}
		/// <summary>
		/// ｺﾝｽﾄﾗｸﾀ
		/// </summary>
		public ViewModel()
		{
			_data = 0;
			Trace.CreateLog("ROWTHON", 50000, 0);

			Trace.TraceFormat("Init Rowthon ViewModel");

			//設定ﾌｧｲﾙの読み込み
			if(ReadSettings() < 0) {
				//設定ﾌｧｲﾙが無かったので、ﾃﾞﾌｫﾙﾄ値でﾌｧｲﾙを書き出す
				WriteSettings();
				MessageBox.Show("ﾃﾞﾌｫﾙﾄ設定で設定ﾌｧｲﾙ(Rowthon.ini)を生成しました", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			//ｿｹｯﾄ用のIPｱﾄﾞﾚｽを設定する
			//  ※LANﾎﾟｰﾄが二個以上ある場合など、IPを指定する必要がある
			//     LAN1:172.170.100.110
			//     LAN2:192.168.100.120
			//      などの場合、LAN2 でｿｹｯﾄを受け付けようとした場合、localhost とするとLAN1が使用される(?)
			//      LANﾎﾟｰﾄがひとつしかないような場合は localhost で良い
			if(Setting.IPAddress == "localhost") {
				_serverIP = Dns.GetHostEntry("localhost").AddressList[1];
			}
			else {
				_serverIP = IPAddress.Parse(Setting.IPAddress);
			}

			//I/Oの初期化
			_ioCtrl = new IoModel();

			//ｻｰﾎﾞの初期化
			_svCtrl = new ServoModel();

			//visionﾓﾃﾞﾙの初期化
			_visionCtrl = new VisionModel();

			//ｲﾍﾞﾝﾄﾘｽﾄを生成
			_eventCtrl.CreateEventList(255);

			//I/Oｲﾍﾞﾝﾄの設定
			for (int bit = 0; bit < 32; bit++) {
				//OFF ｲﾍﾞﾝﾄ
				IoControl.SetIoEvent(_eventCtrl.GetEvent(), 0, bit, 0);

				//ONｲﾍﾞﾝﾄ
				IoControl.SetIoEvent(_eventCtrl.GetEvent(), 0, bit, 1);
			}
			//取り込み完了ｲﾍﾞﾝﾄﾊﾝﾄﾞﾗ
			_GrabEvent += new GrabEventHandler(grab_event_func);

			//OpenCVで使用するﾃﾝﾌﾟﾚｰﾄ名を登録する
			// ※VisionModel でのﾃﾞｰﾀ登録も同じ手法(ﾌｧｲﾙ一覧)を使用しているので注意
			_cv_tplList = Directory.GetFiles(System.Environment.CurrentDirectory + "\\Cv_template", "*.png");

			ConnectionMsg = "未接続";

//			Add1Command = CreateCommand(AddData, canExe);
			CmdServoReset = CreateCommand(SvReset, canSvReset);
			CmdServoPower = CreateCommand(SvPower, canSvPow);
			CmdServoErrClr = CreateCommand(SvErrorClear, canSvErrorClear);
			CmdServoErrInfo = CreateCommand(SvErrorInfo, canSvErrorInfo);
			CmdInitCamera = CreateCommand(CameraInit, canCameraInit);
			CmdGrabCamera = CreateCommand(GrabCamera, canGrabCamera);
//			CmdVisionProc = CreateCommand(VisionProc, canVisionProc);
			CmdDrawCenter = CreateCommand(DrawCenter, canDrawCenter);

		}

		public int GetMsgEventID()
		{
			_msgEvent = GetEvent();
			return (_msgEvent._number);
		}
		protected void BitOnOff(int bitNo, bool status)
		{
			UInt32 bit = ((UInt32)1 << bitNo);
			UInt32 data = IoInputValue;

			if (status == true) {
				data |= bit;
			}
			else {
				data &= ~bit;
			}
			IoInputValue = data;
		}
		protected bool OnOff(int bitNo)
		{
			return ((_ioInput & (1 << bitNo)) == 0 ? false : true);
		}
		protected void BitSet(int bitNo)
		{
//			BitOnOff(bitNo, ((_ioInput & (1 << bitNo)) == 0) ? false : true);
		}
		/// <summary>
		///  I/O Binding 用
		/// </summary>
		public bool IsChecked01 { get { return (OnOff(0)); } set { BitSet(0); } }
		public bool IsChecked02 { get { return (OnOff(1)); } set { BitSet(1); } }
		public bool IsChecked03 { get { return (OnOff(2)); } set { BitSet(2); } }
		public bool IsChecked04 { get { return (OnOff(3)); } set { BitSet(3); } }
		public bool IsChecked05 { get { return (OnOff(4)); } set { BitSet(4); } }
		public bool IsChecked06 { get { return (OnOff(5)); } set { BitSet(5); } }
		public bool IsChecked07 { get { return (OnOff(6)); } set { BitSet(6); } }
		public bool IsChecked08 { get { return (OnOff(7)); } set { BitSet(7); } }
		public bool IsChecked09 { get { return (OnOff(8)); } set { BitSet(8); } }
		public bool IsChecked10 { get { return (OnOff(9)); } set { BitSet(9); } }
		public bool IsChecked11 { get { return (OnOff(10)); } set { BitSet(10); } }
		public bool IsChecked12 { get { return (OnOff(11)); } set { BitSet(11); } }
		public bool IsChecked13 { get { return (OnOff(12)); } set { BitSet(12); } }
		public bool IsChecked14 { get { return (OnOff(13)); } set { BitSet(13); } }
		public bool IsChecked15 { get { return (OnOff(14)); } set { BitSet(14); } }
		public bool IsChecked16 { get { return (OnOff(15)); } set { BitSet(15); } }
		public bool IsChecked17 { get { return (OnOff(16)); } set { BitSet(16); } }
		public bool IsChecked18 { get { return (OnOff(17)); } set { BitSet(17); } }
		public bool IsChecked19 { get { return (OnOff(18)); } set { BitSet(18); } }
		public bool IsChecked20 { get { return (OnOff(19)); } set { BitSet(19); } }
		public bool IsChecked21 { get { return (OnOff(20)); } set { BitSet(20); } }
		public bool IsChecked22 { get { return (OnOff(21)); } set { BitSet(21); } }
		public bool IsChecked23 { get { return (OnOff(22)); } set { BitSet(22); } }
		public bool IsChecked24 { get { return (OnOff(23)); } set { BitSet(23); } }
		public bool IsChecked25 { get { return (OnOff(24)); } set { BitSet(24); } }
		public bool IsChecked26 { get { return (OnOff(25)); } set { BitSet(25); } }
		public bool IsChecked27 { get { return (OnOff(26)); } set { BitSet(26); } }
		public bool IsChecked28 { get { return (OnOff(27)); } set { BitSet(27); } }
		public bool IsChecked29 { get { return (OnOff(28)); } set { BitSet(28); } }
		public bool IsChecked30 { get { return (OnOff(29)); } set { BitSet(29); } }
		public bool IsChecked31 { get { return (OnOff(30)); } set { BitSet(30); } }
		public bool IsChecked32 { get { return (OnOff(31)); } set { BitSet(31); } }

		protected void OutBitOnOff(int bitNo, bool status)
		{
			UInt32 bit = ((UInt32)1 << bitNo);
			UInt32 data = IoOutputValue;

			if (status == true) {
				data |= bit;
			}
			else {
				data &= ~bit;
			}
			IoOutputValue = data;
		}
		protected bool OutOnOff(int bitNo)
		{
			return ((_ioOutput & (1 << bitNo)) == 0 ? false : true);
		}
		protected void OutBitSet(int bitNo)
		{
			OutBitOnOff(bitNo, ((_ioOutput & (1 << bitNo)) == 0) ? false : true);
		}
		public bool IsOut01 { get { return (OutOnOff(0)); } set { OutBitSet(0); } }
		public bool IsOut02 { get { return (OutOnOff(1)); } set { OutBitSet(1); } }
		public bool IsOut03 { get { return (OutOnOff(2)); } set { OutBitSet(2); } }
		public bool IsOut04 { get { return (OutOnOff(3)); } set { OutBitSet(3); } }
		public bool IsOut05 { get { return (OutOnOff(4)); } set { OutBitSet(4); } }
		public bool IsOut06 { get { return (OutOnOff(5)); } set { OutBitSet(5); } }
		public bool IsOut07 { get { return (OutOnOff(6)); } set { OutBitSet(6); } }
		public bool IsOut08 { get { return (OutOnOff(7)); } set { OutBitSet(7); } }
		public bool IsOut09 { get { return (OutOnOff(8)); } set { OutBitSet(8); } }
		public bool IsOut10 { get { return (OutOnOff(9)); } set { OutBitSet(9); } }
		public bool IsOut11 { get { return (OutOnOff(10)); } set { OutBitSet(10); } }
		public bool IsOut12 { get { return (OutOnOff(11)); } set { OutBitSet(11); } }
		public bool IsOut13 { get { return (OutOnOff(12)); } set { OutBitSet(12); } }
		public bool IsOut14 { get { return (OutOnOff(13)); } set { OutBitSet(13); } }
		public bool IsOut15 { get { return (OutOnOff(14)); } set { OutBitSet(14); } }
		public bool IsOut16 { get { return (OutOnOff(15)); } set { OutBitSet(15); } }
		public bool IsOut17 { get { return (OutOnOff(16)); } set { OutBitSet(16); } }
		public bool IsOut18 { get { return (OutOnOff(17)); } set { OutBitSet(17); } }
		public bool IsOut19 { get { return (OutOnOff(18)); } set { OutBitSet(18); } }
		public bool IsOut20 { get { return (OutOnOff(19)); } set { OutBitSet(19); } }
		public bool IsOut21 { get { return (OutOnOff(20)); } set { OutBitSet(20); } }
		public bool IsOut22 { get { return (OutOnOff(21)); } set { OutBitSet(21); } }
		public bool IsOut23 { get { return (OutOnOff(22)); } set { OutBitSet(22); } }
		public bool IsOut24 { get { return (OutOnOff(23)); } set { OutBitSet(23); } }
		public bool IsOut25 { get { return (OutOnOff(24)); } set { OutBitSet(24); } }
		public bool IsOut26 { get { return (OutOnOff(25)); } set { OutBitSet(25); } }
		public bool IsOut27 { get { return (OutOnOff(26)); } set { OutBitSet(26); } }
		public bool IsOut28 { get { return (OutOnOff(27)); } set { OutBitSet(27); } }
		public bool IsOut29 { get { return (OutOnOff(28)); } set { OutBitSet(28); } }
		public bool IsOut30 { get { return (OutOnOff(29)); } set { OutBitSet(29); } }
		public bool IsOut31 { get { return (OutOnOff(30)); } set { OutBitSet(30); } }
		public bool IsOut32 { get { return (OutOnOff(31)); } set { OutBitSet(31); } }

		private string IoOutToolTip(int bit)
		{
			switch(bit) {
			case  0: return ("ｱﾝﾌﾟ電源");
			case 16: return ("ﾉｽﾞﾙPick");
			case 17: return ("ﾉｽﾞﾙOFF");
			case 18: return ("ﾊﾟｰﾂ吸着");
			case 19: return ("ﾊﾟｰﾂOFF");
			case 20: return ("ﾊﾟｰﾂ確認");
			case 21: return ("LEDﾗｲﾄ");
			default: return ("未定義");
			}
		}
		public string IoOutInfo01 { get { return (IoOutToolTip(0)); } }
		public string IoOutInfo02 { get { return (IoOutToolTip(1)); } }
		public string IoOutInfo03 { get { return (IoOutToolTip(2)); } }
		public string IoOutInfo04 { get { return (IoOutToolTip(3)); } }
		public string IoOutInfo05 { get { return (IoOutToolTip(4)); } }
		public string IoOutInfo06 { get { return (IoOutToolTip(5)); } }
		public string IoOutInfo07 { get { return (IoOutToolTip(6)); } }
		public string IoOutInfo08 { get { return (IoOutToolTip(7)); } }
		public string IoOutInfo09 { get { return (IoOutToolTip(8)); } }
		public string IoOutInfo10 { get { return (IoOutToolTip(9)); } }
		public string IoOutInfo11 { get { return (IoOutToolTip(10)); } }
		public string IoOutInfo12 { get { return (IoOutToolTip(11)); } }
		public string IoOutInfo13 { get { return (IoOutToolTip(12)); } }
		public string IoOutInfo14 { get { return (IoOutToolTip(13)); } }
		public string IoOutInfo15 { get { return (IoOutToolTip(14)); } }
		public string IoOutInfo16 { get { return (IoOutToolTip(15)); } }
		public string IoOutInfo17 { get { return (IoOutToolTip(16)); } }
		public string IoOutInfo18 { get { return (IoOutToolTip(17)); } }
		public string IoOutInfo19 { get { return (IoOutToolTip(18)); } }
		public string IoOutInfo20 { get { return (IoOutToolTip(19)); } }
		public string IoOutInfo21 { get { return (IoOutToolTip(20)); } }
		public string IoOutInfo22 { get { return (IoOutToolTip(21)); } }
		public string IoOutInfo23 { get { return (IoOutToolTip(22)); } }
		public string IoOutInfo24 { get { return (IoOutToolTip(23)); } }
		public string IoOutInfo25 { get { return (IoOutToolTip(24)); } }
		public string IoOutInfo26 { get { return (IoOutToolTip(25)); } }
		public string IoOutInfo27 { get { return (IoOutToolTip(26)); } }
		public string IoOutInfo28 { get { return (IoOutToolTip(27)); } }
		public string IoOutInfo29 { get { return (IoOutToolTip(28)); } }
		public string IoOutInfo30 { get { return (IoOutToolTip(29)); } }
		public string IoOutInfo31 { get { return (IoOutToolTip(30)); } }
		public string IoOutInfo32 { get { return (IoOutToolTip(31)); } }
	}
	#endregion
}
