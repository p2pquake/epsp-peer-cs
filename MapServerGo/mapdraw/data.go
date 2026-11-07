package mapdraw

import (
	"bytes"
	"encoding/json"
	"fmt"
	"image"
	"image/png"
	"os"
	"path/filepath"
	"regexp"
	"strconv"
	"strings"
	"sync"
)

// lazy は初回アクセス時に一度だけ読み込む値です。
type lazy[T any] struct {
	once sync.Once
	v    T
	err  error
}

func (l *lazy[T]) get(load func() (T, error)) (T, error) {
	l.once.Do(func() { l.v, l.err = load() })
	return l.v, l.err
}

type station struct {
	pref     string
	isArea   bool
	isJMA    bool
	lat, lon float64
}

// stations は Map/Model/Stations.cs の移植です。
type stations struct {
	all    map[string]*station
	byPref map[string]map[string]*station
}

// shortenPattern は Map/Model/StationNameShorter.cs と同じ正規表現です。
var shortenPattern = regexp.MustCompile(`^((?:余市町|田村市|玉村町|東村山市|武蔵村山市|羽村市|十日町市|上市町|大町市|名古屋中村区|大阪堺市.+?区|下市町|大村市|野々市市|四日市市|廿日市市|大町町|.+?[市区町村]))`)

func (s *stations) search(name, pref string) *station {
	target := s.all
	if m, ok := s.byPref[pref]; ok {
		target = m
	}
	if st, ok := target[name]; ok {
		return st
	}
	if short := shortenPattern.FindString(name); short != "" {
		if st, ok := target[short]; ok {
			return st
		}
	}
	return nil
}

// point は観測点 (または区域) の座標を返します。
func (s *stations) point(name, pref string) (Geo, bool) {
	if st := s.search(name, pref); st != nil {
		return Geo{st.lat, st.lon}, true
	}
	return Geo{}, false
}

// area は区域 (Areas.csv) の座標を返します。
func (s *stations) area(name string) (Geo, bool) {
	if st, ok := s.all[name]; ok && st.isArea {
		return Geo{st.lat, st.lon}, true
	}
	return Geo{}, false
}

type rings = [][]Geo

// resources は地図描画に使う画像やデータを保持します。各データは初回利用時に読み込みます。
type resources struct {
	dir string

	maps     [6]lazy[image.Image]
	stations lazy[*stations]
	eqGeo    lazy[*eqGeo]
	tsunami  lazy[map[string]rings]
	uq       lazy[*uqData]

	imgMu sync.Mutex
	imgs  map[string]image.Image
}

func newResources(dir string) *resources {
	return &resources{dir: dir, imgs: map[string]image.Image{}}
}

func (r *resources) read(name string) ([]byte, error) {
	return os.ReadFile(filepath.Join(r.dir, filepath.FromSlash(name)))
}

// text は UTF-8 のテキストを BOM と CR を除いて行に分割します。
func (r *resources) lines(name string) ([]string, error) {
	b, err := r.read(name)
	if err != nil {
		return nil, err
	}
	s := strings.TrimPrefix(string(b), "\ufeff")
	s = strings.ReplaceAll(s, "\r", "")
	var out []string
	for _, l := range strings.Split(s, "\n") {
		if l != "" {
			out = append(out, l)
		}
	}
	return out, nil
}

func (r *resources) csv(name string, skipHeader bool) ([][]string, error) {
	ls, err := r.lines(name)
	if err != nil {
		return nil, err
	}
	if skipHeader && len(ls) > 0 {
		ls = ls[1:]
	}
	out := make([][]string, len(ls))
	for i, l := range ls {
		out[i] = strings.Split(l, ",")
	}
	return out, nil
}

func (r *resources) baseMap(t MapType) (image.Image, error) {
	return r.maps[t].get(func() (image.Image, error) {
		b, err := r.read(t.file())
		if err != nil {
			return nil, err
		}
		return png.Decode(bytes.NewReader(b))
	})
}

// image は Images/ 以下の画像を読み込みます (キャッシュ付き)。
func (r *resources) image(name string) (image.Image, error) {
	r.imgMu.Lock()
	defer r.imgMu.Unlock()
	if img, ok := r.imgs[name]; ok {
		return img, nil
	}
	b, err := r.read("Images/" + name)
	if err != nil {
		return nil, err
	}
	img, err := png.Decode(bytes.NewReader(b))
	if err != nil {
		return nil, err
	}
	r.imgs[name] = img
	return img, nil
}

func (r *resources) loadStations() (*stations, error) {
	return r.stations.get(func() (*stations, error) {
		s := &stations{all: map[string]*station{}, byPref: map[string]map[string]*station{}}
		put := func(pref, name string, st *station) {
			if s.byPref[pref] == nil {
				s.byPref[pref] = map[string]*station{}
			}
			s.byPref[pref][name] = st
		}

		areas, err := r.csv("Points/Areas.csv", true)
		if err != nil {
			return nil, err
		}
		for _, it := range areas {
			lat, lon, err := parseLatLon(it[2], it[3])
			if err != nil {
				return nil, err
			}
			st := &station{pref: it[0], isArea: true, isJMA: true, lat: lat, lon: lon}
			s.all[it[1]] = st
			put(it[0], it[1], st)
		}

		points, err := r.csv("Points/Stations.csv", false)
		if err != nil {
			return nil, err
		}
		for _, it := range points {
			lat, lon, err := parseLatLon(it[2], it[3])
			if err != nil {
				return nil, err
			}
			st := &station{pref: it[0], isJMA: it[4] == "気象庁", lat: lat, lon: lon}
			s.all[it[1]] = st
			put(it[0], it[1], st)

			// 省略名 (気象庁の震度観測点を優先)
			if short := shortenPattern.FindString(it[1]); short != "" {
				if cur, ok := s.all[short]; !ok || !cur.isJMA {
					s.all[short] = st
				}
				if cur, ok := s.byPref[it[0]][short]; !ok || !cur.isJMA {
					s.byPref[it[0]][short] = st
				}
			}
		}
		return s, nil
	})
}

func parseLatLon(lat, lon string) (float64, float64, error) {
	la, err := strconv.ParseFloat(strings.TrimSpace(lat), 64)
	if err != nil {
		return 0, 0, err
	}
	lo, err := strconv.ParseFloat(strings.TrimSpace(lon), 64)
	return la, lo, err
}

type multiPolygonGeoJSON struct {
	Features []struct {
		Geometry struct {
			Coordinates [][][][]float32 `json:"coordinates"`
		} `json:"geometry"`
		Properties struct {
			Code string `json:"code"`
		} `json:"properties"`
	} `json:"features"`
}

// codeRings は GeoJSON (MultiPolygon) を「コード → 各多角形の外周」にします。
// 同じコードが複数あれば連結します。dst が nil でなければ dst に追加します。
func (r *resources) codeRings(name string, dst map[string]rings) (map[string]rings, error) {
	b, err := r.read(name)
	if err != nil {
		return nil, err
	}
	var gj multiPolygonGeoJSON
	if err := json.Unmarshal(bytes.TrimPrefix(b, []byte("\ufeff")), &gj); err != nil {
		return nil, fmt.Errorf("%s: %w", name, err)
	}
	if dst == nil {
		dst = map[string]rings{}
	}
	for _, f := range gj.Features {
		for _, polygon := range f.Geometry.Coordinates {
			if len(polygon) == 0 {
				continue
			}
			ring := make([]Geo, len(polygon[0]))
			for i, c := range polygon[0] {
				ring[i] = Geo{float64(c[1]), float64(c[0])}
			}
			dst[f.Properties.Code] = append(dst[f.Properties.Code], ring)
		}
	}
	return dst, nil
}

// eqGeo は地震情報の区域 GeoJSON 由来のデータです。
type eqGeo struct {
	byCode      map[string]rings    // 区域コード → 多角形
	byName      map[string]rings    // 区域名 → 多角形 (ObservationAreas.cs)
	codeToName  map[string]string   // 区域コード → 区域名
	eewToEqCode map[string][]string // EEW 府県予報区コード → 区域コード
	eewRings    map[string]rings    // EEW 府県予報区コード → 多角形 (EEWAreas.cs)
}

func (r *resources) loadEqGeo() (*eqGeo, error) {
	return r.eqGeo.get(func() (*eqGeo, error) {
		g := &eqGeo{byName: map[string]rings{}, codeToName: map[string]string{}, eewToEqCode: map[string][]string{}, eewRings: map[string]rings{}}
		var err error
		if g.byCode, err = r.codeRings("Points/earthquake.json", nil); err != nil {
			return nil, err
		}
		codes, err := r.csv("Points/EarthquakeAreaCodes.csv", false)
		if err != nil {
			return nil, err
		}
		for _, it := range codes {
			g.codeToName[it[0]] = it[1]
		}
		for code, rs := range g.byCode {
			if name, ok := g.codeToName[code]; ok {
				g.byName[name] = append(g.byName[name], rs...)
			}
		}

		eew, err := r.csv("Points/EEWPrefs2EarthquakeAreaCodes.csv", true)
		if err != nil {
			return nil, err
		}
		for _, it := range eew {
			g.eewToEqCode[it[0]] = it[1:]
			var rs rings
			for _, c := range it[1:] {
				rs = append(rs, g.byCode[c]...)
			}
			g.eewRings[it[0]] = rs
		}
		return g, nil
	})
}

type multiLineGeoJSON struct {
	Features []struct {
		Geometry struct {
			Coordinates [][][]float32 `json:"coordinates"`
		} `json:"geometry"`
		Properties struct {
			Code string `json:"code"`
		} `json:"properties"`
	} `json:"features"`
}

// loadTsunami は津波予報区名 → 海岸線 (折れ線) を返します (TsunamiAreas.cs)。
func (r *resources) loadTsunami() (map[string]rings, error) {
	return r.tsunami.get(func() (map[string]rings, error) {
		b, err := r.read("Points/tsunami.json")
		if err != nil {
			return nil, err
		}
		var gj multiLineGeoJSON
		if err := json.Unmarshal(b, &gj); err != nil {
			return nil, fmt.Errorf("tsunami.json: %w", err)
		}
		codes, err := r.csv("Points/TsunamiAreaCodes.csv", false)
		if err != nil {
			return nil, err
		}
		codeToName := map[string]string{}
		for _, it := range codes {
			codeToName[it[0]] = it[1]
		}
		out := map[string]rings{}
		for _, f := range gj.Features {
			name, ok := codeToName[f.Properties.Code]
			if !ok {
				continue
			}
			for _, line := range f.Geometry.Coordinates {
				l := make([]Geo, len(line))
				for i, c := range line {
					l[i] = Geo{float64(c[1]), float64(c[0])}
				}
				out[name] = append(out[name], l)
			}
		}
		return out, nil
	})
}

// uqData は地震感知情報の地域データです (UserquakeAreas.cs)。
type uqData struct {
	centers map[string]Geo
	rings   map[string]rings
}

func (r *resources) loadUserquake() (*uqData, error) {
	return r.uq.get(func() (*uqData, error) {
		d := &uqData{centers: map[string]Geo{}, rings: map[string]rings{}}
		areas, err := r.csv("Points/UserquakeAreas.csv", true)
		if err != nil {
			return nil, err
		}
		for _, it := range areas {
			lat, lon, err := parseLatLon(it[4], it[5])
			if err != nil {
				return nil, err
			}
			d.centers[it[0]] = Geo{lat, lon}
		}

		// 地震情報の区域と地震感知情報独自の区域を同じ表にまとめる
		geo, err := r.codeRings("Points/earthquake.json", nil)
		if err != nil {
			return nil, err
		}
		if geo, err = r.codeRings("Points/userquake.json", geo); err != nil {
			return nil, err
		}
		uq2eq, err := r.csv("Points/UserquakeAreas2EarthquakeAreaCodes.csv", true)
		if err != nil {
			return nil, err
		}
		for _, it := range uq2eq {
			var rs rings
			for _, c := range it[1:] {
				rs = append(rs, geo[c]...)
			}
			d.rings[it[0]] = rs
		}
		return d, nil
	})
}
