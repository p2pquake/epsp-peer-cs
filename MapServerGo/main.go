// MapServerGo は `CLI map server` (C#) と同じ API を持つ地図生成サーバーです。
//
// エンドポイント: /test, /earthquake, /tsunami, /eew, /userquake
package main

import (
	"context"
	"encoding/json"
	"errors"
	"flag"
	"io"
	"log"
	"net/http"
	"os"
	"os/signal"
	"path/filepath"
	"strconv"
	"strings"
	"syscall"
	"time"

	"github.com/p2pquake/epsp-peer-cs/MapServerGo/mapdraw"
)

// aspectRatio は C# 版の地図サーバーと同じ 3:2 です。
const aspectRatio = 3.0 / 2.0

func main() {
	port := flag.Int("port", 0, "待ち受けポート (省略時は環境変数 PORT、それもなければ 17101)")
	resources := flag.String("resources", "", "Map/Resources 相当のディレクトリ (省略時は環境変数 MAP_RESOURCES、実行ファイルと同じ場所の Resources、../Map/Resources の順に探す)")
	preload := flag.String("preload", "", "起動時に読み込んでおく地図種別 (カンマ区切り。例: JAPAN_1024,JAPAN_4096)")
	flag.Parse()

	dir, err := resourceDir(*resources)
	if err != nil {
		log.Fatal(err)
	}
	renderer := mapdraw.NewRenderer(dir)

	if *preload != "" {
		var types []mapdraw.MapType
		for _, s := range strings.Split(*preload, ",") {
			t, err := mapdraw.ParseMapType(strings.TrimSpace(s))
			if err != nil {
				log.Fatal(err)
			}
			types = append(types, t)
		}
		start := time.Now()
		if err := renderer.Preload(types...); err != nil {
			log.Fatal(err)
		}
		log.Printf("preloaded %s in %v", *preload, time.Since(start))
	}

	addr := ":" + strconv.Itoa(listenPort(*port))
	srv := &http.Server{Addr: addr, Handler: newMux(renderer)}

	ctx, stop := signal.NotifyContext(context.Background(), syscall.SIGINT, syscall.SIGTERM)
	defer stop()
	go func() {
		<-ctx.Done()
		shutdownCtx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
		defer cancel()
		srv.Shutdown(shutdownCtx)
	}()

	log.Printf("HTTPServer listen %s (resources: %s)", addr, dir)
	if err := srv.ListenAndServe(); err != nil && !errors.Is(err, http.ErrServerClosed) {
		log.Fatal(err)
	}
}

// newMux は C# 版の地図サーバーと同じエンドポイントを持つハンドラーを返します。
func newMux(renderer *mapdraw.Renderer) *http.ServeMux {
	mux := http.NewServeMux()
	mux.HandleFunc("/test", func(w http.ResponseWriter, _ *http.Request) {
		w.Header().Set("Content-Type", "text/plain")
		io.WriteString(w, "OK")
	})
	mux.Handle("/earthquake", handler(renderer, earthquakeRequest))
	mux.Handle("/tsunami", handler(renderer, tsunamiRequest))
	mux.Handle("/eew", handler(renderer, eewRequest))
	mux.Handle("/userquake", handler(renderer, userquakeRequest))
	mux.HandleFunc("/", func(w http.ResponseWriter, _ *http.Request) {
		w.Header().Set("Content-Type", "text/plain")
		w.WriteHeader(http.StatusNotFound)
		io.WriteString(w, "Function not found")
	})
	return mux
}

func listenPort(flagPort int) int {
	if flagPort > 0 {
		return flagPort
	}
	if p, err := strconv.Atoi(os.Getenv("PORT")); err == nil && p > 0 {
		return p
	}
	return 17101
}

func resourceDir(flagDir string) (string, error) {
	candidates := []string{flagDir, os.Getenv("MAP_RESOURCES")}
	if exe, err := os.Executable(); err == nil {
		candidates = append(candidates, filepath.Join(filepath.Dir(exe), "Resources"))
	}
	candidates = append(candidates, filepath.Join("..", "Map", "Resources"))
	for _, c := range candidates {
		if c == "" {
			continue
		}
		if st, err := os.Stat(filepath.Join(c, "Maps")); err == nil && st.IsDir() {
			return c, nil
		}
	}
	return "", errors.New("resources directory not found (use -resources)")
}

func handler(r *mapdraw.Renderer, build func([]byte) (mapdraw.Request, error)) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, req *http.Request) {
		start := time.Now()
		body, err := io.ReadAll(req.Body)
		if err == nil {
			var mr mapdraw.Request
			if mr, err = build(body); err == nil {
				var png []byte
				if png, err = r.Render(mr); err == nil {
					w.Header().Set("Content-Type", "image/png")
					w.Write(png)
					log.Printf("%s %d bytes in %v", req.URL.Path, len(png), time.Since(start))
					return
				}
			}
		}
		log.Printf("Failed to call function with %s: %v", req.URL.Path, err)
		w.Header().Set("Content-Type", "text/plain")
		w.WriteHeader(http.StatusInternalServerError)
		io.WriteString(w, err.Error())
	})
}

// 以下のリクエスト形式は CLI/Command/ServerSubCommand.cs と同じです。

type quakeParams struct {
	Trim      bool     `json:"trim"`
	MapType   string   `json:"mapType"`
	Latitude  *float64 `json:"latitude"`
	Longitude *float64 `json:"longitude"`
	Points    []struct {
		Pref  string `json:"pref"`
		Name  string `json:"name"`
		Scale int    `json:"scale"`
	} `json:"points"`
}

func hypocenter(lat, lon *float64) *mapdraw.Geo {
	if lat != nil && *lat != -1 && lon != nil && *lon != -1 {
		return &mapdraw.Geo{Lat: *lat, Lon: *lon}
	}
	return nil
}

func earthquakeRequest(body []byte) (mapdraw.Request, error) {
	var p quakeParams
	if err := json.Unmarshal(body, &p); err != nil {
		return mapdraw.Request{}, err
	}
	t, err := mapdraw.ParseMapType(p.MapType)
	if err != nil {
		return mapdraw.Request{}, err
	}
	req := mapdraw.Request{MapType: t, Trim: p.Trim, AspectRatio: aspectRatio, Hypocenter: hypocenter(p.Latitude, p.Longitude)}
	for _, pt := range p.Points {
		req.ObservationPoints = append(req.ObservationPoints, mapdraw.ObservationPoint{Pref: pt.Pref, Name: pt.Name, Scale: pt.Scale})
	}
	return req, nil
}

type tsunamiParams struct {
	Trim    bool   `json:"trim"`
	Draw    bool   `json:"draw"`
	MapType string `json:"mapType"`
	Regions []struct {
		Region   string `json:"region"`
		Category string `json:"category"`
	} `json:"regions"`
}

func tsunamiCategory(s string) mapdraw.TsunamiCategory {
	switch s {
	case "Advisory", "津波注意報":
		return mapdraw.Advisory
	case "Warning", "津波警報":
		return mapdraw.Warning
	case "MajorWarning", "大津波警報":
		return mapdraw.MajorWarning
	}
	return mapdraw.TsunamiUnknown
}

func tsunamiRequest(body []byte) (mapdraw.Request, error) {
	var p tsunamiParams
	if err := json.Unmarshal(body, &p); err != nil {
		return mapdraw.Request{}, err
	}
	t, err := mapdraw.ParseMapType(p.MapType)
	if err != nil {
		return mapdraw.Request{}, err
	}
	req := mapdraw.Request{MapType: t, Trim: p.Trim, HideDraw: !p.Draw, AspectRatio: aspectRatio}
	for _, r := range p.Regions {
		req.TsunamiPoints = append(req.TsunamiPoints, mapdraw.TsunamiPoint{Name: r.Region, Category: tsunamiCategory(r.Category)})
	}
	return req, nil
}

type eewParams struct {
	Trim      bool     `json:"trim"`
	MapType   string   `json:"mapType"`
	Latitude  *float64 `json:"latitude"`
	Longitude *float64 `json:"longitude"`
	Areas     []struct {
		Pref string `json:"pref"`
	} `json:"areas"`
}

func eewRequest(body []byte) (mapdraw.Request, error) {
	var p eewParams
	if err := json.Unmarshal(body, &p); err != nil {
		return mapdraw.Request{}, err
	}
	t, err := mapdraw.ParseMapType(p.MapType)
	if err != nil {
		return mapdraw.Request{}, err
	}
	req := mapdraw.Request{MapType: t, Trim: p.Trim, AspectRatio: aspectRatio, Hypocenter: hypocenter(p.Latitude, p.Longitude)}
	seen := map[string]bool{}
	for _, a := range p.Areas {
		if seen[a.Pref] {
			continue
		}
		seen[a.Pref] = true
		req.EEWAreaCodes = append(req.EEWAreaCodes, mapdraw.EEWAreaCode(a.Pref))
	}
	return req, nil
}

type userquakeParams struct {
	Trim        bool   `json:"trim"`
	MapType     string `json:"mapType"`
	Confidences []struct {
		AreaCode   string  `json:"areaCode"`
		Confidence float64 `json:"confidence"`
	} `json:"confidences"`
}

func userquakeRequest(body []byte) (mapdraw.Request, error) {
	var p userquakeParams
	if err := json.Unmarshal(body, &p); err != nil {
		return mapdraw.Request{}, err
	}
	t, err := mapdraw.ParseMapType(p.MapType)
	if err != nil {
		return mapdraw.Request{}, err
	}
	req := mapdraw.Request{MapType: t, Trim: p.Trim, AspectRatio: aspectRatio}
	for _, c := range p.Confidences {
		req.UserquakePoints = append(req.UserquakePoints, mapdraw.UserquakePoint{AreaCode: c.AreaCode, Confidence: c.Confidence})
	}
	return req, nil
}
