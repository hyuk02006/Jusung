//packet.h
#pragma once

#define PACK_POINT		1	   //���̵�,�н����� , �̸�
#define ACK_POINT		11	   //���̵�,�н����� , �̸�


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
