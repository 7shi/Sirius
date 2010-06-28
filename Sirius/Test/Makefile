CC      = alpha-elf-gcc
CFLAGS  = -mcpu=21164a
LDFLAGS = -nostdlib -e entry -Ttext 0x10000 -s

1: 1.c
	$(CC) $(LDFLAGS) $(CFLAGS) -o $@ $<

clean:
	rm -f 1
