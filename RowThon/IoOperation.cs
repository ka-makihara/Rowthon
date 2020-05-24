using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using Microsoft.Windows.Controls.Ribbon;

namespace RowThon
{
/*
	public partial class MainWindow : RibbonWindow
	{
		/// <summary>
		///  I/O のﾁｪｯｸﾎﾞｯｸｽの操作でI/O出力する
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="bit"></param>
		private void ioOutputData(object sender, int bit)
		{
			CheckBox chBox = (CheckBox)sender;

			if(chBox.IsChecked == true) {
				//現在が true でこのﾊﾝﾄﾞﾗが呼び出された時はfalse へ移行する時なので
				VM.IoOutputValue = (VM.IoOutputValue & ~(UInt32)(1 << bit)); 
			}
			else {
				VM.IoOutputValue = (VM.IoOutputValue | (UInt32)(1 << bit));
			}
			VM.SendAppMessage(0);
		}
		private void ioOut01_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 0); }
		private void ioOut02_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 1); }
		private void ioOut03_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 2); }
		private void ioOut04_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 3); }
		private void ioOut05_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 4); }
		private void ioOut06_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 5); }
		private void ioOut07_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 6); }
		private void ioOut08_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 7); }
		private void ioOut09_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 8); }
		private void ioOut10_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 9); }
		private void ioOut11_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 10); }
		private void ioOut12_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 11); }
		private void ioOut13_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 12); }
		private void ioOut14_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 13); }
		private void ioOut15_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 14); }
		private void ioOut16_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 15); }
		private void ioOut17_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 16); }
		private void ioOut18_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 17); }
		private void ioOut19_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 18); }
		private void ioOut20_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 19); }
		private void ioOut21_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 20); }
		private void ioOut22_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 21); }
		private void ioOut23_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 22); }
		private void ioOut24_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 23); }
		private void ioOut25_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 24); }
		private void ioOut26_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 25); }
		private void ioOut27_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 26); }
		private void ioOut28_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 27); }
		private void ioOut29_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 28); }
		private void ioOut30_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 29); }
		private void ioOut31_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 30); }
		private void ioOut32_Click(object sender, RoutedEventArgs e) { ioOutputData(sender, 31); }
	}
*/
}
