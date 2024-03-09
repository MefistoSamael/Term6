#include <unistd.h>
#include <sys/types.h>
#include <sys/syscall.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <time.h>
#include <pthread.h>

#define N_THREADS 3

#define gettid() ((pid_t)syscall(SYS_gettid))

struct thread_control_block {
	unsigned id;
	pthread_mutex_t* mutex_stdout;
};
typedef struct thread_control_block TCB;

long sleep_msec( long delay_msec)
{
	struct timespec time_req, time_rem;
//-
	time_req.tv_sec = (time_t)(delay_msec / 1000);
	time_req.tv_nsec = (delay_msec % 1000) * 1000000l;
	nanosleep( &time_req, &time_rem);
//-
	return (long)(time_rem.tv_sec)*1000 + (time_rem.tv_nsec+500000l)/1000000l;
}

int tputs( const char* psz_msg, pthread_mutex_t* p_mutex)
{
	int n_res;
	if (p_mutex != NULL) {
		if (pthread_mutex_lock( p_mutex) != 0) {
			fputs( "Error while synchronization\n", stderr);
			return EOF;
		}
	}
	n_res = fputs( psz_msg, stdout);
	if (p_mutex != NULL)
		pthread_mutex_unlock( p_mutex);
	return n_res;
}

void* thread_routine( void* p_arg)
{
	TCB* p_tcb;
	char sz_msg[256];
	pid_t tid;
//-
	if ((p_tcb = (TCB*)p_arg) == NULL)
		return NULL;
	tid = gettid();
//-
	sleep_msec( 2000);
	sprintf( sz_msg, "Hello world from pthread %u (tid = %u)\n", 
		p_tcb->id, (unsigned)tid);
	tputs( sz_msg, p_tcb->mutex_stdout);
	sleep_msec( 1000);
//-
	return NULL;
}

int main( int argc, char** argv)
{
	pthread_mutex_t mtx;
	TCB tcbs[N_THREADS];
	pthread_t pthreads[N_THREADS];
	unsigned i;
	void* res;
//-
	fputs( "Threads hello's\n", stdout);
	if (pthread_mutex_init( &mtx, NULL) != 0) {
		fputs( "Error initializing mutex\n", stderr);
		exit( 1);
	}
	fprintf( stdout, "Parent pid = %u\n", getpid());
//-
	for (i=0; i<N_THREADS; ++i) {
		tcbs[i].id = i; tcbs[i].mutex_stdout = &mtx;
		if (pthread_create( &pthreads[i], NULL, thread_routine, (void*)(&tcbs[i])) != 0) {
			fputs( "Error creating pthread(s)\n", stderr);
			exit( 1);
		}
	}
//-
	sleep_msec( 1000);
	//sleep( 1);
	tputs( "Wait for threads...\n", &mtx);
	for (i=0; i<N_THREADS; ++i) {
		if (pthread_join( pthreads[i], &res) != 0) {
			fputs( "Error joining pthread(s)\n", stderr);
			exit( 1);
		}
	}
//-
	pthread_mutex_destroy( &mtx);
	exit( 0);
}
