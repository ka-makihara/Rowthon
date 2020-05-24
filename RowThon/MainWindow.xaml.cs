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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Windows.Controls.Ribbon;

using System.Runtime.InteropServices;   //for DllImport
using System.Windows.Interop;           //for HwndSource
using System.Threading;					//for Thread
using System.Threading.Tasks;			//for Sleep
using System.Net;						//for Dns
using System.Net.Sockets;
using System.Diagnostics;

using ICSharpCode.AvalonEdit;
using Microsoft.Scripting.Hosting;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;

namespace RowThon
{
	/// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
		static ViewModel _viewModel = new ViewModel();					//viewModel

		private System.Windows.Threading.DispatcherTimer _timer = null;	//ﾀｲﾏｰ処理

		private WriteableBitmap _col_wb = new WriteableBitmap(640, 640, 96, 96, PixelFormats.Rgb24, null);

        public MainWindow()
        {
			InitializeComponent();

			//ViewModel からのｽｸﾘﾌﾟﾄ実行用のdelegate
			DataContextChanged += (o, e) =>
				{
					ViewModel vm = DataContext as ViewModel;
					if( vm != null ) {
						vm._ExecuteScript += (sender, arg) =>
							{
								Dispatcher.Invoke(new Action(() => { pythonConsole.Pad.Console.RunStatements(arg.cmd); }));
							};

						vm._DrawCameraBitmap += (sender, arg) =>
							{
								Dispatcher.BeginInvoke(new Action(() =>
								{
									IplImage img = vm.VisionControl.GetCameraImage();

									DrawCameraViewEventArgs a = arg as DrawCameraViewEventArgs;

									if( a._draw == 1 ) {
										CvRect rect = new CvRect(a._x1, a._y1, a._x2, a._y2);
										img.DrawRect(rect, new CvScalar(255, 0, 0), 2);
									}
									else if(a._draw == 2) {
										int x1 = a._x1 - a._x2 / 2;
										int x2 = a._x1 + a._x2 / 2;
										int y1 = a._y1 - a._y2 / 2;
										int y2 = a._y1 + a._y2 / 2;
										img.DrawLine(x1, a._y1, x2, a._y1, new CvScalar(255, 0, 0),2);
										img.DrawLine(a._x1, y1, a._x1, y2, new CvScalar(255, 0, 0),2);
									}

									if(VM.CenterLine == true) {
										img.DrawLine(0, 320, 640, 320, new CvScalar(255, 0, 0,0),2);
										img.DrawLine(320,0, 320, 640, new CvScalar(255, 0, 0,0),2);
									}

									WriteableBitmapConverter.ToWriteableBitmap(img, _col_wb);

									cameraImage.Source = _col_wb;

									img.Dispose();

									//cameraImage.Source = vm.VisionControl.GetCameraBitmap();
								}));
							};
					}
				};

            pythonConsole.Pad.Host.ConsoleCreated += new PythonConsoleControl.ConsoleCreatedEventHandler(Host_ConsoleCreated);
        }
		/// <summary>
		/// 
		/// </summary>
		private ViewModel VM
		{
			get { return (_viewModel); }
		}
        /// <summary>
        /// ｳｲﾝﾄﾞｳが生成された後の処理で、独自の WndProc を追加する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
			// I/O ﾎﾞｰﾄﾞのI/O変化はｳｲﾝﾄﾞｳに対するﾒｯｾｰｼﾞという形で通知されるため
			// ﾒｯｾｰｼﾞをﾌｯｸするために使用します。
            HwndSource src = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);

            src.AddHook(new HwndSourceHook(WndProc));

			this.DataContext = _viewModel;
			WordModel.Vm = _viewModel;

			object obj = ModelData;

			VM.PythonConsole = pythonConsole;

			comboBox1.SelectedIndex = 0;

			//ｽﾃｰﾀｽ取得用のﾀｲﾏｰｲﾍﾞﾝﾄを設定
			_timer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Normal,this.Dispatcher);
			_timer.Interval = TimeSpan.FromSeconds(0.1);
			_timer.Tick += new EventHandler(TimerFunc);
//ﾀｲﾏｰの開始はｽｸﾘﾌﾟﾄのﾛｰﾄﾞが完了してからにする
//			_timer.Start();

        }
        /// <summary>
        ///  ｳｲﾝﾄﾞｳがｸﾛｰｽﾞされる時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RibbonWindow_Closed(object sender, EventArgs e)
        {
			VM.QuitView();
        }
        /// <summary>
        ///   I/O 変化の通知受信用に WndProc をﾌｯｸﾙｰﾁﾝとして定義する
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
			//I/O用のWindowﾒｯｾｰｼﾞ処理
			VM.wnd_proc(hwnd, msg, wParam, lParam);
			if(msg == 0x10) {
				Trace.TraceText("end","Wnd");
			}
            return IntPtr.Zero;
        }
		/// <summary>
		///  DispatcherTimer でのﾊﾝﾄﾞﾗ
		///    UIｽﾚｯﾄﾞで実行される
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="arg"></param>
		private void TimerFunc(object sender, EventArgs arg)
		{
			Double pos1 = 0.0, pos2 = 0.0, pos3 = 0.0, pos4 = 0.0, pos5 = 0.0, pos6 = 0.0;

			//現在値取得
			VM.SvControl.GetCurPos(ref pos1, ref pos2, ref pos3, ref pos4, ref pos5, ref pos6);

			posX.Text = pos1.ToString("0.000");
			posY.Text = pos2.ToString("0.000");
			posZ.Text = pos3.ToString("0.000");
			posA.Text = pos4.ToString("0.000");
			posB.Text = pos6.ToString("0.000");//変換した座標を表示

			string cmd = string.Format("moveLib.areaName(moveLib.GetAreaNo({0},{1}))",(int)pos1*10000.0, (int)pos2 * 10000.0);

			ScriptSource src = pythonConsole.Pad.Console.ScriptScope.Engine.CreateScriptSourceFromString(cmd);
			Object obj = src.Execute(pythonConsole.Pad.Console.ScriptScope);
			SvArea.Text = obj as string;

			if(comboBox1.SelectedIndex == 0) {
				IList<int> cur = VM.SvControl.GetServoPos();

				Ph_posX.Text = (cur[0] / 10000.0).ToString("0.000");
				Ph_posY.Text = (cur[1] / 10000.0).ToString("0.000");
				Ph_posZ.Text = (cur[2] / 10000.0).ToString("0.000");
				Ph_posA.Text = (cur[3] / 10000.0).ToString("0.000");
				Ph_posB.Text = (cur[4] / 10000.0).ToString("0.000");
			}
			else if(comboBox1.SelectedIndex == 1) {
				//機械座標
				VM.SvControl.GetMcPos(ref pos1, ref pos2, ref pos3, ref pos4, ref pos5);
				Ph_posX.Text = pos1.ToString("0.000");
				Ph_posY.Text = pos2.ToString("0.000");
				Ph_posZ.Text = pos3.ToString("0.000");
				Ph_posA.Text = pos4.ToString("0.000");
				Ph_posB.Text = pos5.ToString("0.000");
			}
			//ｻｰﾎﾞｴﾗｰ情報の取得
			VM.UpdateServoStatus();
		}

		/// <summary>
		///  操作ﾓｰﾄﾞの変更
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RibbonGallery_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			string str = ((RibbonGalleryItem)e.NewValue).Content.ToString();

			if( str == "ﾏﾆｭｱﾙ操作" ) {
				VM._appCtrlMode = CtrlMode.Mode_Manual;
			}
			else if( str == "ﾘﾓｰﾄ操作" ) {
				VM._appCtrlMode = CtrlMode.Mode_Remote;

			}
		}
		private void BtPlusX_MouseUp(object sender, MouseButtonEventArgs e)   { VM.ServoJog((int)AxisNo.Axis_X, 0, 0); }
		private void BtPlusX_MouseDown(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_X, 1, Int32.Parse(JogSpd.Text)); }
		private void BtMinusX_MouseUp(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_X, 0, 0); }
		private void BtMinusX_MouseDown(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_X, -1, Int32.Parse(JogSpd.Text)); }

		private void BtPlusY_MouseUp(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_Y, 0, 0); }
		private void BtPlusY_MouseDown(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_Y, 1, Int32.Parse(JogSpd.Text)); }
		private void BtMinusY_MouseUp(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_Y, 0, 0); }
		private void BtMinusY_MouseDown(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_Y, -1, Int32.Parse(JogSpd.Text)); }

		private void BtPlusZ_MouseUp(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_Z, 0, 0); }
		private void BtPlusZ_MouseDown(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_Z, 1, Int32.Parse(JogSpd.Text)); }
		private void BtMinusZ_MouseUp(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_Z, 0, 0); }
		private void BtMinusZ_MouseDown(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_Z, -1, Int32.Parse(JogSpd.Text)); }

		private void BtPlusA_MouseUp(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_A, 0, 0); }
		private void BtPlusA_MouseDown(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_A, 1, Int32.Parse(JogSpd.Text)); }
		private void BtMinusA_MouseUp(object sender, MouseButtonEventArgs e)  { VM.ServoJog((int)AxisNo.Axis_A, 0, 0); }
		private void BtMinusA_MouseDown(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_A, -1, Int32.Parse(JogSpd.Text)); }

		private void BtPlusB_MouseUp(object sender, MouseButtonEventArgs e)   { VM.ServoJog((int)AxisNo.Axis_B, 0, 0); }
		private void BtPlusB_MouseDown(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_B, 1, Int32.Parse(JogSpd.Text)); }
		private void BtMinusB_MouseUp(object sender, MouseButtonEventArgs e)  { VM.ServoJog((int)AxisNo.Axis_B, 0, 0); }
		private void BtMinusB_MouseDown(object sender, MouseButtonEventArgs e) { VM.ServoJog((int)AxisNo.Axis_B, -1, Int32.Parse(JogSpd.Text)); }

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
			VM.SendIoMessage(0, MessageCode.SYS_IO_UPDATE);
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

		private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
		}

		private void Help_Click(object sender, RoutedEventArgs e)
		{
			VersionInfo info = new VersionInfo();

			info.Show();
//			MessageBox.Show("AAA", "sss", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}
