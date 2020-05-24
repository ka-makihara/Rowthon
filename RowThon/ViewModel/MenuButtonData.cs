using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;

namespace RowThon.Model
{
    public class MenuButtonData : ControlData
    {
        public bool IsVerticallyResizable
        {
            get
            {
                return _isVerticallyResizable;
            }

            set
            {
                if (_isVerticallyResizable != value)
                {
                    _isVerticallyResizable = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsVerticallyResizable"));
                }
            }
        }

        public bool IsHorizontallyResizable
        {
            get
            {
                return _isHorizontallyResizable;
            }

            set
            {
                if (_isHorizontallyResizable != value)
                {
                    _isHorizontallyResizable = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsHorizontallyResizable"));
                }
            }
        }

        private bool _isVerticallyResizable, _isHorizontallyResizable;
    }
}
