# Current Phase

## Current Phase

Phase 1.7 準備中

## Previous Phase

Phase 1.4 Ground基礎編

## Phase 1.4で完了したこと

- 初回訓練内の出発訓練前半にGround基礎を追加
- Pushbackを追加し、AJJ202をGate 2から後退させて出発準備する流れを追加
- Taxi to Holding Pointを、滑走路手前まで地上走行させるGround基礎として説明
- Hold Shortを追加し、滑走路に入る前に手前で待機させる流れを追加
- Hold ShortとLine Up and Waitの違いを短く説明
- Contact Ground / Contact Tower とGround管制の本格切替は後続Phaseに回す

## Current Phaseの目的

Phase 1.7では、管制文ログを扱います。
初回訓練の没入感を高めるため、下部パネルを管制官とパイロットの通信ログへ発展させます。

## Phase 1.7で実装してよいこと

- 管制官の指示を日本語で表示する通信ログ
- パイロット復唱を日本語で表示する通信ログ
- Phase 1の既存コマンドに対応した短い管制文
- 将来の英語音声に接続しやすい文言整理
- 下部パネルの役割整理
- Docs/READMEの更新

## Phase 1.7で実装しないこと

- Contact Ground
- Contact Tower
- Contact Departure
- 英語音声の実装
- フライトストリップ本実装
- レーダー/ミニマップ
- 空港全体俯瞰ビュー
- 経路プレビュー
- スコアランク制
- 効率性評価の本格実装

## 作業判断

Phase 1.7では、操作そのものを増やすよりも、初回訓練の没入感と理解しやすさを高めることを優先します。
航空機データ拡張やフライトストリップへ進む前に、現在の1本チュートリアルで出した指示がログとして残る体験を整えます。
