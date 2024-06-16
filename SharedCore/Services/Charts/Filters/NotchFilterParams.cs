using System;

namespace SharedCore.Services.Charts.Filters
{
    /// <summary>
    /// Parameters of the notch filter.
    /// </summary>
    internal struct NotchFilterParams
    {
        /// <summary>
        /// 'a' transfer function coefficients of the filter.
        /// </summary>
        public readonly double[] CoefficientsA;

        /// <summary>
        /// 'b' transfer function coefficients of the filter.
        /// </summary>
        public readonly double[] CoefficientsB;

        /// <summary>
        /// Rank of the filter.
        /// </summary>
        public readonly int FilterRank;

        /// <summary>
        /// Creates params for the notch filter.
        /// </summary>
        /// <param name="aFilterRank">The filter rank, e.g. 2.</param>
        /// <param name="aCoefficientsA">Transfer function coefficients 'a'.</param>
        /// <param name="aCoefficientsB">Transfer function coefficients 'b'.</param>
        public NotchFilterParams(int aFilterRank, double[] aCoefficientsA, double[] aCoefficientsB)
        {
            if (!(aFilterRank >= 1 && aFilterRank <= 3))
            {
                throw new ArgumentException($"Unsupported rank {aFilterRank}", nameof(aFilterRank));
            }

            FilterRank = aFilterRank;

            var expectedCoefficientsCount = 2 * FilterRank + 1;
            if (aCoefficientsA.Length != expectedCoefficientsCount)
            {
                throw new ArgumentException($"Coefficient count is wrong: {aCoefficientsA.Length}",
                    nameof(aCoefficientsA));
            }

            if (aCoefficientsB.Length != expectedCoefficientsCount)
            {
                throw new ArgumentException($"Coefficient count is wrong: {aCoefficientsB.Length}",
                    nameof(aCoefficientsB));
            }

            CoefficientsA = aCoefficientsA;
            CoefficientsB = aCoefficientsB;
        }
    }
}