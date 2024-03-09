#define _CRT_SECURE_NO_DEPRECATE
#include <stdio.h>
#include <stdlib.h>
#include <stdbool.h>
#include "inverter.h"
#define BUFFER_SIZE 1024

int main(int argc, char* argv[]) {
    bool IsFileOutput = false;
    char* fileName = NULL;
    if (argc == 2)
    {
        fileName = argv[1];
        IsFileOutput = true;
    }

    FILE* inputFile, * outputFile = NULL;
    unsigned char buffer[BUFFER_SIZE];
    size_t bytesRead;

    inputFile = fopen("input.bin", "rb");
    if (inputFile == NULL) {
        perror("Error opening input file");
        return 1;
    }

    if (IsFileOutput)
    {
        outputFile = fopen(fileName, "wb");

        if (outputFile == NULL) {
            perror("Error opening output file");
            fclose(inputFile);
            return 1;
        }
    }

    // Читаем данные блоками и инвертируем порядок байтов
    while ((bytesRead = fread(buffer, sizeof(unsigned char), BUFFER_SIZE, inputFile)) > 0) {
        invertBytes(buffer, bytesRead);
        if (IsFileOutput)
            fwrite(buffer, sizeof(unsigned char), bytesRead, outputFile);
        else
            fwrite(buffer, sizeof(unsigned char), bytesRead, stdout);
    }

    if (IsFileOutput)
    {
        fclose(outputFile);
    }
    

    fclose(inputFile);

    return 0;
}
