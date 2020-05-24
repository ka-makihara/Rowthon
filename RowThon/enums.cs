
namespace RowThon
{
	public enum ErrCode
	{
		RET_OK = 0,
		ERR_INIT = -1,
		ERR_PARAM = -2,
		ERR_EXEC = -3
	}
	public enum AxisNo
	{
		Axis_X = 0x00,
		Axis_Y = 0x01,
		Axis_Z = 0x02,
		Axis_A = 0x03,
		Axis_B = 0x04
	}
	public enum CtrlMode
	{
		Mode_Manual = 0,		//ﾏﾆｭｱﾙ操作状態
		Mode_Remote = 1,		//ﾘﾓｰﾄ操作状態
	}
	public enum SvParaType
	{
		MAX_SPD = 2,
		JOG_SPD = 4,
	}
	public enum MessageCode
	{
		SYS_INIT = 0,
		SYS_IO_UPDATE = 1,
		SYS_CMD_RETOK = 2,
		SYS_CMD_RETNG = 3,
		SYS_IO_EMG = 4,
		SYS_SV_ALM = 5,
	}
	public enum SvStatType
	{
		CURRENT_POS	= 0,
		ERROR_INFO	= 1,
		POWER_INFO	= 2,
	}
}