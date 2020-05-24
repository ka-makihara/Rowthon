#-*- coding: UTF-8 -*-

class MsUnit:
	def __init__(self,iQty,jQty,span,iUnit,jUnit):
		self._iQty = iQty
		self._jQty = jQty
		self._span = span
		self._iUnit = iUnit
		self._jUnit = jUnit

		self._iSpan = iUnit * (iQty - 1)
		self._jSpan = jUnit * (jQty - 1)
		self._iCenter = self._iSpan / 2
		self._jCenter = self._jSpan / 2
		self._adr = 0

	def vdGet(self, xDist, yDist):
		yDist = self._jCenter - yDist
		xDist += self._iCenter

		iQty = xDist / self._iUnit
		iMod = xDist % self._iUnit
		jQty = yDist / self._jUnit
		jMod = yDist % self._jUnit

		offset = jQty * self._span + iQty

		upLeft = Vision.GetPixel(offset)
		upRight= Vision.GetPixel(offset+1)
		dwLeft = Vision.GetPixel(offset + self._span)
		dwRight = Vision.GetPixel(offset + self._span + 1)
		upDiff = upRight - upLeft
		dwDiff = dwLeft - dwRight

		upLogical = ((upDiff * iMod)/self._iUnit) + upLeft
		dwLogical = ((dwDiff * iMod)/self._jUnit) + dwLeft

		ret = (((dwLogical - upLogical) * jMod) / self._jUnit) + upLogical

		return ret


unit = MsUnit(640,640,640,180000,180000)

def get_pixel(xDist, yDist):
	aa = unit.vdGet(xDist,yDist)
	print aa
