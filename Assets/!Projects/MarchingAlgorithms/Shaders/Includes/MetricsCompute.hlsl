static const uint numOfThreads = 8;

int _ChunkSize;
int _Scale;


int indexFromCoord(int x, int y, int z)
{
	return x + _ChunkSize * (y + _ChunkSize * z);
}

