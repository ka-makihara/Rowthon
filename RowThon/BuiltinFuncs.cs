using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Threading;
using Microsoft.Windows.Controls.Ribbon;


//
//
// 動作ﾌﾟﾛｸﾞﾗﾑから呼び出すことができる MainWindowｸﾗｽのC#側APIを定義する
//  
//
//  MainWindow 側の呼び出し用のｵﾌﾞｼﾞｪｸﾄとして Rowthon 設定している
//     void Console_ConsoleInitialized() 内
//         pythonConsole.Pad.Console.ScriptScope.SetVariable("Vm", _viewModel)
//
namespace RowThon
{
//	public partial class MainWindow : RibbonWindow
	public partial class ViewModel : ViewModelBase
	{
		/// <summary>
		///  Rowthonｱﾌﾟﾘの操作権限を操作する
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		public int SetCtrlMode(int mode)
		{
			if( mode < 0 ) {
				return ((int)ErrCode.ERR_PARAM);
			}
			_appCtrlMode = (CtrlMode)mode;
			return ((int)ErrCode.RET_OK);
		}
		/// <summary>
		///  Rowthonｱﾌﾟﾘの操作権限状態を取得する
		/// </summary>
		/// <returns></returns>
		public int GetCtrlMode()
		{
			return ((int)_appCtrlMode);
		}
		/// <summary>
		///  ｼｽﾃﾑ全体の状態取得
		/// </summary>
		/// <returns></returns>
		public int GetStatus()
		{
			int status = 0;

			//ｻｰﾎﾞ軸情報(0-7 bit: 異常あり=1)
			int[] axis = new int[]{ (int)AxisNo.Axis_X,
									(int)AxisNo.Axis_Y,
									(int)AxisNo.Axis_Z,
									(int)AxisNo.Axis_A,
									(int)AxisNo.Axis_B
								};

			for( int i = 0; i < 5; i++ ){
				status |= (SvControl.GetAxisAlam(axis[i]) != 0) ? (1 << i) : 0;	//異常ｱﾘ
			}
			//ｻｰﾎﾞｴﾘｱ情報(8-15 bit)
			Double pos1 = 0.0, pos2 = 0.0, pos3 = 0.0, pos4 = 0.0, pos5 = 0.0, pos6 = 0.0;
			SvControl.GetCurPos(ref pos1, ref pos2, ref pos3, ref pos4, ref pos5, ref pos6);

			int areaCode = 0;

			status |= (areaCode << 8);

			//Rowthonﾕﾆｯﾄ情報(16-19)
			byte rowthonStat = 0;
			status |= (rowthonStat << 16);

			//I/Oﾕﾆｯﾄ情報(20-23)
			byte ioStat = 0;
			status |= (ioStat << 16);

			//Visionﾕﾆｯﾄ情報(24-27)
			byte visionStat = 0;
			status |= (visionStat << 24);

			return (status);
		}
		/// <summary>
		/// 動作中のｽｸﾘﾌﾟﾄ実行を強制停止
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public int StopProgram(int id)
		{
			Trace.TraceFormat("Entry:ID={0}", id);

			if( id == -1 ) {
				SvControl.Power(0);
				if( _scriptThread != null ) {
					Trace.TraceFormat("Thread Abort");
					_scriptThread.Abort();
					_scriptThread.Join();
				}
			}
			else {
				if( id < 0 || id > 255 ) {
					Trace.TraceFormat("[ERR]ID={0} Param", id);
					return ((int)ErrCode.ERR_PARAM);
				}
				PEventManager.PEvent ev = GetEvent(id);

				//ｽﾚｯﾄﾞで実行しているｽｸﾘﾌﾟﾄを停止させる
				if( _scriptThread != null ) {
					_scriptThread.Abort();
					_scriptThread.Join();
				}
				Trace.TraceFormat("[ERR]Leave ID={0} Exec", id);
				ev.Cancel((int)ErrCode.ERR_EXEC);
			}
			Trace.TraceFormat("Leave ID={0}", id);
			return ((int)ErrCode.RET_OK);
		}
		/// <summary>
		///  実行したｽｸﾘﾌﾟﾄの中の変数の値(int)を取得する
		/// </summary>
		/// <param name="resultName">変数名</param>
		/// <returns></returns>
		public int GetProgramResult(string resultName)
		{
			Trace.TraceFormat("Entry name={0}", resultName);
			try {
				var obj = _pythonConsole.Pad.Console.ScriptScope.GetVariable<int>(resultName);
				Trace.TraceFormat("Leave OK value ={0}", (int)obj);
				return (obj);
			}
			catch(Exception err) {
				Trace.TraceFormat("[ERR]Leave name={0}", err.Message);
				return ((int)ErrCode.ERR_PARAM);
			}
		}
		/// <summary>
		///  実装ﾕﾆｯﾄのｴﾗｰｸﾘｱ
		/// </summary>
		/// <returns></returns>
		public int ClearError()
		{
			int st;

			Trace.TraceFormat("Entry");

			//ｻｰﾎﾞｼｽﾃﾑのｴﾗｰｸﾘｱ
			if((st = SvControl.Reset()) < 0) {
				Trace.TraceFormat("[ERR]ServoReset:{0}", st);
				return (st);
			}
			if((st = SvControl.ClrError()) < 0) {
				Trace.TraceFormat("[ERR]ServoErrorClear:{0}", st);
				return (st);
			}
			//I/Oｼｽﾃﾑのｴﾗｰｸﾘｱ

			//画像ｼｽﾃﾑのｴﾗｰｸﾘｱ

			Trace.TraceFormat("Leave:OK");
			return ((int)ErrCode.RET_OK);
		}
		/// <summary>
		///  ｲﾍﾞﾝﾄをｷｬﾝｾﾙ状態ｾｯﾄする
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public int CancelWait(int id)
		{
			Trace.TraceFormat("Entry:ID={0}", id);
			try {
				PEventManager.PEvent ev = _eventCtrl.GetEvent(id);

				ev.Cancel((int)ErrCode.ERR_EXEC);
			}
			catch(Exception) {
				Trace.TraceFormat("Exception: NotEventID={0}", id);
				return ((int)ErrCode.ERR_PARAM);
			}
			Trace.TraceFormat("Leave");
			return ((int)ErrCode.RET_OK);
		}
		/// <summary>
		///  ｲﾍﾞﾝﾄ状態を取得
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public int EventStatus(int id)
		{
			Trace.TraceFormat("Entry:ID={0}", id);
			try {
				PEventManager.PEvent ev = _eventCtrl.GetEvent(id);
				Trace.TraceFormat("Leave:Status={0}", ev.Status());
				return (ev.Status());
			}
			catch(Exception) {
				Trace.TraceFormat("[ERR]Exception NotEventID={0}", id);
				return ((int)ErrCode.ERR_PARAM);
			}
		}
		/// <summary>
		///  ｲﾍﾞﾝﾄに設定されたｴﾗｰｺｰﾄﾞ取得
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public int GetEventErrCode(int id)
		{
			try {
				PEventManager.PEvent ev = _eventCtrl.GetEvent(id);
				return (ev.ErrorCode());
			}
			catch(Exception) {
				return ((int)ErrCode.ERR_PARAM);
			}
		}
		/// <summary>
		///  ｲﾍﾞﾝﾄ待ち
		/// </summary>
		/// <param name="eventList">ｲﾍﾞﾝﾄIDﾘｽﾄ</param>
		/// <param name="time">ﾀｲﾑｱｳﾄ(ms)</param>
		/// <returns>発生したｲﾍﾞﾝﾄIDのｲﾝﾃﾞｯｸｽ</returns>
		public int Wait(IList<int> eventList, int time)
		{
			WaitHandle[] evl = new WaitHandle[eventList.Count];
			int[] evNo = new int[eventList.Count];

			string msg = "[";
			Trace.TraceFormat("EventCount={0} Time={1}", eventList.Count, time);

			//ｲﾍﾞﾝﾄIDから真のｲﾍﾞﾝﾄｵﾌﾞｼﾞｪｸﾄを取り出す
			int idx = 0;
			foreach(int evId in eventList) {
				if(evId < 0 || evId >= 255) {
					evNo = null;
					Trace.TraceFormat("[ERR]EventRangeOver={0}", evId);
					return ((int)ErrCode.ERR_PARAM);
				}
				PEventManager.PEvent ev = _eventCtrl.GetEvent(evId);

				Object obj;
				if((obj = ev.GetEvent() as AutoResetEvent) != null) {
					evl[idx] = obj as AutoResetEvent;
				}
				else {
					evl[idx] = obj as ManualResetEvent;
				}
				evNo[idx] = evId;
				idx++;
				msg += (evId.ToString() + ",");
			}
			msg += "]";
			Trace.TraceFormat("Wait({0},{1})", msg, time);

			int ret = WaitHandle.WaitAny(evl, time);


			//使用したｲﾍﾞﾝﾄIDは未使用状態にする
			_eventCtrl.Flush(eventList);

			if(ret == WaitHandle.WaitTimeout) {
				//ﾀｲﾑｱｳﾄの場合
				Trace.TraceFormat("Leave TimeOut:{0}", time);
				return (ret);
			}

			//発生したｲﾍﾞﾝﾄ(ｲﾝﾃﾞｯｸｽ)のｲﾍﾞﾝﾄが持つID番号
			Trace.TraceFormat("Leave:ID={0}", _eventCtrl.GetEvent(evNo[ret])._number);
	Trace.TraceFormat("Leave:St={0}", _eventCtrl.GetEvent(evNo[ret]).Status());
			return (_eventCtrl.GetEvent(evNo[ret])._number);
		}
/*
		/// <summary>
		///  I/O に割り当てられたｲﾍﾞﾝﾄIDを取得する
		/// </summary>
		/// <param name="port"></param>
		/// <param name="bit"></param>
		/// <param name="onoff"></param>
		/// <returns></returns>
		public int GetIoEvent(int port, int bit, int onoff)
		{
			PEventManager.PEvent ev = IoControl.GetIoEvent(port, bit, onoff);
			return (ev._number);
		}
*/
		/// <summary>
		///  I/O 出力
		/// </summary>
		/// <param name="port">ﾎﾟｰﾄ番号:0～</param>
		/// <param name="bit">ﾋﾞｯﾄ番号:0～31</param>
		/// <param name="onoff">On:1 Off:0</param>
		/// <returns></returns>
		public int IoOut(int port, int bit, int onoff)
		{
			UInt32 data = IoOutputValue;

			if(onoff == 1) {
				data |= (UInt32)(1 << bit);
			}
			else {
				data &= (UInt32)(~(1 << bit));
			}
			IoOutputValue = data;

			Trace.TraceFormat("IoOut({0},{1},{2})", port, bit, onoff);

			return (((IoOutputValue & (1 << bit)) == 0) ? 0 : 1);
		}
		/// <summary>
		///  I/O　のﾎﾟｰﾄ単位での出力
		/// </summary>
		/// <param name="port">ﾎﾟｰﾄ番号</param>
		/// <param name="data">32ﾋﾞｯﾄﾃﾞｰﾀ</param>
		/// <returns>実行後のI/O出力状態</returns>
		public uint IoOutPort(int port, uint data)
		{
			IoOutputValue = data;
			Trace.TraceFormat("IoOutPort({0},{1:X})", port, data);

			return (IoOutputValue);
		}
		/// <summary>
		///  I/O の入力値を取得します
		/// </summary>
		/// <param name="port">ﾎﾟｰﾄ番号</param>
		/// <param name="bit">ﾋﾞｯﾄ番号</param>
		/// <returns>1=ON, 0=OFF</returns>
		public int IoIn(int port, int bit)
		{
			int ret = (((IoInputValue & (1 << bit)) != 0) ? 1 : 0);

			Trace.TraceFormat("IoIn({0},{1}):{2}", port, bit,ret);

			return ret;
		}
		// <summary>
		///  I/O のﾎﾟｰﾄ単位での入力状態を取得
		/// </summary>
		/// <param name="port"></param>
		/// <returns></returns>
		public uint IoInport(int port)
		{
			uint ret = IoInputValue;

			Trace.TraceFormat("IoInport({0})0x:{1:X}", port, ret);

			return (ret);
		}
		/// <summary>
		///  引数で戻り値を取得するﾃｽﾄ
		///    <python>
		///     ref = clr.Reference[Int32](0)
		///     GetValue(ref,1111)
		///     vv = ref.Value
		///       Int32 などは from System import * として import する 
		///    </python>
		/// </summary>
		/// <param name="data"></param>
		/// <param name="xx"></param>
		/// <returns></returns>
		public int GetValueData(ref int data, int xx)
		{
			data = xx;
			return ((int)ErrCode.RET_OK);
		}
	}
}
