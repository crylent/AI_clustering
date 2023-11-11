using System.Collections.Generic;

namespace AI_clusterization.DataProcessing;

public class Dataset: List<Spectrum>
{
    public Dataset()
    {
    }

    public Dataset(IEnumerable<Spectrum> collection) : base(collection)
    {
    }
}