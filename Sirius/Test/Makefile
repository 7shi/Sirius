TARGET  = $(SOURCES:.c=)
CC      = alpha-elf-gcc
CFLAGS  = -mcpu=21164a
LDFLAGS = -nostdlib -e entry -Ttext 0x10000 -s
SOURCES = 1.c 2.c 3.c 4.c

all: $(TARGET)

clean:
	rm -f $(TARGET)
