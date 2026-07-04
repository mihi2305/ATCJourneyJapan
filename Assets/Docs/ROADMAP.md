# ATCJourneyJapan Roadmap

このロードマップは、ATCJourneyJapanの開発Phaseを整理する正本です。
作業前には `CURRENT_PHASE.md` と合わせて確認してください。

| Phase | 状態 | 目的 | 内容 |
| --- | --- | --- | --- |
| Phase 0 | 完了 | 動く土台 | A滑走路 / 到着機1機 / 出発機1機 / Stage Clear |
| Phase 1.1 | ほぼ完了 | UI可読性・基本HUD改善 | 日本語HUD / ボタン整理 / 機体ラベル整理 / 右側指示欄 / チュートリアルUIの基本改善 |
| Phase 1.15 | 完了 | 基本動作とチュートリアル体験の修正 | 航空機が滑走路・誘導路・ゲート上を自然に動く / 固定waypointで到着機・出発機を自然に移動 / 一時停止型チュートリアル / 1回に1概念だけ説明 / チュートリアル中の下部ガイド重複を削除 / 結果画面の日本語化 / チュートリアル中の遅延評価を緩和 / 下部パネルは将来的に日本語の管制通信ログへ発展 / 英語音声は将来Phaseで扱う |
| Phase 1.2 | 完了 | 初回訓練 前半: Tower到着編 | 1本の初回訓練内で扱う / 滑走路1本1機ルール / Clear to Land / 滑走路使用中 / 滑走路を空ける / Taxi to Gate / 到着機AJJ101を中心に、到着処理の意味を段階的に教える / A滑走路・AJJ101・Gate 1・対象指示ボタンを軽くハイライト |
| Phase 1.3 | 完了 | 初回訓練 後半: Tower出発編 | 1本の初回訓練内で扱う / Taxi to Holding Point / Line Up and Wait / Cleared for Takeoff / 離陸 / 出発機AJJ202の基本処理 |
| Phase 1.4 | 完了 | Ground基礎編 | Pushback / Taxi to Holding Pointの意味の深掘り / Hold Short / Hold ShortとLine Up and Waitの違い / 地上移動の基礎理解 / Contact Ground・Contact TowerとGround管制の本格切替は後続Phase |
| Phase 1.5 | 未着手 | PC/WebGL・スマホ横画面対応 | 大きめボタン / タップ対応 / 中央を邪魔しないUI / 横画面で破綻しない配置 |
| Phase 1.6 | 未着手 | 管制手順リサーチDocs化 | 既存航空管制ゲームの分析 / YouTube等の操作フロー分析 / 公式資料を参照した管制手順整理 / 実装前の根拠整理 |
| Phase 1.7 | 完了 | 管制文ログ | 下部パネルを管制官指示ログへ発展 / 管制官の指示を日本語表示 / 初回チュートリアルの読みやすさを優先し、パイロット復唱は今回は非表示 / 将来的な英語音声に接続できる構造にする |
| Phase 1.8 | 完了 | 管制文テンプレート化 | commandId / 日本語管制文 / 英語管制文候補 / パイロット復唱候補 / futureAudioKey / コマンドごとにテンプレート管理 / 表示は日本語の管制官指示のみ |
| Phase 2.0 | 完了 | 航空機データ拡張 | 便名 / 機種 / 運航種別 / 使用滑走路 / Spot / 状態 / 担当管制ポジション / 出発地/目的地 / nextTarget / 推奨コマンドID / A滑走路をRWY_A・18L/36Rとして扱う土台 / B滑走路は将来拡張用データのみ |
| Phase 2.2 | 次に実装予定 | 複数機運用 | 到着2機 / 出発2機 / 時間差出現 / マルチタスク発生 |
| Phase 2.3 | 完了 | フライトストリップ方式 | 左側に到着 ARRIVAL / 出発 DEPARTURE リスト / 便名帯で航空機を選択 / 推奨指示待ちストリップをハイライト / 機体クリック依存から脱却する土台 |
| Phase 2.4 | 完了 | ストリップ連動コマンド | 選択ストリップ右側に独立したコマンドポップアップを表示 / 右側固定指示欄を暫定維持 / ストリップ側ボタンから既存コマンドを実行 |
| Phase 2.5 | 未着手 | 簡易レーダー/ミニマップ | 右上に空港/接近機/滑走路/誘導路/位置表示 / 接近機の距離・方位・進入経路を表示 |
| Phase 2.6 | 未着手 | 経路プレビュー | 選択中機体の予定ルートを線で表示 / Taxi to GateやLine Up後の動きが予測できるようにする |
| Phase 2.7 | 未着手 | 空港全体俯瞰ビュー | 上空カメラ / 滑走路 / 誘導路 / ゲート / 航空機ラベル / 空港全体を管制している感覚 |
| Phase 3.0 | 未着手 | 安全管理をゲーム化 | 滑走路占有 / Hold Short違反 / 間隔不足 / 危険操作ペナルティ / Hold Taxi / Resume Taxi / Go-Around / Safetyに意味を持たせる |
| Phase 3.2 | 未着手 | スコア・ランク・制限時間 | Point / Rank / Clear条件 / Time Bonus / 遅延評価 / 効率性評価 / S/A/B/Cランク / 既存ゲームを参考に評価設計を検討 |
| Phase 3.4 | 未着手 | Clearance Delivery / 出発承認基礎 | 出発承認 / Departure Clearance / Pushback前の前提承認 / Clearance Delivery・Ground・Towerの役割整理 |
| Phase 3.5 | 未着手 | Contact / Handoff導入 | Contact Ground / Contact Tower / Contact Departure / 管制ポジション引き継ぎ |
| Phase 4.0 | 未着手 | ステージ制 | 必要に応じて到着訓練・出発訓練・Ground訓練を別ステージ化 / Tower -> Ground -> Approach -> Departure の段階解放 / 空港管制官として成長していく構造 |
| Phase 5.0 | 未着手 | ストーリー導入 | 沖縄出身の新人管制官 / 教官 / 那覇配属 / 訓練イベント / 成長物語 |
| Phase 6.0以降 | 未着手 | 視覚・没入感強化 | 簡易飛行機モデル / 滑走路・誘導路・ゲート改善 / 那覇らしさ / 海、島、管制塔、A/B滑走路 / 英語音声 / 効果音 / 天候 / 夜間 / 視点切替 |
| Phase 6.1 | 未着手 | UI Visual Polish | フォント統一 / ストリップの質感改善 / ボタンデザイン改善 / 選択中・指示待ち・警告の視覚表現強化 / 航空管制ゲームらしいHUD表現 / アイコン追加 |

## 運用ルール

- Phaseの追加・変更・延期を行う場合は、必ず `CHANGELOG_ROADMAP.md` に記録する
- 現在作業するPhaseの詳細は `CURRENT_PHASE.md` に書く
- 実装中に範囲外の良いアイデアが出た場合は、実装せず後続Phase候補としてDocsに残す
- Phase 1.2とPhase 1.3は、現時点では別ステージとして分離せず、1本の初回訓練チュートリアルの前半/後半として扱う
- Phase 2.0以降、初心者向けUIでは「A滑走路」を基本表記にしつつ、内部データでは`RWY_A`と実滑走路番号`18L/36R`を保持する
- 将来的な管制文テンプレートでは、状況に応じて`A滑走路`から`RWY 18L` / `Runway 18L`へ切り替えられるようにする
- B滑走路は`RWY_B`、`18R/36L`として将来拡張用データに残すが、Phase 2.0では画面表示や運用には使わない
- プッシュバック車両/TowCarの見た目や実アニメーションは、Phase 6.0以降またはPhase 10.0相当の没入感強化で扱う
- Phase 2.0 QA以降、機種は`B737級`のような曖昧表記ではなく、`B737` / `A320`のような機種名表記を基本にする
- 空港内の駐機位置はGateよりもSpot表記を基本にし、AJJ101の到着先は`SPOT 01`、AJJ202の出発位置は`SPOT 02`として扱う
- 外部表示では`RWY 18L` / `SPOT 01`のような短い目的地表示を使い、内部データではRWY、Spot、nextTarget、recommendedCommandIdを保持する
- 将来的な情報表示は左下に集約せず、左側フライトストリップ、右下詳細、機体ラベルへ役割分担する
- Phase 2.3以降、航空機Objectクリックに加えてフライトストリップクリックでも航空機を選択できるようにする
- 将来的には航空機Objectクリックを補助操作とし、フライトストリップを主操作にする
- 右側固定の指示欄は暫定UIであり、Phase 2.4で選択中ストリップ内またはストリップ横の指示UIへ段階的に統合する
- Phase 2.4では、選択中ストリップの右側に独立したコマンドポップアップを表示し、右側固定指示欄と同じ既存コマンド処理を呼び出す
- コマンドポップアップはストリップ領域から右にはみ出してもよく、ボタン内の日本語/英語2行が潰れない幅と高さを確保する
- 将来的に最大4個程度のコマンドボタンを縦に並べられる構造を想定する
- ストリップ連動コマンドボタンはストリップ下に置かず、縦リストの一覧性を維持する
- 1機に対して常に全ボタンを表示せず、現在状態で出せる指示だけを1〜2個表示する
- プレイヤーにボタン探しをさせるのではなく、どの機体にいつ指示を出すかに集中させる
- 右側固定指示欄は当面の暫定UIとして残し、将来的に縮小または廃止してストリップ側へ統合する
- 複数機運用を増やす前に、ストリップ方式とストリップ連動コマンドUIを整える
- Phase 2.3 QA以降、ストリップは一覧性を優先し、便名、機種、RWY、SPOT、短い状態/推奨指示だけを表示する
- Phase 2.3 QA再修正以降、ストリップ表示はさらに絞り、原則として便名 + RWY/SPOTのみを表示する
- ストリップは横長の細い長方形とし、複数機運用に備えて多数の便を縦に並べられるサイズにする
- ストリップでは便名を大きく、RWY/SPOTを小さく表示する
- 機種、状態、推奨指示、出発地、到着地、時刻は右下詳細パネルへ移す
- 時刻、出発地、到着地、詳細ルートは将来的な右下詳細パネルへ分離する
- 航空機データには将来的に scheduledDepartureTime / estimatedDepartureTime / actualDepartureTime / scheduledArrivalTime / estimatedArrivalTime / actualArrivalTime / delayMinutes を持たせる
- 左側ストリップには必要になった場合のみ `DEP 08:10` / `ARR 08:25` のように短く時刻を表示し、詳細な時刻比較や遅延分は右下詳細で扱う
- 遅延や効率性評価の本格実装はPhase 3.2で扱う
- Phase 3.4では、Pushback前に「出発承認 / Departure Clearance」を受ける流れを追加し、Clearance Delivery、Ground、Towerの違いを教える入口にする
- Phase 3.4ではDeparture ClearanceボタンやClearance Delivery管制ポジションを検討するが、Phase 2.4 QAでは実装しない
- 将来的な出発便コマンド候補は、Flight Clearance、Pushback、Pushback Direction、Taxi Permit、滑走路/誘導路ルート選択、Line Up and Wait、Cleared for Takeoff、Hand-off to Departureとする
- 将来的な到着便コマンド候補は、Approach Contact、Runway Select / ILS Approach、Clear to Land、Go-Around、Hand-off to Ground、Taxi to Spotとする
- 将来的な共通/緊急コマンド候補は、Hold Taxi、Resume Taxi、Go-Around、Hand-offとする
- Flight Clearance / 出発承認はPhase 3.4、Hold Taxi / Resume TaxiはPhase 3.0、Go-AroundはPhase 3.0〜3.2、Hand-offはPhase 3.5で扱う
- Pushback方向選択やTaxiルート選択は、Phase 3.0以降またはステージ制導入後に扱う
- Phase 2.xでは、見た目の完成度よりも操作構造を優先する
- フォント、色、枠、ボタン質感、航空管制ゲームらしいHUD表現は、Phase 6.1 UI Visual Polishで扱う
