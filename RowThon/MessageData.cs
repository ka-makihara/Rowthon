using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Threading;
using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.Windows.Controls.Ribbon;

namespace RowThon
{
	/// <summary>
	///  Rowthon からｿｹｯﾄで外部へ通知されるﾃﾞｰﾀ
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	unsafe public struct MessageData
	{
		public uint _size;			//構造体ｻｲｽﾞ(ｺﾝｽﾄﾗｸﾀ使用以外では生成時に設定すること)
		public MessageCode _code;	//ﾒｯｾｰｼﾞｺｰﾄﾞ
		public uint _ioInput;		//I/Oの入力状態
		public uint _ioOutput;		//I/Oの出力状態
		public int _retCode;		//ｺﾏﾝﾄﾞ戻り値
		public fixed uint _data[12];
//		public fixed char _msg[64];	//ﾒｯｾｰｼﾞ文字(char->2byte)(未使用)

		/// <summary>
		///  ｺｰﾄﾞ指定のｺﾝｽﾄﾗｸﾀ
		///    標準のｺﾝｽﾄﾗｸﾀはﾃﾞﾌｫﾙﾄで生成される。その場合は全ﾒﾝﾊﾞｰは0で初期化
		/// </summary>
		/// <param name="code"></param>
		public MessageData(MessageCode code)
		{
			_size = (uint)sizeof(MessageData);
			_code = code;
			_ioInput = 0;
			_ioOutput = 0;
			_retCode = 0;
		}
		public byte[] Binary()
		{
			/*
			 *  構造体の全てをﾊﾞｲﾅﾘとして byte[] にとりだす
			 *   例えばstring を使用していたりすると、受信側がどうなるか???
			 */
			var size = Marshal.SizeOf(typeof(MessageData));
			var buffer = new byte[size];
			var ptr = IntPtr.Zero;

			ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(this, ptr, false);
			Marshal.Copy(ptr, buffer, 0, size);

			Marshal.FreeHGlobal(ptr);

			return (buffer);
		}
	}
	public class PostSystem
	{
		Queue _quData = new Queue();
		AutoResetEvent _postEvent = new AutoResetEvent(false);

		/// <summary>
		///  送信ﾃﾞｰﾀをﾎﾟｽﾄする
		///  　ﾃﾞｰﾀをﾎﾟｽﾄすることで送信ｽﾚｯﾄﾞから送信される
		/// </summary>
		/// <param name="obj"></param>
		public void Post(Object obj)
		{
			lock ( _quData.SyncRoot ) {
				_quData.Enqueue(obj);
				_postEvent.Set();
			}
		}
		/// <summary>
		///  送信ﾃﾞｰﾀがあるかどうかを監視する
		/// </summary>
		/// <param name="ms">監視ﾀｲﾑｱｳﾄ(ms)</param>
		/// <returns></returns>
		public int IsPosted(int ms)
		{
			int ret = WaitHandle.WaitAny(new WaitHandle[] { _postEvent }, ms);

			if ( ret == WaitHandle.WaitTimeout ) {
				return (-1);
			}
			return (0);
		}
		/// <summary>
		///  ﾃﾞｰﾀの取り出し
		/// </summary>
		/// <returns></returns>
		public Object GetData()
		{
			return (_quData.Dequeue());
		}
		public int Count
		{
			get { return (_quData.Count); }
		}
		public Object SyncRoot
		{
			get { return (_quData.SyncRoot); }
		}
	}
	public partial class MainWindow : RibbonWindow
	{

	}
}
