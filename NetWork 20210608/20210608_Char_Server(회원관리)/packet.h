//packet.h
#pragma once

#define PACK_NEWMEMBER 	1     //���̵�,�н����� , �̸�
#define PACK_LOGIN		2
#define PACK_LOGOUT		3

#define ACK_NEWPMEMBER_S     11   // ������ ������. echo
#define ACK_NEWPMEMBER_F     12  //  ������ ������ echo 
#define ACK_LOGIN_S			  13  //  ���̵� �н����� �̸�
#define ACK_LOGIN_F			  14  //  ������ ������ echo 
#define ACK_LOGOUT_S		  15  //  ������ ������ echo 	
#define ACK_LOGOUT_F		  16  //  ������ ������ echo 


struct PACKETNEWMEMBER
{
	int flag;
	char id[10];
	char pw[10];
	char name[20];

};
typedef PACKETNEWMEMBER PACKLOGIN, PACKLOGOUT;
