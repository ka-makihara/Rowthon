#-*- coding: UTF-8 -*-

#
# Rowthon ｺﾝﾄﾛｰﾙ・ﾗｲﾌﾞﾗﾘ
#

from ioData import *
import servoLib as servo

version = 1.00

#
# ｲﾍﾞﾝﾄ待ち
#
def wait(eventList, time):
#	evList = [ev if isinstance(ev,IoData) == False else ev.eventNo for ev in eventList]
	evList = [ev.eventNo if isinstance(ev,IoData) == True else (ev if isinstance(ev,int) == True else ev._number) for ev in eventList]
#	print evList
#	evList = []
#	for ev in eventList:
#		if isinstance(ev, IoData):
#			evList.append(ev.eventNo)
#		else:
#			evList.append(ev)
#	ret = Rowthon.Wait( evList, time )
	ret = Vm.Wait( evList, time )
	return ret

#
# 状態取得
#
def get_status():
	return Vm.GetStatus()

#
# 制御ﾓｰﾄﾞ取得
#
def get_ctrl_mode():
	return Vm.GetCtrlMode()

#
# 制御ﾓｰﾄﾞ設定
#
def set_ctrl_mode(mode):
	return Vm.SetCtrlMode(mode)

#
# ｴﾗｰｸﾘｱ
#
def clear_error():
	return Vm.ClearError()

#
# ｲﾍﾞﾝﾄ状態の取得
#
def event_status(eventId):
	return Vm.EventStatus(eventId)

#
# wait 状態になっているｲﾍﾞﾝﾄを強制的にｷｬﾝｾﾙ状態にする
#
def cancel_wait(eventId):
	return Vm.CancelWait(eventId)

#
# ｽｸﾘﾌﾟﾄで定義されている変数の値を取得する
#   ｽｸﾘﾌﾟﾄﾌﾟﾛｸﾞﾗﾑで、定義した変数に実行結果を設定することで
#   ｽｸﾘﾌﾟﾄの実行結果を取得する
#
def get_program_result(resultName):
#	str = "'" + resultName + "'"
	return Vm.GetProgramResult(resultName)

#
# execute_program()を実行すると実行完了待ち用のｲﾍﾞﾝﾄIDが取得されるので
#  このIDを停止させることで、ｽｸﾘﾌﾟﾄﾌﾟﾛｸﾞﾗﾑの実行を中止させます
#
def stop_program(eventId):
	Sv.StopCommand()
	if setting.SvCtrlName == "TECHNO":
		Sv.Reset()

	return Vm.StopProgram(eventId)

#
# 非常停止時に実行される処理
#
def exec_emg(ioInput):
	servo.power(0)
