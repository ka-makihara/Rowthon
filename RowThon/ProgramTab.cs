using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Windows.Controls.Ribbon;
using Microsoft.Win32;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;

using IronPython.Hosting;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

using System.Diagnostics;

namespace RowThon
{
    public partial class MainWindow : RibbonWindow
    {
        /// <summary>
        ///  ｴﾃﾞｨﾀの作成
        /// </summary>
        /// <returns></returns>
        private TextEditor CreateNewEditor()
        {
            //ｴﾃﾞｨﾀ生成
            TextEditor tx = new TextEditor();
            tx.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x38, 0x33, 0x33));
            tx.Foreground = new SolidColorBrush(Colors.White);
            tx.ShowLineNumbers = true;
            tx.FontSize = 14;
            tx.FontFamily = new FontFamily("Consolas");

            //ｴﾃﾞｨﾀのｼﾝﾀｯｸｽﾊｲﾗｲﾄを設定(for python)
            IHighlightingDefinition pythonHighlighting;

            using ( Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("RowThon.Python.xshd") ) {
                if ( s == null ) {
                    throw new InvalidOperationException("組み込みリソース(Python.xshd)が読み込めません");
                }
                using ( XmlReader reader = new XmlTextReader(s) ) {
                    pythonHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            //HighlightingManager に登録
            HighlightingManager.Instance.RegisterHighlighting("Python", new string[] { ".cool" }, pythonHighlighting);

            tx.SyntaxHighlighting = pythonHighlighting;

            return (tx);
        }
        /// <summary>
        /// 動作ﾌﾟﾛｸﾞﾗﾑﾌｧｲﾙの選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void progRefBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.FileName = "";
            ofd.DefaultExt = "*.py";
            ofd.Title = "動作プログラムファイル";
            ofd.Filter = "動作プログラムファイル|*.py|全てのファイル(*.*)|*.*";

            if ( ofd.ShowDialog() == true ) {
                progFileName.Text = ofd.FileName;

                FileStream inf = new FileStream(ofd.FileName, FileMode.Open);

                //               textEdit.Load( inf );

                inf.Close();
            }
        }
        /// <summary>
        ///  「動作ﾌﾟﾛｸﾞﾗﾑ」「開く」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgOpenBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.FileName = "";
            ofd.DefaultExt = "*.py";
            ofd.Title = "動作プログラムファイル";
            ofd.Filter = "動作プログラムファイル|*.py|全てのファイル(*.*)|*.*";

            //ﾀﾞｲｱﾛｸﾞを表示
            if ( ofd.ShowDialog() == false ) {
                return;
            }
            //
            progFileName.Text = ofd.FileName;

            //読み込み用のｽﾄﾘｰﾑ生成
            FileStream inf = new FileStream(ofd.FileName, FileMode.Open);

            //ｴﾃﾞｨﾀ生成
            TextEditor tx = CreateNewEditor();

            //ｴﾃﾞｨﾀにﾃﾞｰﾀ設定
            tx.Load(inf);

            inf.Close();

            //新規ﾀﾌﾞの生成
            TabItem tab = new CloseTabItem();
            tab.Content = tx;
            tab.Header = ofd.SafeFileName;
            tab.ToolTip = ofd.FileName;

            //ﾀﾌﾞ追加
            editorControl.EditTabCtrl.Items.Add(tab);

            //追加したﾀﾌﾞを選択状態にする
            int idx = editorControl.EditTabCtrl.Items.Count - 1;
            ((TabItem)editorControl.EditTabCtrl.Items[idx]).IsSelected = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="edit"></param>
        /// <param name="fileName"></param>
        private void saveProgram(TextEditor edit, string fileName)
        {
            Stream s = new FileStream(fileName, FileMode.Create, FileAccess.Write);

            edit.Save(s);

            s.Close();
        }
        /// <summary>
        /// 「動作ﾌﾟﾛｸﾞﾗﾑ」「上書き保存」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            int idx = editorControl.EditTabCtrl.SelectedIndex;

            if ( idx < 0 ) {
                MessageBox.Show("ファイルが作成されていません", "エラー", (MessageBoxButton.OK), MessageBoxImage.Error);
                return;
            }

            TabItem item = editorControl.EditTabCtrl.Items.GetItemAt(idx) as TabItem;

            if ( item == null ) {
                return;
            }
            TextEditor edit = item.Content as TextEditor;
            if ( edit == null ) {
                return;
            }
            if ( item.ToolTip　== null ) {
				//ﾌｧｲﾙ名が「新規動作」の場合は別名で保存する
				ProgSaveOvBtn_Click(sender, e);
            }
            else {
                saveProgram(edit, item.ToolTip.ToString());
            }
        }
        /// <summary>
        /// 「動作ﾌﾟﾛｸﾞﾗﾑ」「別名で保存」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgSaveOvBtn_Click(object sender, RoutedEventArgs e)
        {
            int idx = editorControl.EditTabCtrl.SelectedIndex;

            if ( idx < 0 ) {
                MessageBox.Show("ファイルが作成されていません", "エラー", (MessageBoxButton.OK), MessageBoxImage.Error);
                return;
            }

            TabItem item = editorControl.EditTabCtrl.Items.GetItemAt(idx) as TabItem;

            if ( item == null ) {
                return;
            }
            TextEditor edit = item.Content as TextEditor;
            if ( edit == null ) {
                return;
            }

            //ﾌｧｲﾙの保存ﾊﾟｽ
            string path;
            if ( item.ToolTip == null ) {
                //新規ﾌｧｲﾙの場合は現在の作業ﾌｫﾙﾀﾞ
                path = System.Environment.CurrentDirectory;
            }
            else {
                //ファイルのﾌﾙﾊﾟｽはﾂｰﾙﾁｯﾌﾟに設定
                path = System.IO.Path.GetDirectoryName(item.ToolTip.ToString());
            }

            //ﾌｧｲﾙﾀﾞｲｱﾛｸ
            SaveFileDialog ofd = new SaveFileDialog();

            ofd.FileName = "新規動作.py"; //ﾃﾞﾌｫﾙﾄのﾌｧｲﾙ名
            ofd.InitialDirectory = path;
            ofd.DefaultExt = "*.py";
            ofd.Title = "動作プログラムファイル";
            ofd.Filter = "動作プログラムファイル|*.py|全てのファイル(*.*)|*.*";
            ofd.OverwritePrompt = true;     //上書き確認
            ofd.RestoreDirectory = true;    //ﾃﾞｨﾚｸﾄﾘ保持
            ofd.CheckPathExists = true;     //途中ﾊﾟｽの存在確認

            //ﾀﾞｲｱﾛｸﾞを表示
            if ( ofd.ShowDialog() == false ) {
                return;
            }
			item.ToolTip = ofd.FileName;
			item.Header = ofd.SafeFileName;
            saveProgram(edit, ofd.FileName);
        }
        /// <summary>
        ///  「動作ﾌﾟﾛｸﾞﾗﾑ」「新規作成」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgCreateBtn_Click(object sender, RoutedEventArgs e)
        {
            //ｴﾃﾞｨﾀ生成
            TextEditor tx = CreateNewEditor();

            //新規ﾀﾌﾞの生成
            TabItem tab = new CloseTabItem();
            tab.Content = tx;
            tab.Header = "新規動作.py";

            //ﾀﾌﾞ追加
            editorControl.EditTabCtrl.Items.Add(tab);

            //追加したﾀﾌﾞを選択状態にする
            int idx = editorControl.EditTabCtrl.Items.Count - 1;
            ((TabItem)editorControl.EditTabCtrl.Items[idx]).IsSelected = true;
        }

		/// <summary>
		///  ｽｸﾘﾌﾟﾄ・ｽﾚｯﾄﾞ
		/// </summary>
		/// <param name="argObj"></param>
		public void ScriptThread(Object argObj)
		{
			ThreadArgs args = (ThreadArgs)argObj;

			try {
				ScriptSource src = pythonConsole.Pad.Console.ScriptScope.Engine.CreateScriptSourceFromString(args.cmd);
				Object obj = src.Execute(pythonConsole.Pad.Console.ScriptScope);
			}
			catch( Exception err ) {
				//ｽｸﾘﾌﾟﾄの実行で例外発生
				// (ｽﾚｯﾄﾞのAbortでも例外となる)
				string errMsg = pythonConsole.Pad.Console.ScriptScope.Engine.GetService<ExceptionOperations>().FormatException(err);
//				MessageBox.Show(errMsg, "err", MessageBoxButton.OK, MessageBoxImage.Error);

				//ｺﾝｿｰﾙにﾒｯｾｰｼﾞを表示する
				pythonConsole.Pad.Console.WriteLine(errMsg, Microsoft.Scripting.Hosting.Shell.Style.Out);

				args._event.Cancel(-2);
				return;
			}
			args._event.Set(0);
		}
		Thread _waitThread;
		/// <summary>
		/// ｽｸﾘﾌﾟﾄ実行完了待ちｽﾚｯﾄﾞ
		/// </summary>
		/// <param name="argObj"></param>
		public void EventThread(Object argObj)
		{
			ThreadArgs args = (ThreadArgs)argObj;

			int[] hdl = new int[1] { args._event._number };

			int ret;
			if( args._timeOut == 0 ) {
				ret = VM.Wait(hdl, Timeout.Infinite);
			}
			else {
				ret = VM.Wait(hdl, args._timeOut);
			}
			
			if( ret == WaitHandle.WaitTimeout ) {
				if( _progThread != null ) {
					_progThread.Abort();
				}
				//MessageBox.Show("指定時間内に指定ﾌﾟﾛｸﾞﾗﾑが完了しませんでした", "ｴﾗｰ", (MessageBoxButton.OK), (MessageBoxImage.Error));
				pythonConsole.Pad.Console.WriteLine("指定時間内に指定ﾌﾟﾛｸﾞﾗﾑが完了しませんでした", Microsoft.Scripting.Hosting.Shell.Style.Out);
				_progThread = null;
			}
			else {
				if( args._event.Status() == 3 ) {
//					MessageBox.Show("実行が中断されました", "中断", (MessageBoxButton.OK), (MessageBoxImage.Stop));
				}
			}

			Dispatcher.Invoke(new Action(() =>
				{
					//「待ち」解除
					pythonConsole.Pad.Console.RunStatements("\n");
					//ﾎﾞﾀﾝを非選択
					ProgExecBtn.IsChecked = false;
				}));
		}
		Thread _progThread;
		/// <summary>
		/// ｽｸﾘﾌﾟﾄ実行ｽﾚｯﾄﾞ
		/// </summary>
		/// <param name="source"></param>
		public void execute_script(string source)
		{
			PEventManager.PEvent ev = VM.GetEvent();

			ThreadArgs args = new ThreadArgs();
			args.vm = VM;
			args.cmd = source;
			args._event = ev;
			args._timeOut = Int32.Parse(ProgTimeOut.Text);

			//ｽｸﾘﾌﾟﾄ実行待ちのｽﾚｯﾄﾞ
			_waitThread = new Thread(new ParameterizedThreadStart(EventThread));

			//ｽｸﾘﾌﾟﾄ・ｽﾚｯﾄﾞ
			_progThread = new Thread(new ParameterizedThreadStart(ScriptThread));

			_waitThread.Start(args);
			_progThread.Start(args);
		}
		
        /// <summary>
        ///  「動作ﾌﾟﾛｸﾞﾗﾑ」・「選択ﾌｧｲﾙを実行」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgExecBtn_Click(object sender, RoutedEventArgs e)
        {
            int idx = editorControl.EditTabCtrl.SelectedIndex;
			TextEditor edit = null;

			try {
				if( idx < 0 ) {
					throw new ArgumentException("ファイル・プログラムが指定されていません");
				}
				TabItem item = editorControl.EditTabCtrl.Items.GetItemAt(idx) as TabItem;

				if( item == null ) {
					throw new ArgumentException("実行ﾌﾟﾛｸﾞﾗﾑ未設定");
				}
				edit = item.Content as TextEditor;
				if( edit == null ) {
					throw new ArgumentException("実行ﾌﾟﾛｸﾞﾗﾑ未設定");
				}
			}
			catch(Exception err){
				MessageBox.Show(err.Message,"エラー", (MessageBoxButton.OK), MessageBoxImage.Error);
				Dispatcher.Invoke(new Action(() => { ProgExecBtn.IsChecked = false; }));
				return;
			}
			//ｽｸﾘﾌﾟﾄｽﾚｯﾄﾞを起動
			execute_script(edit.Text);
        }
		
        /// <summary>
        /// 「動作ﾌﾟﾛｸﾞﾗﾑ」・「実行中ﾌﾟﾛｸﾞﾗﾑの停止」
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgStopBtn_Click(object sender, RoutedEventArgs e)
        {
			if( _progThread != null ) {
				VM.SvControl.StopCommand();
				_progThread.Abort();
			}
        }
    }
}
