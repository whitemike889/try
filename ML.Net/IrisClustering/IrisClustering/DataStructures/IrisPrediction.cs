using Microsoft.ML.Data;

namespace IrisClustering.DataStructures
{
    // IrisPrediction is the result returned from prediction operations
    public class IrisPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint SelectedClusterId;

        [ColumnName("Score")]
        public float[] Distance;
    }
}
