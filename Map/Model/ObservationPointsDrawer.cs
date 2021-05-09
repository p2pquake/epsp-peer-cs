using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SixLabors.ImageSharp.Drawing;

namespace Map.Model
{
    public record ObservationPoint(string Prefecture, string Name, int Scale);

    class ObservationPointsDrawer : AbstractDrawer
    {
        public IList<ObservationPoint> ObservationPoints { get; init; }

        public override LTRBCoordinate CalcDrawLTRB()
        {
            throw new NotImplementedException();
        }

        public override void Draw()
        {
            throw new NotImplementedException();
        }
    }
}
