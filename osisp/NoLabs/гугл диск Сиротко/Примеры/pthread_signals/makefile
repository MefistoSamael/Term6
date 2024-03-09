pthread_signals : pthread_signals.o
	g++ -pthread -o $@ $^
pthread_signals.o : pthread_signals.cpp
	g++ -c -o $@ $^
clean :
	rm *.o pthread_signals
