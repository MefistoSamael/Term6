TARGET = BufferInversion
CC = gcc

PREF_SRC = ./src/
PREF_OBJ = ./obj/
PREF_TEST = ./tests/

SRC = $(wildcard $(PREF_SRC)*.c)
OBJ = $(patsubst $(PREF_SRC)%.c, $(PREF_OBJ)%.o, $(SRC))

$(TARGET) : $(OBJ)
	$(CC) $(OBJ) -o $(TARGET)

$(PREF_OBJ)%.o : $(PREF_SRC)%.c
	$(CC) -c $< -o $@

clean :
	rm $(TARGET) $(PREF_OBJ)*.o
	rm $(PREF_TEST)*.bin

tests : SimpleTest NewLineTest

SimpleTest :
	@echo "12345 " > ./input.bin
	@echo "54321" > $(PREF_TEST)answer.bin
	@./BufferInversion $(PREF_TEST)output.bin
	@diff $(PREF_TEST)output.bin $(PREF_TEST)answer.bin
	@rm $(PREF_TEST)*.bin

NewLineTest :
	@echo "123\n456" > ./input.bin
	@echo "654\n321" > $(PREF_TEST)answer.bin
	@./BufferInversion $(PREF_TEST)output.bin
	@diff $(PREF_TEST)output.bin $(PREF_TEST)answer_1.bin
	@rm $(PREF_TEST)*.bin
