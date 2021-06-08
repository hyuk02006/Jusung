//packet.h
#pragma once

#define PACK_NEWMEMBER 		1	   //아이디,패스워드 , 이름
#define PACK_LOGIN			2	
#define PACK_LOGOUT			3
#define PACK_SHORTMESSAGE	4		//이름,메시지,일자
#define PACK_MEMOMESSAGE	5		//이름,메시지,일자



#define ACK_NEWPMEMBER_S     11	 // 나머지 정보는. echo
#define ACK_NEWPMEMBER_F     12  //  나머지 정보는 echo 
#define ACK_LOGIN_S			  13  //  아이디 패스워드 이름
#define ACK_LOGIN_F			  14  //  나머지 정보는 echo 
#define ACK_LOGOUT_S		  15  //  나머지 정보는 echo 	
#define ACK_LOGOUT_F		  16  //  나머지 정보는 echo 
#define ACK_SHORTMESSAGE	  17  //이름,메시지,일자
#define ACK_MEMOMESSAGE		  18		//이름,메시지,일자



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