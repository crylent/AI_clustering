namespace AI_clusterization.DataProcessing;

public class Cluster: Dataset
{
    public double[] CenterOfMass()
    {
        var center = new double[this[0].Length];
        foreach (var spectrum in this)
        {
            for (var i = 0; i < spectrum.Length; i++)
            {
                center[i] += spectrum.Power[i];
            }
        }
        
        for (var i = 0; i < center.Length; i++)
        {
            center[i] /= Count;
        }

        return center;
    }
}