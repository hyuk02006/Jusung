//packet.h
#pragma once

#define PACK_NEWMEMBER 		1	   //���̵�,�н����� , �̸�
#define PACK_LOGIN			2	
#define PACK_LOGOUT			3
#define PACK_SHORTMESSAGE	4		//�̸�,�޽���,����
#define PACK_MEMOMESSAGE	5		//�̸�,�޽���,����



#define ACK_NEWPMEMBER_S     11	 // ������ ������. echo
#define ACK_NEWPMEMBER_F     12  //  ������ ������ echo 
#define ACK_LOGIN_S			  13  //  ���̵� �н����� �̸�
#define ACK_LOGIN_F			  14  //  ������ ������ echo 
#define ACK_LOGOUT_S		  15  //  ������ ������ echo 	
#define ACK_LOGOUT_F		  16  //  ������ ������ echo 
#define ACK_SHORTMESSAGE	  17  //�̸�,�޽���,����
#define ACK_MEMOMESSAGE		  18		//�̸�,�޽���,����



struct PACKETNEWMEMBER
{
	int flag;
	char id[10];
	char pw[10];
	char name[20];

	static PACKETNEWMEMBER CreatePacket(const char* _name, const char* _id, const char* _pw);

	static PACKETNEWMEMBER CreatePacket(const char* _id, const char* _pw);
	static PACKETNEWMEMBER CreatePacket(const char* _id);

};
typedef PACKETNEWMEMBER PACKLOGIN, PACKLOGOUT;

struct PACKETSHORTMESSAGE
{
	int flag;
	char name[20];
	char msg[512];
	int hour;
	int min;
	int second;

	static PACKETSHORTMESSAGE CreatePacket(const char* _name,const char* _msg);
	static PACKETSHORTMESSAGE CreatePacket(const char* _name, const char* _msg,bool dummy);

};


typedef PACKETSHORTMESSAGE PAKETMEMO;