using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Model
{
    class HypocenterDrawer : AbstractDrawer
    {
        public double Latitude { get; init; }
        public double Longitude { get; init; }

        public override LTRBCoordinate CalcDrawLTRB()
        {
            return new LTRBCoordinate(Longitude, Latitude, Longitude, Latitude);
        }

        public override void Draw()
        {
            throw new NotImplementedException();
        }
    }
}
