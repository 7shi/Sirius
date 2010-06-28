TARGET  = $(SOURCES:.c=)
CC      = alpha-elf-gcc
CFLAGS  = -mcpu=21164a
LDFLAGS = -nostdlib -e entry -Ttext 0x10000 -s
SOURCES = 1.c 2.c

all: $(TARGET)

1: 1.c
2: 2.c

clean:
	rm -f $(TARGET)
