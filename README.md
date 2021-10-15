# FileSorting
The program generates and sorts a text file according to specific criteria.
To use for big files one should change chunkSizeLimit to e.g. 1000000000 (for 1Gb chunk size).
Algorithm splits the input file into separate chunks, sorts each chunk and saves as separate temporary files. Finally, the files are merged into a single output file.
