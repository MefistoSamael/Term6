#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <time.h>
#include <locale.h>

int main( int argc, char** argv)
{
	char sz_msgbuf[128];
	time_t cnt_time;
	struct tm timebuf;
//-
	setlocale( LC_TIME, ""); //switch from "C" locale to default (defined by external variables)
	cnt_time = time( NULL); //current time as counter
	localtime_r( &cnt_time, &timebuf); //unpack to structured representation
	strftime( sz_msgbuf, sizeof(sz_msgbuf), "%x %X", &timebuf); //as string
	puts( sz_msgbuf);
	strftime( sz_msgbuf, sizeof(sz_msgbuf), "%a %b %d %H:%M:%S %Y", &timebuf); //as string
	puts( sz_msgbuf);
//-
	return 0;
}
