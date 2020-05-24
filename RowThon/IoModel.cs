using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RowThon
{
	public class IoData
	{
		public IoData(int port, int bit, int onoff)
		{ _portNo = port; _bitNo = bit; _onoff = onoff; }
		public int _portNo;
		public int _bitNo;
		public int _onoff;
	}
	#region ** Class : IoModel
	public class IoModel
	{
		private static ioModule.ContecDio32 _ioCtrl;		//ｺﾝﾃｯｸ・I/Oﾎﾞｰﾄﾞ制御
		private static int ioHandle;
		public UInt32 _ioInput = 0;		// I/O Input
		public UInt32 _ioOutput = 0;	// I/O Output

		private PEventManager.PEvent[] _ioAssert = new PEventManager.PEvent[32];
		private PEventManager.PEvent[] _ioNagate = new PEventManager.PEvent[32];

		public IoModel()
		{
			_ioCtrl = new ioModule.ContecDio32();
			ioHandle = 0;
		}
		public void SetIoEvent(PEventManager.PEvent ev, int port, int bit, int onoff)
		{
			ev.SetRsc(new IoData(port, bit, onoff));
			if( onoff == 1 ) {
				_ioAssert[bit] = ev;
			}
			else {
				_ioNagate[bit] = ev;
			}
		}
		public PEventManager.PEvent GetIoEvent(int port, int bit, int onoff)
		{
			if( onoff == 1 ) {
				return( _ioAssert[bit] );
			}
			else {
				return( _ioNagate[bit] );
			}
		}
		/// <summary>
		///  I/O ﾎﾞｰﾄﾞのｵｰﾌﾟﾝ
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public int Open(string name)
		{
			Trace.TraceFormat("Open:{0}", name);
			try {
				if( (ioHandle = _ioCtrl.open(name, 0)) < 0 ) {
					Trace.TraceFormat("[ERR]Open:{0}", name);
					return ((int)ErrCode.ERR_EXEC);
				}
			}
			catch( Exception err ) {
				Trace.TraceFormat("[ERR]Open:{0}", err.Message);
				return ((int)ErrCode.ERR_EXEC);
			}
			Trace.TraceFormat("Leave Open:{0}", ioHandle);
			return (ioHandle);
		}
		/// <summary>
		///  I/O ﾎﾞｰﾄﾞの終了処理
		/// </summary>
		/// <returns></returns>
		public int Close()
		{
			return (_ioCtrl.close(ioHandle));
		}
		/// <summary>
		/// I/O からのﾃﾞｰﾀ読み込み
		/// </summary>
		/// <param name="port"></param>
		/// <returns></returns>
		public uint Read(int port)
		{
			_ioInput = (uint)_ioCtrl.getport(0);
			Trace.TraceFormat("port={0} data=0x{1:X}", port,_ioInput );
			return ( _ioInput);
		}
		/// <summary>
		/// I/Oへのﾃﾞｰﾀ出力
		/// </summary>
		/// <param name="port"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public int Write(int port, UInt32 value)
		{
			_ioOutput = value;
			return (_ioCtrl.write(ioHandle, port, value));
		}
		public int Out(int port, int bitNo, int onoff)
		{	
			UInt32 data = _ioOutput;

			Trace.TraceFormat("port={0} bit={1} onoff={2}", port, bitNo, onoff);
			if (onoff == 1) {
				data |= ((UInt32)1 << bitNo);
			}
			else {
				data &= ~((UInt32)1 << bitNo);
			}
			return( Write(port,data) );
		}
		/// <summary>
		///  I/O 更新時のｲﾍﾞﾝﾄ処理
		///   Window のﾒｯｾｰｼﾞﾙｰﾌﾟで呼び出される
		/// </summary>
		/// <param name="hwnd"></param>
		/// <param name="msg"></param>
		/// <param name="wParam"></param>
		/// <param name="lParam"></param>
		public void wnd_proc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
		{
			// I/O の更新が発生
			_ioCtrl.wnd_proc(hwnd, msg, wParam, lParam);

			uint newValue = (uint)_ioCtrl.getport(0);

			UInt32 diff = _ioInput ^ newValue;

			for( int bit = 0; bit < 32; bit++ ) {
				if( (diff & (1 << bit)) != 0 ) {

					if( (newValue & (1 << bit)) != 0 ) {
						if( _ioAssert[bit] != null ) {
							_ioNagate[bit].Flush();	//Offｲﾍﾞﾝﾄは消去
							_ioAssert[bit].Set(0);	//Onｲﾍﾞﾝﾄをｾｯﾄ
						}
					}
					else {
						if( _ioNagate[bit] != null ) {
							_ioAssert[bit].Flush();	//Onｲﾍﾞﾝﾄを消去
							_ioNagate[bit].Set(0);	//Offｲﾍﾞﾝﾄをｾｯﾄ
						}
					}
				}
			}
			//入力ﾃﾞｰﾀを更新
			_ioInput = newValue;
		}
	}
	#endregion
}
