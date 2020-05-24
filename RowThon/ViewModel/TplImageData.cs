using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace RowThon
{
    /// <summary>
    /// DataModel for TplImageData Gallery
    /// </summary>
    public class TplImageData : INotifyPropertyChanged
    {
        public string Label
        {
            get
            {
                return _label;
            }

            set
            {
                if (_label != value)
                {
                    _label = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Label"));
                }
            }
        }
        private string _label;

		public string FilePath
		{
			get{ return( _path );}
			set{ _path = value;}
		}
		private string _path;

        public string Field1
        {
            get
            {
                return _field1;
            }

            set
            {
                if (_field1 != value)
                {
                    _field1 = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Field1"));
                }
            }
        }
        private string _field1;

		
		public override string ToString()
		{
			return ( Path.GetFileNameWithoutExtension(_label));
		}

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        #endregion
    }
}
