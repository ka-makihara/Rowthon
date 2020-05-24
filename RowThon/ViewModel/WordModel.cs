using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;

using RowThon.Model;

namespace RowThon
{
	//
	// static ｸﾗｽにすると、XAML での Windows.Resouce でｺﾝｽﾄﾗｸﾀが呼び出されて、ｴﾗｰとなるので
	// とりあえず、static を外す。
	//  ViewModel 側からWordModelﾘｿｰｽにｱｸｾｽできるようであれば、Windows.Resource の設定も無くでよい
	//
    public /*static*/ class WordModel
    {
        #region Clipboard Group

        public static ControlData Clipboard
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "ｸﾘｯﾌﾟﾎﾞｰﾄﾞ";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        GroupData Data = new GroupData(Str)
                        {
                            SmallImage = new Uri("Images/Paste_16x16.png", UriKind.Relative),
                            LargeImage = new Uri("Images/Paste_32x32.png", UriKind.Relative),
                            KeyTip = "ZC",
                        };
                        _dataCollection[Str] = Data;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static ControlData Paste2
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "ﾍﾟｰｽﾄｺﾏﾝﾄﾞ";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        string TooTipTitle = "ﾍﾟｰｽﾄ (Ctrl+V)";
                        string ToolTipDescription = "ｸﾘｯﾌﾟﾎﾞｰﾄﾞからｺﾋﾟｰします";
                        string DropDownToolTipDescription = "Click here for more options such as pasting only the values or formatting.";

                        SplitButtonData splitButtonData = new SplitButtonData()
                        {
                            Label = Str,
                            SmallImage = new Uri("Images/Paste_16x16.png", UriKind.Relative),
                            LargeImage = new Uri("Images/Paste_32x32.png", UriKind.Relative),
                            ToolTipTitle = TooTipTitle,
                            ToolTipDescription = ToolTipDescription,
                            Command = ApplicationCommands.Paste,
                            KeyTip = "V",
                        };
                        splitButtonData.DropDownButtonData.ToolTipTitle = TooTipTitle;
                        splitButtonData.DropDownButtonData.ToolTipDescription = DropDownToolTipDescription;
                        splitButtonData.DropDownButtonData.Command = new DelegateCommand(DefaultExecuted, DefaultCanExecute);
                        splitButtonData.DropDownButtonData.KeyTip = "V";
                        _dataCollection[Str] = splitButtonData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static ControlData PasteSpecial
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "Paste _Special";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        string TooTipTitle = "Paste Special (Alt+Ctrl+V)";

                        MenuItemData menuItemData = new MenuItemData()
                        {
                            Label = Str,
                            SmallImage = new Uri("Images/Paste_16x16.png", UriKind.Relative),
                            ToolTipTitle = TooTipTitle,
                            Command = new DelegateCommand(DefaultExecuted, DefaultCanExecute),
                        };
                        _dataCollection[Str] = menuItemData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static ControlData PasteAsHyperlink
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "Paste As _Hyperlink";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        MenuItemData menuItemData = new MenuItemData()
                        {
                            Label = Str,
                            SmallImage = new Uri("Images/Paste_16x16.png", UriKind.Relative),
                            Command = new DelegateCommand(DefaultExecuted, DefaultCanExecute),
                        };
                        _dataCollection[Str] = menuItemData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

		/// <summary>
		///  削除ｺﾏﾝﾄﾞ
		/// </summary>
        public static ControlData Cut
        {
            get {
                lock (_lockObject) {
                    string Str = "ｶｯﾄ";

                    if (!_dataCollection.ContainsKey(Str)) {
                        string CutToolTipTitle = "ｶｯﾄ (Ctrl+X)";
                        string CutToolTipDescription = "ﾃﾞｰﾀを削除してｸﾘｯﾌﾟﾎﾞｰﾄﾞへｺﾋﾟｰします";

                        ButtonData buttonData = new ButtonData()
                        {
                            Label = Str,
                            SmallImage = new Uri("Images/Cut_16x16.png", UriKind.Relative),
                            ToolTipTitle = CutToolTipTitle,
                            ToolTipDescription = CutToolTipDescription,
                            Command = ApplicationCommands.Cut,
                            KeyTip = "X",
                        };
                        _dataCollection[Str] = buttonData;
                    }

                    return _dataCollection[Str];
                }
            }
        }
		/// <summary>
		///  ｺﾋﾟｰｺﾏﾝﾄﾞ
		/// </summary>
        public static ControlData Copy
        {
            get {
                lock (_lockObject) {
                    string Str = "ｺﾋﾟｰ";

                    if (!_dataCollection.ContainsKey(Str)) {
                        string ToolTipTitle = "ｺﾋﾟｰ (Ctrl+C)";
                        string ToolTipDescription = "ﾃﾞｰﾀをｸﾘｯﾌﾟﾎﾞｰﾄﾞへｺﾋﾟｰします";

                        ButtonData buttonData = new ButtonData()
                        {
                            Label = Str,
                            SmallImage = new Uri("Images/Copy_16x16.png", UriKind.Relative),
                            ToolTipTitle = ToolTipTitle,
                            ToolTipDescription = ToolTipDescription,
                            Command = ApplicationCommands.Copy,
                            KeyTip = "C",
                        };
                        _dataCollection[Str] = buttonData;
                    }

                    return _dataCollection[Str];
                }
            }
        }
		/// <summary>
		/// ﾍﾟｰｽﾄｺﾏﾝﾄﾞ
		/// </summary>
		public static ControlData Paste
		{
			get {
				lock( _lockObject ) {
					string Str = "ﾍﾟｰｽﾄ";

					if( !_dataCollection.ContainsKey(Str) ) {
						string ToolTipTitle = "ﾍﾟｰｽﾄ (Ctrl+V)";
						string ToolTipDescription = "ｸﾘｯﾌﾟﾎﾞｰﾄﾞからﾃﾞｰﾀをｺﾋﾟｰします";

						ButtonData buttonData = new ButtonData()
						{
							Label = Str,
							SmallImage = new Uri("Images/Paste_16x16.png", UriKind.Relative),
							ToolTipTitle = ToolTipTitle,
							ToolTipDescription = ToolTipDescription,
							Command = ApplicationCommands.Paste,
							KeyTip = "V",
						};
						_dataCollection[Str] = buttonData;
					}

					return _dataCollection[Str];
				}
			}
		}

        public static ControlData FormatPainter
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "Format Painter";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        string ToolTipTitle = "Format Painter (Ctrl+Shift+C)";
                        string ToolTipDescription = "Copy the formatting from one place and apply it to another. \n\n Double click this button to apply the same formatting to multiple places in the document.";

                        ButtonData buttonData = new ButtonData()
                        {
                            Label = Str,
                            SmallImage = new Uri("Images/FormatPainter_16x16.png", UriKind.Relative),
                            ToolTipTitle = ToolTipTitle,
                            ToolTipDescription = ToolTipDescription,
                            ToolTipFooterTitle = HelpFooterTitle,
                            ToolTipFooterImage = new Uri("Images/Help_16x16.png", UriKind.Relative),
                            Command = new DelegateCommand(DefaultExecuted, DefaultCanExecute),
                            KeyTip = "FP",
                        };
                        _dataCollection[Str] = buttonData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        #endregion Clipboard Group

        #region Styles Group

        public static ControlData Styles
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "ｽﾀｲﾙ";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        GroupData Data = new GroupData(Str)
                        {
                            LargeImage = new Uri("Images/StylesGroup.png", UriKind.Relative),
                            KeyTip = "ZS",
                        };
                        _dataCollection[Str] = Data;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static ControlData ChangeStyles
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "ｽﾀｲﾙ変更";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        string ToolTipTitle = "Change Styles";
                        string ToolTipDescription = "Change the set of styles, colors, fonts, and paragraph spacing used in this document.";

                        MenuButtonData menuButtonData = new MenuButtonData()
                        {
                            Label = Str,
                            LargeImage = new Uri("Images/Styles_32x32.png", UriKind.Relative),
                            ToolTipTitle = ToolTipTitle,
                            ToolTipDescription = ToolTipDescription,
                            KeyTip = "G",
                        };
                        _dataCollection[Str] = menuButtonData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static ControlData StylesSet
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "ｽﾀｲﾙ設定";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        MenuItemData menuItemData = new MenuItemData()
                        {
                            Label = Str,
                            SmallImage = new Uri("Images/Forecolor_16x16.png", UriKind.Relative),
                            IsVerticallyResizable = true,
                            KeyTip = "Y",
                        };
                        _dataCollection[Str] = menuItemData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static ControlData Colors
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "Colors";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        MenuItemData menuItemData = new MenuItemData()
                        {
                            Label = Str,
                            SmallImage = new Uri("Images/ChooseColor_16x16.png", UriKind.Relative),
                            IsVerticallyResizable = true,
                            KeyTip = "C",
                        };
                        _dataCollection[Str] = menuItemData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static ControlData Fonts
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "Fonts";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        MenuItemData menuItemData = new MenuItemData()
                        {
                            Label = Str,
                            SmallImage = new Uri("Images/Font_16x16.png", UriKind.Relative),
                            IsVerticallyResizable = true,
                            KeyTip = "F",
                        };
                        _dataCollection[Str] = menuItemData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static ControlData ParagraphSpacing
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "Paragraph Spacing";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        MenuItemData menuItemData = new MenuItemData()
                        {
                            Label = Str,
                            SmallImage = new Uri("Images/ParagraphSpacing_16x16.png", UriKind.Relative),
                            KeyTip = "P",
                        };
                        _dataCollection[Str] = menuItemData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static ControlData SetAsDefault
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "ﾃﾞﾌｫﾙﾄ設定";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        string ToolTipTitle = Str;
                        string ToolTipDescription = "Set the cuurent style set and theme as the default when you create a new document.";

                        MenuItemData menuItemData = new MenuItemData()
                        {
                            Label = Str,
                            ToolTipTitle = ToolTipTitle,
                            ToolTipDescription = ToolTipDescription,
                            Command = new DelegateCommand(DefaultExecuted, DefaultCanExecute),
                            KeyTip = "S",
                        };
                        _dataCollection[Str] = menuItemData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static GalleryData<StylesSet> StylesSetGalleryData
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "StylesSetGalleryData";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        // TODO: replace string with an object (IsChecked, StyleName). Define DataTemplate
                        GalleryData<StylesSet> stylesData = new GalleryData<StylesSet>();
                        GalleryCategoryData<StylesSet> singleCategory = new GalleryCategoryData<StylesSet>();

                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "Default (Black and White)", IsSelected = true });
                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "Distinctive" });
                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "Elegant" });
                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "Fancy" });
                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "ﾌｫｰﾏﾙ" });
                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "Manuscript" });
                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "Modern" });
                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "Newsprint" });
                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "Perspective" });
                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "Simple" });
                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "Thatch" });
                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "Traditional" });
                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "Word 2003" });
                        singleCategory.GalleryItemDataCollection.Add(new StylesSet() { Label = "Word 2010" });

                        stylesData.CategoryDataCollection.Clear();
                        stylesData.CategoryDataCollection.Add(singleCategory);
                        _dataCollection[Str] = stylesData;
                    }

                    return _dataCollection[Str] as GalleryData<StylesSet>;
                }
            }
        }

        public static ControlData ResetToQuickStylesFromTemplate
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "_Reset to Quick Styles from Template";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        MenuItemData menuItemData = new MenuItemData()
                        {
                            Label = Str,
                            Command = new DelegateCommand(DefaultExecuted, DefaultCanExecute),
                            KeyTip = "R",
                        };
                        _dataCollection[Str] = menuItemData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static ControlData ResetDocumentQuickStyles
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "Reset _Document Quick Styles";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        MenuItemData menuItemData = new MenuItemData()
                        {
                            Label = Str,
                            Command = new DelegateCommand(DefaultExecuted, DefaultCanExecute),
                            KeyTip = "D",
                        };
                        _dataCollection[Str] = menuItemData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static ControlData SaveAsQuickStyleSet
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "Save as _Quick Style Set...";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        MenuItemData menuItemData = new MenuItemData()
                        {
                            Label = Str,
                            Command = new DelegateCommand(DefaultExecuted, DefaultCanExecute),
                            KeyTip = "Q",
                        };
                        _dataCollection[Str] = menuItemData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static GalleryData<string> StylesColorsGalleryData
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "StylesColorsGalleryData";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        // TODO: replace string with an object (Color Palette, StyleName). Define DataTemplate
                        GalleryData<string> stylesData = new GalleryData<string>();
                        GalleryCategoryData<string> singleCategory = new GalleryCategoryData<string>();

                        singleCategory.Label = "Built-In";
                        singleCategory.GalleryItemDataCollection.Add("Office");
                        singleCategory.GalleryItemDataCollection.Add("Grayscale");
                        singleCategory.GalleryItemDataCollection.Add("Adjacency");
                        singleCategory.GalleryItemDataCollection.Add("Angles");
                        singleCategory.GalleryItemDataCollection.Add("Apex");
                        singleCategory.GalleryItemDataCollection.Add("Apothecary");
                        singleCategory.GalleryItemDataCollection.Add("Aspect");
                        singleCategory.GalleryItemDataCollection.Add("Austin");
                        singleCategory.GalleryItemDataCollection.Add("Black Tie");
                        singleCategory.GalleryItemDataCollection.Add("Civic");
                        singleCategory.GalleryItemDataCollection.Add("Clarity");
                        singleCategory.GalleryItemDataCollection.Add("Composite");
                        singleCategory.GalleryItemDataCollection.Add("Concourse");
                        singleCategory.GalleryItemDataCollection.Add("Couture");
                        singleCategory.GalleryItemDataCollection.Add("Elemental");
                        singleCategory.GalleryItemDataCollection.Add("Equity");
                        singleCategory.GalleryItemDataCollection.Add("Essential");
                        singleCategory.GalleryItemDataCollection.Add("Executive");
                        singleCategory.GalleryItemDataCollection.Add("Flow");
                        singleCategory.GalleryItemDataCollection.Add("Foundry");
                        singleCategory.GalleryItemDataCollection.Add("Grid");
                        singleCategory.GalleryItemDataCollection.Add("Horizon");
                        singleCategory.GalleryItemDataCollection.Add("Median");
                        singleCategory.GalleryItemDataCollection.Add("Newsprint");
                        singleCategory.GalleryItemDataCollection.Add("Perspective");
                        singleCategory.GalleryItemDataCollection.Add("Solstice");
                        singleCategory.GalleryItemDataCollection.Add("Technic");
                        singleCategory.GalleryItemDataCollection.Add("Urban");
                        singleCategory.GalleryItemDataCollection.Add("Verve");
                        singleCategory.GalleryItemDataCollection.Add("Waveform");

                        stylesData.CategoryDataCollection.Add(singleCategory);
                        _dataCollection[Str] = stylesData;
                    }

                    return _dataCollection[Str] as GalleryData<string>;
                }
            }
        }

        public static ControlData CreateNewThemeColors
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "_Create New Theme Colors";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        MenuItemData menuItemData = new MenuItemData()
                        {
                            Label = Str,
                            Command = new DelegateCommand(DefaultExecuted, DefaultCanExecute),
                            KeyTip = "C",
                        };
                        _dataCollection[Str] = menuItemData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static GalleryData<ThemeFonts> StylesFontsGalleryData
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "StylesFontsGalleryData";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        GalleryData<ThemeFonts> stylesData = new GalleryData<ThemeFonts>();
                        GalleryCategoryData<ThemeFonts> singleCategory = new GalleryCategoryData<ThemeFonts>();

                        singleCategory.Label = "Built-In";
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office 2", Field2 = "Calibri", Field3 = "Cambria", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office Classic", Field2 = "Arial", Field3 = "Times New Roman", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office Classic 2", Field2 = "Arial", Field3 = "Arial", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Adjacency", Field2 = "Franklin Gothic", Field3 = "Franklin Gothic Book", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Angles", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Apex", Field2 = "Lucida Sans", Field3 = "Book Antiqua", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Apothecary", Field2 = "Book Antiqua", Field3 = "Century Gothic", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Aspect", Field2 = "Verdana", Field3 = "Verdana", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Austin", Field2 = "Century Gothic", Field3 = "Century Gothic", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Black Tie", Field2 = "Garamond", Field3 = "Garamond", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Civic", Field2 = "Georgia", Field3 = "Georgia", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });
                        singleCategory.GalleryItemDataCollection.Add(new ThemeFonts() { Label = "Office", Field2 = "Cambria", Field3 = "Calibri", Field1 = "Images/ThemeFonts.png" });

                        stylesData.CategoryDataCollection.Add(singleCategory);
                        _dataCollection[Str] = stylesData;
                    }

                    return _dataCollection[Str] as GalleryData<ThemeFonts>;
                }
            }
        }

        public static ControlData CreateNewThemeFonts
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "_Create New Theme Fonts";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        MenuItemData menuItemData = new MenuItemData()
                        {
                            Label = Str,
                            Command = new DelegateCommand(DefaultExecuted, DefaultCanExecute),
                            KeyTip = "C",
                        };
                        _dataCollection[Str] = menuItemData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        public static GalleryData<string> StylesParagraphGalleryData
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "StylesParagraphGalleryData";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        GalleryData<string> stylesData = new GalleryData<string>();
                        GalleryCategoryData<string> firstCategory = new GalleryCategoryData<string>();
                        firstCategory.Label = "Style Set";
                        firstCategory.GalleryItemDataCollection.Add("Traditional");

                        GalleryCategoryData<string> secondCategory = new GalleryCategoryData<string>();
                        secondCategory.Label = "Built-In";
                        secondCategory.GalleryItemDataCollection.Add("No Paragraph Space");
                        secondCategory.GalleryItemDataCollection.Add("Compact");
                        secondCategory.GalleryItemDataCollection.Add("Tight");
                        secondCategory.GalleryItemDataCollection.Add("Open");
                        secondCategory.GalleryItemDataCollection.Add("Relaxed");
                        secondCategory.GalleryItemDataCollection.Add("Double");

                        stylesData.CategoryDataCollection.Add(firstCategory);
                        stylesData.CategoryDataCollection.Add(secondCategory);
                        _dataCollection[Str] = stylesData;
                    }

                    return _dataCollection[Str] as GalleryData<string>;
                }
            }
        }

		/// <summary>
		/// 画像処理ﾃﾞｰﾀの登録
		/// </summary>
		public static GalleryData<TplImageData> ImageTemplateGalleryData
		{
			get
			{
				lock( _lockObject ) {
					string Str = "ImageTemplateGalleryData";

					//Imaging の iniﾌｧｲﾙからﾃﾞｰﾀのﾌｫﾙﾀﾞ名を取得
					string shape_path;
					string mark_path;

					try {
						shape_path = App.Setting.IniFile.getValueString("Options", "ShapeFolder");
						mark_path = App.Setting.IniFile.getValueString("Options", "MarkFolder");
					}
					catch(Exception) {
						shape_path = AppDomain.CurrentDomain.BaseDirectory;
						mark_path = AppDomain.CurrentDomain.BaseDirectory;
					}

					if( !_dataCollection.ContainsKey(Str) ) {
						GalleryData<TplImageData> stylesData = new GalleryData<TplImageData>();

						//
						//Imagingで使用するﾃﾞｰﾀの一覧
						//
						// ﾏｰｸﾃﾞｰﾀ
						string[] mark_files;
						try {
							mark_files = System.IO.Directory.GetFiles(mark_path, "*.Jlb");
						}
						catch( Exception ) {
							mark_files = new string[] { "" };
						}
						GalleryCategoryData<TplImageData> firstCategory = new GalleryCategoryData<TplImageData>();
						firstCategory.Label = "ﾏｰｸﾃﾞｰﾀ";
						foreach( string ff in mark_files ) {
							firstCategory.GalleryItemDataCollection.Add(new TplImageData() {
								FilePath = ff,
								Label = System.IO.Path.GetFileName(ff),
								Field1 = "Images/Imaging.png"
							});
						}
						//shapeﾃﾞｰﾀ
						string[] shape_files;
						try {
							shape_files = System.IO.Directory.GetFiles(shape_path, "*.Jlb");
						}
						catch( Exception ) {
							shape_files = new string[] { "" };
						}
						GalleryCategoryData<TplImageData> secondCategory = new GalleryCategoryData<TplImageData>();
						secondCategory.Label = "shapeﾃﾞｰﾀ";
						foreach( string ff in shape_files ) {
							secondCategory.GalleryItemDataCollection.Add(new TplImageData() {
								FilePath = ff,
								Label = System.IO.Path.GetFileName(ff),
								Field1 = "Images/Imaging.png"
							});
						}

						//OpenCv で使用するﾃﾞｰﾀの一覧
						GalleryCategoryData<TplImageData> thirdCategory = new GalleryCategoryData<TplImageData>();
						thirdCategory.Label = "OpenCv(ﾏｰｸ)";

						//指定ﾌｫﾙﾀﾞにあるpngﾌｧｲﾙをﾃﾝﾌﾟﾚｰﾄﾃﾞｰﾀとする
						string path = System.Environment.CurrentDirectory + "\\Cv_template";
						string[] dirs = System.IO.Directory.GetFiles(path, "*.png");
						foreach( string dir in dirs ) {
							thirdCategory.GalleryItemDataCollection.Add(new TplImageData() {
								FilePath = dir,
								Label = System.IO.Path.GetFileName(dir),
								Field1 = "Images/32_32_black_RGB_24.png"
							});
						}
						//選択が変更された場合のdelegate
						stylesData.PropertyChanged += new PropertyChangedEventHandler(stylesData_PropertyChanged);

						stylesData.CategoryDataCollection.Add(firstCategory);	//Imaging ﾏｰｸ
						stylesData.CategoryDataCollection.Add(secondCategory);	//Imaging ﾊﾟｰﾂ
						stylesData.CategoryDataCollection.Add(thirdCategory);	//OpenCv ﾏｰｸ

						_dataCollection[Str] = stylesData;
					}

					return _dataCollection[Str] as GalleryData<TplImageData>;
				}
			}
		}

		/// <summary>
		///  画像処理ﾃﾝﾌﾟﾚｰﾄの選択変更用のﾊﾝﾄﾞﾗ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void stylesData_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			GalleryData<TplImageData> data = sender as GalleryData<TplImageData>;
			TplImageData name = data.SelectedItem;

			if( name != null ) {
				int idx = 0;
				int type = 0;
				foreach( GalleryCategoryData<TplImageData> d1 in data.CategoryDataCollection ) {
					foreach( TplImageData ss in d1.GalleryItemDataCollection ) {
						if( ss.FilePath == name.FilePath ) {
							if( type == 0 ) {
								//Imaging mark
								string fileNo = System.IO.Path.GetFileNameWithoutExtension(name.Label);
								string cmd = String.Format("import missionLib\nmissionLib.load_mark(0,{0})",fileNo);
								_viewModel.exec_cmd_script(cmd);
							}
							else if( type == 1 ) {
								//Imaging shape
								string fileNo = System.IO.Path.GetFileNameWithoutExtension(name.Label);
								string cmd = String.Format("import missionLib\nmissionLib.load_shape(1,{0})",fileNo);
								_viewModel.exec_cmd_script(cmd);
							}
							else if( type == 2 ) {
								//OpenCv 画像処理
								if( _tpl_img != null ) {
									_tpl_img.Dispose();
								}
								string path = System.Environment.CurrentDirectory + "\\Cv_template\\" + name.Label;
								_tpl_img = Cv.LoadImage(path, LoadMode.GrayScale);
							}
							else {
							}
							return;
						}
						idx++;
					}
					type++;
					idx = 0;
				}
			}
		}
		
		/// <summary>
		///  画像処理ﾎﾞﾀﾝ
		/// </summary>
		public static ControlData VisionProc
		{
			get
			{
				lock( _lockObject ) {
					string Str = "画像処理";

					if( !_dataCollection.ContainsKey(Str) ) {
						string CutToolTipTitle = "画像処理";
						string CutToolTipDescription = "選択されたﾃﾝﾌﾟﾚｰﾄで画像処理を実行";

						ButtonData buttonData = new ButtonData()
						{
							Label = Str,
							SmallImage = new Uri("Images/startVision.png", UriKind.Relative),
							ToolTipTitle = CutToolTipTitle,
							ToolTipDescription = CutToolTipDescription,
							Command = new DelegateCommand(VisionExecute, CanVisionExecute),
							KeyTip = "X",
						};
						_dataCollection[Str] = buttonData;
					}
					
					return _dataCollection[Str];
				}
			}
		}
		/// <summary>
		///  画像処理実行
		/// </summary>
		public static void VisionExecute()
		{
			GalleryData<TplImageData> data = _dataCollection["ImageTemplateGalleryData"] as GalleryData<TplImageData>;

			int idx = 0;
			int type = 0;
			foreach( GalleryCategoryData<TplImageData> d1 in data.CategoryDataCollection ) {
				foreach( TplImageData ss in d1.GalleryItemDataCollection ) {
					if( ss.Label == data.SelectedItem.Label ) {
						string cmd = "print 'error'";
						string fileNo = "1";
						switch(type){
						case 0:	//Imaging ﾏｰｸ処理
							fileNo = System.IO.Path.GetFileNameWithoutExtension(data.SelectedItem.Label);
							cmd = string.Format("vision.img_mark_proc({0})", fileNo);
							break;
						case 1:	//Imaging ﾊﾟｰﾂ処理
							fileNo = System.IO.Path.GetFileNameWithoutExtension(data.SelectedItem.Label);
							cmd = string.Format("vision.img_part_proc({0})", fileNo);
							break;
						case 2:	//OpenCv ﾏｰｸ画像処理
							cmd = string.Format("vision.cv_mark_proc({0})", idx);
							break;
						}
						_viewModel.exec_cmd_script(cmd);
						return;
					}
					idx++;
				}
				type++;
				idx = 0;
			}
		}
		/// <summary>
		/// 画像処理実行条件
		/// </summary>
		/// <returns></returns>
		public static bool CanVisionExecute()
		{
			try {
				GalleryData<TplImageData> data = _dataCollection["ImageTemplateGalleryData"] as GalleryData<TplImageData>;

				if( data.SelectedItem == null ) {
					return false;
				}
				return true;
			}
			catch( Exception ) {
				return false;
			}

			//return ((_tpl_img == null) ? false : true);
		}

		/// <summary>
		///  画像処理ﾀｽｸの起動と終了ﾎﾞﾀﾝ
		/// </summary>
		public static ControlData Imaging
		{
			get
			{
				lock( _lockObject )
				{
					string Str = "ImagingTask";
					if( !_dataCollection.ContainsKey(Str)){
						ToggleButtonData data = new ToggleButtonData()
						{
							Label = "Imagingﾀｽｸ",
							LargeImage = new Uri("Images/Imaging_32x32.png", UriKind.Relative),
							//SmallImage = new Uri("Images/Paste_16x16.png",UriKind.Relative),
							ToolTipTitle ="Imagingﾀｽｸ制御",
							ToolTipDescription = "Imagingﾀｽｸの起動・終了",
							IsChecked = false,
							//Command = new DelegateCommand(WakeupImaging,CanWakeupImaging)
						};
//						data.Command = new DelegateCommand(WakeupImaging, data,CanWakeupImaging);

						data.Command = new DelegateCommand(
							(obj) => { WakeupImaging(obj); },
							data,
							CanWakeupImaging
							);


						_dataCollection[Str] = data;
					}
					return _dataCollection[Str];
				}
			}
		}
		/// <summary>
		///  Imagingﾀｽｸの起動・終了
		/// </summary>
		/// <param name="obj">ToggleButtonData object</param>
		public static void WakeupImaging(object obj)
		{
			// 引数のobjとdataは同じもの(のはず)-->DelegateCommand に引数を設定している
			ToggleButtonData data = _dataCollection["ImagingTask"] as ToggleButtonData;

			string path = App.Setting.ImagingPath;
			string cmd = "";

			if( data.IsChecked ) {
				cmd = String.Format("import missionLib\nmissionLib.start_imaging('{0}')",path);
			}
			else {
				cmd = String.Format("import missionLib\nmissionLib.shutdown_imaging()");
			}
			_viewModel.exec_cmd_script(cmd);
		}
		/// <summary>
		///  Imagingﾀｽｸ起動条件
		/// </summary>
		/// <returns></returns>
		public static bool CanWakeupImaging()
		{
			return (true);
		}



        public static ControlData CustomParagraphSpacing
        {
            get
            {
                lock (_lockObject)
                {
                    string Str = "Custom Paragraph Spacing...";

                    if (!_dataCollection.ContainsKey(Str))
                    {
                        MenuItemData menuItemData = new MenuItemData()
                        {
                            Label = Str,
                            Command = new DelegateCommand(DefaultExecuted, DefaultCanExecute),
                            KeyTip = "C",
                        };
                        _dataCollection[Str] = menuItemData;
                    }

                    return _dataCollection[Str];
                }
            }
        }

        #endregion Styles Group

        private static void DefaultExecuted()
        {
        }


        private static bool DefaultCanExecute()
        {
            return true;
        }

		/// <summary>
		///  WordModelから、ViewMode へのｱｸｾｽが可能となるように
		//     ※ｱｸｾｽする場合は WordModel.Vm = _viewMode
		/// </summary>
		public static ViewModel Vm
		{
			get { return _viewModel; }
			set { _viewModel = value; }
		}

        #region Data

        private const string HelpFooterTitle = "Press F1 for more help.";
        private static object _lockObject = new object();
        private static Dictionary<string, ControlData> _dataCollection = new Dictionary<string,ControlData>();

		public static IplImage _tpl_img = null;//new IplImage();
		public static ViewModel _viewModel;

		/// <summary>
		///  ｺﾝｽﾄﾗｸﾀ
		///    ※static ｸﾗｽにすると、ｺﾝｽﾄﾗｸﾀは使用できなくなる
		/// </summary>
		public WordModel()
		{

		}

        #endregion Data
    }
}
