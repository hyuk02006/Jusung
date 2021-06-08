//packet.h
#pragma once


#define PACK_SHORTMESSAGE	4		//이름,메시지,일자
#define PACK_MEMOMESSAGE	5		//이름,메시지,일자

#define ACK_SHORTMESSAGE	17  //이름,메시지,일자
#define ACK_MEMOMESSAGE		18		//이름,메시지,일자



struct PACKETSHORTMESSAGE
{
	int flag;
	char name[20];
	char msg[512];
	int hour;
	int min;
	int second;
};
typedef PACKETSHORTMESSAGE PAKETMEMO;


