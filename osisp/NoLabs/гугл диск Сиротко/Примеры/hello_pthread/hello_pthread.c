#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <pthread.h>

#define N_THREADS 3

struct thread_control_block {
	unsigned id;
	pthread_mutex_t* mutex_stdout;
};
typedef struct thread_control_block TCB;

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
//-
	if ((p_tcb = (TCB*)p_arg) == NULL)
		return NULL;
//-
	sleep( 2);
	sprintf( sz_msg, "Hello world from pthread %u\n", p_tcb->id);
	tputs( sz_msg, p_tcb->mutex_stdout);
	sleep( 1);
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
//-
	for (i=0; i<N_THREADS; ++i) {
		tcbs[i].id = i; tcbs[i].mutex_stdout = &mtx;
		if (pthread_create( &pthreads[i], NULL, thread_routine, (void*)(&tcbs[i])) != 0) {
			fputs( "Error creating pthread(s)\n", stderr);
			exit( 1);
		}
	}
//-
	sleep( 1);
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
