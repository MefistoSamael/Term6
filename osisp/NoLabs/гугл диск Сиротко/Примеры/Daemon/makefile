dumbd : dumbd.o timer.o logger.o
	gcc -o $@ $^
dumbd.o : dumbd.c
	gcc -c $^
timer.o : timer.c
	gcc -c $^
logger.o : logger.c
	gcc -c $^
clean :
	rm *.o dumbd
