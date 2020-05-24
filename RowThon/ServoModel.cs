using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

namespace RowThon
{
	#region ** Class : ServoModel
	public class ServoModel
	{
		private static svModule.servoManager _svCtrl;		//ｻｰﾎﾞﾏﾈｰｼﾞｬｰ
		private static int svHandle;

		public ServoModel()
		{
			_svCtrl = new svModule.servoManager();
			svHandle = 0;
		}
		/// <summary>
		/// ｻｰﾎﾞﾎﾞｰﾄﾞの初期化
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public int Open(string name)
		{
			Trace.TraceFormat("Open:{0}", name);
			try {
				if( (svHandle = _svCtrl.open(name, 0)) < 0 ) {
					Trace.TraceFormat("[ERR]Open:{0}", name);
					return ((int)ErrCode.ERR_EXEC);
				}
			}
			catch( Exception err ) {
				Trace.TraceFormat("[ERR]Open:{0}", err.Message);
				return ((int)ErrCode.ERR_EXEC);
			}
			Trace.TraceFormat("Leave Open:{0}", svHandle);
			return (svHandle);
		}
		/// <summary>
		/// ｻｰﾎﾞﾎﾞｰﾄﾞの終了
		/// </summary>
		/// <returns></returns>
		public int Close()
		{
			return (_svCtrl.close(svHandle));
		}
		/// <summary>
		/// ｻｰﾎﾞｼｽﾃﾑの異常通知用ｲﾍﾞﾝﾄの設定
		/// </summary>
		/// <param name="ev"></param>
		public void SetUnitEvent(PEventManager.PEvent ev)
		{
			_svCtrl.SetUnitEvent(ev);
		}
		/// <summary>
		///  ｻｰﾎﾞ電源のOn/Off
		/// </summary>
		/// <param name="onoff">On:1 Off:0</param>
		/// <returns>Error: <0 </returns>
		public int Power(int m)
		{
			return (_svCtrl.Power(m));
		}
		/// <summary>
		///  ｻｰﾎﾞ現在値の取得
		/// </summary>
		/// <param name="pos1">X座標</param>
		/// <param name="pos2">Y座標</param>
		/// <param name="pos3">Z座標</param>
		/// <param name="pos4">A座標</param>
		/// <param name="pos5">B座標</param>
		public void GetCurPos(ref Double pos1, ref Double pos2, ref Double pos3, ref Double pos4, ref Double pos5, ref Double pos6)
		{
			_svCtrl.GetPos(ref pos1, ref pos2, ref pos3, ref pos4, ref pos5, ref pos6);
		}
		/// <summary>
		///  ｻｰﾎﾞ機械座標の取得
		/// </summary>
		/// <param name="pos1">J1座標</param>
		/// <param name="pos2">J2座標</param>
		/// <param name="pos3">J3座標</param>
		/// <param name="pos4">J4座標</param>
		/// <param name="pos5">J5座標</param>
		public void GetMcPos(ref Double pos1, ref Double pos2, ref Double pos3, ref Double pos4, ref Double pos5)
		{
			_svCtrl.GetMcPos(ref pos1, ref pos2, ref pos3, ref pos4, ref pos5);
		}
		/// <summary>
		///  ｻｰﾎﾞ指令座標の取得(ｱﾌﾟﾘ、内部記憶値)
		/// </summary>
		/// <param name="pos1">X座標</param>
		/// <param name="pos2">Y座標</param>
		/// <param name="pos3">Z座標</param>
		/// <param name="pos4">A座標</param>
		/// <param name="pos5">B座標</param
		public void GetCmdPos(ref Double pos1, ref Double pos2, ref Double pos3, ref Double pos4, ref Double pos5)
		{
			_svCtrl.GetCommandPos(ref pos1, ref pos2, ref pos3, ref pos4, ref pos5);
		}
		/// <summary>
		///  動作ｺﾏﾝﾄﾞの停止要求
		///    軸移動やJog動作の停止を行う
		/// </summary>
		/// <returns></returns>
		public int StopCommand()
		{
			return (_svCtrl.StopCommand());
		}
		/// <summary>
		///  ｻｰﾎﾞ・ﾘｾｯﾄ
		/// </summary>
		/// <returns>Error: <0 </returns
		public int Reset()
		{
			int sts;

			Trace.TraceFormat("Entry");

			_svCtrl.Reset();

			Thread.Sleep(1000);

			for( int i = 0; i < 5; i++ ) {
				if( (sts = GetAxisAlam(i)) != 0 ) {
					Trace.TraceFormat("[ERR]Axis:{0} Alm:{1:X}", i, sts);
					return ((int)ErrCode.ERR_EXEC);
				}
			}
			Trace.TraceFormat("Leave:OK");
			return ((int)ErrCode.RET_OK);
		}
		/// <summary>
		/// ｻｰﾎﾞｴﾗｰｸﾘｱ
		/// </summary>
		/// <returns></returns>
		public int ClrError()
		{
			Trace.TraceFormat("Entry");
			int ret = _svCtrl.ClrError();
			Trace.TraceFormat("Leave:{0}", ret);
			return (ret);
		}
		/// <summary>
		/// 指定された軸のｱﾗｰﾑ情報を取得
		/// </summary>
		/// <param name="axis">軸番号</param>
		/// <returns></returns
		public int GetAxisAlam(int axis)
		{
			return (_svCtrl.GetAxisAlam(axis));
		}
		/// <summary>
		/// 軸移動指示(単一動作)
		/// </summary>
		/// <param name="data">{"X":xxx,"Y":yyy,...}</param>
		/// <param name="evObj">ｲﾍﾞﾝﾄID</param>
		/// <returns></returns>
		public int MoveAxis(Dictionary<string, int> data, PEventManager.PEvent evObj)
		{
			return (_svCtrl.MoveAxis(data, evObj));
		}
		/// <summary>
		/// 軸移動指示(複数動作)
		/// </summary>
		/// <param name="data">{"X":xxx,"Y":xxx},{"X":xxx,"Y":yyy},...</param>
		/// <param name="evObj">ｲﾍﾞﾝﾄID</param>
		/// <returns></returns>
		public int MoveProgAxis(IList<Dictionary<string, int>> data, PEventManager.PEvent evObj)
		{
			return (_svCtrl.MoveProgAxis(data, evObj));
		}
		/// <summary>
		///  ｻｰﾎﾞｼｽﾃﾑの状態取得
		/// </summary>
		/// <returns></returns>
		public uint GetMcStatus()
		{
			return (_svCtrl.GetMcStatus());
		}
		/// <summary>
		/// ｻｰﾎﾞ軸情報の取得
		/// </summary>
		/// <param name="axisNo"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public int GetAxisStatus(int axisNo, int type)
		{
			int ret;

			//Trace.TraceFormat("Entry:axis={0} type={1}", axisNo, type);
			ret = _svCtrl.GetAxisStatus(axisNo, type);
			//Trace.TraceFormat("Leave:{0}", ret);

			return (ret);
		}
		/// <summary>
		/// 軸指定のﾊﾟﾗﾒｰﾀ取得
		/// </summary>
		/// <param name="axisNo">軸番号</param>
		/// <param name="type">取得したい値</param>
		/// <returns></returns>
		public int GetAxisParam(int axisNo, int type)
		{
			int ret;

			Trace.TraceFormat("Entry:axis={0} type={1}", axisNo, type);
			ret = _svCtrl.GetAxisParam(axisNo, type);
			Trace.TraceFormat("Leave:{0}", ret);

			return (ret);
		}
		/// <summary>
		/// 指定軸のﾊﾟﾗﾒｰﾀ設定
		/// </summary>
		/// <param name="axisNo">軸番号</param>
		/// <param name="type">設定したい値</param>
		/// <param name="value">設定値</param>
		/// <returns></returns>
		public int SetAxisParam(int axisNo, int type, int value)
		{
			int ret;

			Trace.TraceFormat("Entry:axis={0} type={1}", axisNo, type);
			ret = _svCtrl.SetAxisParam(axisNo, type, value);
			Trace.TraceFormat("Leave:{0}", ret);

			return (ret);
		}
		/// <summary>
		///  指定軸のJog動作開始/停止
		/// </summary>
		/// <param name="axisNo">軸番号(0～)</param>
		/// <param name="dir">方向 1:+ -1:- 0:停止</param>
		/// <param name="spd">0:停止 ※現時点では設定不可</param>
		/// <returns></returns
		public int JogAxis(int axisNo, int dir, int spd)
		{
			if( axisNo < 0 || axisNo > 4 ) {
				Trace.TraceFormat("[ERR]axisNo={0}", axisNo);
				return ((int)ErrCode.ERR_PARAM);
			}
			if( dir != -1 && dir != 0 && dir != 1 ) {
				Trace.TraceFormat("[ERR]dir={0}", dir);
				return ((int)ErrCode.ERR_PARAM);
			}
			if( spd < 0 ) {
				Trace.TraceFormat("[ERR]Minus spd={0}", spd);
				return ((int)ErrCode.ERR_PARAM);
			}
			return (_svCtrl.JogAxis(axisNo, dir, spd));
		}
		/// <summary>
		///  ｻｰﾎﾞの現在値を取得する
		///     aa = Rowthon.SvPos()
		///      RX=aa[0], RY=aa[1]
		/// </summary>
		/// <returns>double の配列</returns>
		public IList<int> GetServoPos()
		{
			IList<int> pos = new int[6];

			Double p1 = 0.0, p2 = 0.0, p3 = 0.0, p4 = 0.0, p5 = 0.0, p6 = 0.0;

			GetCurPos(ref p1, ref p2, ref p3, ref p4, ref p5, ref p6);

			pos[0] = (int)(p1 * 10000.0);
			pos[1] = (int)(p2 * 10000.0);
			pos[2] = (int)(p3 * 10000.0);
			pos[3] = (int)(p4 * 10000.0);
			pos[4] = (int)(p5 * 10000.0);
			pos[5] = (int)(p6 * 10000.0);

			return (pos);
		}
		/// <summary>
		///  ｻｰﾎﾞ指令位置を取得
		/// </summary>
		/// <returns></returns>
		public IList<int> GetServoCommandPos()
		{
			IList<int> pos = new int[5];

			Double p1 = 0.0, p2 = 0.0, p3 = 0.0, p4 = 0.0, p5 = 0.0;

			//※取得される値は、本来の指令値ではなく、指定位置付近でのふらつき値
			GetCmdPos(ref p1, ref p2, ref p3, ref p4, ref p5);

			pos[0] = (int)(p1 * 10000.0);
			pos[1] = (int)(p2 * 10000.0);
			pos[2] = (int)(p3 * 10000.0);
			pos[3] = (int)(p4 * 10000.0);
			pos[4] = (int)(p5 * 10000.0);

			return (pos);
		}
		/// <summary>
		///  複数の移動指令による軸動作
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public int ProcMove(IList<Dictionary<string, int>> data, PEventManager.PEvent evObj)
		{
			Trace.TraceFormat("Entry");

			int id = MoveProgAxis(data, evObj);
			if( id < 0 ) {
				Trace.TraceFormat("[ERR]ret={0}", id);
				evObj.Flush();
				return (evObj.ErrorCode());
			}
			Trace.TraceFormat("Leave:EventID={0}", evObj._number);
			return (evObj._number);
		}
		/// <summary>
		///  軸の移動
		/// </summary>
		/// <param name="data">{"RX":xxxx,"RY":yyyy}</param>
		/// <returns></returns>
		public int Move(Dictionary<string, int> data, PEventManager.PEvent evObj)
		{
			Trace.TraceFormat("Entry");

			int id = MoveAxis(data, evObj);
			if( id < 0 ) {
				Trace.TraceFormat("[ERR]ret={0}", id);
				evObj.Flush();
				return (evObj.ErrorCode());
			}
			Trace.TraceFormat("Leave:EventID={0}", evObj._number);
			return (evObj._number);
		}
		public string GetAlamMsg(int code)
		{
			return (_svCtrl.GetAlamMsg(code));
		}
		public int StartLog()
		{
			return( _svCtrl.StartLog() );
		}
		public int StopLog()
		{
			return (_svCtrl.StopLog());
		}
	}
	#endregion
}
