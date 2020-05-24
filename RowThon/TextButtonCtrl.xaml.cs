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

using System.ComponentModel;    //for [Browsable]

namespace RowThon
{
    /// <summary>
    /// TextButtonCtrl.xaml の相互作用ロジック
    /// </summary>
    public partial class TextButtonCtrl : UserControl
    {
        public TextButtonCtrl()
        {
            InitializeComponent();
        }

 //       int count = 0;
        private void ribbonButton1_Click(object sender, RoutedEventArgs e)
        {
 //           count++;
 //           ribbonTextBox1.Text = count.ToString();

            OnUpClicked(EventArgs.Empty);
			e.Handled = false;
        }

        private void ribbonButton2_Click(object sender, RoutedEventArgs e)
        {
//            count--;
//            ribbonTextBox1.Text = count.ToString();

			OnDownClicked(EventArgs.Empty);
			
        }
        [Description("+ﾎﾞﾀﾝｲﾍﾞﾝﾄ")]
        public event EventHandler<EventArgs> UpClick;
        [Description("-ﾎﾞﾀﾝｲﾍﾞﾝﾄ")]
        public event EventHandler<EventArgs> DownClick;

        [Browsable(true)]
        [Description("ﾗﾍﾞﾙを設定します")]
        public string Label
        {
            get { return (ribbonTextBox1.Label); }
            set { ribbonTextBox1.Label = value; }
        }
        [Description("ﾃｷｽﾄﾎﾞｯｸｽの値")]
        public int Value
        {
            get { return ( int.Parse(ribbonTextBox1.Text) ); }
            set { ribbonTextBox1.Text = value.ToString(); }
        }
        [Description("ﾃｷｽﾄﾎﾞｯｸｽの値(Double)")]
        public Double DValue
        {
            get { return ( Double.Parse(ribbonTextBox1.Text) ); }
            set {
				string ss = String.Format("{0:f4}",value);
				ribbonTextBox1.Text = value.ToString("f4"); }
        }

        [Browsable(true)]
        protected virtual void OnUpClicked(EventArgs e)
        {
            EventHandler<EventArgs> eventHandler = UpClick;

            if ( eventHandler != null ) {
                eventHandler(this, e);
            }
        }
        [Browsable(true)]
        protected virtual void OnDownClicked(EventArgs e)
        {
            EventHandler<EventArgs> eventHandler = DownClick;

            if ( eventHandler != null ) {
                eventHandler(this, e);
            }
        }
    }
}
