//packet.h
#pragma once

#define PACK_NEWMEMBER 	1     //아이디,패스워드 , 이름
#define PACK_LOGIN		2
#define PACK_LOGOUT		3

#define ACK_NEWPMEMBER_S     11   // 나머지 정보는. echo
#define ACK_NEWPMEMBER_F     12  //  나머지 정보는 echo 
#define ACK_LOGIN_S			  13  //  아이디 패스워드 이름
#define ACK_LOGIN_F			  14  //  나머지 정보는 echo 
#define ACK_LOGOUT_S		  15  //  나머지 정보는 echo 	
#define ACK_LOGOUT_F		  16  //  나머지 정보는 echo 


struct PACKETNEWMEMBER
{
	int flag;
	char id[10];
	char pw[10];
	char name[20];

};
typedef PACKETNEWMEMBER PACKLOGIN, PACKLOGOUT;
