#include "pch.h"
#include "packet.h"

PACKETPOINT PACKETPOINT::CreatePacket(int x, int y)
{
	PACKETPOINT packet;

	packet.flag = PACK_POINT;
	packet.x = x;
	packet.y = y;

	return packet;
}
