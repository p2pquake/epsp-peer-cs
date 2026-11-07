package main

import (
	"bytes"
	"encoding/json"
	"image/png"
	"net/http"
	"net/http/httptest"
	"os"
	"path/filepath"
	"testing"

	"github.com/p2pquake/epsp-peer-cs/MapServerGo/mapdraw"
)

// testdata/cases.json の width/height は、同じリクエストを C# 版 (`CLI map server`) に送ったときの出力サイズです。
// トリム範囲の計算が C# 版と一致していることを確認します。
type testCase struct {
	Name   string `json:"name"`
	Path   string `json:"path"`
	Width  int    `json:"width"`
	Height int    `json:"height"`
}

func newTestServer(t *testing.T) *httptest.Server {
	t.Helper()
	dir := filepath.Join("..", "Map", "Resources")
	srv := httptest.NewServer(newMux(mapdraw.NewRenderer(dir)))
	t.Cleanup(srv.Close)
	return srv
}

func TestRenderMatchesCSharpSize(t *testing.T) {
	srv := newTestServer(t)

	b, err := os.ReadFile(filepath.Join("testdata", "cases.json"))
	if err != nil {
		t.Fatal(err)
	}
	var cases []testCase
	if err := json.Unmarshal(b, &cases); err != nil {
		t.Fatal(err)
	}

	for _, c := range cases {
		t.Run(c.Name, func(t *testing.T) {
			body, err := os.ReadFile(filepath.Join("testdata", c.Name+".json"))
			if err != nil {
				t.Fatal(err)
			}
			res, err := http.Post(srv.URL+c.Path, "application/json", bytes.NewReader(body))
			if err != nil {
				t.Fatal(err)
			}
			defer res.Body.Close()
			if res.StatusCode != http.StatusOK {
				t.Fatalf("status = %d", res.StatusCode)
			}
			if ct := res.Header.Get("Content-Type"); ct != "image/png" {
				t.Fatalf("Content-Type = %q", ct)
			}
			cfg, err := png.DecodeConfig(res.Body)
			if err != nil {
				t.Fatal(err)
			}
			if cfg.Width != c.Width || cfg.Height != c.Height {
				t.Errorf("size = %dx%d, want %dx%d (C# 版の出力)", cfg.Width, cfg.Height, c.Width, c.Height)
			}
		})
	}
}

func TestEndpoints(t *testing.T) {
	srv := newTestServer(t)

	for _, c := range []struct {
		method, path, body string
		status             int
		want               string
	}{
		{"GET", "/test", "", http.StatusOK, "OK"},
		{"GET", "/unknown", "", http.StatusNotFound, "Function not found"},
		{"POST", "/earthquake", `{"trim":true,"mapType":"INVALID","points":[]}`, http.StatusInternalServerError, ""},
		// 描画対象がなくトリム範囲を決められない場合はエラー (C# 版も 500)
		{"POST", "/earthquake", `{"trim":true,"mapType":"JAPAN_1024","points":[]}`, http.StatusInternalServerError, ""},
	} {
		req, _ := http.NewRequest(c.method, srv.URL+c.path, bytes.NewBufferString(c.body))
		res, err := http.DefaultClient.Do(req)
		if err != nil {
			t.Fatal(err)
		}
		var buf bytes.Buffer
		buf.ReadFrom(res.Body)
		res.Body.Close()
		if res.StatusCode != c.status {
			t.Errorf("%s %s: status = %d, want %d", c.method, c.path, res.StatusCode, c.status)
		}
		if c.want != "" && buf.String() != c.want {
			t.Errorf("%s %s: body = %q, want %q", c.method, c.path, buf.String(), c.want)
		}
	}
}

func TestEEWAreaCode(t *testing.T) {
	if got := mapdraw.EEWAreaCode("千葉"); got != "174" {
		t.Errorf("千葉 = %s", got)
	}
	if got := mapdraw.EEWAreaCode("存在しない"); got != "-1" {
		t.Errorf("unknown = %s", got)
	}
}
