using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Threading;

namespace RowThon
{
	/// <summary>
	/// ServoErrInfoWnd.xaml の相互作用ロジック
	/// </summary>
	public partial class ServoErrInfoWnd : Window
	{
		private ViewModel _view;
		public ServoErrInfoWnd(ViewModel vm)
		{
			InitializeComponent();

			_view = vm;

			PrintErrorMessage();
		}
		private void PrintErrorMessage()
		{
			string[] axisName = { "J1軸", "J2軸", "J3軸", "J4軸", "J5軸" };

			for(int i = 0; i < 5; i++) {
				int code = (int)_view.GetSvStat(i);
				if(code == -2) {
					infoBox.Items.Add("サーボ未生成");
					break;
				}
				else if(code != 0) {
					string msg = ViewModel.SvAlamMsg(code);
					infoBox.Items.Add(axisName[i] + msg);
				}
			}
		}

		private void ErrClrbtn_Click(object sender, RoutedEventArgs e)
		{
			infoBox.Items.Clear();
			_view.ClearError();

			Thread.Sleep(1000);
			PrintErrorMessage();
		}
	}
}
