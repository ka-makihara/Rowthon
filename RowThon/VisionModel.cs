using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using System.Runtime.InteropServices;   //for DllImport
using System.Windows.Media;
using System.Windows.Media.Imaging;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;

namespace RowThon
{
	#region ** Class : VisionModel
	public class VisionModel
	{
		private WriteableBitmap _wb = new WriteableBitmap(640, 640, 96, 96, PixelFormats.Gray8, null);
		private WriteableBitmap _col_wb = new WriteableBitmap(640, 640, 96, 96, PixelFormats.Rgb24, null);
		public List<IplImage> _tpl_img = null;

		//画像処理用配列ﾃﾞｰﾀ
		private Mat _mat = null;// = new Mat(640, 640, MatType.CV_8UC1);		//ｸﾞﾚｰｽｹｰﾙ用
		private Mat _col_mat = null;// = new Mat(640, 640, MatType.CV_8UC3);	//ｶﾗｰ画像用

		unsafe delegate void grab_callback(int arg);
		private grab_callback _grab_proc = null;//画像取り込み完了ｺｰﾙﾊﾞｯｸ(delegate)

		private GrabEventHandler _grabEventHandler;
		private AutoResetEvent _grabEvent = new AutoResetEvent(false);

		private bool _initialized = false;
		private bool _grabStatus = false;

		private CvRect _searchArea;
		private IList<int> _visionPos = new int[5];	//画像処理結果

		private static visionInterface.VisionInterface _visionInterface = new visionInterface.VisionInterface();

		/// <summary>
		///  必要なDLLの取り込み設定
		/// </summary>
		/// <param name="proc"></param>
		/// <returns></returns>
		[DllImport("CameraLib.dll")]
		static extern int init_camera(grab_callback proc);
		[DllImport("CameraLib.dll")]
		static extern int grab_camera();
		[DllImport("CameraLib.dll")]
		static extern int grab_stop();
		[DllImport("CameraLib.dll")]
		static extern int close_camera();
		[DllImport("CameraLib.dll")]
		static extern int snap_camera();
		[DllImport("CameraLib.dll")]
		static extern int set_triggerMode(int mode);
		[DllImport("CameraLib.dll")]
		static extern int camera_trigger();
		[DllImport("CameraLib.dll")]
		static extern int set_soft_trigger();
		[DllImport("CameraLib.dll")]
		static extern int set_normal_trigger();

		unsafe public void grab_callback_func(int arg)
		{
			if( _mat == null ) {
				_mat = new Mat(640, 640, MatType.CV_8UC1);		//ｸﾞﾚｰｽｹｰﾙ用
				_col_mat = new Mat(640, 640, MatType.CV_8UC3);	//ｶﾗｰ画像用
			}

			//DLLで取得されたｸﾞﾚｰｽｹｰﾙﾃﾞｰﾀを画像処理用のﾃﾞｰﾀ領域へｺﾋﾟｰ
			byte* pp = (byte*)arg;
			byte* p = _mat.DataPointer;
			for( int yy = 0; yy < 640; yy++ ) {
				for( int xx = 0; xx < 640; xx++ ) {
					p[yy * 640 + xx] = *(pp + (yy * 640 + xx));
				}
			}
			_grabEventHandler(this, new DrawCameraViewEventArgs(0,0,0,0,0));

			_grabEvent.Set();
		}
		unsafe public byte GetPixel(int offset)
		{
			byte* p = _mat.DataPointer;
			return (p[offset]);
		}
		/// <summary>
		///  visionModel ｺﾝｽﾄﾗｸﾀ
		/// </summary>
		public VisionModel()
		{
//ｺﾝｽﾄﾗｸﾀ内でOpenCv のDLLを参照しに行く?のでXAMLの記述で例外が発生
//  (実行時のﾊﾟｽとｴﾃﾞｨﾀ時のﾊﾟｽが異なるためOpenCvのDLLを見つけられない)
//XAMLｴﾃﾞｨﾀで正常に表示されなくなる。(実行時は問題ない)

//			_mat = new Mat(640, 640, MatType.CV_8UC1);		//ｸﾞﾚｰｽｹｰﾙ用
//			_col_mat = new Mat(640, 640, MatType.CV_8UC3);	//ｶﾗｰ画像用

			//ﾃﾝﾌﾟﾚｰﾄ画像の準備
			_tpl_img = new List<IplImage>();

			string path = System.Environment.CurrentDirectory + "\\Cv_template";
			string[] dirs = Directory.GetFiles(path, "*.png");

			int idx = 0;
			foreach( string dir in dirs ) {
				_tpl_img.Add(new IplImage()); _tpl_img[idx++] = Cv.LoadImage(dir, LoadMode.GrayScale);
			}
		}

		public bool GrabStatus()
		{
			if( _initialized == false ) {
				return (false);
			}
			return (_grabStatus);
		}
		/// <summary>
		///  画像取り込み
		/// </summary>
		/// <param name="m">開始=1 停止=0</param>
		/// <returns></returns>
		public int GrabCamera(int m)
		{
			int ret;

			if( _initialized == false ) {
				return ( (int)ErrCode.ERR_EXEC);
			}
			_grabEvent.Reset();
			switch( m ){
			case -1: _grabStatus = true;	return( grab_camera() );
			case  0: _grabStatus = false;	return( grab_stop()   );
			case  1: _grabStatus = false;
				if( (ret = snap_camera()) < 0 ){
					return( ret );
				}
				//取り込み完了待ち
				WaitHandle[] hdls = new WaitHandle[]{ _grabEvent };
				if(WaitHandle.WaitAny(hdls, 1000) == WaitHandle.WaitTimeout) {
					return ((int)ErrCode.ERR_EXEC);
				}

				return( ret );
			}
			return ( (int)ErrCode.ERR_PARAM);
		}

		/// <summary>
		///  取り込んだ画像をRGBﾋﾞｯﾄﾏｯﾌﾟで取得する
		/// </summary>
		/// <returns></returns>
		public WriteableBitmap GetCameraBitmap()
		{
			//RGB画像へ変換
			Cv2.CvtColor(_mat, _col_mat, ColorConversion.GrayToRgb);

			IplImage img = _col_mat.ToIplImage();

			WriteableBitmapConverter.ToWriteableBitmap(img, _col_wb);

			img.Dispose();

			return (_col_wb);
		}
		/// <summary>
		/// OpenCv::IplImage 形式のｶﾒﾗ画像ﾃﾞｰﾀ(RGB)形式
		/// </summary>
		/// <returns>IplImage (640×640:RGB)</returns>
		public IplImage GetCameraImage()
		{
			if( _mat == null ) {
				_mat = new Mat(640, 640, MatType.CV_8UC1);		//ｸﾞﾚｰｽｹｰﾙ用
				_col_mat = new Mat(640, 640, MatType.CV_8UC3);	//ｶﾗｰ画像用
			}
			//RGB画像へ変換
			Cv2.CvtColor(_mat, _col_mat, ColorConversion.GrayToRgb);

			return( _col_mat.ToIplImage() );
		}

		/// <summary>
		///  ｶﾒﾗ初期化
		/// </summary>
		/// <param name="eventHandler">取り込み完了時に呼び出すﾊﾝﾄﾞﾗ</param>
		/// <returns></returns>
		public int InitCamera(GrabEventHandler eventHandler)
		{
			int ret;

			_grabEventHandler = eventHandler;

			// ｶﾒﾗﾓｼﾞｭｰﾙからの取り込み完了通知用ｺｰﾙﾊﾞｯｸ生成
			if( _grab_proc == null ) {
				_grab_proc = new grab_callback(grab_callback_func);
			}
			if( (ret = init_camera(_grab_proc)) < 0 ){
				return (ret);
			}
			_initialized = true;
			return (0);
		}

		/// <summary>
		///  ｶﾒﾗ画像用のｲﾒｰｼﾞ領域にﾃﾞｰﾀ描画
		/// </summary>
		/// <param name="x">中心座標</param>
		/// <param name="y">中心座標</param>
		/// <param name="tempNo"></param>
		/// <returns></returns>
		public int DrawCenterRect(int x, int y, int tempNo)
		{
			if(_tpl_img.Count < tempNo) {
				return ((int)ErrCode.ERR_PARAM);
			}
			IplImage template_img = _tpl_img[tempNo];

			//左上座標は中心座標に対してﾃﾝﾌﾟﾚｰﾄｻｲｽﾞ/2分ｵﾌｾｯﾄ
			int xx = x - template_img.Width / 2;
			int yy = y - template_img.Height / 2;

			_grabEventHandler(this, new DrawCameraViewEventArgs(1, xx, yy, template_img.Width, template_img.Height));

			return ((int)ErrCode.RET_OK);
		}

		public int DrawCenter()
		{
			_grabEventHandler(this, new DrawCameraViewEventArgs(2, 320, 320, 640, 640));
			return (0);
		}

		/// <summary>
		/// OpenCv画像処理
		/// </summary>
		/// <param name="src">画像ﾃﾞｰﾀ</param>
		/// <param name="tpl">ﾃﾝﾌﾟﾚｰﾄﾃﾞｰﾀ</param>
		public void Cv_Execute(Mat src, IplImage tpl)
		{
			CvPoint minPoint = new CvPoint();
			CvPoint maxPoint = new CvPoint();
			double min_val = 0.0, max_val = 0.0;

			IplImage img = src.ToIplImage();
			//ﾊﾟﾀｰﾝﾏｯﾁﾝｸﾞ
			CvMat result = new CvMat(img.Height - tpl.Height + 1, img.Width - tpl.Width + 1, MatrixType.F32C1);
			Cv.MatchTemplate(img, tpl, result, MatchTemplateMethod.CCoeffNormed);

			//結果取得
			Cv.MinMaxLoc(result, out min_val, out max_val, out minPoint, out maxPoint);

			img.Dispose();

			//結果代入
			_visionPos[0] = maxPoint.X + (tpl.Width / 2);
			_visionPos[1] = maxPoint.Y + (tpl.Height / 2);
			_visionPos[2] = 0;
			_visionPos[3] = 0;
			_visionPos[4] = (int)(max_val * 100.0);
		}
		/// <summary>
		/// 画像処理
		///   戻り座標は、ﾋﾟｸｾﾙ座標
		/// </summary>
		/// <param name="tempNo">ﾃﾝﾌﾟﾚｰﾄ番号</param>
		/// <returns></returns>
		public int Execute(int tempNo)
		{
			if( _mat == null ) {
				_mat = new Mat(640, 640, MatType.CV_8UC1);		//ｸﾞﾚｰｽｹｰﾙ用
				_col_mat = new Mat(640, 640, MatType.CV_8UC3);	//ｶﾗｰ画像用
			}
			if( _tpl_img.Count < tempNo ) {
				//ﾃﾝﾌﾟﾚｰﾄ番号ｴﾗｰ
				return ((int)ErrCode.ERR_PARAM);
			}

			//OpenCv ﾊﾟﾀｰﾝﾏｯﾁﾝｸﾞ
			Cv_Execute(_mat, _tpl_img[tempNo]);

			return (0);
		}
		/// <summary>
		///  画像処理結果取得
		/// </summary>
		/// <param name="posList"></param>
		/// <returns></returns>
		public int GetVisionPos(IList<int> posList)
		{
			try {
				for(int i = 0; i < 5; i++) {
					posList[i] = _visionPos[i];
				}
			}
			catch(Exception err) {
				return ((int)ErrCode.ERR_PARAM);
			}
			return (0);
		}
		/// <summary>
		///  画像処理・ｻｰﾁｴﾘｱ設定
		/// </summary>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="x2"></param>
		/// <param name="y2"></param>
		/// <returns></returns>
		public int SetSearchArea(int x1, int y1, int x2, int y2)
		{
			_searchArea.X = x1;
			_searchArea.Y = y1;
			_searchArea.Width = Math.Abs(x2 - x1);
			_searchArea.Height = Math.Abs(y2 - y1);

			return (0);
		}
		public int wnd_proc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
		{
			_visionInterface.GetWinMsgData(hwnd, msg, wParam.ToInt32(), lParam.ToInt32());

			return (0);
		}
		public IList<int> GetMissionRet()
		{
			IList<int> data = new int[10];

			_visionInterface.GetMissionRet(data);

			return (data);
		}
	}
	#endregion
}
