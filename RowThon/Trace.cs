using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RowThon
{
	#region ** Class : Trace
	/// <summary>
	///  ﾄﾚｰｽｼｽﾃﾑ
	///    ｼｽﾃﾑ全体で使用するために、ｸﾞﾛｰﾊﾞﾙで定義する
	/// </summary>
	public static class Trace
	{
		private static TraceLibs.TraceCtrl _trace = new TraceLibs.TraceCtrl();

		public static void CreateLog(string name, int cnt, int mode)
		{
			_trace.CreateLog(name, cnt, mode);
		}
		public static void TraceFormat(string format, params object[] args)
		{
			_trace.TraceFormat(format, args);
		}
		public static void TraceText(string msg, string func)
		{
			_trace.TraceText(msg, func);
		}
	}
	#endregion
}
