using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.ComponentModel;            //for INotifyPropertyChanged

namespace RowThon
{
	public class IoData
	{
		public IoData(int port, int bit, int onoff)
		{ _portNo = port; _bitNo = bit; _onoff = onoff; }
		public int _portNo;
		public int _bitNo;
		public int _onoff;
	}
	public class IoCtrl : INotifyPropertyChanged
	{
//		private AutoResetEvent[] _ioAssert = new AutoResetEvent[32];
//		private AutoResetEvent[] _ioNagete = new AutoResetEvent[32];

		private PEventManager.PEvent[] _ioAssert = new PEventManager.PEvent[32];
		private PEventManager.PEvent[] _ioNagate = new PEventManager.PEvent[32];

		private UInt32 _ioValue;
		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChange(string propertyName)
		{
			if (this.PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
//		public AutoResetEvent this[int onoff, int bitNo]
		public PEventManager.PEvent this[int onoff, int bitNo]
		{
			get
			{
				return ((onoff == 0) ? _ioNagate[bitNo] : _ioAssert[bitNo]);
			}
			set
			{
				if( onoff == 0 ) {
					_ioNagate[bitNo] = value;
				}
				else {
					_ioAssert[bitNo] = value;
				}
			}
		}
		public UInt32 Value
		{
			get { return (_ioValue); }
			set
			{
				UInt32 data = _ioValue;

				_ioValue = value;

				//変化のあった
				UInt32 diff = _ioValue ^ data;
				if(diff != 0) {
					NotifyPropertyChange("Value");
				}
				for (int bit = 0; bit < 32; bit++) {
					if ((diff & (1 << bit)) != 0) {

						string propName = "IsChecked" + (bit + 1).ToString("00");
						NotifyPropertyChange(propName);

						if ((_ioValue & (1 << bit)) != 0) {
							if (_ioAssert[bit] != null) {
								_ioNagate[bit].Flush();
								_ioAssert[bit].Set(0);
							}
						}
						else {
							if (_ioNagate[bit] != null) {
								_ioAssert[bit].Flush();
								_ioNagate[bit].Set(0);
							}
						}
					}
				}
			}
		}
		protected void BitOnOff(int bitNo, bool status)
		{
			UInt32 bit = ((UInt32)1 << bitNo);
			if(status == true) {
				_ioValue |= bit;
			}
			else {
				_ioValue &= ~bit;
			}
			NotifyPropertyChange("Value");
		}
		protected bool OnOff(int bitNo)
		{
			return ((_ioValue & (1 << bitNo)) == 0 ? false : true);
		}
		protected void BitSet(int bitNo)
		{
			BitOnOff(bitNo, ((_ioValue & (1 << bitNo)) == 0) ? false : true);
		}
		/// <summary>
		///  Binding 用
		/// </summary>
		public bool IsChecked01 { get { return (OnOff(0)); } set { BitSet(0); } }
		public bool IsChecked02 { get { return (OnOff(1)); } set { BitSet(1); } }
		public bool IsChecked03 { get { return (OnOff(2)); } set { BitSet(2); } }
		public bool IsChecked04 { get { return (OnOff(3)); } set { BitSet(3); } }
		public bool IsChecked05 { get { return (OnOff(4)); } set { BitSet(4); } }
		public bool IsChecked06 { get { return (OnOff(5)); } set { BitSet(5); } }
		public bool IsChecked07 { get { return (OnOff(6)); } set { BitSet(6); } }
		public bool IsChecked08 { get { return (OnOff(7)); } set { BitSet(7); } }
		public bool IsChecked09 { get { return (OnOff(8)); } set { BitSet(8); } }
		public bool IsChecked10 { get { return (OnOff(9)); } set { BitSet(9); } }
		public bool IsChecked11 { get { return (OnOff(10)); } set { BitSet(10); } }
		public bool IsChecked12 { get { return (OnOff(11)); } set { BitSet(11); } }
		public bool IsChecked13 { get { return (OnOff(12)); } set { BitSet(12); } }
		public bool IsChecked14 { get { return (OnOff(13)); } set { BitSet(13); } }
		public bool IsChecked15 { get { return (OnOff(14)); } set { BitSet(14); } }
		public bool IsChecked16 { get { return (OnOff(15)); } set { BitSet(15); } }
		public bool IsChecked17 { get { return (OnOff(16)); } set { BitSet(16); } }
		public bool IsChecked18 { get { return (OnOff(17)); } set { BitSet(17); } }
		public bool IsChecked19 { get { return (OnOff(18)); } set { BitSet(18); } }
		public bool IsChecked20 { get { return (OnOff(19)); } set { BitSet(19); } }
		public bool IsChecked21 { get { return (OnOff(20)); } set { BitSet(20); } }
		public bool IsChecked22 { get { return (OnOff(21)); } set { BitSet(21); } }
		public bool IsChecked23 { get { return (OnOff(22)); } set { BitSet(22); } }
		public bool IsChecked24 { get { return (OnOff(23)); } set { BitSet(23); } }
		public bool IsChecked25 { get { return (OnOff(24)); } set { BitSet(24); } }
		public bool IsChecked26 { get { return (OnOff(25)); } set { BitSet(25); } }
		public bool IsChecked27 { get { return (OnOff(26)); } set { BitSet(26); } }
		public bool IsChecked28 { get { return (OnOff(27)); } set { BitSet(27); } }
		public bool IsChecked29 { get { return (OnOff(28)); } set { BitSet(28); } }
		public bool IsChecked30 { get { return (OnOff(29)); } set { BitSet(29); } }
		public bool IsChecked31 { get { return (OnOff(30)); } set { BitSet(30); } }
		public bool IsChecked32 { get { return (OnOff(31)); } set { BitSet(31); } }
	}
}
