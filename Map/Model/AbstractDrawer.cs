using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Model
{
    /// <summary>
    /// 座標範囲
    /// </summary>
    /// <param name="LeftLongitude">左端の経度 (0~180: 東経、 180~360: 西経 (180~0 に対応))</param>
    /// <param name="TopLatitude">上端の緯度 (90~0: 北緯、 -0~-90: 南緯)</param>
    /// <param name="RightLongitude">右端の経度 (0~180: 東経、 180~360: 西経 (180~0 に対応))</param>
    /// <param name="BottomLatitude">下端の緯度 (90~0: 北緯、 -0~-90: 南緯)</param>
    public record LTRBCoordinate(double LeftLongitude, double TopLatitude, double RightLongitude, double BottomLatitude);

    public abstract class AbstractDrawer
    {
        public Image Image { get; init; }
        public LTRBCoordinate LTRB { get; init; }
        public bool IsMercator { get; init; }

        public abstract void Draw();
        public abstract LTRBCoordinate CalcDrawLTRB();
    }
}
