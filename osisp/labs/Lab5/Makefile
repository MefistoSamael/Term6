CC=gcc
CFLAGS=-lpthread

sort: sort.c
	$(CC) sort.c $(CFLAGS) -o sort

run: sort
	./sort

clean:
	rm -f sort
