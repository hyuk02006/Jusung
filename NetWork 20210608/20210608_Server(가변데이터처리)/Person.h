//Person.h
#pragma once

#include <string.h>

struct Person
{
	char name[20];
	int age;
	bool gender;

	Person() 
	{
		strcpy_s(name, sizeof(name), "");
		age = -1;
		gender = false;
	}

	Person(const char* _name, int _age, bool _gender)
	{
		strcpy_s(name, sizeof(name), _name);
		age = _age;
		gender = _gender;
	}
};
