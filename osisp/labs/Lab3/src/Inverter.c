#include <stdlib.h>

void invertBytes(unsigned char* buffer, size_t bufferSize) {
    size_t left = 0;
    size_t right = bufferSize - 1;
    while (left < right) {
        unsigned char temp = buffer[left];
        buffer[left] = buffer[right];
        buffer[right] = temp;
        left++;
        right--;
    }
}
