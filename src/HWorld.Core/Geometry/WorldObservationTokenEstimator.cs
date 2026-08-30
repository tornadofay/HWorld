using System;

namespace HWorld.Core.Geometry
{
    /// <summary>
    /// Cheap, provider-neutral context size estimate. It is not a tokenizer and
    /// must not be treated as an exact provider billing value.
    /// </summary>
    public static class WorldObservationTokenEstimator
    {
        public static int EstimateTokens(string text, int charactersPerToken = 4)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (charactersPerToken <= 0) throw new ArgumentOutOfRangeException(nameof(charactersPerToken));
            if (text.Length == 0) return 0;
            return (text.Length + charactersPerToken - 1) / charactersPerToken;
        }
    }
}
