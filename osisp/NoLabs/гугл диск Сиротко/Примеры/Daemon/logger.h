#ifndef	_LOGGER_H_DEFINED_

#define	_LOGGER_H_DEFINED_

#include <stdlib.h>

int LogOpen( const char* psz_logname, pid_t n_pid);
int LogPost( const char* psz_msg, pid_t n_pid);
int LogClose( void);

#endif
