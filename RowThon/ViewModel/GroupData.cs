using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RowThon.Model
{
    public class GroupData : ControlData
    {
        public GroupData(string header)
        {
            Label = header;
        }
    }
}
