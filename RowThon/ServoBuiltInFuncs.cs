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
//   ここでは主に、ｻｰﾎﾞ関連の公開APIを定義する
//
//  MainWindow 側の呼び出し用のｵﾌﾞｼﾞｪｸﾄとして Rowthon を設定している
//     void Console_ConsoleInitialized() 内
//         pythonConsole.Pad.Console.ScriptScope.SetVariable("Rowthon", this)
//
namespace RowThon
{
	public partial class ViewModel : ViewModelBase
	{
/*
		/// <summary>
		///  ｻｰﾎﾞ電源のOn/Off
		/// </summary>
		/// <param name="onoff">On:1 Off:0</param>
		/// <returns>Error: <0 </returns>
		public int ServoPower(int onoff)
		{
			//ｺﾝﾄﾛｰﾗｰの電源ON
			if( onoff == 0 ) {
				SvControl.Power(0);
			}

			//制御電源ONの手順
			if( onoff == 1 ) {
				OutBitOnOff(0, true);
				OutBitOnOff(1, true);
				OutBitOnOff(2, true);
				Thread.Sleep(100);
				OutBitOnOff(2, false);
			}
			else {
				OutBitOnOff(2, false);
				OutBitOnOff(1, false);
				Thread.Sleep(500);
				OutBitOnOff(0, false);
			}
			if( onoff == 1) {
				Thread.Sleep(2000);	//ｱﾝﾌﾟ電源が入ってから待ちを入れる
				SvControl.Power(1);
			}
			return ((SvPowStat == true) ? 1 : 0);
		}
*/
		/// <summary>
		///  ｻｰﾎﾞのｽﾃｰﾀｽ情報を更新する
		/// </summary>
		public void UpdateServoStatus()
		{
			for( int axisNo = 0; axisNo < 5; axisNo++ ) {
				uint data = (uint)SvControl.GetAxisStatus(axisNo, 1);
				if( _svStatus[axisNo] != data ) {
					_svStatus[axisNo] = data;

					string propName = "AxisBackGround" + axisNo.ToString();
					RaisePropertyChanged(propName);

					propName = "AxisErrMsg" + axisNo.ToString();
					RaisePropertyChanged(propName);
				}

			}
		}
		/// <summary>
		///  軸の移動
		/// </summary>
		/// <param name="data">{"RX":xxxx,"RY":yyyy}</param>
		/// <returns></returns>
		public int SvMove(Dictionary<string, int> data)
		{
			Trace.TraceFormat("Entry");
			try {
				return (SvControl.Move(data, _eventCtrl.GetEvent()));
			}
			catch( Exception ) {
				//ｲﾍﾞﾝﾄか取得できない場合は例外がｽﾛｰされる
				Trace.TraceFormat("[ERR]No Event");
				return ((int)ErrCode.ERR_EXEC);
			}
		}
		/// <summary>
		///  複数の移動指令による軸動作
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public int SvProcMove(IList<Dictionary<string, int>> data)
		{
			Trace.TraceFormat("Entry");
			try {
				return( SvControl.ProcMove(data,_eventCtrl.GetEvent()) );
			}
			catch( Exception ) {
				//ｲﾍﾞﾝﾄか取得できない場合は例外がｽﾛｰされる
				Trace.TraceFormat("[ERR]No Event");
				return ((int)ErrCode.ERR_EXEC);
			}
		}
		
		/// <summary>
		///  軸移動指令の中止
		/// </summary>
		/// <returns></returns>
		public int StopMove(int id)
		{
			Trace.TraceFormat("Entry:id={0}", id);
			if( SvControl != null ) {
				try {
					PEventManager.PEvent evObj = _eventCtrl.GetEvent(id);

					if( evObj.GetRscType() == 0 ) {
						//ｲﾍﾞﾝﾄが使用中である場合のみ
						uint st = SvControl.GetMcStatus();

						//ﾌﾟﾛｸﾞﾗﾑは停止させる(動作中かどうかは確認しない)
						SvControl.StopCommand();
						SvControl.Reset();	//ｽﾚｯﾄﾞをAbort()させると"ﾌﾞﾛｯｸ停止"なので

						//ﾌﾟﾛｸﾞﾗﾑの停止を待つ
						// ※ﾌﾟﾛｸﾞﾗﾑを停止させることによって、動作完了扱いとなるため
						//   停止ｺﾏﾝﾄﾞを出した直後にｲﾍﾞﾝﾄのｽﾃｰﾀｽを変更しても
						//   動作ｺﾏﾝﾄﾞの完了の方が遅くなるので、結果的にｲﾍﾞﾝﾄは
						//   ｷｬﾝｾﾙから成功になってしまう。
						//   このため、ﾌﾟﾛｸﾞﾗﾑの完了を待ってから、改めてｲﾍﾞﾝﾄをｷｬﾝｾﾙ設定する
						Trace.TraceFormat("Wait:program stop");
						uint nn;
						do {
							Thread.Sleep(100);
							nn = SvControl.GetMcStatus();
							Trace.TraceFormat("programStatus:{0:X}", nn);
						} while( (nn & (0x10 << 16)) != 0 );

						if( (st & (0x10 << 16)) != 0 ) {
							//ﾌﾟﾛｸﾞﾗﾑ実行中であった場合はｲﾍﾞﾝﾄをｷｬﾝｾﾙ扱いとする
							evObj.Cancel(0);
							Trace.TraceFormat("programEvent Canceled");
						}
					}
					else {
						Trace.TraceFormat("Event Not Used");
					}
					Trace.TraceFormat("Leave");
					return ((int)ErrCode.RET_OK);
				}
				catch( Exception ) {
					Trace.TraceFormat("[ERR]Exception Parameter");
					return ((int)ErrCode.ERR_PARAM);
				}
			}
			else {
				return ((int)ErrCode.ERR_EXEC);
			}
		}
		/// <summary>
		///  指定軸のJog動作開始/停止
		/// </summary>
		/// <param name="axisNo">軸番号(0～)</param>
		/// <param name="dir">方向 1:+ -1:- 0:停止</param>
		/// <param name="spd">0:停止 ※現時点では設定不可</param>
		/// <returns></returns>
		public int ServoJog(int axisNo, int dir, int spd)
		{
			Trace.TraceFormat("Entry:axisNo={0} dir={1},spd={2}", axisNo, dir, spd);

			if( spd > Setting.MaxJogSpd ) {
				Trace.TraceFormat("[ERR]Over spd={0}-{1}", spd, Setting.MaxJogSpd);
				return ((int)ErrCode.ERR_PARAM);
			}
			if( spd == 0 && dir != 0 ) {
				if( spd == 0 ) {
					//設定値が0なら現在の設定値を取得
					spd = SvControl.GetAxisParam(axisNo, (int)SvParaType.JOG_SPD);
				}
				if( spd == 0 ) {
					//それでも0なら適当な値
					spd = 10000;
				}
			}
			int ret = SvControl.JogAxis(axisNo, dir, spd);
			Trace.TraceFormat("Leave:{0}", ret);
			return (ret);
		}
	}
}
