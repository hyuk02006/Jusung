//packet.h
#pragma once

#define PACK_POINT		1	   //아이디,패스워드 , 이름
#define ACK_POINT		11	   //아이디,패스워드 , 이름


struct PACKETPOINT
{
	int flag;
	int x;
	int y;
	int width;
	int r;
	int g;
	int b;

	static PACKETPOINT CreatePacket(int x, int y);
};
