namespace AI_clusterization.UI;

public partial class DatasetCluster
{
    public DatasetCluster(int index)
    {
        InitializeComponent();
        ClusterName.Text = $"Cluster {index + 1}";
    }
}