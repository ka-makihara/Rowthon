using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Runtime.InteropServices;

using Microsoft.Scripting.Hosting;

using Microsoft.Windows.Controls.Ribbon;
using IronPython.Hosting;               //for PythonConsole(ImportModule)


namespace RowThon
{		
	/// <summary>
	/// ｽﾚｯﾄﾞへの引数
	/// </summary>
	public class ThreadArgs
	{
		public ViewModel vm;
		public string cmd;
		public PEventManager.PEvent	_event;
		public int _timeOut;
	}

	public class SockCtrl
	{
		int _port;
		IPEndPoint _enp;
		TcpListener _listener;
		TcpClient _client;

		public int Init(IPAddress adr, int port)
		{
			_port = port;

			try {
				_enp = new IPEndPoint(adr, port);
				_listener = new TcpListener(_enp);
				_listener.Start();
			}
			catch ( Exception ) {
				return (-1);
			}
			return (0);
		}
		public void Accept()
		{
			_client = _listener.AcceptTcpClient();
		}
		public void Close()
		{
			if ( _client != null ) {
				_client.Close();
			}
		}
		public NetworkStream stream
		{
			get { return (_client.GetStream()); }
		}
		public TcpListener listener
		{
			get { return (_listener); }
		}
		public TcpClient client
		{
			get { return (_client); }
		}
	}
//	public partial class MainWindow : RibbonWindow
	public partial class ViewModel : ViewModelBase
	{
		private IPAddress _serverIP;
		private Thread _cmdThread;

		[ThreadStatic]
		private static SockCtrl _sock;

		private Thread _scriptThread;
		private static UInt32 _threadStop = 0;
		private static UInt32 _threadRestart = 0;

		public MessageData CreateMsg(MessageCode code, object obj)
		{
			MessageData msg = new MessageData(0);

			msg._code = code;
			msg._ioInput = IoInputValue;
			msg._ioOutput = IoOutputValue;
			msg._retCode = (obj == null) ? 0 : Int32.Parse(obj.ToString());

			return (msg);
		}
		/// <summary>
		///  ｺﾏﾝﾄﾞ受信ｽﾚｯﾄﾞ(生成部)
		/// </summary>
		/// <param name="port">ﾎﾟｰﾄ番号(ｺﾏﾝﾄﾞは10010を使用)</param>
		public void CreateCmdServer(int port)
		{
			Object[] argObj = new object[2];
			argObj[0] = port;

			_cmdThread = new Thread(new ParameterizedThreadStart(CommandThread));
			_cmdThread.Start(argObj);
		}
		/// <summary>
		///  wait 用の専用ｽﾚｯﾄﾞ
		/// </summary>
		private Thread _waitThread;

		public void CreateWaitServer(int port)
		{
			Object[] argObj = new object[2];
			argObj[0] = port;

			_waitThread = new Thread(new ParameterizedThreadStart(CommandThread));
			_waitThread.Start(argObj);
		}

		/// <summary>
		///  ｽｸﾘﾌﾟﾄ・ｽﾚｯﾄﾞ
		/// </summary>
		/// <param name="argObj"></param>
		public static void ScriptThread(Object argObj)
		{
			ThreadArgs args = (ThreadArgs)argObj;

			Trace.TraceFormat("Script Start:ID={0}", args._event._number);
			try {
				ScriptSource src = _pythonConsole.Pad.Console.ScriptScope.Engine.CreateScriptSourceFromString(args.cmd);
				Object obj = src.Execute(_pythonConsole.Pad.Console.ScriptScope);

				//この場合、例外はｺﾝｿｰﾙ内で取得、表示される。また、実行開始が非常に遅い(?)
				//ｽｸﾘﾌﾟﾄの実行ｴﾗｰをｲﾍﾞﾝﾄとして通知する必要があるので、この方法は不可とする
//				args.vm._ExecuteScript(args.vm, new ScriptEventArgs(args.cmd));
			}
			catch (Exception err) {
				//ｽｸﾘﾌﾟﾄの実行で例外発生
				string errMsg = _pythonConsole.Pad.Console.ScriptScope.Engine.GetService<ExceptionOperations>().FormatException(err);
				Trace.TraceFormat("[ERR]Script:{0}", errMsg);

//				MessageBox.Show(errMsg, "err", MessageBoxButton.OK, MessageBoxImage.Error);
				_pythonConsole.Pad.Console.WriteLine(errMsg, Microsoft.Scripting.Hosting.Shell.Style.Out);

				args._event.Cancel(-2);
				args.vm._scriptThread = null;
				return;
			}
			finally{
				//例外が発生した場合でも、return 後に finally が実行される

				//Script内で printなどを行っていると、ｺﾝｿｰﾙが「待ち」になってしまうので
				//ﾀﾞﾐｰの改行でｺﾝｿｰﾙの「待ち」を解除する
				args.vm._ExecuteScript(args.vm, new ScriptEventArgs("\n"));
			}
			args._event.Set(0);
			args.vm._scriptThread = null;
			Trace.TraceFormat("Script Complete");
		}
		/// <summary>
		///  ｺﾏﾝﾄﾞ受信ｽﾚｯﾄﾞ
		/// </summary>
		unsafe private void CommandThread(Object argObj)
		{
			//引数の展開
			object[] argsTmp = (object[])argObj;

			byte[] buffer = new byte[1024];

			//ｿｹｯﾄの生成
			_sock = new SockCtrl();
			_sock.Init(_serverIP,(int)argsTmp[0]);

			while( _sock.listener == null ) {
				Thread.Sleep(10);
			}
			try {
				_sock.listener.Pending();
			}
			catch( Exception err ) {
				_sock.listener.Start();
			}

			while ( true ) {
				if ( _sock.listener.Pending() == true ) {
					_sock.Accept();

					ConnectionMsg = "接続中";
					Trace.TraceFormat("Socket Connected");

					int len = 0;
					NetworkStream stream = _sock.stream;
					MemoryStream ms = new MemoryStream();

					int cmdMode = 0;
					bool loop = true;
					while ( loop ) {
						//ｺﾏﾝﾄﾞ文字列受信
						do {
							try {
								len = stream.Read(buffer, 0, 1);

								if ( len == 0 ) {
									//ｸﾗｲｱﾝﾄ側から切断された
									loop = false;
									ms.Position = 0;
									ms.Dispose();
									break;
								}
								ms.Write(buffer, 0, len);
							}
							catch ( Exception err ) {
								//Read中に切断された
								//MessageBox.Show("CMD::" + err.Message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
								WriteConsole(err.Message);
								loop = false;
								ms.Flush();
								ms.Position = 0;
								ms.Dispose();
								break;
							}
							//ｺﾏﾝﾄﾞの最終文字は';' または'@'とする
							if( buffer[0] == ';' ){
								cmdMode = 1;
								break;
							}
							if( buffer[0] == '@' ){
								cmdMode = 2;
								break;
							}
							if( buffer[0] == '?' ) {
								cmdMode = 3;
								break;
							}
						}while ( stream.DataAvailable );

						//ｺﾏﾝﾄﾞを受信した
						if ( loop == true && (int)ms.Position != 0 ) {
							Encoding enc = Encoding.UTF8;

							string cmd = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length - 1);

							ms.Flush();
							ms.Position = 0;
							ms.Dispose();

							//ｺﾏﾝﾄﾞｽｸﾘﾌﾟﾄを実行する
							if(cmdMode == 1) {
								MessageData msg;
								try {
									ScriptSource src = _pythonConsole.Pad.Console.ScriptScope.Engine.CreateScriptSourceFromString(cmd);
									Object obj = src.Execute(_pythonConsole.Pad.Console.ScriptScope);

									msg = CreateMsg(MessageCode.SYS_CMD_RETOK, obj);
								}
								catch(Exception err) {
									//ｽｸﾘﾌﾟﾄの実行で例外発生
									string errMsg = _pythonConsole.Pad.Console.ScriptScope.Engine.GetService<ExceptionOperations>().FormatException(err);
									Trace.TraceFormat("[ERR]Script:{0}", errMsg);

									//MessageBox.Show(errMsg, "err", MessageBoxButton.OK, MessageBoxImage.Error);
									_pythonConsole.Pad.Console.WriteLine(errMsg, Microsoft.Scripting.Hosting.Shell.Style.Out);

									msg = CreateMsg(MessageCode.SYS_CMD_RETNG, (int)ErrCode.ERR_EXEC);
								}
								try {
									//結果を送信する
									stream.Write(msg.Binary(), 0, Marshal.SizeOf(msg));
								}
								catch(Exception err) {
									//MessageBox.Show("CMD_Write::" + err.Message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
									WriteConsole(err.Message);
									loop = false;
								}
							}
							else if( cmdMode == 3 ) {
								//ｽｸﾘﾌﾟﾄﾌｧｲﾙで実行
								MessageData msg;
								try {
									ScriptSource src = _pythonConsole.Pad.Console.ScriptScope.Engine.CreateScriptSourceFromFile(cmd);
									Object obj = src.Execute(_pythonConsole.Pad.Console.ScriptScope);

									msg = CreateMsg(MessageCode.SYS_CMD_RETOK, obj);
								}
								catch( Exception err ) {
									//ｽｸﾘﾌﾟﾄの実行で例外発生
									string errMsg = _pythonConsole.Pad.Console.ScriptScope.Engine.GetService<ExceptionOperations>().FormatException(err);
									Trace.TraceFormat("[ERR]Script:{0}", errMsg);

//									MessageBox.Show(errMsg, "err", MessageBoxButton.OK, MessageBoxImage.Error);

									msg = CreateMsg(MessageCode.SYS_CMD_RETNG, (int)ErrCode.ERR_EXEC);
								}
								try {
									//結果を送信する
									stream.Write(msg.Binary(), 0, Marshal.SizeOf(msg));
								}
								catch( Exception err ) {
									Trace.TraceFormat("[ERR]Socket Write:{0}", err.Message);
									//MessageBox.Show("CMD_Write::" + err.Message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
									WriteConsole(err.Message);
									loop = false;
								}
								_ExecuteScript(this, new ScriptEventArgs("\n"));
							}
							else {
								// 別ｽﾚｯﾄﾞでのｺﾏﾝﾄﾞ実行
								bool threadRun = true;
								if( _scriptThread != null ) {
									if( _scriptThread.ThreadState != ThreadState.Stopped ) {
										//前回のｽｸﾘﾌﾟﾄｽﾚｯﾄﾞが動作中
										Trace.TraceFormat("Script already Run");
										MessageData msg = CreateMsg(MessageCode.SYS_CMD_RETNG, (int)ErrCode.ERR_EXEC);
										try {
											//結果を送信する
											stream.Write(msg.Binary(), 0, Marshal.SizeOf(msg));
										}
										catch( Exception err ) {
											Trace.TraceFormat("[ERR]Socket Write:{0}", err.Message);
											//MessageBox.Show("CMD_Write::" + err.Message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
											WriteConsole(err.Message);
											loop = false;
											threadRun = false;
										}
										threadRun = false;
									}
								}
								if( threadRun == true ) {
									//ｽｸﾘﾌﾟﾄ実行のｲﾍﾞﾝﾄ
									PEventManager.PEvent evObj = GetEvent();

									ThreadArgs args = new ThreadArgs();
									args.vm = this;
									args.cmd = cmd;
									args._event = evObj;

									_scriptThread = new Thread(new ParameterizedThreadStart(ScriptThread));
									_scriptThread.Start(args);

									Trace.TraceFormat("Script thread started");

									//execute_program() のﾘﾀｰﾝ用ﾃﾞｰﾀ
									MessageData msg = CreateMsg(MessageCode.SYS_CMD_RETOK, evObj._number);
									try {
										stream.Write(msg.Binary(), 0, Marshal.SizeOf(msg));
									}
									catch( Exception err ) {
										Trace.TraceFormat("[ERR]Socket:{0}", err.Message);
										//MessageBox.Show("SCR_Write::" + err.Message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
										WriteConsole(err.Message);
										loop = false;
									}
								}
							}
							ms = new MemoryStream();
						}
					}
					_sock.Close();
					ConnectionMsg = "未接続";
					_threadRestart = 1;
					Trace.TraceFormat("Socket DisConnected");
				}
				else {
					Thread.Sleep(500);
					if ( _threadStop != 0 ) {
						break;
					}
				}
			}
			_sock.Close();
			ConnectionMsg = "未接続";
			_threadRestart = 1;
			Trace.TraceFormat("Socket DisConnected");
		}

		private static SockCtrl _statusSock;
		private Thread _statusThread;
		private AutoResetEvent _statSendEvent;
		/// <summary>
		/// 状態通知(送信)用のｻｰﾊﾞｰ(生成部)
		/// </summary>
		/// <param name="port">ﾎﾟｰﾄ番号(ｺﾏﾝﾄﾞは10011を使用)</param>
		public void CreateStatusServer(int port)
		{
			_statusSock = new SockCtrl();

			_statusSock.Init(_serverIP, port);

			//送信ﾃﾞｰﾀありを示すｲﾍﾞﾝﾄ
			_statSendEvent = new AutoResetEvent(false);

			//ｸﾗｲｱﾝﾄ接続待ち
			_statusThread = new Thread(new ThreadStart(StatusThread));
			_statusThread.Start();
		}
		/// <summary>
		/// ﾃﾞｰﾀ送信用のｽﾚｯﾄﾞ
		/// </summary>
		private void StatusThread()
		{
			while( _statusSock.listener == null ) {
				Thread.Sleep(10);
			}
			try {
				_statusSock.listener.Pending();
			}
			catch( Exception err ) {
				_statusSock.listener.Start();
			}
			while ( true ) {
				if ( _statusSock.listener.Pending() == true ) {
					//接続要求がある場合
					_statusSock.Accept();

					Trace.TraceFormat("Status Server Connected");

					if( _threadRestart == 1 ) {
						_threadRestart = 0;
					}
					//I/O状態の通知
					SendIoMessage(0, MessageCode.SYS_IO_UPDATE);

					NetworkStream stream = _statusSock.stream;

					//送信ﾒｯｾｰｼﾞがﾎﾟｽﾄされるのを待つ
					bool loop = true;
					while ( loop ) {
						if ( _postSys.IsPosted(100) != -1 ) {

							lock ( _postSys.SyncRoot ) {
								int cnt = _postSys.Count;
								//ﾃﾞｰﾀ送信
								for ( int i = 0; i < cnt; i++ ) {
									MessageData msg = (MessageData)_postSys.GetData();
									try {
										stream.Write(msg.Binary(), 0, Marshal.SizeOf(msg));
									}
									catch ( Exception err ) {
										//Write中に切断された
										Trace.TraceFormat("Status Server DisConnected:{0}",err.Message);
										//MessageBox.Show("Status Write::"+ err.Message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
										WriteConsole(err.Message);
										loop = false;
										break;
									}
								}
							}
						}
						else {
							if ( _threadStop != 0 ) {
								loop = false;
							}
							if( _threadRestart != 0 ){
								loop = false;
							}
						}
					}
					_statusSock.Close();
					Trace.TraceFormat("Status Server DisConnected");
				}
				else {
					Thread.Sleep(500);
					if ( _threadStop != 0 ) {
						break;
					}
				}
			}
			_statusSock.Close();
			Trace.TraceFormat("Status Server DisConnected");
		}
	}
}
