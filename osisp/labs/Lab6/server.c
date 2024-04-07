#include <stdio.h>
#include <string.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <unistd.h>
#include <pthread.h>

#define MAX_CLIENTS 100
#define BUF_SIZE 2000
#define NAME_SIZE 20

struct client {
    int id;
    char name[NAME_SIZE];
    int sock;
};

struct client clients[MAX_CLIENTS];
int client_count = 0;

void *handle_client(void *arg) {
    int id = *((int *)arg);
    char buf[BUF_SIZE];
    int len;

    while((len = recv(clients[id].sock, buf, BUF_SIZE, 0)) > 0) {
        buf[len] = '\0';
        for(int i=0; i<client_count; i++) {
            if(i != id) {
                send(clients[i].sock, buf, strlen(buf), 0);
            }
        }
    }

    close(clients[id].sock);
    clients[id].sock = -1;

    return NULL;
}

int main(int argc, char *argv[]) {
    int server_sock, client_sock, c;
    struct sockaddr_in server, client;
    pthread_t client_thread;
    int *thread_id;

    if(argc != 2) {
        printf("Usage: %s <port>\n", argv[0]);
        return 1;
    }

    server_sock = socket(AF_INET, SOCK_STREAM, 0);
    if(server_sock == -1) {
        printf("Could not create socket");
        return 1;
    }

    server.sin_family = AF_INET;
    server.sin_addr.s_addr = INADDR_ANY;
    server.sin_port = htons(atoi(argv[1]));

    if(bind(server_sock, (struct sockaddr *)&server, sizeof(server)) < 0) {
        printf("Bind failed");
        return 1;
    }

    listen(server_sock, 3);

    printf("Waiting for incoming connections...\n");
    c = sizeof(struct sockaddr_in);

    while((client_sock = accept(server_sock, (struct sockaddr *)&client, (socklen_t*)&c))) {
        printf("Connection accepted\n");
        clients[client_count].sock = client_sock;
        clients[client_count].id = client_count;
        sprintf(clients[client_count].name, "Client %d", client_count);

        thread_id = malloc(sizeof(int));
        *thread_id = client_count;
        pthread_create(&client_thread, NULL, handle_client, (void *)thread_id);

        client_count++;
    }

    if(client_sock < 0) {
        printf("Accept failed");
        return 1;
    }

    return 0;
}