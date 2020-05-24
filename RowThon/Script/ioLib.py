#-*- coding: UTF-8 -*-

#
# Rowthon I/O ﾗｲﾌﾞﾗﾘ
#
import traceLib
from iomap import *

version = 1.00

#
#
#
def init(boardName):
	return Io.Open(boardName)

#
# ﾋﾞｯﾄ単位のI/O出力
#
def out(port, bit, onoff):
	traceLib.trace("out=({0},{1},{2})".format(port,bit,onoff))
	return Vm.IoOut(port,bit,onoff)

#
# ﾎﾟｰﾄ単位のI/O出力
#
def out_port(port, data):
	return Vm.IoOutPort(port,data)

#
# ﾎﾟｰﾄ指定のI/O入力値取得
#
def get(port,bit):
	traceLib.trace("get=({0},{1})".format(port,bit))
	return Vm.IoIn(port,bit)
