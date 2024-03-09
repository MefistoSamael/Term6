#include "logger.h"

#include <unistd.h>
#include <stdlib.h>
#include <stdio.h>

#include "timer.h"

static FILE* p_log = NULL;
static pid_t n_storedpid = 0;

int LogOpen( const char* psz_logname, pid_t n_pid)
{
	char sz_tsbuf[128];
//-
	n_storedpid = (n_pid != 0) ? n_pid : getpid();
//-
	if (p_log != NULL) { fclose( p_log); p_log = NULL; }
	if ((p_log = fopen( psz_logname, "a")) == NULL)
		return -1;
	fprintf( p_log, "[%s] Log started by [%lu]\n", GetTimestamp( sz_tsbuf, sizeof(sz_tsbuf)), n_storedpid);
	fflush( p_log);
	return 0;
}

int LogPost( const char* psz_msg, pid_t n_pid)
{
	char sz_tsbuf[128];
//-
	if (n_pid != 0) n_storedpid = n_pid;
	if (n_storedpid == 0) n_storedpid = getpid();
//-
	if (p_log != NULL) {
		fprintf( p_log, "[%s] [%lu] %s\n", GetTimestamp( sz_tsbuf, sizeof(sz_tsbuf)), n_storedpid, psz_msg);
		fflush( p_log);
		return 0;
	}
	else return -1;
}

int LogClose( void)
{
	if (p_log != NULL) { fclose( p_log); p_log = NULL; }
	return 0;
}
