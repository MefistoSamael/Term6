#define _CRT_SECURE_NO_DEPRECATE
#include <stdio.h>
#include <stdlib.h>
#include <stdbool.h>
#include "inverter.h"
#define BUFFER_SIZE 1024

int main(int argc, char* argv[]) {
    bool IsFileOutput = false;
    char* inputFileName = NULL;
    char* outputFileName = NULL;

    if (argc == 2) {
        inputFileName = argv[1];
    } else if (argc == 3) {
        IsFileOutput = true;
        inputFileName = argv[1];
        outputFileName = argv[2];
    } else {
        fprintf(stderr, "Usage: %s <output_inputFileName>\n", argv[0]);
        return 1;
    }

    FILE* inputFile, * outputFile = NULL;
    unsigned char buffer[BUFFER_SIZE];
    size_t bytesRead;

    inputFile = fopen(inputFileName, "rb");
    if (inputFile == NULL) {
        perror("Error opening input file");
        return 1;
    }

    if (IsFileOutput) {
        outputFile = fopen(outputFileName, "wb");

        if (outputFile == NULL) {
            perror("Error opening output file");
            fclose(inputFile);
            return 1;
        }
    }

    // Инвертируем байты и записываем в выходной файл или stdout
    while ((bytesRead = fread(buffer, sizeof(unsigned char), BUFFER_SIZE, inputFile)) > 0) {
        invertBytes(buffer, bytesRead);
        if (IsFileOutput)
            fwrite(buffer, sizeof(unsigned char), bytesRead, outputFile);
        else
            fwrite(buffer, sizeof(unsigned char), bytesRead, stdout);
    }
    
    //fwrite("\n", strlen("\n"), 1, outputFile);
    if (IsFileOutput)
    	fputc('\n', outputFile);
    else
    	fputc('\n', stdout);
    
    if (IsFileOutput) {
        fclose(outputFile);
    }

    fclose(inputFile);

    return 0;
}

