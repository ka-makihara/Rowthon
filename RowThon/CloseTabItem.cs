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
    /// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
    ///
    /// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:RowThon"
    ///
    ///
    /// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
    /// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
    /// 追加します:
    ///
    ///     xmlns:MyNamespace="clr-namespace:RowThon;assembly=RowThon"
    ///
    /// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
    /// リビルドして、コンパイル エラーを防ぐ必要があります:
    ///
    ///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
    ///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
    ///
    ///
    /// 手順 2)
    /// コントロールを XAML ファイルで使用します。
    ///
    ///     <MyNamespace:CloseTabItem/>
    ///
    /// </summary>
    public class CloseTabItem : TabItem
    {
        static CloseTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CloseTabItem), new FrameworkPropertyMetadata(typeof(CloseTabItem)));
        }
        ///ﾙｰﾃｨﾝｸﾞｲﾍﾞﾝﾄの登録
        public static readonly RoutedEvent CloseTabItemEvent =
            System.Windows.EventManager.RegisterRoutedEvent("ClosedTab", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CloseTabItem));

        ///CloseTabｲﾍﾞﾝﾄ
        public event RoutedEventHandler ClosedTab
        {
            add { AddHandler(CloseTabItemEvent, value); }
            remove { RemoveHandler(CloseTabItemEvent, value); }
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button closeTabButton = this.GetTemplateChild("PART_Close") as Button;
            if ( closeTabButton != null ) {
                closeTabButton.Click += new System.Windows.RoutedEventHandler(closeButton_Click);
            }
        }

        ///[X]ﾎﾞﾀﾝが押された時の処理
        private void closeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //CloseTabｲﾍﾞﾝﾄを発生させる
            this.RaiseEvent(new RoutedEventArgs(CloseTabItemEvent));
        }
    }
}
