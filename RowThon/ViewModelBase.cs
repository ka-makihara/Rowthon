using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;            //for INotifyPropertyChanged
using System.Windows.Input;

using System.Linq.Expressions;

namespace RowThon
{
	public static class PropertyChangedEventHandlerExtensions
	{
		/// <summary>
		///  ｲﾍﾞﾝﾄを発行する
		/// </summary>
		/// <typeparam name="TResult">ﾌﾟﾛﾊﾟﾃｨの型</typeparam>
		/// <param name="_this">ｲﾍﾞﾝﾄﾊﾝﾄﾞﾗ</param>
		/// <param name="propertyName">ﾌﾟﾛﾊﾟﾃｨ名を表す Expression ()=> Name のように指定する</param>
		public static void Raise<TResult>(this PropertyChangedEventHandler _this, Expression<Func<TResult>> propertyName)
		{
			//ﾊﾝﾄﾞﾗに何も登録されていない場合は何もしない
			if( _this == null ) return;

			//ﾗﾑﾀﾞ式りBodyを取得する、MemberExpressionでなければ駄目
			var memberEx = propertyName.Body as MemberExpression;
			if( memberEx == null ) throw new ArgumentException();

			//()=>Name の Name部分の左側に暗黙的に存在しているｵﾌﾞｼﾞｪｸﾄを取得する式を取り出す
			var senderExpression = memberEx.Expression as ConstantExpression;
			// ConstantExpressionでなければ駄目
			if( senderExpression == null ) throw new ArgumentException();

			//式を評価してsender用のｲﾝｽﾀﾝｽを作る
			var sender = System.Linq.Expressions.Expression.Lambda(senderExpression).Compile().DynamicInvoke();

			//ｲﾍﾞﾝﾄ発行
			_this(sender, new PropertyChangedEventArgs(memberEx.Member.Name));
		}
	}
	#region ** Class : ViewModelBase
	/// <summary>
	///  ViewModel の基底ｸﾗｽ
	/// </summary>
	public abstract class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
	{
		#region == implement of InotifyPropertyChanged ==

		//INotifyPropertyChanged.PropertyChanged の実装
		public event PropertyChangedEventHandler PropertyChanged;

		//INotifyPropertyChanged.PropertyChanged ｲﾍﾞﾝﾄを発生させる
//		protected virtual void RaisePropertyChanged(string propertyName)
		public virtual void RaisePropertyChanged(string propertyName)
		{
			if(PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion

		#region == implement of IDataErrorInfo ==

		//IDataErrorInfo用のｴﾗｰﾒｯｾｰｼﾞを保持する辞書
		private Dictionary<string, string> _ErrorMessages = new Dictionary<string, string>();

		//IDataErrorInfo.Error の実装
		string IDataErrorInfo.Error
		{
			get { return (_ErrorMessages.Count > 0) ? "Has Error" : null; }
		}

		//IDataErrorInfo.Item の実装
		string IDataErrorInfo.this[string columnName]
		{
			get
			{
				if(_ErrorMessages.ContainsKey(columnName)) {
					return (_ErrorMessages[columnName]);
				}
				else {
					return null;
				}
			}
		}
		//ｴﾗｰﾒｯｾｰｼﾞのｾｯﾄ
		protected void SetError(string propertyName, string errorMessage)
		{
			_ErrorMessages[propertyName] = errorMessage;
		}
		//ｴﾗｰﾒｯｾｰｼﾞのｸﾘｱ
		protected void ClearError(string propertyName)
		{
			if(_ErrorMessages.ContainsKey(propertyName)) {
				_ErrorMessages.Remove(propertyName);
			}
		}
		#endregion

		#region == implement of ICommand Helper ==

		#region ** Class : _DelegateCommand
		// ICommand実装用のﾍﾙﾊﾟｰｸﾗｽ
		private class _DelegateCommand : ICommand
		{
			private Action<object> _Command;		//ｺﾏﾝﾄﾞ本体
			private Func<object, bool> _CanExecute;	//実行可否

			//ｺﾝｽﾄﾗｸﾀ
			public _DelegateCommand(Action<object> command, Func<object, bool> canExecute)
			{
				if(command == null) {
					throw new ArgumentNullException();
				}
				_Command = command;
				_CanExecute = canExecute;
			}

			// ICommand.Executeの実装
			void ICommand.Execute(object parameter)
			{
				_Command(parameter);
			}
			// ICommand.CanExecuteの実装
			bool ICommand.CanExecute(object parameter)
			{
				if(_CanExecute != null) {
					return (_CanExecute(parameter));
				}
				else {
					return true;
				}
			}
			// ICommand.CanExecuteChanged の実装
			event EventHandler ICommand.CanExecuteChanged
			{
				add { CommandManager.RequerySuggested += value; }
				remove { CommandManager.RequerySuggested -= value; }
			}
		}
		#endregion

		//ｺﾏﾝﾄﾞの生成
		protected ICommand CreateCommand(Action<object> command, Func<object, bool> canExecute)
		{
			return (new _DelegateCommand(command, canExecute));
		}
		#endregion
	}
	#endregion
}
