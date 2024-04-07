CC=gcc
CFLAGS=-Wall -g
LDFLAGS=

all: server client

server: server.o
	$(CC) $(LDFLAGS) -o $@ $^

client: client.o
	$(CC) $(LDFLAGS) -o $@ $^

%.o: %.c
	$(CC) $(CFLAGS) -c -o $@ $<

run-server: server
	./server

run-client: client
	./client

clean:
	rm -f *.o server client
