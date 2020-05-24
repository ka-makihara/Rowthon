#-*- coding: UTF-8 -*-

import time
import ctypes
import ctypes.wintypes as winTypes

import missionCode
import winApi

import System
from System.Diagnostics import Process,ProcessStartInfo

import ctrlLib as ctrl

hWnd = winApi.FindWindow('0016_IMAGING')
#hWnd = winApi.FindWindow('無題 - メモ帳')
#hWnd = winApi.FindWindow('PyMsgTest')
hOwnWnd = winApi.FindWindow('Rowthon')

#print hWnd
IMAGING_TASK_NAME = '0016_IMAGING'

ANS_WINDOW_NAME = '画像処理結果'

def close_ret_window():
	hWnd = winApi.FindWindow(ANS_WINDOW_NAME)
	if hWnd != None:
		winApi.SendMessage(hWnd,0x10,0,0)

#
# Windowsﾒｯｾｰｼﾞ(WM_COPYDATA)を使用して任意ｻｲｽﾞのﾃﾞｰﾀを送信する
#   ※WM_COPYDATA では、Windowsが内部で共有ﾒﾓﾘ(?)的な事を行ってくれるので
#     ﾛｰｶﾙ変数のﾒﾓﾘを渡してもよい
#
#
def send_copy_msg(windowName, missionData, dwData):
	copyData = winApi.COPYDATASTRUCT()
	copyData.dwData = dwData
	copyData.cbData = ctypes.sizeof(missionData)
	copyData.lpData = ctypes.cast(ctypes.pointer(missionData),ctypes.c_void_p)
	
	hWnd = winApi.FindWindow(windowName)

	if hWnd != None:
		winApi.SendMessage(hWnd,winApi.WM_COPYDATA,hOwnWnd,ctypes.cast(ctypes.pointer(copyData),winTypes.c_void_p).value)
	else:
		print "[{0}] Window Not Found".format(windowName)
		return -1

	return 0

#
# ﾐｯｼｮﾝの実行結果の受け取り
#   最後のﾐｯｼｮﾝの結果がｱﾌﾟﾘ側に保持されているので、ﾀﾌﾟﾙで取得して
#   ｸﾗｽに代入してﾘﾀｰﾝする
#
def get_mission_ret():
	data = missionCode.SendMissionArg()
	(tr_id,no,ret,sp,arg1,arg2,arg3,arg4,arg5,arg6) = Vision.GetMissionRet()

	data.transaction_id = tr_id
	data.CommandNo = no
	data.result = ret
	data.spare = sp
	data.arg1 = arg1
	data.arg2 = arg2
	data.arg3 = arg3
	data.arg4 = arg4
	data.arg5 = arg5
	data.arg6 = arg6

	print "ID:{0} No:{1:0>4X} R:{2} 1:{3} 2:{4} 3:{5} 4:{6} 5:{7}".format(data.transaction_id,data.CommandNo,data.result,data.arg1,data.arg2,data.arg3,data.arg4,data.arg6)

	return data

#
# ﾐｯｼｮﾝ送信
#
def send_mission(tr_id, cmd, arg1=0, arg2=0, arg3=0, arg4=0, arg5=0, arg6=0, timeout=1000):
	data = missionCode.SendMissionArg()
	data.CommandNo = cmd
	data.transaction_id =tr_id 
	data.arg1 = arg1
	data.arg2 = arg2
	data.arg3 = arg3
	data.arg4 = arg4
	data.arg5 = arg5
	data.arg6 = arg6

	id = Vm.GetMsgEventID()
	if send_copy_msg(IMAGING_TASK_NAME,data, missionCode.DATA_TYPE_MISSION) >= 0:
		ret = ctrl.wait([id],timeout)

		if ret == id:
			#ﾘﾀｰﾝﾃﾞｰﾀの受け取り
			ret_data = get_mission_ret()
			if ret_data.transaction_id == tr_id:
				return ret_data

		elif ret == 0x102:
			#結果ｳｲﾝﾄﾞｳが表示されてﾀｲﾑｱｳﾄなので強制ｸﾛｰｽﾞする
			close_ret_window()
			time.sleep(1)
			ret_data = get_mission_ret()
			if ret_data.transaction_id == tr_id:
				return ret_data

		data.result = 2
		data.arg1 = ret
	else:
		data.result = 2
		data.arg1 = -3 

	return data

#
#
#
def send_ctrl(tr_id, cmd, dwCode, arg1=0,arg2=0,arg3=0,arg4=0,arg5=0,arg6=0):
	data = missionCode.SendMissionArg()
	data.CommandNo = cmd
	data.transaction_id =tr_id 
	data.arg1 = arg1
	data.arg2 = arg2
	data.arg3 = arg3
	data.arg4 = arg4
	data.arg5 = arg5
	data.arg6 = arg6

	send_copy_msg(IMAGING_TASK_NAME,data, dwCode)

	#ﾘﾀｰﾝﾃﾞｰﾀの受け取り
	ret_data = get_mission_ret()

	return ret_data


#
# imageﾀｽｸ(image.exe)に終了ﾒｯｾｰｼﾞを送信する
#
def shutdown_imaging(tr_id=999):
	#send_mission(tr_id,0xFFFF)
	send_ctrl(tr_id,0xFFFF,missionCode.DATA_TYPE_SHUTDOWN)

#
# Imageﾀｽｸ(image.exe)を起動させる
#   path: imaging.exe があるﾊﾟｽ
#
def start_imaging(path):
	psInfo = ProcessStartInfo()
	psInfo.FileName = path + "Imaging.exe"
	psInfo.Arguments = "Rowthon 1"
	psInfo.CreateNoWindow = False 
	psInfo.UseShellExecute = False
	psInfo.RedirectStandardOutput = True

	p = Process.Start(psInfo)

#
# 部品画像処理
#   tr_id: ﾄﾗﾝｻﾞｸｼｮﾝID
# partsId: ﾊﾟｰﾂID
#     ang: 画像処理時の処理角度 (30.5°--> 30.5 * 100000) [-180°～180°]
#      cx: ﾊﾟｰﾂ中心位置X        (+5mm -> 5 * 10000)       [-20mm ～ 20mm]
#      cy: ﾊﾟｰﾂ中心位置Y
#    mode: 画像処理ﾓｰﾄﾞ区分     (0:単一部品画像処理)
#
def parts_imaging(tr_id, partsId, ang, cx, cy, mode=0, tim=1000):
	# Imaging での単位は x100000 => Rowthon:x10000 なので 10倍する
	ang = ang * 10

	if ang < (-180*100000) or ang > (180 * 100000):
		return (-2,0,0,0)
	if cx < (-20 * 10000) or cx > (20 * 10000):
		return (-2,0,0,0)
	if cy < (-20 * 10000) or cy > (20 * 10000):
		return (-2,0,0,0)
	if mode != 0:
		return (-2,0,0,0)

	ret = send_mission(tr_id,missionCode.CODE_IMAGING_PARTIMAGING,partsId,ang,cx,cy,mode,timeout=tim)

	if ret.result != 1:
		if ret.arg1 == 0x164102:
			print "err[0x{0:X}]: mission argument:{1},{2}".format(ret.arg1,partsId,ang)
		elif ret.arg1 == 0x164017:
			print "err[0x{0:X}]: data group".format(ret.arg1)
		elif ret.arg1 == 0x16401B:
			print "err[0x{0:X}]: ID={1} err".format(ret.arg1,partsId)
		elif ret.arg1 == 0x164002:
			print "err[0x{0:X}]: etc...".format(ret.arg1)
		elif ret.arg1 == 0x164011:
			print "err[0x{0:X}]: shared memory error".format(ret.arg1)
		else:
			print "err[0x{0:X}]: imageProc error".format(ret.arg1)

		return (-3,ret.arg1,0,0)

	# 角度は Rowthon単位へ変換する
	return (0,ret.arg2,ret.arg3,(ret.arg4 / 10))

#
# ﾏｰｸ画像処理
#    tr_id: ﾄﾗﾝｻﾞｸｼｮﾝID
#   markId: ﾏｰｸID
#      ang: 画像処理時の処理角度 (30.5°--> 30.5 * 100000) [-90°～90°]
#      tim: ﾀｲﾑｱｳﾄ(ms)
#
def mark_imaging(tr_id, markId, ang, tim = 1000):
	# Imaging での単位は x100000 => Rowthon:x10000 なので 10倍する
	ang = ang * 10

	if ang < (-90 * 100000) or ang > (90 * 100000):
		return (-2,0,0,0)

	ret = send_mission(tr_id,missionCode.CODE_IMAGING_WORKIMAGING,markId,ang, timeout = tim)

	if ret.result != 1:
		if ret.arg1 == 0x164102:
			print "err[0x{0:X}]: mission argument:{1},{2}".format(ret.arg1,markId,ang)
		elif ret.arg1 == 0x164017:
			print "err[0x{0:X}]: data group".format(ret.arg1)
		elif ret.arg1 == 0x16401B:
			print "err[0x{0:X}]: ID={1} err".format(ret.arg1,markId)
		elif ret.arg1 == 0x164002:
			print "err[0x{0:X}]: etc...".format(ret.arg1)
		elif ret.arg1 == 0x164011:
			print "err[0x{0:X}]: shared memory error".format(ret.arg1)
		else:
			print "err[{0}-0x{1:X}]: imageProc error".format(ret.result,ret.arg1)

		return (-3,ret.arg1,0,0)

	# 角度は Rowthon単位へ変換する
	return (0,ret.arg2,ret.arg3,(ret.arg4 / 10))

#
# 画像取り込み指示
#  tr_id: ﾄﾗﾝｻﾞｸｼｮﾝID
#    tim: 撮像ﾀｲﾑｱｳﾄ時間(ms) [1～10000ms]
#
def take_image(tr_id, tim):
	if tim < 1 or tim > 10000:
		return -2

	ret = send_mission(tr_id,missionCode.CODE_IMAGING_TAKEIMAGE,tim)

	if ret.result != 1:
		if ret.arg1 == 0x164102:
			print "err[0x{0:X}]: mission argument:{1}".format(ret.arg1,tim)
		elif ret.arg1 == 0x164005:
			print "err[0x{0:X}]: grab".format(ret.arg1)
		return -3

	return 0

#
# 画像処理準備処理
#    tr_id: ﾄﾗﾝｻﾞｸｼｮﾝID
# dataType: ﾃﾞｰﾀ区分      (1:ﾊﾟｰﾂﾃﾞｰﾀ 2:ﾏｰｸﾃﾞｰﾀ)
#   dataId: ﾊﾟｰﾂID,ﾏｰｸID
#      spd: ｼｬｯﾀｰｽﾋﾟｰﾄﾞ%  (100% -> 100)  [1 ～ 16100]
#
def ready_imaging(tr_id, dataType, dataId, spd):
	if dataType != 1 and dataType != 2:
		return -2
	if spd < 1 or spd > 16100:
		return -2	

	ret = send_mission(tr_id,missionCode.CODE_IMAGING_READYIMAGING,dataType,dataId,spd)

	if ret.result != 1:
		if ret.arg1 == 0x164102:
			print "err[0x{0:X}]: mission argument:{1}".format(ret.arg1,dataType)
		elif ret.arg1 == 0x164013:
			print "err[0x{0:X}]: shutter spd:{1}".format(ret.arg1,spd)
		elif ret.arg1 == 0x164006:
			print "err[0x{0:X}]: etc..".format(ret.arg1)
		else:
			print "err[0x{0:X}]: etc2..".format(ret.arg1)

		return -3

	return 0

#
# ｼｪｲﾌﾟﾃﾞｰﾀの読み込み
#   tr_id: ﾄﾗﾝｻﾞｸｼｮﾝID
# partsId: ﾊﾟｰﾂID
#
def load_shape(tr_id,partsId):
	ret = send_mission(tr_id,missionCode.CODE_IMAGING_LOAD_SHAPE,partsId)

	if ret.result != 1:
		print "err[{0}]: load_shape error ID:{1}".format(ret.arg1,partsId)
		return -3

	return 0

#
# ｼｪｲﾌﾟﾃﾞｰﾀの削除処理
#   tr_id: ﾄﾗﾝｻﾞｸｼｮﾝID
# partsId: ﾊﾟｰﾂID
#
def delete_shape(tr_id, partsId):
	ret = send_mission(tr_id,missionCode.CODE_IMAGING_DEL_SHAPE,partsId)

	if ret.result != 1:
		print "err[{0}]: delete_shape error ID:{1}".format(ret.arg1,partsId)
		return -3

	return 0

#
# ﾏｰｸﾃﾞｰﾀの読み込み
#  tr_id: ﾄﾗﾝｻﾞｸｼｮﾝID
# markId: ﾏｰｸID
#
def load_mark(tr_id,markId):
	ret = send_mission(tr_id,missionCode.CODE_IMAGING_LOAD_MARK,markId)

	if ret.result != 1:
		print "err[{0}]: load_mark error ID:{1}".format(ret.arg1,markId)
		return -3

	return 0

#
# ﾏｰｸﾃﾞｰﾀの削除処理
#  tr_id: ﾄﾗﾝｻﾞｸｼｮﾝID
# markId: ﾏｰｸID
#
def delete_mark(tr_id, markId):
	ret = send_mission(tr_id,missionCode.CODE_IMAGING_DEL_MARK,markId)

	if ret.result != 1:
		print "err[0x{0:X}]: delete_mark error ID:{1}".format(ret.arg1,markId)
		return -3

	return 0

#start_imaging()
#shutdown_imaging()
