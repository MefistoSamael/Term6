#include <stdlib.h>

void invertBytes(unsigned char* buffer, size_t bufferSize) {
    size_t left = 0;
    size_t right = bufferSize - 1;
    
    // Инвертируем байты, оставляя символ конца строки/файла на месте
    while (left < right) {
        //if (buffer[left] != '\n' && buffer[right] != '\n') {
    	unsigned char temp = buffer[left];
    	buffer[left] = buffer[right];
    	buffer[right] = temp;
        //}
        left++;
        right--;
    }
}

