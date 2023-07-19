#ifndef MARCHING_TETRAHEDRAS_TABLE
#define MARCHING_TETRAHEDRAS_TABLE

static const int MarchingTetrahedraEdgeTable[64] = {
    -1,  0,  1,  3,  5,  6,  9,  4,  8, 10, 11,  2,  7, 12, 13, 14,
     2,  3, -1, -1,  7, -1, -1, 12,  1,  4, -1, -1,  8, -1, -1, -1,
    12,  6,  8, -1, 14, -1, -1, -1, 11, 13, -1,  0,  9, -1, -1, -1,
     6, 10, 11,  2, -1, -1, -1, 14,  3,  5, -1, -1, -1, -1, -1, -1
};

// Marching Tetrahedra triangle table
static const int MarchingTetrahedraTriangleTable[64] = {
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
     0,  8,  3, -1,  0,  1,  9, -1,  1,  8,  3, -1,  1,  9,  0, -1,
     8,  9,  1, -1,  8,  1,  3, -1,  9,  0,  1, -1,  3,  1,  9, -1,
     0,  8,  3, -1,  9,  0,  8, -1,  9,  8,  1, -1,  1,  8,  3, -1
};

// Marching Tetrahedra vertices
static const int MarchingTetrahedraVertices[12] = {
    0, 1, 2,
    0, 2, 3,
    0, 3, 1,
    1, 3, 2
};
#endif