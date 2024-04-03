#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <signal.h>

static int count = 0;

void signal_handler(int signum)
{
    pid_t pid = fork();
    if (pid == 0)
    {
        printf("Child process created and continues execution\n");
        while(1) {
            count++;
            FILE *fp = fopen("count.txt", "w");
            if (fp != NULL)
            {
                fprintf(fp, "%d", count);
                fclose(fp);
            }
            sleep(1);
            
            if (count == 200)
            	exit(0);
        }   
    }
    else
    {
        printf("Parent process terminates\n");
        printf("%d", pid);
        exit(0);
    }
}

int main()
{
    signal(SIGINT, signal_handler);
    while (1)
    {
        count++;
        FILE *fp = fopen("count.txt", "w");
        if (fp != NULL)
        {
            fprintf(fp, "%d", count);
            fclose(fp);
        }
        
        if (count == 200)
            	exit(0);
        sleep(1);
    }
    return 0;
}
