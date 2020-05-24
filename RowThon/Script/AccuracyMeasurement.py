#-*- coding: UTF-8 -*-


import debugLib as debug
import visionLib as vision
import servoLib as servo
import ctrlLib as ctrl
import ioLib as io

import math
import random

XY_MOVE_SPEED = 10000000	#軸移動速度

ACC_COUNT = 15	#同一箇所での測定回数

INPOS_MODE = 0	#静定待ちﾓｰﾄﾞ (0:ﾀｲﾏｰ 1:座標確認)

INPOS_TIME = 20	#ﾀｲﾏｰによる静定待ちの場合の待ち時間(秒) ※100ms -> 0.1

#INPOS_MODE が 1(座標確認)の場合のﾊﾟﾗﾒｰﾀ
INPOS_CNT = 5	#静定確認回数(ｻｰﾎﾞ座標差分が変化しない回数)
X_SUB = 1000	#静定しているとみなす変化量
Y_SUB = 1000

#TARGET_POS = {"X":3894820,"Y":2611590,"Z":260682,"A":1398160}	#最終目的位置
#TARGET_POS = {"X":5456730,"Y":2535120,"Z":260682,"A":1398160}	#最終目的位置
#TARGET_POS = {"X":-1007580,"Y":3477070,"Z":260682,"A":1398160}	#最終目的位置
TARGET_POS = {"X":4690000,"Y":-106200,"Z":260682,"A":1398160}	#最終目的位置

#移動開始ｴﾘｱ
SRC_POS = [
[ 300.00,-270.00, 640.00, 330.00]	#供給ｴﾘｱ
]
#[ 150.00, 270.00, 270.00, 305.00],	#搬送ｴﾘｱ
#[-135.00, 270.00, 150.00, 550.00]	#装着ｴﾘｱ
#]

#
# ﾌｧｲﾙへの書き込みと標準出力
#
def write_data(fid,str):
	print str
	fid.write(str+"\n")
#
# 引数で指定されたｴﾘｱ内でﾗﾝﾀﾞﾑにX,Yを得る
#
def rand_pos(areaData):
	#sx = random.randint(areaData[0],areaData[2])
	#sy = random.randint(areaData[1],areaData[3])

	#vpos=(int(sx * 10000.0),int(sy * 10000.0))
	#return vpos

	#固定座標
	return [2970000,913030]


#
# 軸移動と移動完了待ち
#   移動完了:0
#   移動失敗:< 0
#
def move_servo(pos,tim):
	id = servo.move_unsafe({"X":pos[0],"Y":pos[1]})
	if id < 0:
		return -1

	ret= ctrl.wait([id],30000)
	if ret != id:
		return -2

	return 0

#
# 静定待ち
#   静定完了:0
#   静定失敗:1
#
def wait_inpos(delayTime = 0.0, mode = 0):

	if mode == 0:
		#時間で静定待ちをするなら
		time.sleep(delayTime)

		pos1 = Sv.GetServoPos()	#現在座標
		return 0,pos1

	else:
		#座標を確認しつつ静定待ち
		stCnt = 0
		time.sleep(0.1)	#100ms
		for nn in range(10 * INPOS_CNT):
			#現在座標
			pos2 = Sv.GetServoPos()

			#100ms前の座標との差分
			ret = [pos2[i]-pos1[i] for i in range(len(pos1))]

			if ret[0] < X_SUB and ret[1] < Y_SUB:
				#差分が規定値以下である
				stCnt += 1
				if stCnt > INPOS_CNT:
					#位置差分が指定値以下という状態が指定回数連続しているなら静定
					return 0,pos2
			else:
				#差分が規定値以上なので連続回数はﾘｾｯﾄする
				stCnt = 0

			#座標を更新
			pos1 = pos2
			time.sleep(0.1)	#100ms

		#静定失敗
		return 1,pos2


#
# 測定
#   fid : ﾌｧｲﾙID
#   delayTime: InPos を待つ時間
#
def execute(fid, count, delayTime=0.0):
	cnt = 0
	for pos in SRC_POS:
		#指定ｴﾘｱ内で25回
		for n in range(count):
			#ﾗﾝﾀﾞﾑなXY座標
			sPos = rand_pos(pos)

			#開始位置へ移動
			ret = move_servo(sPos,30000)
			if ret < 0:
				return

			#目標位置へ移動
			id = servo.move_unsafe(TARGET_POS)
			if id < 0:
				return

			#移動完了待ち
			ret = ctrl.wait([id],30000)

			#静定待ち
			(ret,st_pos) = wait_inpos(delayTime,INPOS_MODE)
			if ret != 0:
				#静定失敗
				return

			#画像取り込み
			vision.grab(0,1)

			#基準ﾏｰｸ(64×64 黒丸)の画像処理
			vision.execute(2)

			vPos = []	#画像処理結果領域
			vision.get_position(0,vPos)	#角度補正は0で処理結果を取得する

			#開始位置(X,Y) 目標位置(X,Y,Z,A,B) 画像処理結果(X,Y)
			write_data(fid,"{0},{1},{2},{3},{4},{5},{6},{7},{8}".format(sPos[0],sPos[1],st_pos[0],st_pos[1],st_pos[2],st_pos[3],st_pos[4],vPos[0],vPos[1]))
			cnt += 1

###########################################
#
# 実行開始
#

#結果出力先ﾌｧｲﾙ(<実行ﾊﾟｽ>/bin/Debug/)
fid = open("Accuracy.txt","w")

#ﾀｲﾄﾙ
write_data(fid,"Delay,{0} sec".format(INPOS_TIME))
write_data(fid,"Speed,{0}".format(XY_MOVE_SPEED))
write_data(fid,"CommandPos, ,{0},{1},{2},{3}".format(TARGET_POS['X'],TARGET_POS['Y'],TARGET_POS['Z'],TARGET_POS['A']))
write_data(fid,"SX,SY,TX,TY,TZ,TA,TB,VX,VY")

io.out(0,21,1)	#LED On

#移動速度設定
servo.set_param(5,2,XY_MOVE_SPEED)
servo.set_param(6,2,XY_MOVE_SPEED)

#測定
execute(fid,ACC_COUNT, INPOS_TIME)

io.out(0,21,0)	#LED On

fid.close()
