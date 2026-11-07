# リポジトリのルートをビルドコンテキストにしてください:
#   docker build -f MapServerGo/Dockerfile -t mapserver-go .
FROM golang:1.24 AS build
WORKDIR /src/MapServerGo
COPY MapServerGo/go.mod MapServerGo/go.sum ./
RUN go mod download
COPY MapServerGo/ ./
RUN CGO_ENABLED=0 go build -trimpath -ldflags="-s -w" -o /mapserver .

FROM gcr.io/distroless/static-debian12:nonroot
COPY --from=build /mapserver /app/mapserver
COPY Map/Resources/Maps/japan-gsi_*-8bit.png Map/Resources/Maps/world_512.png Map/Resources/Maps/world_1024.png /app/Resources/Maps/
COPY Map/Resources/Images/*.png /app/Resources/Images/
COPY Map/Resources/Points/*.csv Map/Resources/Points/earthquake.json Map/Resources/Points/tsunami.json Map/Resources/Points/userquake.json /app/Resources/Points/
# Cloud Run は環境変数 PORT で待ち受けポートを指定します (未指定時は 17101)
EXPOSE 17101
ENTRYPOINT ["/app/mapserver"]
