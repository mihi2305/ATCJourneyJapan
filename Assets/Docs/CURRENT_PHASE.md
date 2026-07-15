# Current Phase

## Current Phase

Phase 3.9-5U-0C: Departure Airborne Exit Flow追加

## Phase 3.9-5U-0Cで直したこと

- 出発機がairborne後に滑走路端付近で滞留せず、滑走路方向に沿って空港外のDeparture Exitへ抜けるrouteを追加した
- Takeoff Roll / Rotation / Liftoffまでは既存profileを維持し、airborne到達後にInitial Climb point、Departure Exit pointへ進むようにした
- RWY 18L / 36Rの離陸方向は既存のRunwayGeometryから取得し、滑走路方向と矛盾しないようにした
- 滑走路占有はTakeoff Roll中に維持し、airborne後の既存releaseを保ったままDeparture Exit到達時に処理完了へ進める
- 広域空域、Departure Route本格実装、Approach MapはPhase 6以降に回す

Phase 3.9-5U-0B: 固定4機シナリオ土台

## Phase 3.9-5U-0Bで直したこと

- 固定シナリオを2 arrival + 2 departureの4機構成へ整理した
- ARRIVALはAJJ101/B737/SPOT 01、AJJ103/A320/SPOT 04、DEPARTUREはAJJ202/A320/SPOT 02、AJJ204/B737/SPOT 03に割り当てた
- 出発機2機はそれぞれ別SPOT上でAtGate開始し、滑走路横や誘導路外に直接配置されない状態を維持した
- 航空機生成時にcallsign、Arrival/Departure、aircraftType、spot、positionを1回だけDebug Logへ出すようにした
- Stage Clearは2機処理ではなく、登録済み4機すべての処理完了を待つようにした
- Runway Conflict / Taxi Conflict / Near Miss検知、Mission Failed、事故演出は次Phase以降に回す

Phase 3.9-5U-0A: 出発機初期SPOT配置修正

## Phase 3.9-5U-0Aで直したこと

- 出発機の初期配置を、固定のDepartureSpawnPositionやHold Short付近ではなく、割り当てられたSPOT定義のpositionから決めるようにした
- AJJ202はSPOT 02、AJJ204はSPOT 04でAtGate開始になり、滑走路横・誘導路外に直接置かれないようにした
- spotIdが未設定またはSpot定義に見つからない場合は、SPOT 01へ安全にfallbackし、AircraftData側のSpot表示も揃えるようにした
- 航空機生成時にcallsign、Departure、spot、position、fallback有無を1回だけDebug Logへ出すようにした
- 最大4機程度の複数機運用、インシデント検知、Mission Failed演出は次Phase以降に回す

Phase 3.9-5T-3B: 着陸Final Approach表示調整

## Phase 3.9-5T-3Bで直したこと

- Unity上で着陸進入が浅く見えすぎたため、ゲーム表示用のFinal Approach降下角を3度基準から4度前後へ強めた
- Final Approach開始点を少し手前に延長し、touchdown pointへ向かう高度差が画面上で見えるようにした
- 降下経路角と機体の見た目pitchを分離し、Final中は軽い機首上げ、Flare中はさらに機首を起こす表示へ調整した
- Touchdown後は既存のLanding Rollout減速profileを使いながら、pitchを0度へ戻す流れを維持した
- 今回の値は実機完全再現ではなく、ゲーム画面で「降りてくる感」を出すための暫定表示調整値として扱う

Phase 3.9-5T-3: 離陸・着陸Pitch Profile調整

## Phase 3.9-5T-3で直したこと

- Runway Performance Profileに、rotation pitch、initial climb path angle、approach path angle、flare pitch、pitch smoothingを追加した
- 離陸時はTakeoff Roll終盤でゆっくりRotationし、airborne後は3〜6度程度の浅い上昇経路へ入るようにした
- 機体の見た目pitchと実際の上昇経路角を分け、30度以上の急上昇に見える挙動を避けるようにした
- 着陸時は約3度のFinal Approach、接地前のFlare、Rollout中の水平姿勢へつながる簡易profileを追加した
- Taxi / Pushback / Line Up中は基本pitch 0度へ戻し、既存のheading / turn / route選択 / 逆走防止は維持した
- 角度値は実機完全再現ではなくゲーム用の見た目調整値であり、将来は機種・重量・天候・滑走路長に応じて調整する

Phase 3.9-5T-2: 機種別Runway Performance Profile土台

## Phase 3.9-5T-2で直したこと

- A320/B737をmedium、B787/B777をheavyとして扱う、ゲーム用の暫定Runway Performance Profileを追加した
- 離陸滑走ではprofileごとの `takeoffRollDistanceRatio` を使い、全機が滑走路全長を使い切る見え方を避けるようにした
- 着陸滑走ではprofileごとの `landingRolloutDistanceRatio` と減速profileを使い、機種ごとにtaxi速度へ落ちる距離を変えられるようにした
- 滑走路占有、Line Up / Takeoffの逆走防止、到着後Taxi-to-Spotの既存フローは維持した
- 今回の値は実機性能の完全再現ではなく暫定ゲームバランス値であり、将来の機種データ・インシデント判定・ステージ難易度に合わせて調整する

Phase 3.9-5T: 管制交信ログ土台整理

## Phase 3.9-5Tで直したこと

- 既存の下部ログを作り直さず、管制官発話とパイロット復唱を1セットで表示する `管制交信ログ` へ整理した
- `CommandPhrase` を日本語表示文と将来英語音声用フレーズに分けて持てる構造へ整理した
- Pushback / Taxi to runway / Line Up / Takeoff clearance / Landing clearance / Taxi to Spot など既存主要指示の文言を `{CALLSIGN}` / `{RUNWAY}` / `{SPOT}` のテンプレートで扱うようにした
- 今回は音声再生、音声ファイル追加、外部TTS連携は実装していない
- STRIPS、右下情報パネル、Route選択、Taxi movementの既存フローは変更していない

Phase 3.9-5S: STRIPSの管制フェーズUI整理

## Phase 3.9-5Sで直したこと

- 左STRIPSを詳細情報カードではなく、便名・最低限のRWY/SPOT・管制フェーズ・次に出せる主指示中心の一覧UIへ寄せた
- STRIP内から目的地/出発地、予定時刻、Route詳細、詳細状態文を外し、詳細は右下航空機情報パネルへ寄せる方針にした
- STRIPS下部に簡易管制フェーズ表示を追加し、APPROACH / TOWER / GROUND / DEPARTURE の機体数と選択中機体のフェーズを確認できるようにした
- 選択中STRIPは従来通り色で強調し、主操作ボタンは選択中STRIP横のポップアップにだけ出す構造を維持した
- 出発/到着の滑走路選択、Route選択、Taxi / Line Up / Takeoff / Landing の既存フローは変更していない

Phase 3.9-5R-5C: Airport Map Overlay大型化

## Phase 3.9-5R-5Cで直したこと

- 滑走路選択Overlayとタクシールート選択Overlayを画面中央の大型パネルへ広げ、Airport Mapを主要表示として見やすくした
- 共通Airport Map描画は維持したまま、Overlay内のMap表示サイズを大きくし、滑走路・入口・SPOT・Route線を判断しやすくした
- 選択ボタンは右側、説明文は下部へ整理し、閉じるボタンは右上に維持した
- 出発機/到着機の滑走路選択、出発Route選択、Pushback後Taxi、Line Up / Takeoffの既存フローは変更していない

Phase 3.9-5R-5B: 空港マップ描画の共通化

## Phase 3.9-5R-5Bで直したこと

- 右上ミニマップ、滑走路選択Overlay、タクシールート選択Overlayの空港マップ描画を `UIManager` 内の共通描画helperへ寄せた
- A滑走路、主平行誘導路、5つの取付誘導路、SPOT 01〜04、ターミナル/エプロン簡略表示を同じworld座標基準から描くようにした
- Route highlight、滑走路選択表示、ミニマップ上の選択Route線が同じworld->map座標変換を使うようにした
- 出発/到着の滑走路選択、出発Route選択、Pushback後Taxi、Line Up / Takeoffの既存フローは維持した
- 到着機Exit選択、高速脱出/通常離脱UI、B滑走路運用は引き続き次Phase以降に回す

Phase 3.9-5R-5 追加修正: 到着機滑走路選択とMinimap整理

## Previous Phase

Phase 3.9-5R-4 Taxi Route Highlight QA / タクシールート見える化の完成

## Phase 3.9-5R-5追加修正で直したこと（到着機/Minimap）

- 右上MinimapのA滑走路周辺を、主平行誘導路1本と 36R端 / 36R側 / 中央 / 18L側 / 18L端 の5入口が見える構造へ整理した
- ターミナル前に別の長い平行誘導路があるように見えないよう、Minimap上のA側地上構造を現在のOverlay/メイン画面寄りに合わせた
- 到着機が `Inbound` かつRWY未選択の場合、STRIPから滑走路選択Overlayを開くようにした
- 到着機用の滑走路選択Overlayでは `RWY 18Lへ着陸` / `RWY 36Rへ着陸` と表示し、出発向け文言を流用しないようにした
- 初期データ上の既定RWYとは別に、到着機がOverlayで滑走路を選んだかをUI側で管理し、選択後は既存の `ActiveRunwayDesignator` に保持して着陸許可時は選択済みRWYだけを表示する
- 到着機のExit選択、高速脱出/通常離脱UI、Taxi-to-Spot route選択は次Phase以降に回す

## Phase 3.9-5R-5本修正で直したこと

- 出発機のPushback前フローで、STRIPから直接18L/36Rを選ぶのではなく、滑走路選択Overlayを開いてA滑走路の18L端/36R端を見ながら選べるようにした
- 滑走路選択Overlayは、Route selection overlayと同じ簡略空港マップ思想で、A滑走路本体、18L/36R端、主誘導路、18L側入口/中央入口/36R側入口を表示する
- RWY 18L / RWY 36Rボタンのhoverまたは選択時に、該当する滑走路端を黄色で強調し、非選択側を薄く表示する
- 滑走路選択後は既存通り `selectedRunwayDesignator` を航空機に保持し、そのRWYに一致する `TaxiRouteCandidate` だけをRoute selection overlayへ出す
- Route A/Bハイライト、Minimap route highlight、Pushback後Taxi、Line Up / Takeoffへの `activeRunwayEntryUsageId` 引き継ぎは既存挙動を維持した

## Phase 3.9-5R-5追加修正で直したこと

- 18L側Route選択時にMinimap上のRoute表示が静的connector表示に寄ってズレて見える問題を、選択中出発機の `TaxiRouteCandidate.waypoints` をMinimapにも描画する形で修正した
- Minimap route highlightは `SelectedDepartureRouteId + ActiveRunwayDesignator + SpotId` から同じcandidateを再解決し、Overlay / Minimap / 実Taxi movementの由来を揃える
- selected routeが無効な場合のみdefault candidateへfallbackし、segmentIdsだけselected由来、waypointsだけdefault由来になる状態を避ける方針を維持した
- Minimap route描画ログに、callsign、selected runway、selected route、actual route、entry usage、first/last waypoint、minimap first/last pointを残すようにした
- 36R側の既存route candidateやTaxi movementは変更していない

## Phase 3.9-5R-5で修正したこと

- 出発機の滑走路選択後、Route selection overlay上で選択中RWYを黄色のハイライトと `選択RWY 18L / 36R` 表示で確認できるようにした
- 18L / 36Rの端ラベルは選択側を強調し、非選択側は薄くして、どちらの滑走路方向を選んだか分かりやすくした
- Route overlay上に、candidateの `runwayEntryUsageId -> RunwayAccessUsage -> RunwayAccessPoint` から取得した入口位置ラベルを表示するようにした
- 滑走路選択時ログに、選択RWYに紐づくcandidate数、routeId、runwayEntryUsageIdを出し、Route候補が選択滑走路で絞られているか確認しやすくした
- Route A/Bのハイライト、`selectedDepartureRouteId` と実Taxi movementの一致、Pushback後の滑走路選択非表示は既存挙動を維持した
- 到着機新UI、B滑走路、3Dモデル、カメラ、スコア制は今回も対象外とした

## Phase 3.9-5R-4で修正したこと

- Route selection overlay上で、候補candidateの `waypoints` を全て薄い線で表示し、Route A/Bの違いをマップ上で見比べられるようにした
- hover中またはselected中のRouteは太い黄色線で強調し、非選択Routeは薄い線として残す
- 候補が1つだけの場合はRoute overlayを出さず、該当 `routeId` を自動選択する
- 候補数と表示数を一致させ、2候補ならRoute A/Bのみ表示し、存在しないRoute Cは出さない
- Route表示と実Taxi movementの一致確認用に、Route選択ログへ `segmentIds` と first/last waypoint を残す
- Taxi movement側は引き続き `confirmedDepartureRouteId` / `activeRunwayEntryUsageId` を使い、同じcandidate由来の `waypoints` / `segmentIds` / `runwayEntryUsageId` を引き継ぐ
- 到着機新UI、B滑走路、3Dモデル、カメラ、スコア制は今回も対象外とした

## Phase 3.9-5R-3Cで修正したこと

- 出発機の `selectedDepartureRouteId` と実Taxi movementの不一致を防ぐため、Pushback時に確定した `confirmedDepartureRouteId` をTaxi開始時まで保持するようにした
- Taxi開始時は `confirmedDepartureRouteId -> activeDepartureRouteId -> selectedDepartureRouteId` の順で同じ `TaxiRouteCandidate` を解決し、default routeのwaypointsが混ざる状況を避ける
- Taxi routeの `waypoints` / `segmentIds` / `runwayEntryUsageId` は解決済みcandidate由来で統一し、ログでも first/last waypoint とactive routeを確認できるようにした
- Hold Short位置を固定の滑走路方向別座標ではなく、`activeRunwayEntryUsageId` に対応する入口手前へ置くようにして、端側入口routeや中央入口routeとLine Up entryのズレを抑えた
- Line Up / Takeoffログにも `selectedDepartureRouteId` / `confirmedDepartureRouteId` / `activeRunwayEntryUsageId` を含め、同じrouteが引き継がれているか確認しやすくした
- 到着機UI、到着機movement、UIデザイン改善は今回も対象外とした

## Phase 3.9-5R-4 QA確認ポイント

### Phase 3.9-5R-4 Taxi Route Highlight QA / タクシールート見える化の完成

既存のRouteハイライトとRoute選択UIをゼロから作り直さず、選択候補と実際のTaxi movementが同じcandidate由来であることをUnity Playで確認する。

- Route線の視認性を上げる
- hover中Routeとselected Routeの見え方を整理する
- ハイライトRouteと実際のTaxi movementが同じcandidate由来であることを確認する
- 候補が1つなら自動選択する
- 候補が2つならRoute A/Bのみ表示する
- 存在しないRoute Cを出さない
- 表示名は `18L側端入口` / `中央入口` など、プレイヤーが分かる日本語にする

## Phase 5〜10 ロードマップ方針

| Phase | 主目的 | 主な内容 |
| --- | --- | --- |
| Phase 5 | 空港内の地上運用・UI・インシデント基盤 | A滑走路中心に、出発・到着・Taxi・Route選択・失敗判定までをゲームとして成立させる |
| Phase 6 | 空港周辺空域・海上進入・広域ミニマップ | 到着機が海上・沖縄島周辺の空域から接近し、Approach判断と空港内運用につながる構造を作る |
| Phase 7 | 那覇空港らしさ・リアル感強化 | AIP/航空写真を参考に、空港形状・ターミナル・スポット・自衛隊エリア・海上進入の雰囲気を強める |
| Phase 8 | B滑走路・複雑運用・混雑ゲーム性 | A/B滑走路、Crossfield Taxi、混雑、滑走路占有イベントをゲーム化する |
| Phase 9 | 3Dモデル・カメラ・演出強化 | 外部3Dモデル導入ルール、カメラ演出、空港車両、インシデント演出、見た目を強化する |
| Phase 10 | ステージ制・スコア・全国空港展開 | ステージ評価、リザルトレビュー、AirportData / StageData分離、他空港展開を進める |

### Phase 5 空港内の地上運用・UI・インシデント基盤

- Taxi Route Highlight QA、滑走路選択とRoute表示の連動、STRIPS情報階層整理
- 管制交信ログ化、右下情報パネル整理、チュートリアル/レビュー案内
- 到着機Exit選択、到着後Taxi-to-Spot Route選択、Taxi干渉のゲーム化
- インシデント検知、事故/ニアミス演出、失敗原因レビュー
- 設計変更: 通常プレイでは危険を事前警告で防ぎすぎず、危険な判断によって事故/ニアミス/Runway Conflictが発生し、その瞬間を演出として見せる
- Mission Failedやリザルトで原因を理解できるようにする。チュートリアルではヒントを出してよいが、通常プレイでは先に答えを教えすぎない

### Phase 6 空港周辺空域・海上進入・広域ミニマップ

- Arrival Spawn Point、Approach Route、Final Approach状態
- Departure Exit Point、離陸後のAirborne / Departed flow
- Airport Map / Approach Map の切替、広域ミニマップ
- 那覇周辺の海、沖縄島輪郭、那覇市街ブロック、港湾/海岸線の簡略表現
- Approach側の判断とインシデント演出を連動させる

### Phase 7 那覇空港らしさ・リアル感強化

- AIP/航空写真を参考に、誘導路・エプロン・ターミナル・スポットを再整理する
- DOM/INTLエリア拡張、PBB配置、管制塔位置調整
- 自衛隊エリアの雰囲気作り。自衛隊機は最初はプレイヤー操作対象ではなくNPCイベントとして扱う
- 海上進入、島影、市街地、民間/自衛隊混在による那覇ステージらしさを強める

### Phase 8 B滑走路・複雑運用・混雑ゲーム性

- B滑走路18R/36Lのデータ基盤
- A/B滑走路選択、A/B滑走路別占有管理
- Crossfield Taxi Route、混雑パターン
- 自衛隊機による滑走路占有イベント

### Phase 9 3Dモデル・カメラ・演出強化

- Sketchfab / Unity Asset Store等の外部3Dモデル導入ルール、ライセンス管理
- 航空機モデル改善。AJJなど架空塗装を優先する
- PBB、管制塔、ターミナル、空港車両
- 全体俯瞰、選択機追従、滑走路ビュー、管制塔ビュー、Spotビュー
- インシデント発生時のカメラ演出、海・空・光・UIの見た目調整

### Phase 10 ステージ制・スコア・全国空港展開

- ステージ時間、処理機数、遅延評価、安全評価、Near Miss / Incident評価、ランク
- リザルトレビュー、失敗原因の振り返り
- AirportData / StageData分離、空港選択画面
- 那覇から福岡・伊丹・羽田などへの展開

## Phase 3.9-5R-3Bで修正したこと

- 出発Route選択UIのプレイヤー向け表現を `タクシールート選択` / `Taxi Route` に統一し、到着機Spot Taxi route選択へも拡張しやすい言い方にした
- Route A/B/Cの説明を `18L側端入口` / `18L側入口` / `中央入口` / `36R側入口` / `36R側端入口` のような入口位置ベースに変更した
- Route候補は選択滑走路とSpotに一致するcandidateだけを表示し、表示順は `runwayEntryUsageId` の `runwayPositionRatio` を基準に安定化する
- `DEP_SPOT02_36R_B` を `RWY36R_ENTRY_MID` / `A_CONNECTOR_MID_01` / 中央入口waypointsに揃え、表示・segmentIds・waypoints・Line Up entryUsageのズレを修正した
- Taxi開始時はPushback時に確定した `activeDepartureRouteId` を優先してcandidateを再解決し、default routeのwaypointsが混ざらないようにした
- Pushback確定時 / Taxi開始時ログにsegmentIds、first/last waypoint、active route / entry usageを追加し、選択Routeと実移動routeの一致を確認しやすくした
- 表示名ではなく `routeId` / `runwayEntryUsageId` / `segmentIds` で管理する方針を維持した
- 到着機Exit選択UI、到着機movement同期、UI全体のデザイン刷新は次Phase以降に回す

## Phase 3.9-5R-3で修正したこと

- 出発機のRoute selection overlayで、選択滑走路とSpotに一致する実在candidateだけをRoute A/B/Cとして表示する方針を維持した
- 出発Route候補は滑走路進入方向の位置比に沿って並べ、端側取付誘導路が候補にある場合もRoute表示へ反映されやすくした
- Route説明を `18L側端取付誘導路` / `18L側取付誘導路` / `中央取付誘導路` / `36R側取付誘導路` / `36R側端取付誘導路` のような物理位置ベースに整理した
- `A_CONNECTOR_18L_END_01` / `A_CONNECTOR_36R_END_01` と `RWY18L_ENTRY_END` / `RWY36R_ENTRY_END` の既存candidateをRoute候補として認識できる状態を確認した
- Routeボタンのhoverまたは選択中candidateに合わせて、overlay map上にcandidate.waypoints由来の明るい経路ハイライトを表示するようにした
- Route A/B/Cは表示名に留め、内部選択は引き続き `routeId` / `runwayEntryUsageId` / `segmentIds` で管理する
- 到着機Exit選択UI、到着機movement同期、UI全体のデザイン刷新は次Phase以降に回す

## Phase 3.9-5R-2Cで修正したこと

- 出発機がTaxi to runway完了後、`HoldingPoint -> Hold Short -> Line Up -> Takeoff` へ進める流れを確認し、Line Up / Takeoff時のログを追加した
- Line Up時は既存の `activeRunwayEntryUsageId` を `AirportManager.GetLineUpRoute` へ渡し、選択Routeの取付誘導路から滑走路へ入る構造を維持した
- Takeoff時も `activeRunwayEntryUsageId` を使い、滑走路端へ逆走しない既存のintersection takeoff routeを維持した
- 到着機が `VacatingRunway` 中にTaxi to Spotへ切り替わる場合、Primary Runwayを先にReleaseして滑走路占有が残らないようにした
- 滑走路占有 / 解除 / Line Up拒否 / Line Up許可 / Takeoff許可の確認用 `Debug.Log` を追加した
- UI視認性改善、Routeハイライト、到着機Exit選択UIは次Phase以降に回す

## Phase 3.9-5R-2Bで修正したこと

- 出発機の滑走路選択をPushback前の1回に一元化し、Pushback後の `Taxi to runway` では選択済みRWYだけを表示するようにした
- `selectedRunwayDesignator` は既存の `AircraftData.ActiveRunwayDesignator` に保持し、Pushback / Taxi / Line Up / Takeoffへ引き継ぐ
- Pushback後 / Taxi中 / Hold Short以降では、STRIPS側に `滑走路を選択` や出発Route選択を再表示しない
- Route overlayは選択滑走路に一致する実在candidateだけを表示し、候補が2件ならRoute A/Bだけを出す。存在しないRoute Cは出さない
- 端側取付誘導路は既存の `A_CONNECTOR_18L_END_01` / `A_CONNECTOR_36R_END_01` と対応candidateを使う
- Route説明は `18L側端取付誘導路` / `中央取付誘導路` など物理位置ベースを維持し、評価語は使わない
- 将来はRouteボタンhover/selection時に、空港マップ上で該当Route線をハイライトする方針。今回は未実装
- 到着機UI / 到着機movement同期は未対応で次Phase以降に回す

## Phase 3.9-5R-2で修正したこと

- 出発機のPushback前フローを `滑走路選択 -> 取付誘導路/Route選択 -> Pushback -> Taxi` の順に整理した
- 出発機ごとの選択滑走路は既存の `AircraftData.ActiveRunwayDesignator` に `18L` / `36R` として保持する
- STRIPS側で、出発機が `AtGate` かつ滑走路未選択なら `滑走路を選択 RWY 18L / 36R` だけを表示するようにした
- 滑走路選択後は、その `selectedRunwayDesignator` に一致する `TaxiRouteCandidate` だけをRoute selection overlayに出す
- 18L側端の既存candidate `RWY18L_ENTRY_END` / `A_CONNECTOR_18L_END_01` をRoute候補として使えるようにし、表示は `18L側端取付誘導路` のような物理位置ベースにした
- RWYを選び直した時は既存の `selectedDepartureRouteId` をクリアし、別RWYのRouteが残らないようにした
- Pushback開始時は選択滑走路とrouteIdの一致を確認し、無効または未選択なら選択滑走路に合うdefault routeへfallbackする
- 到着機の高速脱出 / 通常離脱UI、到着機movement同期、Spot Taxi route選択UIは未対応で次Phase以降に回す

## Phase 3.9-5R-1で修正したこと

- 出発機の `selectedDepartureRouteId` をPushback開始時に確定し、未選択または無効な場合は `spotId + runwayDesignator` のdefault departure routeへfallbackするようにした
- Taxi to runway開始時に、選択済み `selectedDepartureRouteId` を優先して `TaxiRouteCandidate` を解決し、waypoints / segmentIds / runwayEntryUsageIdを使うようにした
- 確定した `runwayEntryUsageId` を `activeRunwayEntryUsageId` として保持し、既存のLine Up / Takeoff routeへ引き継ぐ構造を明示した
- Route確定時とTaxi適用時に、callsign / selectedDepartureRouteId / runwayEntryUsageId / runway / spot / fallbackUsedを `Debug.Log` で確認できるようにした
- Route未選択時も従来通りdefault route fallbackで動く
- 到着機の高速脱出 / 通常離脱UI、到着機movement同期、Spot Taxi route選択UI、UI視認性改善は次Phase以降に回す

## Phase 3.9-5Q-2で修正したこと

- Route / Exit選択の開始位置を、右下航空機情報パネルからSTRIPS側の管制指示ポップアップへ移した
- 右下航空機情報パネルは状態確認用に戻し、選択済みRoute / 離脱方式の表示だけを残した
- 出発機はPushback前の `AtGate` 状態で、取付誘導路候補が複数ある場合だけ `取付誘導路を選択` を出す方針にした
- 出発機Route overlayは `Route A` / `Route B` / `Route C` 表示のまま、説明を `18L側取付誘導路` / `中央取付誘導路` / `36R側取付誘導路` のような物理位置ベースにした
- 到着機はLanding Rollout中に、候補が複数ある場合だけ `離脱方式を選択` を出し、表示は `高速脱出` / `通常離脱` とした
- 到着後Spot Taxi routeは、滑走路離脱後の `Waiting` 状態で候補が複数ある場合だけRoute A/B/C選択を出す
- 候補が一意の場合はRoute selection overlayを出さず、通常の管制指示だけを表示する
- Route A/B/Cや `高速脱出` / `通常離脱` は表示名であり、内部は `TaxiRouteCandidate.routeId` / `runwayEntryUsageId` / `runwayExitUsageId` で管理する

## Phase 3.9-5Qで修正したこと

- 選択中航空機の右下情報パネルに `Route選択` ボタンを追加した
- `Route Selection` overlayを追加し、A滑走路・主平行誘導路・取付誘導路・ターミナルを簡略マップとして表示するようにした
- 選択中航空機の `spotId + runwayDesignator` から `TaxiRouteCandidate` 候補を取得し、出発は `runwayEntryUsageId`、到着は `runwayExitUsageId` を持つ候補を優先して表示する
- プレイヤー向けには `Route A` / `Route B` / `Route C` と表示し、TWY A1 / TWY A5などの誘導路名は主表示しない
- 選択した候補は `routeId` として `SelectedDepartureRouteId` / `SelectedArrivalRouteId` に保持し、Route A/B/Cの表示名はロジックキーに使わない
- default route fallbackは維持し、本格Route選択UI、ATCメニュー、B滑走路運用は次Phase以降に回す

## Phase 3.9-5Pで修正したこと（Japanese city labels）

- 選択中航空機情報パネルに、日本語都市名の表示を追加した
- 出発機は `東京行き` のように目的地、到着機は `宮崎発` / `福岡発` のように出発地を表示する
- 那覇ステージ前提のため、那覇は毎回 `那覇発` / `那覇行き` として表示しない
- 日本語都市名はUI表示用helperで変換し、routeId / segmentId / runway / spotなどのロジック判定には使わない
- Flight Stripには狭い範囲で `東京行き  RWY 18L` のような短い表示を追加した
- Route選択UI、本格ATCメニュー、B滑走路運用は次Phase以降に回す

## Phase 3.9-5Pで修正したこと（route switching foundation）

- Departure / Arrival taxi route candidateを、`spotId + runwayDesignator` の候補一覧からrouteId指定で選べる内部構造にした
- `selectedDepartureRouteId` / `selectedArrivalRouteId` を保持し、routeIdが一致すればそのcandidate、なければdefault candidateへfallbackするようにした
- 選択中航空機に対して、Debug用に `R` でDeparture候補、`T` でArrival候補を次candidateへ切り替えられるようにした
- Route選択ログにcallsign / purpose / runway / spot / routeId / displayName / routeInstructionText / segmentIds / runwayEntryUsageId / runwayExitUsageIdを含めた
- 本格Route選択UI、ATCメニュー、B滑走路運用は次Phase以降に回す

## Phase 3.9-5O-4で修正したこと

- ミニマップのA滑走路側誘導路表示を、現在のメイン画面に合わせて `A_MAIN_PARALLEL_01` 相当の主平行誘導路1本へ寄せた
- 追加済みのA滑走路端connector 2本と既存connector 2本をミニマップに反映した
- SPOT 01〜04は、長いapron-front taxiwayではなく短いstand lead-inで主平行誘導路へ接続して見えるようにした
- WorldToMiniMapの座標変換、aircraft marker、heading矢印回転、route datasetは変更していない
- Route選択UIと本格ミニマップ整理は次Phase以降に回す

## Phase 3.9-5O-3で修正したこと

- エプロン周辺で2本目の誘導路に見えていた長い黄色のpushback line visualを削除した
- Spot周辺の見た目は、主平行誘導路1本と `STAND_ENTRY_01`〜`04` の短いstand lead-in lineに寄せた
- TaxiwaySegment / RunwayAccessPoint / RunwayAccessUsage / TaxiRouteCandidate のdatasetやroute生成は変更していない
- ミニマップ本格整理は次Phase以降に回す

## Phase 3.9-5O-2で修正したこと

- ROAH AIP Aerodrome Chartを参考に、ゲーム用簡略化としてA滑走路側の主平行誘導路を `A_MAIN_PARALLEL_01` へ集約した
- `APRON_FRONT_01` はlegacy/provisional apron-side connectionとして残しつつ、通常Departure / Arrival routeのsegmentIdsから外した
- `STAND_ENTRY_01`〜`04` は各SPOTから `A_MAIN_PARALLEL_01` へ短く接続するstand linkとしてwaypointと見た目を調整した
- Departure routeは `STAND_ENTRY_x -> A_MAIN_PARALLEL_01 -> connector`、Arrival routeは `connector -> A_MAIN_PARALLEL_01 -> STAND_ENTRY_x` の構造へ寄せた
- 新しいデータ設計、new enum、new field、Route選択UIは追加していない

## Phase 3.9-5Oで修正したこと

- 既存の `TaxiwaySegment` / `RunwayAccessPoint` / `RunwayAccessUsage` / `TaxiRouteCandidate` datasetに沿って、A滑走路端connectorを2本追加した
- 追加connectorは `segmentId` / `accessPointId` / `usageId` / `routeId` をロジックキーとし、displayName / realWorldName / routeInstructionTextは表示用に留めた
- 端connector用のRunwayAccessPoint / RunwayAccessUsageを追加し、Departure entry / Arrival exitの将来候補として非default route candidateへ紐づけた
- エプロン前に新しい太い平行誘導路は追加せず、既存 `A_MAIN_PARALLEL_01` を主平行誘導路として少し延長した
- Route選択UI、本格ATCメニュー、高速脱出誘導路の本実装、B滑走路運用は次Phase以降に回す

## Phase 3.9-5N-6で修正したこと

- 通常Taxi速度をさらに下げ、Departure taxi / Arrival taxi-to-spot / Apron taxi / Vacate後taxiの体感速度を抑えた
- Pushback速度を独立定数化し、`pushbackSpeed < taxiTurnSpeed < taxiSpeed` の速度階層を明確にした
- Taxi turn phaseの速度倍率をさらに下げ、90度旋回で減速して曲がる見え方を強めた
- `landingRolloutEndSpeed` を新しいTaxi速度付近に合わせ、Landing Rollout後の速度差が不自然になりすぎないようにした
- Takeoff / Landing route、heading固定、逆走防止の構造は変更しない

## Phase 3.9-5N-5で修正したこと

- Landing Rolloutの減速を進捗率ベースへ寄せ、取付誘導路・Exit付近までにtaxiSpeed近くへ落ちる見え方を強めた
- `landingInitialSpeed` / `landingRolloutEndSpeed` の差を広げ、減速完了目安を `landingDecelerationCompletionProgress` として追加した
- Taxi turn phaseは `turnAngleThreshold` / `taxiTurnSpeedMultiplier` / `routeHeadingTurnSpeed` を再調整し、90度近い曲がりでより低速に見えるようにした
- Takeoff加速は `takeoffAccelerationTime` を短くし、ゲームテンポが遅くなりすぎないよう微調整した
- 今回は本格Smooth Turn、曲線誘導路生成、高速脱出速度判定、機種別性能は行わない

## Phase 3.9-5N-4で修正したこと

- Taxi / Taxi-to-Spot / Departure taxi / Vacate connector / LineUpで、次waypoint方向とのheading差が大きい間はturn phaseとして低速化するようにした
- `turnAngleThreshold` と `taxiTurnSpeedMultiplier` を追加し、旋回中は通常taxi速度より落としてから進む見え方にした
- route headingの回転速度を下げ、一瞬で向きが変わりすぎないようにした
- Landing Rolloutは初速と終了速度の差を広げ、減速時間を調整して、減速がより目視しやすいようにした
- Takeoff加速時間を少し短くし、前Phaseより離陸加速が遅すぎないようにした
- 今回は本格Smooth Turn、曲線誘導路生成、pitch / bank / liftoff animationは行わない

## Phase 3.9-5N-3で修正したこと

- Taxi-to-SpotやVacate connector移動中に、機体が進行方向を向かず横向きにスライドして見える問題をQA対象にした
- Route移動中は次waypoint方向を先読みし、yaw方向だけ `Quaternion.RotateTowards` で簡易的に追従するようにした
- Pushback中の後退姿勢とTakeoff Roll中の滑走路方向固定は維持し、Taxi / Vacate / Arrival Taxi-to-Spot / Departure Taxi / LineUp / Landing Rolloutの見た目を改善した
- Takeoff加速時間を少し短くし、Landing減速時間を少し長めにして、速度変化の見え方を再調整した
- 今回は本格Smooth Turn、pitch / liftoff animation、機種別性能、正確な離着陸距離計算は行わない

## Phase 3.9-5N-2で修正したこと

- Taxi / Pushback / Taxi-to-Spotの速度を少し下げ、地上走行が速すぎないようにした
- Takeoff Rollは初速を低め、最高速を少し抑えめ、加速時間を長めにして、滑走路上で徐々に速くなる見え方を強めた
- Landing Rolloutは減速時間を長めにし、接地後にすぐTaxi速度へ落ちず、滑走路上で減速していることが見えやすいようにした
- 今回の値はゲーム用の視認性チューニングであり、現実精密な機種別性能や離着陸距離計算ではない

## Phase 3.9-5Nで修正したこと

- Takeoff Roll中は一定速度ではなく、`takeoffInitialSpeed` から `takeoffMaxSpeed` へ簡易的に補間して加速するようにした
- Landing Rollout中は `landingInitialSpeed` から `landingRolloutEndSpeed` へ簡易的に補間して減速するようにした
- 地上走行速度を `taxiSpeed` として分け、Takeoff / Landing用の速度定数と補間時間を `AircraftController` に分離した
- Takeoff開始時とLanding Rollout開始時に、callsign / runway / speed profileを `Debug.Log` で1回だけ確認できるようにした
- 今回は見た目の自然さを上げる簡易モデルであり、機種別性能、正確な離着陸距離、加減速距離計算は今後のPhaseに回す

## Phase 3.9-5Mで修正したこと

- Taxi-to-Runwayで選ばれた `TaxiRouteCandidate.runwayEntryUsageId` を、TaxiToHold -> LineUp -> Takeoffまで保持するようにした
- Line Up時に `runwayEntryUsageId` がある場合は、対応する `RunwayAccessPoint` 付近から滑走路へ入り、滑走路端へ戻らないようにした
- Cleared for Takeoff時に `runwayEntryUsageId` がある場合は、現在位置またはentry pointから指定RWY方向へTakeoff routeを開始するようにした
- Takeoff開始時に、callsign / runway / routeId / runwayEntryUsageId / mode / start position / directionを `Debug.Log` で確認できるようにした
- 加速率、離陸距離、機種別性能、Route選択UI、B滑走路運用は次Phase以降に回す

## Phase 3.9-5Lで修正したこと

- 滑走路と誘導路の物理接続地点を `RunwayAccessPoint` として追加し、`accessPointId` を安定IDにした
- `RunwayAccessUsage` で、同じ物理接続地点をRWY 18L / 36RそれぞれのEntry / Exitとしてどう使うかを分離した
- 36R arrivalは36R側から18L側へ進むため、18L側寄りExitを使うのが自然な場合がある。18L arrivalも同様に36R側寄りExitを使うのが自然な場合がある
- 問題は「runwayDesignatorと反対側のExitを使うこと」ではなく、滑走路上を逆走してExitへ向かうこととして整理した
- `TaxiRouteCandidate` に `runwayEntryUsageId` / `runwayExitUsageId` を追加し、Departure candidateはEntry usage、Arrival candidateはExit usageを参照するようにした
- AccessPoint / Usage / candidate usage参照 / arrival default exitの妥当性を生成時に `Debug.LogWarning` で検証する
- 滑走路上の逆走防止、Intersection departure、加速・減速モデル、Route選択UIは次Phase以降に回す

## Phase 3.9-5K-2で修正したこと

- RWY 36R到着後に18L側connectorへ向かって見える問題を、connector座標、runwayDesignator保持、Vacate route固定targetの3点で切り分けた
- `A_CONNECTOR_18L_01` / `A_CONNECTOR_36R_01` のsegmentIdは維持し、Vacate routeとArrival taxi routeがconnector segmentのwaypointを直接参照する形に寄せた
- 着陸に使ったrunwayDesignatorを `lastLandingRunwayDesignator` として保持し、Taxi-to-Spot開始時はそれを優先してarrival route candidateを選ぶようにした
- Taxi-to-Spot開始時に、callsign / ActiveRunwayDesignator / selected routeId / connector segmentId / connector waypoint / current positionを `Debug.Log` で確認できるようにした
- 今回はRoute選択UI、誘導路完全再現、B滑走路運用、Smooth Turn、速度差・加減速は行わない

## Phase 3.9-5Kで修正したこと

- Arrival taxi-to-spotにも `TaxiwaySegment` / `TaxiRouteCandidate` を適用した
- RWY 18L / 36Rごとに到着後routeを分岐し、36R着陸後に18L側connectorを使う逆マッチングを避ける構造にした
- 到着後routeは `A_CONNECTOR_18L_01` / `A_CONNECTOR_36R_01` -> `A_MAIN_PARALLEL_01` -> `APRON_FRONT_01` -> `STAND_ENTRY_01`〜`04` のprovisional routeとした
- Taxi-to-Spot route選択時に、routeId / routeInstructionText / segmentIdsを `Debug.Log` で確認できるようにした
- 今回はRoute選択UI本実装、AIP完全再現、B滑走路運用、空港3D変更は行わない

## Phase 3.9-5J-2で修正したこと

- `TaxiwaySegment` / `TaxiRouteCandidate.segmentIds` の参照整合性を、生成時に一度だけ検証する処理を追加した
- 検証では、duplicate segmentId、存在しないsegment参照、空の `routeInstructionText`、SPOT+RWYごとのdefault candidate不足を `Debug.LogWarning` で確認できる
- `routeInstructionText` は `segmentIds` から `TaxiwaySegment.displayName` を引いて生成し、表示用として使う。ロジック判定には使わない
- `Taxi to RWY` 実行時に、callsign / spotId / runwayDesignator / routeId / displayName / routeInstructionText / segmentIds を `Debug.Log` で確認できるようにした
- 今回はRoute選択UI本実装、誘導路完全再現、ATC Log表示の本格更新は行わない

## Phase 3.9-5Jで修正したこと

- 誘導路routeを単なるwaypoint列だけでなく、将来のRoute選択UIや誘導路追加に使える `TaxiwaySegment` データとして整理した
- `segmentId` / `displayName` / `realWorldName` を分離し、ロジックは安定IDである `segmentId` を参照する方針にした
- 現在の `displayName` と `realWorldName` はprovisionalであり、将来AIP準拠名へ寄せてもroute logicを壊しにくい
- `TaxiRouteCandidate` に `segmentIds` と表示用 `routeInstructionText` を追加し、既存waypointsとdefault candidate運用は維持した
- 暫定segmentとして `A_MAIN_PARALLEL_01`、`A_CONNECTOR_18L_01`、`A_CONNECTOR_MID_01`、`A_CONNECTOR_36R_01`、`APRON_FRONT_01`、`STAND_ENTRY_01`〜`04`、将来用 `B_MAIN_PARALLEL_01` を追加した
- 今回はRoute選択UI本実装、AIP完全準拠名の確定、B滑走路運用、空港3D変更は行わない

## Phase 3.9-5Iで修正したこと

- 出発Taxi routeは現時点ではprovisionalであり、那覇空港の実誘導路を完全再現したものではない
- SPOT + RWYに対して、複数のTaxi route candidateを持てる構造を追加した
- 各candidateは `routeId`、`displayName`、`description`、`spotId`、`runwayDesignator`、`waypoints`、`isDefault` を持つ
- 現時点のTaxi to RWY 18L / 36Rは、該当SPOT + RWYのdefault candidateを使う
- 将来はコマンドUIで `Taxi via Route A / Route B` のようにcandidateを選択できるようにする
- 今回はRoute選択UI、那覇空港AIPどおりのTaxiway完全再現、B滑走路運用、Smooth Turn、速度差・加減速は実装しない

## Phase 3.9-5H-4で修正したこと

- ミニマップ表記は整ったが、航空機の着陸・離陸実移動が選択RWY方向に十分追従していない問題をQA対象にした
- 現在のミニマップ基準に合わせ、A滑走路は `36R = 左端から右向き`、`18L = 右端から左向き` のrouteを返すよう `RunwayGeometry` 登録を修正した
- `AirportManager.GetTakeoffRoute` は `RunwayGeometry.GetTakeoffRoute` を通し、Final Approach / Landing Rollout / Takeoff Rollをdesignator別routeへ寄せた
- TaxiToHold / HoldShort / LineUp / Takeoffは、開始時に確定した `ActiveRunwayDesignator` を使い、途中で別方向へ戻りにくい構造にした
- A滑走路端点ラベルは `36R` / `18L` に簡略化し、B滑走路には `36L` / `18R` の端点ラベルを追加した
- B滑走路運用、B滑走路route、複数滑走路運用はまだ開始しない

## Phase 3.9-5H-3で修正したこと

- Unity Play確認で、A滑走路ミニマップ端点ラベルが左右逆に見える問題をQA対象にした
- ミニマップ上のA滑走路端点ラベルを `left = 36R THR`、`right = 18L THR` に入れ替えた
- A滑走路中央ラベルは `A RWY 18L / 36R` を維持し、端点確認用の `THR` 表記も残した
- B滑走路には将来確認用の中央ラベル `B RWY 18R / 36L` を追加した
- B滑走路運用、B滑走路route、複数滑走路運用はまだ開始しない

## Phase 3.9-5H-2で修正したこと

- Unity Play確認で、表示上は `RWY 36R` でも到着routeが `18L` 相当の北側外方 -> 南向きに見える問題をQA対象にした
- A滑走路は `18L threshold = -X側北端`、`36R threshold = +X側南端` とし、`18L = 北端から南向き`、`36R = 南端から北向き` の航空ルールを維持する
- `RunwayGeometry` にdesignator別のFinal Approach route / Landing Rollout routeを持たせ、`AirportManager` の到着route生成をそのsource of truthへ寄せた
- `RWY 36R` / `Runway 36R` / `36R` の表記ゆれで36R側routeが18L扱いへ落ちないよう、空港route側と航空機データ側でdesignatorを正規化する
- ミニマップのA滑走路表示を `A RWY 18L / 36R` とし、北端に `18L THR`、南端に `36R THR` の確認ラベルを追加した
- Unity Editorでは、AJJ101の `Clear to Land RWY 36R` が南側外方 -> 36R threshold -> 北向きrolloutになるかを重点確認する

## Phase 3.9-5H QAで修正したこと

- 那覇空港の滑走路番号ルールに合わせ、A滑走路の `18L = 北端から南向き`、`36R = 南端から北向き` を明示した
- Unity座標では、A/B滑走路の `-X側 = 北端`、`+X側 = 南端` と定義する
- `RunwayGeometry` に `DesignatorNorthEnd` / `DesignatorSouthEnd`、`NorthEndPoint` / `SouthEndPoint`、`NorthToSouthDirection` / `SouthToNorthDirection` を追加し、方向定義を読み取りやすくした
- A滑走路は `18L threshold = NorthEndPoint`、`36R threshold = SouthEndPoint` として扱う
- `RWY 18L` / `Runway 18L` / `18L` のような滑走路表記ゆれは、`RunwayGeometry` 内でdesignatorへ正規化して扱う
- B滑走路は将来用に `18R = 北→南`、`36L = 南→北` として残すが、現Phaseでは運用しない

## Phase 3.9-5I前段で整理した到着route方針

- 到着機の `Clear to Land RWY 18L / RWY 36R` で、指定designatorに応じたFinal Approach Start / Final Approach Fix / threshold / touchdown / rollout endを `RunwayGeometry` から取得する
- Clear Landing時に到着機を滑走路中心線延長上のFinal Approach Startへ載せ、そこからFAF、threshold、touchdownへ進入させる
- Landing Rolloutはtouchdown pointからrollout end pointへ、滑走路中心線上を指定designator方向に進める
- Vacate routeはrollout end / vacate start付近から既存TaxiwayRouteDefinitionへつなぐ暫定構造を維持する
- 今回はSmooth Turn、速度・減速、到着後Taxi route全面再設計、B滑走路運用は行わない

## Phase 3.9-5Hで行うこと

- A滑走路を物理滑走路 `A`、運用方向 `RWY 18L / 36R` として整理し、Unity上では `18L = -X側thresholdから+Xへ進む方向`、`36R = +X側thresholdから-Xへ進む方向` と定義する
- B滑走路は将来用の物理滑走路 `B`、運用方向 `RWY 18R / 36L` として `RunwayGeometry` に登録するが、今回はコマンド表示・運用開始はしない
- `RunwayGeometry` に物理滑走路ID、両端designator、designator別heading / direction / threshold / lineup pointを持たせ、離着陸方向のsource of truthにする
- `TaxiToHold` / `LineUp` / `Takeoff` は、選択済みdesignatorに応じて18L側または36R側のHolding Point / Line Up位置へ向かう
- 今回はSmooth Turn、速度・加速、Takeoff pitch、Taxiway全面再設計、B滑走路運用は行わない

## Phase 3.9-5Gで行うこと

- 出発機はPushback後の `Taxi to RWY 18L / RWY 36R` で使用滑走路方向を確定し、`AircraftData.ActiveRunwayDesignator` に保存する
- `TaxiToHold` のrouteは選択済みRWY方向からHolding Pointを切り替える
- Hold Short / Line Up / Cleared for Takeoffは、Taxi clearanceで確定済みのRWY方向を表示・使用し、Line Up / Takeoff時に別方向を再選択させない
- 到着機の `Clear to Land RWY 18L / RWY 36R` は従来通り着陸許可時に選択する
- 今回はSmooth Turn、速度・加速、Takeoff pitch、B滑走路運用、空港レイアウト再配置は行わない

## Phase 3.9-5Fで行うこと

- Clear to Land / Line Up and Wait / Cleared for Takeoffのストリップ横コマンドを、RWY 18L / RWY 36R付きの選択肢として表示する
- 選択した滑走路方向を `CommandSystem.Execute(command, runwayDesignator)` 経由で `GameManager` / `AircraftData.ActiveRunwayDesignator` へ渡す
- 現在はA滑走路のみ運用のため、選択肢はRWY 18L / RWY 36Rに限定し、RWY 18R / 36Lは表示しない
- 管制ログと右下詳細は、航空機が保持する `ActiveRunwayDesignator` を使ってRWY方向を確認できる状態を維持する
- 今回はB滑走路運用、Taxi route全面再設計、Smooth Turn本格実装、速度・Pitch調整は行わない

## Phase 3.9-5Eで行うこと

- Clear to Land / Taxi to Hold / Hold Short / Line Up / Cleared for Takeoffの実行時に、現在のPrimary Runway方向を航空機の `ActiveRunwayDesignator` へ明示的にバインドする
- AJJ202 / AJJ101は `AircraftData.ActiveRunwayDesignator` を通じて、Line Up / Takeoff / Landing Rollout / Vacate routeを `RunwayGeometry` から取得する
- 現在のA滑走路訓練では `RWY 18L` を既定方向として扱い、`RWY 36R` へ切り替えられる構造を維持する
- 管制ログは `A滑走路` 固定表記ではなく、航空機が保持する `RWY 18L` などの滑走路方向を表示する
- 今回は滑走路選択UIの大規模再設計、B滑走路運用、Smooth Turn本格実装、速度・Pitch調整は行わない

## Phase 3.9-5Dで行うこと

- RWY 18L/36Rの中心線、端点、幅、Line Up位置、離着陸方向を `RunwayGeometry` として整理する
- 見た目のRunway A生成と航空機routeが、同じ滑走路ジオメトリのX/Z座標を参照するようにする
- 主要Taxiwayを `TaxiwayRouteDefinition` として定義し、Arrival exit / Gate linkなどのroute生成で参照できる土台を作る
- AJJ202 / AJJ101などは `AircraftData.ActiveRunwayDesignator` をもとにLine Up / Takeoff / Landing Rollout方向を取得する
- 今回はSmooth Turn本格実装、速度・加速調整、Takeoff pitch、Taxiway全面再設計、B滑走路運用は行わない

## Phase 3.9-5Cで行うこと

- AJJ202を中心に、SPOTからPushback Line、Taxiway Main、Holding Short、RWY 18L中心線へつながる出発導線を安定化する
- 出発機のAtGate初期向きは滑走路方向ではなく、ターミナル側を向く駐機姿勢として扱う
- Pushback中は移動方向と機首方向を分け、SPOTから後退しているように見せる
- Line Upは滑走路接続誘導路からRWY 18L中心線へ入り、最後に+X方向へ機首を揃える
- 今回は速度・加速・Takeoff pitch本格実装、全Taxiway精密再現、B滑走路運用は行わない

## Phase 3.9-5Bで行うこと

- 出発機がLine Up / Cleared for Takeoffに入った時、使用滑走路方向へ機首を固定する
- 現在のチュートリアル出発はRWY 18L扱いとして、A滑走路上をUnity座標の+X方向へ離陸する
- 将来RWY 36Rへ切り替える場合は、同じA滑走路上を-X方向へ離陸する前提で方向定義を分ける
- `SimpleRoute` は移動だけを担当し、Transform rotationの直接更新は `AircraftController` に寄せる
- 今回はTakeoff pitch本格実装、速度・加速調整、Taxi waypoint再設計、B滑走路運用は行わない

## Phase 3.9-5Aで行うこと

- 航空機のPushback / Taxi / Hold / Resume / Line Up / Takeoff / Landing Rolloutの速度感、機首方向、pitch基準を整理する
- 現在の `AircraftController` / `SimpleRoute` / `AircraftVisualSpec` における速度、heading、facingDirection、visual rotationの責務を確認する
- Takeoff roll / Rotation / Liftoff / Initial climbを将来分離するための設計案を `AIRCRAFT_GROUND_MOVEMENT_SPEC.md` にまとめる
- 今回は航空機挙動の大規模修正、Taxi waypoint再設計、Takeoff pitch本格実装、B787/B777新規便追加は行わない

## Phase 3.9-4Dで行うこと

- 右上ミニマップを現在のA/B滑走路、海、本体空港島、中央施設帯、旅客ターミナル、基地エリアの相対配置に合わせて更新する
- `AirportSpotDefinition` を参照してSPOT 01〜04をミニマップ表示し、メイン画面との大きなズレを減らす
- 航空機アイコンの `WorldToMiniMap` 投影範囲を現在の空港レイアウトへ合わせ、headingによる矢印回転は維持する
- 今回はメイン画面の再配置、Taxi route再設計、Pushback Line実装、全SPOT表示、B滑走路運用は行わない

## Phase 3.9-4Cで行うこと

- SPOT 01〜04を既存チュートリアル用IDとして維持しながら、将来の那覇空港風SPOT番号へ拡張できるデータ定義へ整理する
- 各SPOTにDOM/INTLなどのエリア、対応機体サイズ、ボーディングブリッジ有無、Unity座標、将来番号候補を持たせる
- AirportManagerはSPOT定義から `gatePositions` とSPOT見た目を生成し、既存Taxi / Hold / Resume / チュートリアル進行は維持する
- 今回は全SPOT実装、SPOT再配置、Taxi route再設計、Pushback Line完全接続、ミニマップ全面更新は行わない

## Phase 3.9-4Bで行うこと

- 現在の旅客ターミナル、DOM/INTL、フィンガー、SPOT、ボーディングブリッジのUnity配置をA滑走路基準で整理する
- AIP AD CHART / ADC-2上の丸数字・番号をSPOT番号として扱い、DOM側/INTL側の配置思想を設計メモへ反映する
- 推定寸法と現在のUnity比率を比較し、次Phaseで再配置すべき具体項目を `TERMINAL_LAYOUT_SPEC.md` にまとめる
- 今回はターミナル再配置、Taxi waypoint調整、航空機ルート修正、全SPOT再現は行わない

## Phase 3.9-4Aで行うこと

- AIP Aerodrome Chart上の丸数字をSPOT番号として読み、DOM TERMINAL前とINTL TERMINAL前に複数SPOT群が並ぶ配置思想をブロックアウトへ反映する
- 旅客ターミナルをA滑走路下側・右寄りに置き、横長本体をDOM TERMINAL風ブロックとINTL TERMINAL風ブロックに分ける
- エプロン側へ伸びるコブ型・フィンガー型の出っ張りを2個だけ追加し、2個の間にはB737級がプッシュバックできそうな広い通路余白を残す
- ターミナル前の旅客エプロンを広げ、DOM側に将来4〜6機分、INTL側に将来2〜4機分程度の駐機余白を残す
- 既存SPOT 01〜04はゲーム用の仮SPOTとして残し、旅客ターミナル前エプロンに自然にまとまるよう再配置する
- 簡易ボーディングブリッジを2〜4本だけPrimitiveで試作し、ターミナルまたはフィンガーからSPOT方向へ伸ばす
- 今回はAIP上の全SPOT完全再現、Taxi waypoint調整、航空機ルート再設計、B滑走路運用、外部アセット導入は行わない

## Phase 3.9-4A QAで修正したこと

- AIP AD CHART / ADC-2のDOM TERMINAL、INTL TERMINAL、Pushback Line、NR/INTL Apron、SPOT配置を参考に、旅客ターミナル全体をより横長の比率へ再調整した
- 推定寸法は航空機・SPOTの見た目スケールを優先し、DOM:INTLの長さ比をおおむね650:300に近づけた
- DOMフィンガー2本の間隔を広げ、フィンガー間をSPOTで埋めずPushback Line / 地上走行余白として見えるようにした
- INTL側はDOM側より広いエプロン奥行きと大型機向けSPOT余白を持つ配置にした
- SPOT 01〜03はB737級、SPOT 04はB787/B777級を想定した広めの区画として再調整した

## Phase 3.9-3Bで行うこと

- Phase 3.9-2A〜3.9-3Aで更新した那覇空港風ブロックアウトに合わせて、右上ミニマップの簡略図を更新する
- A滑走路をターミナル側、B滑走路を海側として表示し、B滑走路島、A/B間水域、中央施設帯、右側接続島の関係を大まかに見せる
- A滑走路下側の基地エリア、JASDF支援・輸送機エリア、旅客エリアを、色分けした矩形で簡略表示する
- `WorldToMiniMap` の投影範囲を現在のゲーム空間に合わせ、航空機位置がメイン画面と大きくズレないようにする
- 航空機矢印の表示、heading / facingDirectionによる回転、選択中航空機の強調は維持する
- 今回はターミナル詳細、ボーディングブリッジ、Taxi waypoint再設計、B滑走路運用は行わない

## Phase 3.9-3Aで行うこと

- 今後B737/B787/B777以外の機種も追加できるように、航空機の実機寸法、ゲーム内見た目サイズ、クリック判定、ラベル高さを機種別に管理する
- 完全実寸ではなく、A滑走路3000m x 45m、B滑走路2700m x 60mを意識した準リアル縮尺を目指す
- 見た目サイズは小さくしつつ、クリック判定は見た目より少し大きめに残す
- 既存のAJJ101 / AJJ103 / AJJ202 / AJJ204は、B737またはA320相当の中型機サイズとして扱う
- B737-800、B787-8、B777-300ERのサイズ定義を用意し、将来の機種追加に備える
- 今回は航空機の逆向き挙動、Taxi waypoint再設計、B787/B777新規便追加、ボーディングブリッジ実装は行わない

## 今後のロードマップ候補

1. Aircraft scale system
2. Camera view redesign
3. Terminal / boarding bridge detail
4. Taxi route alignment

## Phase 3.9-2Bで行うこと

- A滑走路下側の本体空港島を、左側の基地エリアと右側の旅客エリアに分けて見えるようにする
- A滑走路の長手方向を0.00〜1.00の見た目比率として扱い、0.00〜0.25を戦闘機・即応系エリア、0.25〜0.55をJASDF支援・輸送機エリア、0.55〜1.00を旅客エリアとして整理する
- 左端側に小さめ格納庫群と軍用エプロン風ブロックを置き、精密再現ではなく基地風の抽象表現にする
- 左〜中央寄りに大きめ格納庫、広いエプロン、基地施設風ブロックを置き、支援・輸送機エリアとして見えるようにする
- 右側にDOM TERMINAL / INTL TERMINAL、旅客エプロン、SPOT 01〜04をまとめる
- A/B滑走路間の中央くさび形陸地と管制塔は維持する
- 実在軍事施設の精密再現、全Taxiway再現、航空機ルートの本格再設計、B滑走路運用は後続Phaseで扱う

## Phase 3.9-2Aで行うこと

- Aerodrome Chart 1と航空写真を参考に、那覇空港風ブロックアウトの海・陸地・空港島構造を整理する
- B滑走路を、海上の細長い空港島上にある将来解放予定滑走路として見えるようにする
- A滑走路側を、ターミナル側の本体空港島として広めの陸地にする
- A/B滑走路間を全面海ではなく、水域、中央施設帯、右側接続島に分けて表現する
- A/B滑走路の主要接続導線は右側寄りに置き、左側接続は目立たない補助表現に留める
- 新しい管制塔は、A/B滑走路間の中央施設帯寄りに置く
- 旅客ターミナル、JASDFエリア、SPOT細部、誘導路名の完全再現、B滑走路運用は後続Phaseで扱う
- 既存waypoint、航空機移動、滑走路占有、安全管理、ミニマップ、チュートリアルは維持する

## 航空写真から読み取った配置ポイント

- B滑走路は本体空港島から離れた、海上の細長い空港島にある
- A滑走路はターミナル側の本体空港島にあり、その下側に広い陸地と施設群がある
- A/B滑走路間には水域だけでなく、中央施設帯や連絡導線がある
- 主要なA/B接続導線は右側寄りにあり、左側の大きな接続誘導路は目立たない
- B滑走路外側には広い海があり、空港島らしい輪郭が見える
- 新しい管制塔はA/B滑走路間の中央寄り施設帯として扱う

## Phase 3.9-2A QAで修正したこと

- A/B滑走路間の中央陸地を横長の矩形から、A滑走路側の本体陸地からB滑走路方向へ伸びるくさび形に変更した
- 中央陸地と右側A/B接続導線の間に水域の余白を残し、一体化しすぎないようにした
- 左側接続は細い補助的なサービス路として控えめに表示する方針を維持した
- 管制塔は中央くさび形陸地上に自然に見える位置へ微調整した

## Phase 3.9-1で行うこと

- Aerodrome Chart 1 `JP-AD-2.24.1-ROAH-en-JP.pdf` を参考に、那覇空港風ブロックアウトの配置を実際の空港に少し寄せる
- ターミナル側のRWY 18L / 36RをA滑走路として維持し、主運用滑走路として扱う
- 海側のRWY 18R / 36LをB滑走路として、A滑走路より短く幅広い将来解放予定滑走路として表示する
- EAST CHINA SEAがA/B滑走路の間と海側に広がる構造を強める
- DOM TERMINAL、INTL TERMINAL、TWR、WEST APRON、NR APRON、INTL APRONを簡略ブロックで配置する
- Taxiway E/W/T/C/D系の流れを完全再現せず、エプロンから主滑走路へ向かう主要導線として簡略化する
- 既存waypoint、航空機移動、滑走路占有、安全管理、ミニマップ、チュートリアルは維持する

## PDFから読み取った配置ポイント

- RWY 18L / 36Rはターミナル側の3000m x 45m滑走路として下側に配置されている
- RWY 18R / 36Lは海側の2700m x 60m滑走路として上側に並行配置されている
- EAST CHINA SEAはA/B滑走路間とB滑走路外側に広く接している
- DOM TERMINAL / INTL TERMINALはA滑走路の南東側にあり、エプロン群がその前面に広がる
- TWRはターミナル・エプロンと滑走路の間寄りに位置する
- WEST APRON、NR系APRON、INTL APRONがターミナル前に分かれて配置されている
- Taxiway E系はA/B滑走路間、W/T/C/D系はエプロンから主滑走路へ向かう導線として読める

## Phase 3.8-3で行うこと

- 空、光、海、地面の雰囲気を整え、明るい昼間の空港ビューに近づける
- Camera背景色を青空寄りにし、Unity試作画面感を少し減らす
- Directional LightとAmbient Lightを調整し、滑走路、誘導路、ターミナル、管制塔が暗く沈まないようにする
- 海、地面、エプロン、滑走路、誘導路、SPOTの色を必要最小限で調整し、視認性を上げる
- 海と地面の境界を分かりやすくする
- Top View、Oblique View、Wide Viewで見え方を確認する
- 次Phase候補は Phase 3.9-1 Naha Layout Alignment とする

## Phase 3.8-3 QAで確認すること

- Top Viewで操作性が維持されているか
- Oblique Viewで青空、光、海、建物の立体感が分かるか
- Wide ViewでA/B滑走路、海、ターミナル、管制塔の全体関係が分かるか
- 既存の航空機移動、滑走路占有、安全管理、ミニマップ、チュートリアルが壊れていないか

## Phase 3.8-2で行うこと

- A滑走路、B滑走路、海、ターミナル、管制塔、SPOTの位置関係を見やすく調整する
- A滑走路を現在運用中の主滑走路として少し長く、はっきり見えるようにする
- B滑走路を将来解放予定として少し短く、海側に控えめに表示する
- A/B滑走路間とB滑走路外側の海を広げ、那覇空港らしい水辺の存在感を出す
- ターミナル、管制塔、SPOT、エプロンの関係が読み取りやすいように整理する
- 既存waypoint、航空機移動、滑走路占有、安全管理、ミニマップ、CameraPresetは維持する

## Phase 3.8-1で行うこと

- 那覇空港を完全再現せず、ゲーム用に簡略化した3D空港ブロックアウトを作る
- A滑走路をRWY 18L / 36Rの運用滑走路として維持する
- B滑走路をRWY 18R / 36Lの将来解放予定滑走路として薄く仮表示する
- A/B滑走路間とB滑走路外側に海を配置し、那覇空港らしい空間の骨格を作る
- ターミナル、エプロン、管制塔をUnity標準Primitiveの箱で仮配置する
- 既存waypoint、航空機移動、滑走路占有、安全管理、ミニマップ、CameraPresetは維持する
- 正確な誘導路名、全スポット、実寸再現、B滑走路運用、外部アセット追加は今回行わない

## Phase 3.7-2で行うこと

- カメラ視点を `CameraPreset` として管理し、今後の視点追加に備える
- Top View、Oblique View、Wide Viewの3種類を用意する
- Vキーで Top View → Oblique View → Wide View → Top View の順に切り替えられるようにする
- 各プリセットは、見る中心点、カメラ位置offset、LookAtの有無、Orthographic/Perspective、引き具合を分けて管理する
- UI、ストリップ、コマンドポップアップ、管制ログ、右下詳細、ミニマップは既存のScreen Space Overlay表示として維持する
- 本格的な管制塔視点、Runway View、Follow Aircraft View、自由カメラ、ズームUIは今回行わない

## Phase 3.7-1で行うこと

- 既存の見やすいTop Viewを維持する
- 追加で、3D感を確認するための軽いOblique Viewを用意する
- Play中にVキーでTop View / Oblique Viewを切り替えられるようにする
- Oblique Viewでは、航空機、滑走路、誘導路、SPOTの立体感を確認できるようにする
- UI、ストリップ、コマンドポップアップ、管制ログ、右下詳細、ミニマップは既存のScreen Space Overlay表示として維持する
- 本格的な管制塔視点、自由カメラ、ズームUI、カメラ操作UIは今回行わない

## Phase 3.7 QAで修正したこと

- Oblique Viewのカメラが固定角度だけで空港中心を見ておらず、滑走路や航空機が画面中央からズレて見えていた
- 空港全体の中心を `AirportViewCenter` として定義し、Oblique Viewではその中心点を `LookAt` するようにした
- Oblique Viewの視野角を少し広げ、滑走路、誘導路、SPOT、航空機が画面中央付近に収まるようにした
- Top Viewの位置、角度、Orthographic表示は従来通り維持した

## Phase 3.6-2で行うこと

- 既存の滑走路、誘導路、SPOTを、Unity標準Primitiveによる簡易3D空港パーツとして整理する
- 滑走路は薄い板状パーツにし、縁線、閾値、センターラインを追加する
- 誘導路は滑走路より細い板状パーツにし、黄色のセンターラインを追加する
- SPOT 01〜04は駐機区画として見えるように、区画線と停止位置を追加する
- waypoint、航空機移動、滑走路占有、安全管理、ミニマップ、チュートリアルの既存ロジックは維持する
- 本格的な那覇空港再現、B滑走路、カメラ変更、外部3Dモデル追加は今回行わない

## Phase 3.6 QAで修正したこと

- Taxi中に停止 / Hold Taxiを押した時、状態変更でデフォルト方位へ戻さず、最後の有効headingを保持するようにした
- 移動差分がほぼ0の時はheadingを再計算しない方針を維持した
- Resume Taxi時は、保持している次waypoint方向へ向き直ってから移動を再開するようにした
- ミニマップ矢印は航空機データのheadingを使うため、停止中も最後の自然な向きを維持する

## Phase 3.6-1で行うこと

- 既存のCapsule単体の航空機表示を、Unity標準Primitiveを組み合わせた飛行機風の簡易3D Objectへ置き換える
- 親Objectは移動、クリック判定、状態管理に使い、子Objectを見た目専用にする
- 胴体、機首、主翼、尾翼を子Objectとして作成し、機首方向が分かる形にする
- 既存のheading、Transform rotation、waypoint移動、ミニマップ矢印、ストリップ選択、コマンド処理は維持する
- 本格的な3Dモデル、空港3D化、カメラ変更は今回行わない

## Phase 3.5で行うこと

- 現在の空港表示が2D風に見えている理由を、Scene構成、Camera設定、Object構成の観点から確認する
- 航空機、滑走路、誘導路、SPOT、waypointが、将来3D化しても使えるゲーム空間データとして扱われているか確認する
- UI座標とゲーム空間座標を混ぜない方針を明確にする
- `AircraftController` などの航空機ロジックが、見た目の簡易Objectに依存しすぎない方針を明確にする
- ミニマップを、ゲーム空間上の位置を2D表示へ投影するUIとして維持する方針を確認する
- 詳細な設計方針は `3D_ARCHITECTURE_PLAN.md` にまとめる

## Phase 3.6で次に進むこと

Phase 3.6は、Simple 3D Airport View Prototype / 簡易3D空港ビュー試作とします。
本格的な那覇空港再現ではなく、現在のゲームロジックを維持したまま、簡易3D航空機、滑走路、誘導路、SPOT、斜め俯瞰カメラの小さな試作を行います。

## Phase 3.1で完了したこと

- Hold Taxi / 停止を、地上走行中の航空機を現在位置で一時停止させるコマンドとして追加した
- Resume Taxi / 再開を、停止中の航空機の地上走行を再開させるコマンドとして追加した
- AJJ202を中心に、Taxi to Holding Point中にHold Taxi、停止中にResume Taxiを出せるようにした
- Hold中は現在のwaypointルートを保持したまま航空機の移動を止め、Resume後は元のwaypoint移動を再開するようにした
- コマンドポップアップは、地上走行中は停止 / Hold Taxi、停止中は再開 / Resume Taxiを表示するようにした
- 管制ログに「管制官：AJJ202、現在位置で待機してください。」「管制官：AJJ202、地上走行を再開してください。」を追加した
- 右下詳細パネルの状態表示に「現在位置で待機中」を反映した
- AJJ204にも同じHold / Resumeの仕組みは適用可能だが、AJJ204の完全なwaypoint移動は後続Phaseで扱う
- 今回は本格的な誘導路衝突判定ではなく、時間調整コマンドの基礎として扱う

## Phase 3.1 QAで確認すること

- AJJ202がTaxi to Holding Point中に、停止 / Hold Taxiが表示されるか
- Hold Taxiを押すと、AJJ202が現在位置で止まり、状態が「現在位置で待機中」になるか
- 停止中に、再開 / Resume Taxiが表示されるか
- Resume Taxiを押すと、AJJ202が元のwaypoint移動を再開するか
- Hold / Resumeの管制ログが日本語で表示されるか
- 通常のAJJ101 → AJJ202訓練完了フローが壊れていないか
- RWY 18Lの滑走路占有ルールが壊れていないか

## Phase 3.0で完了したこと

- RWY 18Lに runwayOccupied / occupiedByFlightId / occupiedReason 相当の占有状態を持たせた
- AJJ101が着陸中、着陸滑走中、滑走路離脱中はRWY 18Lを占有するようにした
- AJJ101が滑走路を離脱したらRWY 18Lの占有を解除するようにした
- AJJ202がLine Up and Wait中、離陸滑走中はRWY 18Lを占有するようにした
- AJJ202が離陸完了したらRWY 18Lの占有を解除するようにした
- Clear to Land / Line Up and Wait / Cleared for Takeoff は、他機がRWY 18Lを使用中なら実行しない安全チェックを入れた
- 危険な指示を出そうとした場合は、コマンドを実行せず、Safetyを10下げ、短い警告を表示する方針にした
- Phase 3.0 QA追加修正として、AJJ103 / AJJ204を滑走路占有警告のPlay確認に使えるようにした
- AJJ103は着陸許可待ちの到着機として、AJJ202がRWY 18L使用中のClear to Land競合確認に使う
- AJJ204は滑走路手前待機の出発機として、AJJ101がRWY 18L使用中のLine Up and Wait競合確認に使う
- 危険な指示は確認しやすいよう、選択中ストリップ横に赤系ボタンとして表示し、押した時に警告とSafety減点で止める方針にした
- RWY 18L使用中はミニマップ上の滑走路を暖色で強調し、占有状態を見やすくした

## Phase 3.0 QAで確認すること

- 正しい順序で操作した場合、Safety 100のまま訓練完了できるか
- AJJ101が着陸中にAJJ204へLine Up and Waitを出そうとすると警告が出るか
- AJJ202がLine Up and Wait中にAJJ103へClear to Landを出そうとすると警告が出るか
- 危険な指示を試した時、Safetyが10下がり、警告文が表示されるか
- RWY 18L使用中にミニマップ上の滑走路が強調されるか
- 既存のストリップ選択、航空機Object選択、コマンドポップアップ、管制ログ、右下詳細、ミニマップが壊れていないか

## Phase 3.0 安全警告確認手順

### シナリオA

1. Start Training
2. AJJ101を選択し、着陸許可 / Clear to Landを出す
3. AJJ101がRWY 18Lを使用中の間に、AJJ204のストリップを選択する
4. 赤系表示の滑走路上で待機 / Line Up and Waitを押す
5. コマンドが実行されず、RWY 18L使用中の警告が出て、Safetyが10下がることを確認する

### シナリオB

1. 通常訓練をAJJ202のLine Up and Waitまで進める
2. AJJ202がRWY 18L上で待機している間に、AJJ103のストリップを選択する
3. 赤系表示の着陸許可 / Clear to Landを押す
4. コマンドが実行されず、AJJ202がRWY 18Lを使用中という警告が出て、Safetyが10下がることを確認する

## Phase 2.3で完了したこと

- 左側にSTRIPSパネルを追加し、到着 ARRIVAL と出発 DEPARTURE を分けて表示した
- AJJ101を到着ストリップ、AJJ202を出発ストリップに表示した
- ストリップには便名、機種、RWY、SPOT、状態、推奨指示を短く表示する方針にした
- Phase 2.3 QA修正として、ストリップ表示を一覧性重視に整理し、便名とRWY/SPOTを読みやすくした
- ストリップ内の長い説明や詳細情報を減らし、状態/推奨指示は短縮表示にした
- Phase 2.3 QA再修正として、ストリップを横長の細い長方形にし、表示を便名 + RWY/SPOTのみに絞った
- 便名を大きく、RWY/SPOTを小さく表示する方針にした
- 機種、状態、推奨指示、出発地、到着地、時刻は右下詳細パネルへ移した
- ストリップクリックで航空機を選択できるようにした
- 既存の航空機Objectクリック選択も維持した
- 選択中ストリップと推奨指示待ちのストリップを色で分かるようにした
- 右側固定の指示欄は暫定UIとして維持した
- 左下にあった選択中航空機パネルは、ストリップと重ならないよう右下寄りの簡易確認パネルへ移した
- 時刻、出発地、到着地、詳細ルートは将来的な右下詳細パネルへ分離する方針をDocsに残した
- 右下詳細パネルに、選択中航空機の便名、機種、到着/出発、出発地/到着地、予定時刻、RWY、SPOT、状態、担当管制、次の推奨指示を表示するようにした
- 航空機データには予定/推定/実績時刻と遅延分を持てるようにしたが、Phase 2.3 QAでは表示や評価には使わない
- 将来的に、航空機Objectクリックではなくフライトストリップを主操作にする方針をDocsに残した

## Phase 2.4で完了したこと

- 選択中ストリップの近くに、その航空機へ現在出せる指示ボタンを表示する試作を追加した
- Phase 2.4 QA修正として、指示ボタンをストリップ下ではなく右側へ移動し、ストリップ一覧の縦方向の高さを増やさない方針にした
- Phase 2.4 QA再修正として、ストリップ右側の指示ボタンを独立したコマンドポップアップに変更した
- コマンドポップアップはストリップ領域から右にはみ出してもよく、日本語/英語2行表示が潰れない幅と高さを確保する方針にした
- 将来的に最大4個程度のボタンを縦に並べられるポップアップ構造を想定する
- ストリップ側ボタンから、右側固定指示欄と同じ既存コマンド処理を呼び出せるようにした
- AJJ101の着陸許可、Taxi to Spot、AJJ202のPushback、Taxi to Holding Point、Hold Short、Line Up and Wait、Cleared for Takeoffをストリップ側から実行できるようにした
- 1機に対して常に全ボタンを表示せず、現在状態で出せる指示だけを選択中ストリップ右側に表示する方針にした
- 管制ログ、航空機移動、状態遷移、訓練完了までの既存フローを維持した
- 右側固定の指示欄は暫定UIとして維持した
- 航空機Objectクリック選択とストリップクリック選択の両方を維持した
- Phase 2.xでは操作構造を優先し、フォント/色/質感などの磨き込みはPhase 6.1 UI Visual Polishで扱う方針をDocsに残した

## Phase 2.4.5で完了したこと

- 右側固定の指示ボタンを通常操作では表示しない方針に変更した
- 操作の中心を、選択中ストリップ右側に出るコマンドポップアップへ統一した
- ストリップから航空機を選び、その近くの指示ボタンで実行する流れを主導線にした
- 航空機Objectクリック選択は補助操作として維持した
- チュートリアル文と通常ガイドを、航空機クリックではなくストリップ選択を促す内容へ整理した
- 右側エリアは将来的に、右上の簡易レーダー/ミニマップと右下の選択中航空機詳細に使う方針にした
- 追加機の完全な状態遷移、安全管理、複数機間隔管理は後続Phaseで扱う方針を維持した

## Current Phaseの目的

Phase 3.9-1では、添付Aerodrome Chart 1を参考に、現在のPrimitiveベースのブロックアウトを実際の那覇空港らしい位置関係へ少し近づけます。
PDF画像や外部素材は使わず、A/B滑走路、海、ターミナル、管制塔、エプロン、主要誘導路の大まかな配置だけをゲーム用に簡略化して反映します。

## Phase 2.5で完了したこと

- 右上エリアに簡易レーダー/ミニマップを追加した
- ミニマップにはRWY 18L、誘導路、SPOT 01〜04、Hold Short位置を簡易図形で表示した
- AJJ101 / AJJ103 / AJJ202 / AJJ204を、現在のワールド座標に連動する点として表示した
- 到着機と出発機で点の色を分けた
- 選択中航空機は黄色の大きめの点と便名ラベルで強調するようにした
- ミニマップは見るだけの状況把握UIとし、クリック選択や経路指定は実装しない方針にした
- 右側固定指示UI廃止後の右上スペースを、状況把握UIとして使う方針にした
- 本格的なApproach / Departureレーダー、B滑走路表示、経路プレビューは後続Phaseで扱う方針を維持した
- 本格的な3D空港ビュー化はPhase 6.0以降、那覇空港らしさはPhase 7.0で扱う方針をDocsに残した

## Phase 2.5 QA修正で改善したこと

- 右上ミニマップを約1.4倍の横幅、約1.4倍の高さに拡大した
- ミニマップ内部の空港図も広げ、RWY 18L、誘導路、SPOT、航空機点を読みやすくした
- 滑走路と誘導路を太くし、SPOT 01〜04の四角とラベルを大きくした
- 航空機点を大きくし、選択中航空機はさらに大きい黄色点と便名ラベルで強調した
- 右上エリア内で、右下詳細パネルの上に収まるよう配置した
- ミニマップは引き続き見るだけのUIとし、クリック操作や経路指定は実装しない方針を維持した

## Phase 2.5 QA前UI整理で改善したこと

- 右上の教官コメントパネルを通常プレイ中は表示しない方針にした
- 教官的な説明やチュートリアル説明は、中央下のチュートリアルパネルへ統一する方針にした
- 同じ意味の説明を右上と中央下の2か所に出さない方針を明確にした
- 右上エリアをミニマップ/レーダー専用の状況把握エリアとして整理した
- AIRPORT MAPを教官コメント跡地の右上へ移動し、右上の空間を有効活用した
- 右下詳細パネル、下部管制ログ、左側ストリップとの役割分担を維持した

## Phase 2.5 QA追加修正で改善したこと

- ミニマップ上の航空機表示を四角から矢印アイコン風に変更した
- SPOTは四角のまま維持し、航空機と施設を見分けやすくした
- 航空機矢印は移動中は前回位置との差分で向きを変え、停止中は状態に応じた仮方向を使うようにした
- 到着機と出発機は色で区別し、選択中航空機は大きい黄色矢印と白い枠、便名ラベルで強調するようにした
- ミニマップ上のクリック操作、航空機選択、経路指定は引き続き実装しない方針を維持した

## Phase 2.5 QA追加修正 heading連動で改善したこと

- 航空機データに現在向いている方向として headingDegrees / facingDirection を持たせる方針にした
- 航空機が固定waypointを移動する時、次のwaypoint方向と実際の移動差分からheadingを更新するようにした
- ミニマップ上の航空機矢印は、UI側の推測ではなく航空機データのheadingDegreesに基づいて回転するようにした
- 停止中の航空機はランダムな向きにせず、最後に向いていた方向または状態に応じた自然な向きを維持するようにした
- ミニマップ上のクリック操作、航空機選択、経路指定、方位・速度・高度の本格表示は引き続き実装しない方針を維持した

## Phase 2.2で完了したこと

- AJJ101 / AJJ103を到着 ARRIVAL ストリップに表示するようにした
- AJJ202 / AJJ204を出発 DEPARTURE ストリップに表示するようにした
- AJJ103をFukuoka発・Naha着・B737・SPOT 03・予定到着08:12の到着機として追加した
- AJJ204をNaha発・Osaka行き・A320・SPOT 04・予定出発08:18の出発機として追加した
- 追加機もストリップクリックで選択でき、右下詳細パネルとコマンドポップアップが選択機体に応じて更新されるようにした
- ストリップ本体は便名 + RWY/SPOTのみの表示方針を維持した
- 既存のAJJ101/AJJ202初回訓練フローを維持するため、チュートリアル中のコマンド受付は対象便名も確認するようにした
- 追加機はチュートリアル完了条件には含めず、まず一覧・選択・詳細確認を優先する方針にした
- 追加機の完全な状態遷移、安全管理、複数機間隔管理は後続Phaseで扱う方針にした

## Phase 2.5 QAで確認すること

- 右上ミニマップが右下詳細、中央空港ビュー、中央下チュートリアルと重ならないか
- RWY 18L、誘導路、SPOT、航空機点が読めるか
- ストリップ選択とミニマップ上の選択強調が連動するか
- 航空機の移動に合わせてミニマップ上の点も動くか
- 航空機がターンした時、ミニマップ上の矢印も進行方向に合わせて回転するか
- 画面が重く見えすぎないか

## Phase 2.5 QAで実装しないこと

- 航空機Objectクリックの廃止
- Contact Ground
- Contact Tower
- Contact Departure
- 英語音声の実装
- 音声ファイル追加
- Text to Speech連携
- ミニマップ上のクリック操作
- ミニマップからの航空機選択
- Approach / Departureの本格レーダー
- 空港全体俯瞰ビュー
- 経路プレビュー
- スコアランク制
- 効率性評価の本格実装
- B滑走路の画面表示/運用
- 那覇空港の本格3Dモデル化

## 作業判断

Phase 2.5のミニマップQAはUnity Play確認でOKになったため、Phase 3.0「安全管理 / Runway Safety Rules」へ進みました。
Phase 3.0では、まずRWY 18Lの滑走路占有ルールと危険指示のブロックを実装しました。
Phase 3.1では、地上走行中の航空機を一時停止・再開できるHold / Resume Taxiを追加しました。
ただし、移動停止・再開、管制ログ、右下詳細、既存チュートリアルとの相性はUnity Editor上で再確認する必要があるため、次はPhase 3.1 QA準備中とします。
Phase 3.5では、3D化へ進む前に構造監査を行い、`3D_ARCHITECTURE_PLAN.md` に設計方針を整理しました。
Phase 3.6-1では、まず航空機Objectだけを簡易3D化し、親Objectに移動・クリック・状態管理を残したまま、子Objectを見た目専用にしました。
Phase 3.6-2では、滑走路、誘導路、SPOTの見た目を簡易3Dパーツとして整理し、既存waypointと安全管理ロジックは維持しました。
Phase 3.7-1では、VキーでTop View / Oblique Viewを切り替え、既存UIを維持したまま3D感を確認できるようにしました。
Phase 3.7-2では、カメラ設定をCameraPresetとして整理し、Top / Oblique / Wideを順番に切り替えられる土台を作りました。
Phase 3.8-1では、海、将来B滑走路、A/B連絡誘導路、ターミナル、管制塔を仮配置し、那覇空港風の空間ブロックアウトを追加しました。
Phase 3.8-2では、A滑走路を主滑走路として少し長く、B滑走路を将来解放予定として短く控えめにし、海とターミナル周辺の見え方を調整しました。
Phase 3.8-3では、青空背景、昼間寄りの光、海と地面の視認性、ターミナルと管制塔の明るさを調整しました。
Phase 3.9-1では、Aerodrome Chart 1を参考に、A/B滑走路の並行配置、海、DOM/INTL Terminal、TWR、WEST/NR/INTL Apron、主要Taxiway導線を簡略反映しました。
次は、3D表示QA、空港パーツのPrefab化、Tower Viewなどの追加視点検討へ進みます。
