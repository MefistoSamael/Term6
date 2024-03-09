#include "timer.h"

#include <unistd.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <locale.h>

char* GetTimestamp( char* psz_tsbuf, size_t n_buflen)
{
	char sz_strbuf[128];
	time_t cnt_time;
	struct tm timebuf;
//-
	cnt_time = time( NULL);
	localtime_r( &cnt_time, &timebuf);
	strftime( sz_strbuf, sizeof(sz_strbuf), "%x %X", &timebuf);
//-
	if (psz_tsbuf != NULL) { strncpy( psz_tsbuf, sz_strbuf, n_buflen-1); psz_tsbuf[n_buflen-1] = '\0';}
	return psz_tsbuf;
}
