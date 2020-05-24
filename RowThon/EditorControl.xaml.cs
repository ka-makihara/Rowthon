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

namespace RowThon
{
    /// <summary>
    /// EditorControl.xaml の相互作用ロジック
    /// </summary>
    public partial class EditorControl : UserControl
    {
        public EditorControl()
        {
            InitializeComponent();
        }
        /// <summary>
        /// [X]ボタンクリック時の処理
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        public void CloseTab(object source, RoutedEventArgs args)
        {
            TabItem tabItem = args.OriginalSource as TabItem;
            if ( tabItem != null ) {
                System.Windows.Controls.TabControl tabCtrl =
                    tabItem.Parent as System.Windows.Controls.TabControl;
                if ( tabCtrl != null )
                    tabCtrl.Items.Remove(tabItem);
            }
        }
        /// <summary>
        /// ｺﾝﾄﾛｰﾙ生成時にﾊﾝﾄﾞﾗを登録
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            this.AddHandler(CloseTabItem.CloseTabItemEvent, new RoutedEventHandler(this.CloseTab));
        }
    }
}
