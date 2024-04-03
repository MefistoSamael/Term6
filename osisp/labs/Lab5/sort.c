#include <pthread.h>
#include <stdio.h>
#include <stdlib.h>

#define MAX_THREAD 4

struct thread_data {
    int l;
    int r;
    int *arr;
};

void merge(int arr[], int l, int m, int r) {
    int i, j, k;
    int n1 = m - l + 1;
    int n2 = r - m;

    int L[n1], R[n2];

    for (i = 0; i < n1; i++)
        L[i] = arr[l + i];
    for (j = 0; j < n2; j++)
        R[j] = arr[m + 1 + j];

    i = 0;
    j = 0;
    k = l;
    while (i < n1 && j < n2) {
        if (L[i] <= R[j]) {
            arr[k] = L[i];
            i++;
        } else {
            arr[k] = R[j];
            j++;
        }
        k++;
    }

    while (i < n1) {
        arr[k] = L[i];
        i++;
        k++;
    }

    while (j < n2) {
        arr[k] = R[j];
        j++;
        k++;
    }
}

void *merge_sort(void *threadarg) {
    struct thread_data *data = (struct thread_data *)threadarg;
    int l = data->l;
    int r = data->r;
    int *arr = data->arr;

    if (l < r) {
        int m = l + (r - l) / 2;

        struct thread_data data1, data2;
        data1.l = l;
        data1.r = m;
        data1.arr = arr;

        data2.l = m + 1;
        data2.r = r;
        data2.arr = arr;

        pthread_t threads[2];

        pthread_create(&threads[0], NULL, merge_sort, (void *)&data1);
        pthread_create(&threads[1], NULL, merge_sort, (void *)&data2);

        pthread_join(threads[0], NULL);
        pthread_join(threads[1], NULL);

        merge(arr, l, m, r);
    }
    pthread_exit(NULL);
}

int main(int argc, char *argv[]) {
    int arr[] = {12, 11, 13, 5, 6, 7};
    int arr_size = sizeof(arr) / sizeof(arr[0]);

    struct thread_data data;
    data.l = 0;
    data.r = arr_size - 1;
    data.arr = arr;

    pthread_t sort_thread;
    pthread_create(&sort_thread, NULL, merge_sort, (void *)&data);

    pthread_join(sort_thread, NULL);

    printf("Sorted array: ");
    for (int i = 0; i < arr_size; i++)
        printf("%d ", arr[i]);
    printf("\n");
    return 0;
}