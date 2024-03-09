#include <unistd.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <signal.h>

//#include <time.h>
//#include <locale.h>

#include "timer.h"
#include "logger.h"

#define PERIOD 10

static unsigned long n_count;
static int b_exitflag;

/* -- Signal handler (old-style signal handling) 
-- */
void SigHandler( int n_signal)
{
	switch (n_signal) {
		case SIGHUP:
			n_count = 0; //re-initializing must be here!
			break;
		case SIGTERM:
			b_exitflag = 1; //flag to exit the process
			break;
		default:
			break;
	}
	signal( n_signal, SigHandler); //restore disposition
	return;
}

/* -- Main module entry point
-- */
int main( int argc, char* argv[])
{
	char sz_strbuf[512];
	pid_t n_pid;
	unsigned long n_lastcount = n_count;
//- initializing
	n_count = 0;
	n_pid = getpid();
	strcpy( sz_strbuf, argv[0]); strcat( sz_strbuf, ".log");
	LogOpen( sz_strbuf, n_pid);
	LogPost( "Starting", 0);
//- installing daemon
//- step 1
	n_pid = fork();
	switch (n_pid) {
		case -1: //the parent is here, an error occured
			LogPost( "fork() error!", 0);
			LogClose();
			exit( -1);
		case 0: //the child is here
			n_pid = getpid();
			LogPost( "The process was switched", n_pid); //with new PID
			break;
		default: //the parent is here, a child was started
			LogClose();
			exit( 0);
	}
//- step 2 (the child is here)
	setsid();
//- step 3
	n_pid = fork();
	switch (n_pid) {
		case -1: //the parent is here, an error occured
			LogPost( "fork() error!", 0);
			LogClose();
			exit( -1);
		case 0: //the child is here
			n_pid = getpid();
			LogPost( "The process was double switched", n_pid); //with new PID
			break;
		default: //the parent is here, a child was started
			LogClose();
			exit( 0);
	}
//- step 4 (the second child is here)
	signal( SIGTTOU, SIG_IGN);
	signal( SIGTTIN, SIG_IGN);
	signal( SIGTSTP, SIG_IGN);
	signal( SIGHUP, SigHandler);
	signal( SIGTERM, SigHandler);
//- step 5
	chdir( "/");
	fclose( stdin); fclose( stdout); fclose( stderr); 
//- main loop
	LogPost( "Waiting for events", 0);
	while (! b_exitflag) {
		sleep( PERIOD);
		if (n_count < n_lastcount) { //detect re-initializing (indirectly)
			LogPost( "Re-initialization occured!", 0);
		}
		n_count += PERIOD;
		n_lastcount = n_count;
		sprintf( sz_strbuf, "Counter: %ld seconds", n_count);
		LogPost( sz_strbuf, 0);
	}
//- closing, then exit
	LogPost( "Exiting", 0);
	LogClose();
	exit( 0);
}
