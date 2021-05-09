using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Model
{
    public record UserquakePoint(string Areacode, double Confidence);

    class UserquakeDrawer : AbstractDrawer
    {
        public IList<UserquakePoint> UserquakePoints { get; init; }

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
