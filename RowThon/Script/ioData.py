#-*- coding: UTF-8 -*-

#
#
#

class IoData:
	def __init__(self, port, bit, onoff, evNo):
		self.port = port
		self.bit = bit
		self.onoff = onoff
		self.eventNo = evNo

	def ON(self):
		Vm.IoOut(self.port,self.bit,1)

	def OFF(self):
		Vm.IoOut(self.port,self.bit,0)

	def wait(self, timeout):
		return Vm.Wait([self.eventNo],timeout)
