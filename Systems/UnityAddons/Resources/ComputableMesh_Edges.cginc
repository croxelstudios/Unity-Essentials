struct Edge
{
    int v1; // ID of first vertex
    int v2; // ID of second vertex
    int index1; // ID of first colindant triangle
    int index2;
    // Positive: ID of second colindant triangle
    // -1: No second colindant triangle
    // Negative: Edge is a duplicate of the edge with id = -(index2 + 2)

    bool IsDupe()
    {
        return index2 < -1;
    }

    int GetOriginal()
    {
        return IsDupe() ? -(index2 + 2) : -1;
    }
};

RWStructuredBuffer<Edge> edges;

bool Edge_IsDupe(uint id)
{
    return edges[id].IsDupe();
}

int Edge_GetOriginal(uint id)
{
    int orig = edges[id].GetOriginal();
    return (orig < 0) ? id : orig;
}
