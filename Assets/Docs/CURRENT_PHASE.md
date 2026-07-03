# Current Phase

## Current Phase

Phase 2.0 準備中

## Previous Phase

Phase 1.8 管制文テンプレート化

## Phase 1.8で完了したこと

- Phase 1.7で追加した管制官ログ文言をテンプレート化
- commandId、controllerJapaneseText、controllerEnglishText、futurePilotReadbackJapaneseText、futurePilotReadbackEnglishText、futureAudioKey を持てる構造を追加
- Clear to Land / Taxi to Gate / Pushback / Taxi to Holding Point / Hold Short / Line Up and Wait / Cleared for Takeoff の文言テンプレートを登録
- 画面表示は引き続き日本語の管制官指示のみ
- パイロット復唱、英語表示、英語音声は後続Phaseに回す

## Current Phaseの目的

Phase 2.0では、航空機データ拡張を扱います。
便名、機種、使用滑走路、ゲート/スポット、状態、担当管制ポジション、出発地/目的地などを、今後のフライトストリップや複数機運用へ接続できる形に整理します。

## Phase 2.0で実装してよいこと

- 航空機データ構造の整理
- 便名
- 機種
- 使用滑走路
- ゲート/スポット
- 状態
- 担当管制ポジション
- 出発地/目的地
- 将来のフライトストリップ表示に必要な項目整理
- Docs/READMEの更新

## Phase 2.0で実装しないこと

- Contact Ground
- Contact Tower
- Contact Departure
- 英語音声の実装
- 音声ファイル追加
- Text to Speech連携
- フライトストリップ本実装
- レーダー/ミニマップ
- 空港全体俯瞰ビュー
- 経路プレビュー
- スコアランク制
- 効率性評価の本格実装

## 作業判断

Phase 2.0では、操作やUIを大きく増やすよりも、今後のフライトストリップ、複数機運用、管制ポジション表示に耐えられる航空機データの土台を整えます。
フライトストリップ本実装や複数機運用は後続Phaseで扱います。
Phase 2.0の次の候補として、Phase 2.3「フライトストリップ方式」を扱います。
