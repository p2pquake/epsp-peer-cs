package mapdraw

import (
	"image"
	"image/color"
	"image/draw"
	"math"

	"golang.org/x/image/vector"
)

type pt struct{ X, Y float32 }

// fillPolygons は複数の多角形をまとめて (和集合として) アンチエイリアス付きで塗りつぶします。
// 重なりを二重に合成しないよう、和集合にしたい多角形は同じ向きにそろえてください。
func fillPolygons(dst *image.RGBA, polys [][]pt, c color.Color) {
	minX, minY := float32(math.MaxFloat32), float32(math.MaxFloat32)
	maxX, maxY := float32(-math.MaxFloat32), float32(-math.MaxFloat32)
	for _, poly := range polys {
		for _, p := range poly {
			minX, minY = min(minX, p.X), min(minY, p.Y)
			maxX, maxY = max(maxX, p.X), max(maxY, p.Y)
		}
	}
	if minX > maxX {
		return
	}
	b := image.Rect(
		int(math.Floor(float64(minX))), int(math.Floor(float64(minY))),
		int(math.Ceil(float64(maxX)))+1, int(math.Ceil(float64(maxY)))+1,
	).Intersect(dst.Bounds())
	if b.Empty() {
		return
	}

	var z vector.Rasterizer
	z.Reset(b.Dx(), b.Dy())
	z.DrawOp = draw.Over
	ox, oy := float32(b.Min.X), float32(b.Min.Y)
	for _, poly := range polys {
		if len(poly) < 2 {
			continue
		}
		z.MoveTo(poly[0].X-ox, poly[0].Y-oy)
		for _, p := range poly[1:] {
			z.LineTo(p.X-ox, p.Y-oy)
		}
		z.ClosePath()
	}
	z.Draw(dst, b, image.NewUniform(c), image.Point{})
}

// strokePolyline は折れ線を幅 width の線として描くための多角形群 (同じ向き) を返します。
// 線分ごとの四角形と、頂点の丸い継ぎ目で構成します (端は切りっぱなし)。
func strokePolyline(points []pt, width float32) [][]pt {
	hw := width / 2
	var out [][]pt
	for i := 0; i+1 < len(points); i++ {
		a, b := points[i], points[i+1]
		dx, dy := b.X-a.X, b.Y-a.Y
		l := float32(math.Hypot(float64(dx), float64(dy)))
		if l == 0 {
			continue
		}
		nx, ny := -dy/l*hw, dx/l*hw
		out = append(out, oriented([]pt{
			{a.X + nx, a.Y + ny}, {b.X + nx, b.Y + ny},
			{b.X - nx, b.Y - ny}, {a.X - nx, a.Y - ny},
		}))
	}
	n := len(points)
	for i := 1; i+1 < n; i++ {
		out = append(out, circle(points[i], hw))
	}
	if n > 2 && points[0] == points[n-1] {
		out = append(out, circle(points[0], hw))
	}
	return out
}

func circle(c pt, r float32) []pt {
	const seg = 12
	poly := make([]pt, seg)
	for i := range poly {
		a := 2 * math.Pi * float64(i) / seg
		poly[i] = pt{c.X + r*float32(math.Cos(a)), c.Y + r*float32(math.Sin(a))}
	}
	return poly
}

// oriented は多角形の向きを (画面座標で) 時計回りにそろえます。
func oriented(poly []pt) []pt {
	var area float32
	for i := range poly {
		p, q := poly[i], poly[(i+1)%len(poly)]
		area += p.X*q.Y - q.X*p.Y
	}
	if area < 0 {
		for i, j := 0, len(poly)-1; i < j; i, j = i+1, j-1 {
			poly[i], poly[j] = poly[j], poly[i]
		}
	}
	return poly
}

func strokePath(dst *image.RGBA, points []pt, width float32, c color.Color) {
	fillPolygons(dst, strokePolyline(points, width), c)
}

func fillRect(dst *image.RGBA, r image.Rectangle, c color.Color) {
	draw.Draw(dst, r, image.NewUniform(c), image.Point{}, draw.Src)
}

func drawImage(dst *image.RGBA, src image.Image, at image.Point) {
	r := image.Rectangle{Min: at, Max: at.Add(src.Bounds().Size())}
	draw.Draw(dst, r, src, src.Bounds().Min, draw.Over)
}

// withAlpha は ImageSharp の Color.WithAlpha(float) 相当です。
func withAlpha(r, g, b uint8, a float64) color.NRGBA {
	return color.NRGBA{r, g, b, uint8(math.Round(a * 255))}
}
