// Package mapdraw は Map プロジェクト (C#) の地図描画処理を Go に移植したものです。
// 地図サーバーで使う 地震情報・津波予報・緊急地震速報・地震感知情報 の描画に対応します。
package mapdraw

import (
	"bytes"
	"errors"
	"image"
	"image/draw"
	"image/png"
	"sort"

	xdraw "golang.org/x/image/draw"
)

// ObservationPoint は震度観測点 (または区域) です。
type ObservationPoint struct {
	Pref  string
	Name  string
	Scale int
}

// TsunamiCategory は津波予報の種類です (並び順は描画順を兼ねます)。
type TsunamiCategory int

const (
	MajorWarning TsunamiCategory = iota
	Warning
	Advisory
	TsunamiUnknown
)

// TsunamiPoint は津波予報区です。
type TsunamiPoint struct {
	Name     string
	Category TsunamiCategory
}

// UserquakePoint は地震感知情報の地域です。
type UserquakePoint struct {
	AreaCode   string
	Confidence float64
}

// Request は 1 枚の地図の描画内容です (Map/Controller/MapDrawer.cs のプロパティに相当)。
type Request struct {
	MapType     MapType
	Trim        bool
	HideDraw    bool // 描画しない (トリム・凡例のみ)
	HideNote    bool // 凡例や出典を表示しない
	AspectRatio float64

	Hypocenter        *Geo
	ObservationPoints []ObservationPoint
	EEWAreaCodes      []string
	UserquakePoints   []UserquakePoint
	TsunamiPoints     []TsunamiPoint
}

// Renderer は地図を描画します。複数の goroutine から同時に使えます。
type Renderer struct {
	res *resources
}

// NewRenderer は resourceDir (Map/Resources 相当のディレクトリ) を使う Renderer を作ります。
func NewRenderer(resourceDir string) *Renderer {
	return &Renderer{res: newResources(resourceDir)}
}

// Preload は指定した地図と描画用データを先に読み込みます。
func (r *Renderer) Preload(types ...MapType) error {
	for _, t := range types {
		if _, err := r.res.baseMap(t); err != nil {
			return err
		}
	}
	if _, err := r.res.loadStations(); err != nil {
		return err
	}
	_, err := r.res.loadEqGeo()
	return err
}

// canvas は描画先です。img は (トリム後の) 出力画像、off は地図全体に対する img の左上の位置です。
type canvas struct {
	img   *image.RGBA
	off   image.Point
	trans transformation
	res   *resources
	// fullWidth は地図全体の幅です (C# 版ではトリム前の画像に描画するため、記号の大きさの判定に使います)
	fullWidth int
}

func (c *canvas) fpt(g Geo) pt {
	x, y := c.trans.floatPixel(g)
	return pt{x - float32(c.off.X), y - float32(c.off.Y)}
}

func (c *canvas) ipt(g Geo) image.Point {
	x, y := c.trans.pixel(g)
	return image.Point{x - c.off.X, y - c.off.Y}
}

func (c *canvas) path(geos []Geo) []pt {
	out := make([]pt, len(geos))
	for i, g := range geos {
		out[i] = c.fpt(g)
	}
	return out
}

type layer interface {
	// bounds は描画範囲です (トリムに使います)。ok=false なら範囲を持ちません。
	bounds() (b LTRB, ok bool, err error)
	draw(c *canvas) error
}

// Render は地図を描画して PNG を返します。
func (r *Renderer) Render(req Request) ([]byte, error) {
	base, err := r.res.baseMap(req.MapType)
	if err != nil {
		return nil, err
	}
	W, H := base.Bounds().Dx(), base.Bounds().Dy()
	trans := transformation{ltrb: req.MapType.ltrb(), mercator: !req.MapType.isWorld(), width: W, height: H}

	layers, err := r.layers(req)
	if err != nil {
		return nil, err
	}

	crop := image.Rect(0, 0, W, H)
	if !req.MapType.isWorld() && req.Trim {
		if crop, err = trimRect(req, trans, layers); err != nil {
			return nil, err
		}
	}

	// 切り抜く範囲だけを描画先にする (C# 版は全体に描画してから切り抜くが、結果は同じ)
	c := &canvas{
		img:       image.NewRGBA(image.Rect(0, 0, crop.Dx(), crop.Dy())),
		off:       crop.Min,
		trans:     trans,
		res:       r.res,
		fullWidth: W,
	}
	copyBase(c.img, base, crop.Min)

	if !req.HideDraw {
		for _, l := range layers {
			if err := l.draw(c); err != nil {
				return nil, err
			}
		}
	}

	if err := r.drawNotes(c, req); err != nil {
		return nil, err
	}

	var buf bytes.Buffer
	enc := png.Encoder{CompressionLevel: png.BestSpeed}
	if err := enc.Encode(&buf, c.img); err != nil {
		return nil, err
	}
	return buf.Bytes(), nil
}

func (r *Renderer) layers(req Request) ([]layer, error) {
	var ls []layer
	if len(req.ObservationPoints) > 0 {
		st, err := r.res.loadStations()
		if err != nil {
			return nil, err
		}
		geo, err := r.res.loadEqGeo()
		if err != nil {
			return nil, err
		}
		pts := append([]ObservationPoint(nil), req.ObservationPoints...)
		sort.SliceStable(pts, func(i, j int) bool { return pts[i].Scale < pts[j].Scale })
		ls = append(ls, &observationAreas{pts, geo}, &observationPoints{pts, st, geo})
	}
	if len(req.EEWAreaCodes) > 0 {
		st, err := r.res.loadStations()
		if err != nil {
			return nil, err
		}
		geo, err := r.res.loadEqGeo()
		if err != nil {
			return nil, err
		}
		ls = append(ls, &eew{req.EEWAreaCodes, st, geo})
	}
	if req.Hypocenter != nil {
		ls = append(ls, &hypocenter{*req.Hypocenter})
	}
	if len(req.UserquakePoints) > 0 {
		uq, err := r.res.loadUserquake()
		if err != nil {
			return nil, err
		}
		pts := append([]UserquakePoint(nil), req.UserquakePoints...)
		sort.SliceStable(pts, func(i, j int) bool { return pts[i].Confidence < pts[j].Confidence })
		ls = append(ls, &userquake{pts, uq})
	}
	if len(req.TsunamiPoints) > 0 {
		ts, err := r.res.loadTsunami()
		if err != nil {
			return nil, err
		}
		pts := append([]TsunamiPoint(nil), req.TsunamiPoints...)
		sort.SliceStable(pts, func(i, j int) bool { return pts[i].Category < pts[j].Category })
		ls = append(ls, &tsunami{pts, ts})
	}
	return ls, nil
}

var (
	minimumSize = map[MapType]image.Point{
		Japan1024: {240, 160},
		Japan2048: {320, 240},
		Japan4096: {320, 240},
		Japan8192: {1920, 1280},
	}
	trimPadding = map[MapType][2]float64{ // {経度方向, 緯度方向}
		Japan1024: {1.0, 0.75},
		Japan2048: {1.0, 0.75},
		Japan4096: {0.75, 0.5},
		Japan8192: {1.0, 0.75},
	}
)

// trimRect は MapDrawer.cs のトリム処理と同じ計算で切り抜く範囲を求めます。
func trimRect(req Request, trans transformation, layers []layer) (image.Rectangle, error) {
	var bs []LTRB
	for _, l := range layers {
		b, ok, err := l.bounds()
		if err != nil {
			return image.Rectangle{}, err
		}
		if ok {
			bs = append(bs, b)
		}
	}
	if len(bs) == 0 {
		return image.Rectangle{}, errors.New("nothing to draw: cannot determine the area to trim")
	}
	top, left, bottom, right := bs[0].Top, bs[0].Left, bs[0].Bottom, bs[0].Right
	for _, b := range bs[1:] {
		top, left = max(top, b.Top), min(left, b.Left)
		bottom, right = min(bottom, b.Bottom), max(right, b.Right)
	}

	pad := trimPadding[req.MapType]
	ltX, ltY := trans.pixel(Geo{top + pad[1], left - pad[0]})
	rbX, rbY := trans.pixel(Geo{bottom - pad[1], right + pad[0]})
	W, H := trans.width, trans.height

	l, t := max(0, ltX), max(0, ltY)
	r, b := min(W, rbX), min(H, rbY)

	minimum := minimumSize[req.MapType]
	if r-l < minimum.X {
		l = max(0, l-minimum.X/2)
		r = min(W, r+minimum.X/2)
	}
	if b-t < minimum.Y {
		t = max(0, t-minimum.Y/2)
		b = min(H, b+minimum.Y/2)
	}

	if req.AspectRatio > 0 && r > l && b > t {
		ratio := float64(r-l) / float64(b-t)
		if req.AspectRatio > ratio {
			// 横を増やす
			add := float64(b-t)*req.AspectRatio - float64(r-l)
			l = max(0, int(float64(l)-add/2))
			r = min(W, int(float64(r)+add/2))
		} else {
			// 縦を増やす
			add := float64(r-l)/req.AspectRatio - float64(b-t)
			t = max(0, int(float64(t)-add/2))
			b = min(H, int(float64(b)+add/2))
		}
	}
	if r <= l || b <= t {
		return image.Rectangle{}, errors.New("invalid area to trim")
	}
	return image.Rect(l, t, r, b), nil
}

// copyBase は地図の一部を描画先にコピーします。パレット画像は高速な経路を使います。
func copyBase(dst *image.RGBA, src image.Image, at image.Point) {
	p, ok := src.(*image.Paletted)
	if !ok {
		// 透過部分は黒の上に合成する (C# 版は RGB で保存するため透過が残らない)
		draw.Draw(dst, dst.Bounds(), image.Black, image.Point{}, draw.Src)
		draw.Draw(dst, dst.Bounds(), src, at, draw.Over)
		return
	}
	var pal [256][4]uint8
	for i, c := range p.Palette {
		// 黒の上に合成した色 (乗算済みアルファの値をそのまま使う)
		r, g, b, _ := c.RGBA()
		pal[i] = [4]uint8{uint8(r >> 8), uint8(g >> 8), uint8(b >> 8), 0xff}
	}
	w, h := dst.Bounds().Dx(), dst.Bounds().Dy()
	for y := 0; y < h; y++ {
		srow := p.Pix[(at.Y+y-p.Rect.Min.Y)*p.Stride+(at.X-p.Rect.Min.X):]
		drow := dst.Pix[y*dst.Stride:]
		for x := 0; x < w; x++ {
			c := pal[srow[x]]
			copy(drow[x*4:x*4+4], c[:])
		}
	}
}

// resized は画像を w×h に縮小したもの (キャッシュ付き) を返します。ImageSharp の既定 (Bicubic) に近い CatmullRom を使います。
func (res *resources) resized(name string, w, h int) (image.Image, error) {
	key := name + "@" + itoa(w) + "x" + itoa(h)
	res.imgMu.Lock()
	if img, ok := res.imgs[key]; ok {
		res.imgMu.Unlock()
		return img, nil
	}
	res.imgMu.Unlock()

	src, err := res.image(name)
	if err != nil {
		return nil, err
	}
	dst := image.NewRGBA(image.Rect(0, 0, w, h))
	xdraw.CatmullRom.Scale(dst, dst.Bounds(), src, src.Bounds(), xdraw.Src, nil)

	res.imgMu.Lock()
	res.imgs[key] = dst
	res.imgMu.Unlock()
	return dst, nil
}

func itoa(i int) string {
	var b [20]byte
	n := len(b)
	if i == 0 {
		return "0"
	}
	for i > 0 {
		n--
		b[n] = byte('0' + i%10)
		i /= 10
	}
	return string(b[n:])
}

// drawNotes は出典と凡例を描画します。
func (r *Renderer) drawNotes(c *canvas, req Request) error {
	if req.HideNote {
		return nil
	}
	W, H := c.img.Bounds().Dx(), c.img.Bounds().Dy()

	// 地理院タイルの出典
	if !req.MapType.isWorld() {
		desc, err := r.res.image("description.png")
		if err != nil {
			return err
		}
		drawImage(c.img, desc, image.Point{0, H - desc.Bounds().Dy()})
	}

	// 凡例: 元画像を 1/div に縮小して右下に置く
	note := func(name string, div int) error {
		src, err := r.res.image(name)
		if err != nil {
			return err
		}
		img, err := r.res.resized(name, src.Bounds().Dx()/div, src.Bounds().Dy()/div)
		if err != nil {
			return err
		}
		s := img.Bounds().Size()
		drawImage(c.img, img, image.Point{W - s.X - 8, H - s.Y - 8})
		return nil
	}

	// 地震感知情報の凡例
	if len(req.UserquakePoints) > 0 {
		if err := note("userquake_note.png", 5); err != nil {
			return err
		}
	}

	// 地震情報の凡例
	if !req.MapType.isWorld() && req.Hypocenter != nil && len(req.ObservationPoints) > 0 {
		div := 5
		if req.MapType == Japan1024 {
			div = 6
		}
		if err := note("quake_note.png", div); err != nil {
			return err
		}
	}

	// 津波予報の凡例
	if len(req.TsunamiPoints) > 0 {
		name := "tsunami_note_advisory.png"
		if hasCategory(req.TsunamiPoints, MajorWarning) {
			name = "tsunami_note_majorwarning.png"
		} else if hasCategory(req.TsunamiPoints, Warning) {
			name = "tsunami_note_warning.png"
		}
		if err := note(name, 5); err != nil {
			return err
		}
	}
	return nil
}

func hasCategory(ps []TsunamiPoint, c TsunamiCategory) bool {
	for _, p := range ps {
		if p.Category == c {
			return true
		}
	}
	return false
}

// boundsOf は座標群を囲む範囲を返します。
func boundsOf(geos []Geo) (LTRB, bool) {
	if len(geos) == 0 {
		return LTRB{}, false
	}
	b := LTRB{geos[0].Lon, geos[0].Lat, geos[0].Lon, geos[0].Lat}
	for _, g := range geos[1:] {
		b.Left, b.Right = min(b.Left, g.Lon), max(b.Right, g.Lon)
		b.Top, b.Bottom = max(b.Top, g.Lat), min(b.Bottom, g.Lat)
	}
	return b, true
}
