package mapdraw

import (
	"image"
	"image/color"
	"strconv"
)

// 震度ごとの塗りつぶし色 (ObservationAreasDrawer.cs)
var scaleColors = map[int]color.NRGBA{
	10: withAlpha(160, 224, 255, 0.7),
	20: withAlpha(160, 208, 255, 0.7),
	30: withAlpha(176, 192, 255, 0.7),
	40: withAlpha(112, 224, 128, 0.7),
	45: withAlpha(128, 192, 0, 0.7),
	46: withAlpha(128, 192, 0, 0.7),
	50: withAlpha(240, 128, 0, 0.7),
	55: withAlpha(208, 112, 0, 0.7),
	60: withAlpha(224, 32, 32, 0.7),
	70: withAlpha(160, 0, 32, 0.7),
}

// observationAreas は震度ごとに区域を塗りつぶします (ObservationAreasDrawer.cs)。
type observationAreas struct {
	points []ObservationPoint // 震度の昇順
	geo    *eqGeo
}

func (l *observationAreas) bounds() (LTRB, bool, error) { return LTRB{}, false, nil }

func (l *observationAreas) draw(c *canvas) error {
	for _, p := range l.points {
		rs, ok := l.geo.byName[p.Name]
		if !ok {
			continue
		}
		col, ok := scaleColors[p.Scale]
		if !ok {
			continue
		}
		for _, ring := range rs {
			fillPolygons(c.img, [][]pt{c.path(ring)}, col)
		}
	}
	return nil
}

// observationPoints は震度アイコンを描画します (ObservationPointsDrawer.cs)。
type observationPoints struct {
	points []ObservationPoint // 震度の昇順
	st     *stations
	geo    *eqGeo
}

func (l *observationPoints) bounds() (LTRB, bool, error) {
	var geos []Geo
	for _, p := range l.points {
		if g, ok := l.st.point(p.Name, p.Pref); ok {
			geos = append(geos, g)
		}
	}
	b, ok := boundsOf(geos)
	return b, ok, nil
}

func (l *observationPoints) draw(c *canvas) error {
	pointSize, areaSize := 12, 16
	if c.fullWidth > 1024 {
		pointSize, areaSize = 16, 24
	}
	for _, p := range l.points {
		g, ok := l.st.point(p.Name, p.Pref)
		if !ok {
			continue
		}
		if _, ok := scaleColors[p.Scale]; !ok {
			continue // アイコンのない震度
		}
		size := pointSize
		if _, isArea := l.geo.byName[p.Name]; isArea {
			size = areaSize
		}
		icon, err := c.res.resized("scale_"+strconv.Itoa(p.Scale)+".png", size, size)
		if err != nil {
			return err
		}
		pos := c.ipt(g)
		fillRect(c.img, image.Rect(pos.X-(size/2+1), pos.Y-(size/2+1), pos.X-(size/2+1)+size+2, pos.Y-(size/2+1)+size+2), color.Black)
		drawImage(c.img, icon, image.Point{pos.X - size/2, pos.Y - size/2})
	}
	return nil
}

// hypocenter は震源の × 印を描画します (HypocenterDrawer.cs)。
type hypocenter struct{ g Geo }

func (l *hypocenter) bounds() (LTRB, bool, error) {
	return LTRB{l.g.Lon, l.g.Lat, l.g.Lon, l.g.Lat}, true, nil
}

func (l *hypocenter) draw(c *canvas) error {
	const size, thickness = 8, 3
	pos := c.ipt(l.g)
	x, y := float32(pos.X), float32(pos.Y)
	red := color.NRGBA{255, 0, 0, 255}
	for _, v := range []float32{-1, 1} {
		strokePath(c.img, []pt{{x - size, y - size*v}, {x + size, y + size*v}}, thickness, red)
	}
	return nil
}

// eew は緊急地震速報 (警報) の府県予報区を塗りつぶします (EEWDrawer.cs)。
type eew struct {
	codes []string
	st    *stations
	geo   *eqGeo
}

func (l *eew) bounds() (LTRB, bool, error) {
	var geos []Geo
	for _, code := range l.codes {
		for _, eqCode := range l.geo.eewToEqCode[code] {
			name, ok := l.geo.codeToName[eqCode]
			if !ok {
				continue
			}
			if g, ok := l.st.area(name); ok {
				geos = append(geos, g)
			}
		}
	}
	b, ok := boundsOf(geos)
	return b, ok, nil
}

func (l *eew) draw(c *canvas) error {
	lineSize := float32(4)
	if c.fullWidth > 1024 {
		lineSize = 8
	}
	outline := withAlpha(255, 140, 0, 0.8) // DarkOrange
	fill := withAlpha(255, 165, 0, 0.95)   // Orange
	for _, code := range l.codes {
		rs, ok := l.geo.eewRings[code]
		if !ok {
			continue
		}
		paths := make([][]pt, len(rs))
		for i, ring := range rs {
			paths[i] = c.path(ring)
		}
		for _, p := range paths {
			strokePath(c.img, p, lineSize, outline)
		}
		for _, p := range paths {
			fillPolygons(c.img, [][]pt{p}, fill)
		}
	}
	return nil
}

// EEWAreaCode は府県予報区名 (例: "東京都23区") を府県予報区地域コードに変換します。
// 見つからなければ "-1" を返します (EEWConverter.GetAreaCode と同じ)。
func EEWAreaCode(name string) string {
	if code, ok := eewAreaNameToCode[name]; ok {
		return code
	}
	return "-1"
}

// userquake は地震感知情報を描画します (UserquakeDrawer.cs)。
type userquake struct {
	points []UserquakePoint // 信頼度の昇順
	data   *uqData
}

func (l *userquake) bounds() (LTRB, bool, error) {
	var geos []Geo
	for _, p := range l.points {
		if g, ok := l.data.centers[p.AreaCode]; ok {
			geos = append(geos, g)
		}
	}
	b, ok := boundsOf(geos)
	if ok {
		b.Left -= 0.2
		b.Top += 0.2
		b.Right += 0.2
		b.Bottom -= 0.2
	}
	return b, ok, nil
}

func (l *userquake) draw(c *canvas) error {
	size := 12
	if c.fullWidth > 1024 {
		size = 24
	}
	gray := withAlpha(128, 128, 128, 0.5)
	for _, p := range l.points {
		rs, ok := l.data.rings[p.AreaCode]
		if !ok {
			continue
		}
		paths := make([][]pt, len(rs))
		for i, ring := range rs {
			paths[i] = c.path(ring)
		}
		for _, path := range paths {
			strokePath(c.img, path, 2, gray)
		}
		col := confidenceColor(p.Confidence)
		for _, path := range paths {
			fillPolygons(c.img, [][]pt{path}, col)
		}
	}
	for _, p := range l.points {
		g, ok := l.data.centers[p.AreaCode]
		if !ok {
			continue
		}
		icon, err := c.res.resized("confidence_"+confidenceRank(p.Confidence)+".png", size, size)
		if err != nil {
			return err
		}
		pos := c.ipt(g)
		drawImage(c.img, icon, image.Point{pos.X - size/2, pos.Y - size/2})
	}
	return nil
}

func confidenceRank(c float64) string {
	switch {
	case c > 0.8:
		return "A"
	case c > 0.6:
		return "B"
	case c > 0.4:
		return "C"
	case c > 0.2:
		return "D"
	case c > 0.0:
		return "E"
	default:
		return "F"
	}
}

// confidenceColor は UserquakeDrawer.ConvConfidenceColor(...).WithAlpha(0.95f) 相当です。
func confidenceColor(c float64) color.NRGBA {
	switch {
	case c < 0:
		return withAlpha(192, 192, 192, 0.95)
	case c >= 0.5:
		m := (c - 0.5) * 2
		return withAlpha(uint8(244+m*-4), uint8(160+m*-32), uint8(64+m*-64), 0.95)
	default:
		m := c * 2
		return withAlpha(uint8(255+m*-11), uint8(248+m*-88), uint8(240+m*-176), 0.95)
	}
}

// 津波予報の線の色 (TsunamiAreasDrawer.cs)
var tsunamiColors = map[TsunamiCategory]color.NRGBA{
	MajorWarning: {200, 0, 255, 255},
	Warning:      {255, 40, 0, 255},
	Advisory:     {250, 245, 0, 255},
}

// tsunami は津波予報区の海岸線を描画します (TsunamiAreasDrawer.cs)。
type tsunami struct {
	points []TsunamiPoint // 種類の順 (大津波警報 → 津波警報 → 津波注意報)
	areas  map[string]rings
}

func (l *tsunami) bounds() (LTRB, bool, error) {
	var geos []Geo
	for _, p := range l.points {
		for _, line := range l.areas[p.Name] {
			geos = append(geos, line...)
		}
	}
	b, ok := boundsOf(geos)
	return b, ok, nil
}

func (l *tsunami) draw(c *canvas) error {
	for _, p := range l.points {
		lines, ok := l.areas[p.Name]
		if !ok {
			continue
		}
		col, ok := tsunamiColors[p.Category]
		if !ok {
			continue
		}
		width := float32(6)
		if p.Category == MajorWarning {
			width = 12
		}
		for _, line := range lines {
			strokePath(c.img, c.path(line), width, col)
		}
	}
	return nil
}
