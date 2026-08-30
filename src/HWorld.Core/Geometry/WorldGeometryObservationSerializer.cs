using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HWorld.Core.Geometry
{
    /// <summary>
    /// Produces a compact, deterministic text representation of geometry observations.
    /// Semantic names and application-specific kinds are intentionally excluded.
    /// </summary>
    public static class WorldGeometryObservationSerializer
    {
        public static string Serialize(IList<WorldGeometryObservation> observations)
        {
            if (observations == null) throw new ArgumentNullException(nameof(observations));

            var builder = new StringBuilder(Math.Max(32, observations.Count * 72));
            builder.Append("n=");
            builder.Append(observations.Count.ToString(CultureInfo.InvariantCulture));

            for (int i = 0; i < observations.Count; i++)
            {
                var item = observations[i];
                builder.Append(";i=");
                builder.Append(item.EntityId.ToString("N"));
                builder.Append(",x=");
                AppendNumber(builder, item.RelativeX);
                builder.Append(",y=");
                AppendNumber(builder, item.RelativeY);
                builder.Append(",d=");
                AppendNumber(builder, item.Distance);
                builder.Append(",b=");
                AppendNumber(builder, item.BearingDegrees);
                builder.Append(",w=");
                AppendNumber(builder, item.Width);
                builder.Append(",h=");
                AppendNumber(builder, item.Height);
                builder.Append(",r=");
                AppendNumber(builder, item.RotationDegrees);
                builder.Append(",s=");
                builder.Append(item.Solid ? '1' : '0');
            }

            return builder.ToString();
        }

        private static void AppendNumber(StringBuilder builder, double value)
        {
            builder.Append(value.ToString("0.###", CultureInfo.InvariantCulture));
        }
    }
}
