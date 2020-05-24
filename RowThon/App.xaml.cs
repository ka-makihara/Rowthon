using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace RowThon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		private static Settings _appSettings = new Settings();		//ｱﾌﾟﾘｹｰｼｮﾝ・設定ﾌｧｲﾙ

		public static Settings Setting
		{
			get { return _appSettings; }
			set { _appSettings = value; }
		}
    }
}
