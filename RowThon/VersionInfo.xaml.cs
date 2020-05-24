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
using System.Windows.Shapes;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace RowThon
{
	/// <summary>
	/// VersionInfo.xaml の相互作用ロジック
	/// </summary>
	public partial class VersionInfo : Window
	{
		public VersionInfo()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}

	public class FileItem
	{
		public string Path { get; set; }
		public string Name
		{
			get
			{
				return( System.IO.Path.GetFileNameWithoutExtension(Path) );
			}
		}
		public string Version { get { return( FileVersionInfo.GetVersionInfo(Path).FileVersion); } }
		public string MajorVersion { get { return( FileVersionInfo.GetVersionInfo(Path).FileMajorPart.ToString()); } }
		public string MinorPart { get { return (FileVersionInfo.GetVersionInfo(Path).FileMinorPart.ToString()); } }
		public string PrivatePart { get { return (FileVersionInfo.GetVersionInfo(Path).FilePrivatePart.ToString()); } }
		public string BuildNo { get { return (FileVersionInfo.GetVersionInfo(Path).FileBuildPart.ToString()); } }
		public string PrivateVersion { get { return (FileVersionInfo.GetVersionInfo(Path).PrivateBuild.ToString()); } }
		public string SpecialVersion { get { return (FileVersionInfo.GetVersionInfo(Path).SpecialBuild.ToString()); } }
		public string Description { get { return (FileVersionInfo.GetVersionInfo(Path).FileDescription); } }
		public string Copyright { get { return (FileVersionInfo.GetVersionInfo(Path).LegalCopyright); } }
		public string CompanyName { get { return (FileVersionInfo.GetVersionInfo(Path).CompanyName); } }
		public string Comment { get { return (FileVersionInfo.GetVersionInfo(Path).Comments); } }
		public string InternalName { get { return (FileVersionInfo.GetVersionInfo(Path).InternalName); } }
		public string Language { get { return (FileVersionInfo.GetVersionInfo(Path).Language); } }
		public string Trademarks { get { return (FileVersionInfo.GetVersionInfo(Path).LegalTrademarks); } }
		public string FileName { get { return (FileVersionInfo.GetVersionInfo(Path).OriginalFilename); } }
		public string ProductName { get { return (FileVersionInfo.GetVersionInfo(Path).ProductName); } }
		public string ProductVersion { get { return (FileVersionInfo.GetVersionInfo(Path).ProductVersion); } }
		public string ProductMajorVersion { get { return (FileVersionInfo.GetVersionInfo(Path).ProductMajorPart.ToString()); } }
		public string ProductMinorVersion { get { return (FileVersionInfo.GetVersionInfo(Path).ProductMinorPart.ToString()); } }
		public string ProductPrivateVersion { get { return (FileVersionInfo.GetVersionInfo(Path).ProductPrivatePart.ToString()); } }
		public string ProductBuildNo { get { return (FileVersionInfo.GetVersionInfo(Path).ProductBuildPart.ToString()); } }
	}
	public class VersionData
	{
		public ObservableCollection<FileItem> FileList { get; set; }
		public VersionData()
		{
			FileList = new ObservableCollection<FileItem>();

			try {
				string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
				string[] files = Directory.GetFiles(path, "*.dll");

				foreach( string name in files ) {
					FileItem newItem = new FileItem { Path = name };
					FileList.Add(newItem);
				}
			}
			catch( Exception ) {
			}
		}
	}
}
