# DummyPeer

動作確認用ダミーピア実装

## 機能

- 過去情報のランダム配信

## 注意点

- プロトコル時刻
  - コンピュータの時計を使用します
- 署名（公開鍵暗号）
  - 期限内の有効な署名を付与します
  - ただし、サーバー保証用公開鍵・ピア保証用公開鍵は実環境と異なります

### サーバー保証用公開鍵

PKCS #8 (DER) 形式: 
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDB+t0YTWlu3FFiwTb05u2bHWJRpCQJeAzhia6pWJ5BqsVIXgG7zeiHu4cFWrKME7fHQsjlihjnhtaksEtkmnbODUHnNi26FStSSpyo8ex0FZDfXtoQ9VB0m6UxdrGznpzfO9PWbpC0iSoCAyeqILLcDDbuBv5xY6+0D35kQx74kQIDAQAB

### ピア保証用公開鍵

PKCS #8 (DER) 形式: 
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDImNYMTSCn3CWtQ8uq9qbU5WGQcdtAorXGD7/3ahK+CMkngJpkMdfUtuPRbMfDvbt/szNU38BYmMMQWK1OC/iav4Nac7fWHuXMYprCeyULFpQXmyLWySTPUyHs/zYw067wbO751RuV6bAmjH2TlRv8Yb1UY/atZRuYSiu58Z4cCQIDAQAB
