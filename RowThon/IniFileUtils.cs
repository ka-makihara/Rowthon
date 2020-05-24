using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace RowThon
{
	/// <summary>
	/// iniﾌｧｲﾙ取り扱いのためのﾕｰﾃｨﾘﾃｨｸﾗｽ
	/// </summary>
	public class IniFileUtils
	{
		/// <summary>
		///  iniﾌｧｲﾙのﾊﾟｽを保持
		/// </summary>
		private String filePath { get; set; }

		// =============================================================
		[DllImport("KERNEL32.DLL")]
		public static extern uint
			GetPrivateProfileString(string lpAppName,
									string lpKeyName,
									string lpDefault,
									StringBuilder lpRetString,
									uint nSize,
									string lpFileName);
		[DllImport("KERNEL32.DLL")]
		public static extern int
			GetPrivateProfileInt(string lpAppName,
								 string lpKeyName,
								 int nDefault,
								 string lpFileName);
		[DllImport("kernel32.dll")]
		private static extern int
			WritePrivateProfileString(string lpAppName,
									  string lpKeyName,
									  string lpString,
									  string lpFileName);
		//================================================================
		/// <summary>
		/// ｺﾝｽﾄﾗｸﾀ(ﾃﾞﾌｫﾙﾄ)
		/// </summary>
		public IniFileUtils()
		{
			this.filePath = AppDomain.CurrentDomain.BaseDirectory;
		}

		/// <summary>
		/// ｺﾝｽﾄﾗｸﾀ(fileﾊﾟｽを指定する場合)
		/// </summary>
		/// <param name="filePath"></param>
		public IniFileUtils(String filePath)
		{
			this.filePath = filePath;
		}

		/// <summary>
		/// iniﾌｧｲﾙ中のｾｸｼｮﾝのｷｰを指定して文字列を得る
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public String getValueString(String section, String key)
		{
			StringBuilder sb = new StringBuilder(1024);

			GetPrivateProfileString(section, key, "", sb, Convert.ToUInt32(sb.Capacity), filePath);

			return (sb.ToString());
		}

		/// <summary>
		/// iniﾌｧｲﾙのｾｸｼｮﾝのｷｰを指定して整数値を得る
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public int getValueInt(String section, String key)
		{
			return ((int)GetPrivateProfileInt(section, key, 0, filePath));
		}

		/// <summary>
		/// 指定したｾｸｼｮﾝ、ｷｰに数値を書き込む
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="val"></param>
		public void setValue(String section, String key, int val)
		{
			setValue(section, key, val.ToString());
		}

		/// <summary>
		/// 指定したｾｸｼｮﾝ、ｷｰに文字列を書き込む
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <param name="val"></param>
		public void setValue(String section, String key, String val)
		{
			WritePrivateProfileString(section, key, val, filePath);
		}
	}
}
