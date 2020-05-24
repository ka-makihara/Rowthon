using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using Microsoft.Windows.Controls.Ribbon;
using System.Threading;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Scripting;              //for PythonCosole
using Microsoft.Scripting.Hosting;      //for PythonConsole
using IronPython.Hosting;               //for PythonConsole(ImportModule)

namespace RowThon
{
	public partial class MainWindow : RibbonWindow
	{
		/// <summary>
		///  ｽｸﾘﾌﾟﾄ内でｱﾌﾟﾘｹｰｼｮﾝ内の関数が使用できるようにｽｸﾘﾌﾟﾄ・ｽｺｰﾌﾟを設定する
		/// </summary>
		void SetScriptScope()
		{
			//MainWindow ｸﾗｽをｽｺｰﾌﾟへ追加
			pythonConsole.Pad.Console.ScriptScope.SetVariable("Rowthon", this);
			pythonConsole.Pad.Console.ScriptScope.SetVariable("setting", VM.Setting);
			pythonConsole.Pad.Console.ScriptScope.SetVariable("Vm", _viewModel);
			pythonConsole.Pad.Console.ScriptScope.SetVariable("Io", VM.IoControl);
			pythonConsole.Pad.Console.ScriptScope.SetVariable("Sv", VM.SvControl);
			pythonConsole.Pad.Console.ScriptScope.SetVariable("Vision", VM.VisionControl);

			//import したﾓｼﾞｭｰﾙの中で ｽｺｰﾌﾟを使用するために
			pythonConsole.Pad.Console.ScriptScope.Engine.GetBuiltinModule().SetVariable("Rowthon", this);
			pythonConsole.Pad.Console.ScriptScope.Engine.GetBuiltinModule().SetVariable("setting", VM.Setting);
			pythonConsole.Pad.Console.ScriptScope.Engine.GetBuiltinModule().SetVariable("Vm", _viewModel);
			pythonConsole.Pad.Console.ScriptScope.Engine.GetBuiltinModule().SetVariable("Io", VM.IoControl);
			pythonConsole.Pad.Console.ScriptScope.Engine.GetBuiltinModule().SetVariable("Sv", VM.SvControl);
			pythonConsole.Pad.Console.ScriptScope.Engine.GetBuiltinModule().SetVariable("Vision", VM.VisionControl);

			//	double[] test = new double[] { 1.2, 4.6 };
			//	pythonConsole.Pad.Console.ScriptScope.SetVariable("test", test);
			// ｽｸﾘﾌﾟﾄでは、test[0] == 1.2 として使用できる
		}
		void Host_ConsoleCreated(object sender, EventArgs e)
		{
			pythonConsole.Pad.Console.ConsoleInitialized += new PythonConsoleControl.ConsoleInitializedEventHandler(Console_ConsoleInitialized);
		}
		/// <summary>
		///  Pythonｺﾝｿｰﾙの初期化時にｽﾀｰﾄｱｯﾌﾟとして実行するｽｸﾘﾌﾟﾄ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Console_ConsoleInitialized(object sender, EventArgs e)
		{
			ICollection<string> paths = new List<string>();

			//参照ﾊﾟｽを設定
			string programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			string programFilesFolder86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

			//想定では programFiles = C:\Program Files (x86)
			//32bit Windowsでは C:\Program Files
			//64bit 環境に IronPython をｲﾝｽﾄｰﾙすると (x86) にｲﾝｽﾄｰﾙされる

			//IronPython のﾗｲﾌﾞﾗﾘをｻｰﾁﾊﾟｽへ追加
			if ( System.IO.File.Exists(programFilesFolder) ) {
				paths.Add(programFilesFolder + @"\IronPython 2.7");
				paths.Add(programFilesFolder + @"\IronPython 2.7\Lib");
				paths.Add(programFilesFolder + @"\IronPython 2.7\DLLs");
				paths.Add(programFilesFolder + @"\IronPython 2.7\Lib\site-packages");
			}
			else {
				paths.Add(programFilesFolder86 + @"\IronPython 2.7");
				paths.Add(programFilesFolder86 + @"\IronPython 2.7\Lib");
				paths.Add(programFilesFolder86 + @"\IronPython 2.7\DLLs");
				paths.Add(programFilesFolder86 + @"\IronPython 2.7\Lib\site-packages");
			}

			paths.Add(System.Environment.CurrentDirectory);				//現在の実行ﾊﾟｽを追加
			paths.Add(System.Environment.CurrentDirectory + @"\Script");
			paths.Add(System.Environment.CurrentDirectory + @"\Command");
			pythonConsole.Pad.Console.ScriptScope.Engine.SetSearchPaths(paths);

			//追加でﾛｰﾄﾞするﾓｼﾞｭｰﾙ
			pythonConsole.Pad.Console.ScriptScope.ImportModule("os");
			pythonConsole.Pad.Console.ScriptScope.ImportModule("clr");
			pythonConsole.Pad.Console.ScriptScope.ImportModule("time");

			//MainWindow ｸﾗｽをｽｺｰﾌﾟへ追加
			SetScriptScope();

			//Rowthon ﾗｲﾌﾞﾗﾘ(ｽﾀｰﾄｱｯﾌﾟｽｸﾘﾌﾟﾄ)
//			string RowthonScript = "import smtCtrl\nfrom System import *";
			// 基本は ini ﾌｧｲﾙに記述してあるｺﾏﾝﾄﾞを実行するが、ini ﾌｧｲﾙのﾊﾞｰｼﾞｮﾝにより
			// 設定がなされていない場合などの対応でﾃﾞﾌｫﾙﾄ状態を設定しておく
			string RowthonScript = 
@"import IronPythonConsole
import debugLib as debug
import moveLib
import ioLib as io
import servoLib as servo
import traceLib
import ctrlLib as ctrl
import visionLib as vision";
			string cmdFile = "";
			foreach( string sc in VM.Setting.Items ) {
				cmdFile += (sc + "\r\n");
			}
			if( cmdFile != "" ) {
				RowthonScript = cmdFile;
			}

			//ｽｸﾘﾌﾟﾄ実行
			try {
				ScriptSource scriptSource = pythonConsole.Pad.Console.ScriptScope.Engine.CreateScriptSourceFromString(RowthonScript, SourceCodeKind.Statements);
				scriptSource.Execute(pythonConsole.Pad.Console.ScriptScope);
			}
			catch ( Exception err ) {
				//ｽｸﾘﾌﾟﾄ実行ｴﾗｰ
				string msg = pythonConsole.Pad.Console.ScriptScope.Engine.GetService<ExceptionOperations>().FormatException(err);
				MessageBox.Show(msg, "実行ｴﾗｰ", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			if( _timer == null ) {
				do {
					Thread.Sleep(100);
				} while( _timer == null );
			}
			_timer.Start();

			//ﾀﾞﾐｰの改行でｺﾝｿｰﾙの「待ち」を解除する
			Dispatcher.Invoke(new Action(() => { pythonConsole.Pad.Console.RunStatements("\n"); }));

			//ｺﾏﾝﾄﾞ受信ｻｰﾊﾞｰ生成
			VM.CreateCmdServer(VM.Setting.CmdPort);

			//Waitｻｰﾊﾞｰ生成
			VM.CreateWaitServer(VM.Setting.WaitPort);
	
			//ｽﾃｰﾀｽ送信ｻｰﾊﾞｰ生成
			VM.CreateStatusServer(VM.Setting.StatusPort);

			VM.CreateUnitEventThread();

			VM.UpdateIo();

			//初期状態を通知
			VM.SendIoMessage(1, MessageCode.SYS_INIT);
		}
	}
}