using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Windows.Controls.Ribbon;

namespace RowThon
{
	public partial class MainWindow : RibbonWindow
	{
		/// <summary>
		///  ｻｰﾎﾞ・ﾘｾｯﾄ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SvReset_Click(object sender, RoutedEventArgs e)
		{
			int sts = VM.SvControl.Reset();

			if( sts < 0 ){
				string str = "ﾘｾｯﾄできません";
				MessageBox.Show(str, "確認",MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		/// <summary>
		///  ｻｰﾎﾞ・ｴﾗｰｸﾘｱ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SvErrClr_Click(object sender, RoutedEventArgs e)
		{
			VM.SvControl.ClrError();
		}
	}
}
