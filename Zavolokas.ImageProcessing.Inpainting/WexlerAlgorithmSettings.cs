using System;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting
{
    [Serializable]
    public sealed class WexlerAlgorithmSettings
    {
        public byte LevelsToProcess = 0;
        public byte StartLevel = 0;
        public byte LevelsAmount;

        /// <summary>
        /// Gamma is used for calculation of alpha in markup. The confidence.
        /// </summary>
        public double Gamma = 1.3;

        /// <summary>
        /// The value of confidence in non marked areas.
        /// </summary>
        public double ConfidentValue = 1.50;

        /// <summary>
        /// max amount of iterations for algorithm for each iteration to converge. 
        /// </summary>
        public byte[] MaxIterations = new byte[] { 7 };

        public double MinKValue = 0.2;
        public bool AutoDk = true;
        public double Dk = 0.01;
        public double K = 3.0;

        public int ProgressUpdateFrequencyMs = 500;
        //internal Image OriginalImage;
        public bool IsFullyDonored;
        public Vector2D WindowOffset;
        public byte[] Iterations = new byte[] { };
        public ImagePatchDistanceCalculator PatchDistanceCalculator { get; set; }
        public ColorResolver ColorResolveMethod { get; set; }
        public int MaxPointsPerProcess = int.MaxValue;

        public WexlerAlgorithmSettings Clone()
        {
            var clone = new WexlerAlgorithmSettings();
            clone.LevelsToProcess = LevelsToProcess;
            clone.StartLevel = StartLevel;
            clone.LevelsAmount = LevelsAmount;
            clone.Gamma = Gamma;
            clone.ConfidentValue = ConfidentValue;
            clone.MaxIterations = new byte[MaxIterations.Length];
            for (var i = 0; i < MaxIterations.Length; i++)
            {
                clone.MaxIterations[i] = MaxIterations[i];
            }
            clone.MinKValue = MinKValue;
            clone.AutoDk = AutoDk;
            clone.Dk = Dk;
            clone.K = K;
            clone.ProgressUpdateFrequencyMs = ProgressUpdateFrequencyMs;
            clone.IsFullyDonored = IsFullyDonored;
            clone.WindowOffset = WindowOffset;
            //clone.OriginalImage = OriginalImage;
            clone.Iterations = new byte[Iterations.Length];
            for (int i = 0; i < Iterations.Length; i++)
            {
                clone.Iterations[i] = Iterations[i];
            }
            return clone;
        }
    }
}