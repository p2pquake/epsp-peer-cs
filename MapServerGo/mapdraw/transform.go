package mapdraw

import (
	"fmt"
	"math"
)

// Geo は緯度経度です。
type Geo struct {
	Lat, Lon float64
}

// LTRB は座標範囲です (Map/Model/AbstractDrawer.cs の LTRBCoordinate に相当)。
type LTRB struct {
	Left, Top, Right, Bottom float64
}

// MapType は地図の種類です。
type MapType int

const (
	World512 MapType = iota
	World1024
	Japan1024
	Japan2048
	Japan4096
	Japan8192
)

var mapTypeNames = map[string]MapType{
	"WORLD_512":  World512,
	"WORLD_1024": World1024,
	"JAPAN_1024": Japan1024,
	"JAPAN_2048": Japan2048,
	"JAPAN_4096": Japan4096,
	"JAPAN_8192": Japan8192,
}

// ParseMapType は "JAPAN_1024" などの文字列を MapType に変換します。
func ParseMapType(s string) (MapType, error) {
	if t, ok := mapTypeNames[s]; ok {
		return t, nil
	}
	return 0, fmt.Errorf("unknown mapType: %q", s)
}

func (t MapType) isWorld() bool { return t == World512 || t == World1024 }

func (t MapType) file() string {
	switch t {
	case World512:
		return "Maps/world_512.png"
	case World1024:
		return "Maps/world_1024.png"
	case Japan2048:
		return "Maps/japan-gsi_2048-8bit.png"
	case Japan4096:
		return "Maps/japan-gsi_4096-8bit.png"
	case Japan8192:
		return "Maps/japan-gsi_8192-8bit.png"
	default:
		return "Maps/japan-gsi_1024-8bit.png"
	}
}

func (t MapType) ltrb() LTRB {
	if t.isWorld() {
		return LTRB{0, 90, 360, -90}
	}
	return LTRB{121, 47, 150, 23}
}

// transformation は緯度経度を地図全体のピクセル座標に変換します (Map/Model/Transformation.cs)。
type transformation struct {
	ltrb          LTRB
	mercator      bool
	width, height int
}

func mercator(lat float64) float64 {
	return 6357 * math.Log(math.Tan((math.Pi/180*(90+lat))/2))
}

func (t transformation) rates(g Geo) (lonRate, latRate float64) {
	latRate = 1 - (g.Lat-t.ltrb.Bottom)/(t.ltrb.Top-t.ltrb.Bottom)
	lonRate = (g.Lon - t.ltrb.Left) / (t.ltrb.Right - t.ltrb.Left)
	if t.mercator {
		top, bottom, cur := mercator(t.ltrb.Top), mercator(t.ltrb.Bottom), mercator(g.Lat)
		latRate = 1 - (cur-bottom)/(top-bottom)
	}
	return
}

func (t transformation) floatPixel(g Geo) (float32, float32) {
	lr, ar := t.rates(g)
	return float32(lr * float64(t.width)), float32(ar * float64(t.height))
}

func (t transformation) pixel(g Geo) (int, int) {
	lr, ar := t.rates(g)
	return int(lr * float64(t.width)), int(ar * float64(t.height))
}
