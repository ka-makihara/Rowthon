#-*- coding: UTF-8 -*-

#
# 
#
import sys

version = 1.00

def trace(msg):
	funcName = sys._getframe(1).f_code.co_name
	Vm.PyTrace(funcName,msg)
