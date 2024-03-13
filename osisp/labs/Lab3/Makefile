TARGET = BufferInversion
CC = gcc

Source_Folder = ./src/
Object_Folder = ./obj/
Test_Folder = ./tests/

SRC = $(wildcard $(Source_Folder)*.c)

$(TARGET) : ./obj/Inverter.o ./obj/Source.o
	$(CC) ./obj/Inverter.o ./obj/Source.o -o $(TARGET)

$(Object_Folder)%.o : $(Source_Folder)%.c
	$(CC) -c $< -o $@

clean :
	@rm $(wildcard $(Object_Folder)*.o)

tests : SimpleTest NewLineTest

SimpleTest :
	@echo "12345" > $(Test_Folder)input.bin
	@echo "\n54321" > $(Test_Folder)answer.bin
	@./BufferInversion $(Test_Folder)input.bin $(Test_Folder)output.bin
	@diff $(Test_Folder)output.bin $(Test_Folder)answer.bin
	@rm $(Test_Folder)*.bin

NewLineTest :
	@echo "123\n456" > $(Test_Folder)input.bin
	@echo "\n654\n321" > $(Test_Folder)answer.bin
	@./BufferInversion $(Test_Folder)input.bin $(Test_Folder)output.bin
	@diff $(Test_Folder)output.bin $(Test_Folder)answer.bin
	@rm $(Test_Folder)*.bin
