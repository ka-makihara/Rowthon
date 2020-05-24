#-*- coding: UTF-8 -*-

#
# Rowthon ｻｰﾎﾞ ﾗｲﾌﾞﾗﾘ
#
import clr
import time

# C# のﾃﾞｨｸｼｮﾅﾘを使用するためのﾗｲﾌﾞﾗﾘ
clr.AddReference('System.Collections')
from System.Collections import Generic as DotNetCollectionsG

import traceLib
import moveLib

version = 1.00

#
# 初期化
#
def init(motionName):
	'''ｻｰﾎﾞ初期化
	params:('制御名')
		TECHNO or LINSEY
	return: ID'''
	return Sv.Open(motionName)

#
# 軸の移動(回避動作付き)
#  <dict> {"X":xxxx, "Y":yyyy, ...}
#
def move(dict):
	''' ｻｰﾎﾞ移動:回避動作付き
	param :({"AxisName":<pos>,"AxisName":<pos>,...})
	return:wait Id'''
	traceLib.trace("Tgt={0}".format(dict))
	#移動指示の(python)Dictionary ﾘｽﾄ
	moveList = []

	#軸指定が無い
	if len(dict) == 0:
		traceLib.trace("target dict empty")
		return -2

	#現在の座標
	svPos = Sv.GetServoPos()
	if moveLib.is_ok_area(svPos[0],svPos[1]) == False:
		#現在値が移動不可ｴﾘｱ
		traceLib.trace("CurrentPos Can not move:({0:d},{1:d})".format(svPos[0],svPos[1]));
		return -3

	#移動ﾘﾐｯﾄﾁｪｯｸ
	if moveLib.limit_check(dict) == False:
		traceLib.trace("target limit over");
		return -3

	#現在の座標(svPos[0]=X, svPos[1]=Y,Z,R,L)
#	svPos = Sv.GetServoPos()

	#X,Y,Z軸の回避動作
	ret = moveLib.create_xyz_root(dict,svPos,moveList)
	if ret != 0:
		traceLib.trace("ESC Move error:{0:d}".format(ret));
		return ret 

	####
#	posList = [0] * len(moveList)	#移動指示の個数分のﾘｽﾄを生成
	posList = []
	idx = 0
	if len(moveList) == 0:
		traceLib.trace("ESC Move list empty")
		return -3

	for moveDict in moveList:
		## C# 用の Dictionary を生成
		pos = DotNetCollectionsG.Dictionary[str,int]()

		keylist = moveDict.keys()	# 指示軸数
		for key in keylist:
			pos[key] = moveDict[key]

#		posList[idx] = pos
		posList.append(pos)
		idx = idx + 1

	#軸移動指令
	ev = Vm.SvProcMove(posList)

	return ev

#
# 軸の移動(回避動作無し)
#  <dict> {"X":xxxx, "Y":yyyy, ...}
#
def move_unsafe(dict):
	''' ｻｰﾎﾞ移動:回避動作無し
	param :({"AxisName":<pos>,"AxisName":<pos>,...})
	return:wait Id'''
	traceLib.trace("Tgt={0}".format(dict))
	pos = DotNetCollectionsG.Dictionary[str,int]()

	#現在の座標(svPos[0]=X, svPos[1]=Y,Z,R,L)
	svPos = Sv.GetServoPos();

	#現在地->目的地の移動で禁止領域を通過するか?
	if dict.has_key('X') == True or dict.has_key('Y') == True:
		if dict.has_key('X') == True:
			if dict.has_key('Y') == True:
				#X,Yの移動指示
				if moveLib.HitArea(svPos[0],svPos[1],dict['X'],dict['Y']) == True:
					traceLib.trace("A:{0}-{1}-{2}-{3}".format(svPos[0],svPos[1],dict['X'],dict['Y']))
					return -2
			else:
				#X軸の移動指示のみ
				if moveLib.HitArea(svPos[0],svPos[1],dict['X'],svPos[1]) == True:
					traceLib.trace("B:{0}-{1}-{2}-{3}".format(svPos[0],svPos[1],dict['X'],svPos[1]))
					return -2
		else:
			#Y軸の移動指示
			if moveLib.HitArea(svPos[0],svPos[1],svPos[0],dict['Y']) == True:
				traceLib.trace("C:{0}-{1}-{2}-{3}".format(svPos[0],svPos[1],svPos[0],dict['Y']))
				return -2
	else:
		#X,Y軸の移動指示は無い
		pass

	keylist = dict.keys()
	for key in keylist:
		pos[key] = dict[key]

	#軸移動指令
	ev = Vm.SvMove(pos)

	return ev
#
# ﾊﾟﾗﾒｰﾀ設定
#
def set_param(axisNo, code, data):
	'''ﾊﾟﾗﾒｰﾀ設定
	params: (axisNo,code,data)
	axisNo:0～
	code  :MoveSpeed=2 JogSpeed=4
	return: OK=0 ERR<0'''
	if code == 2:	#MoveSpeed
		if axisNo == 5:
			#J1(0),J2(1),J3(2) --> XY軸
			if data > setting.MaxXYZMoveSpd:
				traceLib.trace("MaxXYZSpeed Err:{0}".format(data))
				return -2
			if data < setting.MinXYZMoveSpd:
				traceLib.trace("MinXYZSpeed Err:{0}".format(data))
				return -2
		elif axisNo == 6:
			#J4(3),J5(4) --> 回転軸
			if data > setting.MaxLQMoveSpd:
				traceLib.trace("MaxLQSpeed Err:{0}".format(data))
				return -2
			if data < setting.MinLQMoveSpd:
				traceLib.trace("MinLQSpeed Err:{0}".format(data))
				return -2
		else:
			traceLib.trace("axisNo Errr:{0}".format(axisNo))
			return -2

	elif code == 4:	#Jog Speed
		if data > setting.MaxJogSpd:
			return -2
		if data < setting.MinJogSpd:
			return -2
	else:
		traceLib.trace("Code Err:{0}".format(code))
		return -2

	return Sv.SetAxisParam(axisNo,code,data)
	
#
# ﾊﾟﾗﾒｰﾀ取得
#
def get_param(axisNo, code):
	'''ﾊﾟﾗﾒｰﾀ取得
	params:(axisNo,code)
		axisNo:0～
		  code:MoveSpeed=2 JogSpeed=4
	return: value or ERR<0'''
	return Sv.GetAxisParam(axisNo,code)

#
# 状態取得
#
def get_status(axisNo, code):
	'''ｻｰﾎﾞ状態取得
	params:(axisNo,code)
	  axisNo:0～
	  code  :
	return:value'''
	return Sv.GetAxisStatus(axisNo,code)

#
# 軸動作中止
#
def stop_move(eventId):
	'''移動中止
	params:(Id)
	  Id:move()の戻り値
	return: OK=0 ERR<0'''
	return Vm.StopMove(eventId)

#
# JOG 動作
#
def jog(axisNo, direction, spd):
	'''JOG動作(axisNo,direction,speed)
	 axisNo   :0～
	 direction:-1=-方向,0=停止,1=+方向
	 speed    :速度
	 return: OK=0 ERR<0'''
	return Vm.ServoJog(axisNo,direction,spd)


#
# ｻｰﾎﾞ電源
#
def power(onOff):
	'''ｻｰﾎﾞ電源のON/onOff
	params:(ON=1, OFF=0)
	return: OK=0 ERR<0'''
	ret = 0
	if setting.SvCtrlName == "TECHNO":
		if onOff == 1:
			Vm.IoOut(0,0,1)
			Vm.IoOut(0,1,1)
			Vm.IoOut(0,2,1)
			time.sleep(0.1)
			Vm.IoOut(0,2,0)
			time.sleep(2)
			ret = Sv.Power(1)
		else:
			ret = Sv.Power(0)
			Vm.IoOut(0,2,0)
			Vm.IoOut(0,1,0)
			time.sleep(0.5)
			Vm.IoOut(0,0,0)
	else:
		if onOff == 1:
			Vm.IoOut(0,0,1)
			Vm.IoOut(0,1,1)
			Vm.IoOut(0,2,1)
			time.sleep(0.1)
			Vm.IoOut(0,2,0)
			ret = Sv.Power(1)
		else:
			ret = Sv.Power(0)
			Vm.IoOut(0,2,0)
			Vm.IoOut(0,1,0)

	return ret 
