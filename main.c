#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <time.h>

const unsigned MAX_LENGTH = 256;
const char* FILE_NAME = "scrbl.csv";

int g_error = 0;

// Returns the time in local time
char* get_time() {
	time_t timestamp = time(NULL);
	struct tm *time_info;
	time_info = localtime(&timestamp);
	char* time_str = asctime(time_info);
	return time_str;
}

void add_note(const char* note) {
	FILE *file = fopen(FILE_NAME, "a+");
	if (file == NULL) {
		perror("Error opening file");
		g_error = 1;
		return;
	}

	if (note != NULL) {
		char* time_str = get_time();
		fprintf(file, "%s, %s", note, time_str);
	}

	fclose(file);
}

// This is basically just the cat command at this point
void list_notes() {
	FILE *file = fopen(FILE_NAME, "r");
	if (file == NULL) {
		perror("Error opening file");
		g_error = 1;
		return;
	}

	char buffer[MAX_LENGTH];

	while (fgets(buffer, MAX_LENGTH, file)) {
		printf("%s", buffer);
	}

	fclose(file);
}

int main (int argc, char **argv) {
	int new_flag = 0;
	int list_flag = 0;
	char *note = NULL;
	int option;

	while ((option = getopt (argc, argv, "n:l")) != -1) {
		switch (option) {
			case 'n':
				note = optarg;
				break;
			case 'l':
				list_flag = 1;
				break;
			default:
				fprintf(stderr, "Invalid Usage");
				return EXIT_FAILURE;
		}
	}

	if (note != NULL) {
		add_note(note);
	}

	if (list_flag) {
		list_notes();
	}
}
