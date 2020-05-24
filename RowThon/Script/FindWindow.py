#-*- coding: UTF-8 -*-

from ctypes import *
from ctypes.wintypes import *

import mmap
import struct

import missionCode
import winApi

mm = mmap.mmap(-1,1024,tagname="ShMem")

text = create_unicode_buffer(255)

hWnd = FindWindow(None,'0016_IMAGING')


    copyData = COPYDATASTRUCT(123,456)
    copyData.dwData = 100
    copyData.cbData = sizeof(data)
    copyData.lpData = cast(byref(data),c_void_p)


    hWnd = FindWindow(None,'PyMsgTestSv')
#    text = create_string_buffer(100)
#    text2= create_unicode_buffer(100)

    SendMessage(hWnd,WM_COPYDATA,123,cast(byref(copyData),c_void_p).value)
if hWnd != None:
	GetWindowText(hWnd,text,sizeof(text))


	mm.write( struct.pack('I',int(0xAABBCCDD)) )

	SendMessage(hWnd,0x401,123,text)


mm.close()
