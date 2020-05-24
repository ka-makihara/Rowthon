#-*- coding: UTF-8 -*-


# IoData ｸﾗｽ定義の読み込み
from ioData import *

# I/O 名 = [portNo, bitNo, OnOff]
di000N = IoData(0,0,0,0)
di000A = IoData(0,0,1,1)
di001N = IoData(0,1,0,2)
di001A = IoData(0,1,1,3)
di002N = IoData(0,2,0,4)
di002A = IoData(0,2,1,5)
di003N = IoData(0,3,0,6)
di003A = IoData(0,3,1,7)
di004N = IoData(0,4,0,8)
di004A = IoData(0,4,1,9)
di005N = IoData(0,5,0,10)
di005A = IoData(0,5,1,11)
di006N = IoData(0,6,0,12)
di006A = IoData(0,6,1,13)
di007N = IoData(0,7,0,14)
di007A = IoData(0,7,1,15)

di008N = IoData(0,8,0,16)
di008A = IoData(0,8,1,17)
di009N = IoData(0,9,0,18)
di009A = IoData(0,9,1,19)
di00AN = IoData(0,10,0,20)
di00AA = IoData(0,10,1,21)
di00BN = IoData(0,11,0,22)
di00BA = IoData(0,11,1,23)
di00CN = IoData(0,12,0,24)
di00CA = IoData(0,12,1,25)
di00DN = IoData(0,13,0,26)
di00DA = IoData(0,13,1,27)
di00EN = IoData(0,14,0,28)
di00EA = IoData(0,14,1,29)
di00FN = IoData(0,15,0,30)
di00FA = IoData(0,15,1,31)

di010N = IoData(0,16,0,32)
di010A = IoData(0,16,1,33)
di011N = IoData(0,17,0,34)
di011A = IoData(0,17,1,35)
di012N = IoData(0,18,0,36)
di012A = IoData(0,18,1,37)
di013N = IoData(0,19,0,38)
di013A = IoData(0,19,1,39)
di014N = IoData(0,20,0,40)
di014A = IoData(0,20,1,41)
di015N = IoData(0,21,0,42)
di015A = IoData(0,21,1,43)
di016N = IoData(0,22,0,44)
di016A = IoData(0,22,1,45)
di017N = IoData(0,23,0,46)
di017A = IoData(0,23,1,47)

di018N = IoData(0,24,0,48)
di018A = IoData(0,24,1,49)
di019N = IoData(0,25,0,50)
di019A = IoData(0,25,1,51)
di01AN = IoData(0,26,0,52)
di01AA = IoData(0,26,1,53)
di01BN = IoData(0,27,0,54)
di01BA = IoData(0,27,1,55)
di01CN = IoData(0,28,0,56)
di01CA = IoData(0,28,1,57)
di01DN = IoData(0,29,0,58)
di01DA = IoData(0,29,1,59)
di01EN = IoData(0,30,0,60)
di01EA = IoData(0,30,1,61)
di01FN = IoData(0,31,0,62)
di01FA = IoData(0,31,1,63)

# OUT Map
do000 = IoData(0, 0,0,64)
do001 = IoData(0, 1,0,64)
do002 = IoData(0, 2,0,66)
do003 = IoData(0, 3,0,66)
do004 = IoData(0, 4,0,67)
do005 = IoData(0, 5,0,68)
do006 = IoData(0, 6,0,69)
do007 = IoData(0, 7,0,70)
do008 = IoData(0, 8,0,71)
do009 = IoData(0, 9,0,72)
do00A = IoData(0,10,0,73)
do00B = IoData(0,11,0,74)
do00C = IoData(0,12,0,75)
do00D = IoData(0,13,0,76)
do00E = IoData(0,14,0,77)
do00F = IoData(0,15,0,78)

do010 = IoData(0,16,0,79)
do011 = IoData(0,17,0,80)
do012 = IoData(0,18,0,81)
do013 = IoData(0,19,0,82)
do014 = IoData(0,20,0,83)
do015 = IoData(0,21,0,84)
do016 = IoData(0,22,0,85)
do017 = IoData(0,23,0,86)
do018 = IoData(0,24,0,87)
do019 = IoData(0,25,0,88)
do01A = IoData(0,26,0,89)
do01B = IoData(0,27,0,90)
do01C = IoData(0,28,0,91)
do01D = IoData(0,29,0,92)
do01E = IoData(0,30,0,93)
do01F = IoData(0,31,0,94)

_ioMap = [di000N,di000A, di001N,di001A, di002N,di002A, di003N,di003A, di004N,di004A, di005N,di005A,
		 di006N,di006A, di007N,di007A, di008N,di008A, di009N,di009A, di00AN,di00AA, di00BN,di00BA,
		 di00CN,di00CA, di00DN,di00DA, di00EN,di00EA, di00FN,di00FA, di010N,di010A, di011N,di011A,
		 di012N,di012A, di013N,di013A, di014N,di014A, di015N,di015A, di016N,di016A, di017N,di017A,
		 di018N,di018A, di019N,di019A, di01AN,di01AA, di01BN,di01BA, di01CN,di01CA, di01DN,di01DA,
		 di01EN,di01EA, di01FN,di01FA]