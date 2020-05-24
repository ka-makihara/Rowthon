#-*- coding: UTF-8 -*-

# Start up script for IronPythonConsole

import os
import sys
import time

#sys.path.append('C:\\Program Files (x86)\\IronPython 2.7\\Lib')


import clr
clr.AddReference("System.Windows.Forms")
from System.Windows.Forms import *

import ioLib as io
import servoLib as servo
import visionLib as vision

#
#numpy を使用する場合、import すると以下のDLLが参照できないようなので
# 直接、参照を行うようにする。この状態で import numpy でﾛｰﾄﾞ可能
#
#  IronPython では、*.pyd(ｺﾝﾊﾟｲﾙ済みﾗｲﾌﾞﾗﾘ)のﾛｰﾄﾞは出来ないようである。
#
#clr.AddReference("mtrand.dll")

# Set properties for proper behaviour with PyDoc
os.environ['TERM'] = 'Rowthon'

#ret = Io.Open(setting.IoBoardName)
ret = io.init(setting.IoBoardName)

if ret < 0:
#	MessageBox.Show("IO init error")
	print "I/O Init Error"

#ｻｰﾎﾞｼｽﾃﾑがﾘﾝｾｲの場合は起動時にｱﾝﾌﾟ電源をONにする
if setting.SvCtrlName == "LINSEY":
	Vm.IoOut(0,0,1)
	Vm.IoOut(0,1,1)
#	Vm.IoOut(0,2,1)
#	time.sleep(0.1)
#	Vm.IoOut(0,2,0)	

ret = servo.init(setting.SvCtrlName)
if ret < 0:
#	MessageBox.Show("TECHNO init error")
	print "{0} Servo Init Error".format(setting.SvCtrlName)

#vision.init_camera()
