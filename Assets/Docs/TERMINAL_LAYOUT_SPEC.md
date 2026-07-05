# Terminal Layout Spec

## Purpose

Phase 3.9-4B時点の旅客ターミナル、DOM/INTL、フィンガー、SPOT、ボーディングブリッジのUnity上の配置を整理する。

今回は見た目の再配置ではなく、次Phaseでターミナル前エプロンとTaxi routeを合わせるための設計メモとして扱う。

## Reference Interpretation

- AIP Aerodrome ChartおよびAD CHART / ADC-2上の丸数字・番号はSPOT番号として扱う。
- ADC-2では、DOM TERMINAL前に2本のフィンガーがあり、その周辺にDOM側SPOT群が並ぶ。
- INTL TERMINAL側にはDOMより広いSPOT間隔と、Pushback Line P1/P2/P3を含む大型機向けの余白がある。
- 今回のUnity配置は全SPOT再現ではなく、DOM側とINTL側へ将来拡張するためのブロックアウトである。

## Current Unity Baseline

### A滑走路

`AirportManager.cs` の `CreateRunway` 時点:

| Item | Unity value |
| --- | --- |
| Center | `(3, 0, 0)` |
| Size | `(32.5, 0.2, 2.35)` |
| X range | `-13.25` to `19.25` |
| Z range | `-1.175` to `1.175` |
| Threshold marker | `(-12, 0, 0)` |
| End marker | `(18, 0, 0)` |

実寸基準ではA滑走路は3000m x 45mだが、Unity上では長さ32.5、幅2.35としてかなりゲーム用に圧縮されている。

## Current Passenger Area

### Passenger Apron

| Object | Center | Size | Note |
| --- | --- | --- | --- |
| Passenger Apron Blockout | `(17.1, -0.01, -10.1)` | `(29.2, 0.12, 9.35)` | 旅客エリア全体 |
| DOM Apron Depth Reserve | `(12.2, 0.035, -8.2)` | `(18.5, 0.035, 5.7)` | DOM側の将来SPOT余白 |
| INTL Widebody Apron Depth Reserve | `(26.2, 0.04, -8.5)` | `(7.6, 0.04, 6.95)` | 大型機向け余白 |
| DOM Finger Pushback Line | `(13.6, 0.072, -6.45)` | `(11.6, 0.035, 0.08)` | DOMフィンガー間・前面の走行余白 |
| INTL Pushback Line | `(25.8, 0.074, -6.2)` | `(6.9, 0.035, 0.08)` | INTL側のPushback Line表現 |

A滑走路南側の `z=-5.4` 付近から `z=-14.8` 付近までが旅客エプロンとして見える。

### Terminal Blocks

| Object | Center | Size | Area |
| --- | --- | --- | --- |
| DOM TERMINAL Main Blockout | `(12.5, 0.85, -15.35)` | `(18.2, 1.7, 2.35)` | DOM本体 |
| INTL TERMINAL Main Blockout | `(25.7, 0.92, -15.35)` | `(8.4, 1.85, 2.55)` | INTL本体 |
| Passenger Terminal Concourse Joint | `(21.3, 0.72, -14.05)` | `(1.9, 1.45, 1.35)` | DOM/INTL接続 |
| DOM Finger Pier West Blockout | `(8.6, 0.68, -12.35)` | `(1.55, 1.35, 5.25)` | DOM西側フィンガー |
| DOM Finger Pier East Blockout | `(16.3, 0.68, -12.35)` | `(1.55, 1.35, 5.25)` | DOM東側フィンガー |
| INTL Widebody Pier Blockout | `(25.7, 0.72, -12.55)` | `(5.9, 1.45, 2.05)` | INTL大型機向けピア |

DOM/INTL本体の長さ比は `18.2:8.4 = 2.17:1`。推定基準のDOM 650m / INTL 300mも `2.17:1` なので、比率はかなり近い。

一方、A滑走路長3000mをUnity長32.5に厳密換算すると、旅客ターミナル全体900mは約9.75 world unitsになる。現在のDOM/INTL本体の合計見た目長は接続部込みで約26.5 world unitsあり、滑走路実寸スケールだけで見ると大きすぎる。これは航空機・SPOTの視認性を優先したゲーム用誇張として扱う。

## Finger And Pushback Space

DOMフィンガー2本は、中心Xが `8.6` と `16.3` で、中心間隔は `7.7` world units。各フィンガーの幅は `1.55`、長さは `5.25`。

推定基準のDOMフィンガーは160m x 45mで、長さ/幅の比は約3.56。現在のUnity比は `5.25 / 1.55 = 3.39` で、形状比は近い。

フィンガー間には `DOM Center Pushback Reserve` と `DOM Finger Pushback Line` があり、SPOTで埋めずに地上走行・プッシュバック余白として見える。現状の間隔はB737級には十分だが、将来B787/B777をDOM側へ入れるなら、端側またはINTL側へ寄せる方が自然。

## INTL Widebody Area

INTL側は `INTL Widebody Apron Depth Reserve`、`INTL Widebody Pier Blockout`、`INTL Widebody Future Stand Reserve` でDOMより広い奥行きとSPOT間隔を表現している。

ADC-2のINTL APRONでは、Pushback Lineと大型機向けに読める広いSPOT群がDOMより右側にまとまっている。現在のSPOT 04はこのINTL側余白の代表として扱うのが自然。

## Current SPOTs

| Spot | Position | Size | Current role | Future mapping idea |
| --- | --- | --- | --- | --- |
| SPOT 01 | `(6.3, 0.6, -8.35)` | `(2.35, 0.16, 3.45)` | DOM西端側B737級 | `DOM 21-25 candidate` |
| SPOT 02 | `(10.8, 0.6, -7.95)` | `(2.35, 0.16, 3.45)` | DOM西フィンガー前B737級 | `DOM 23-27 candidate` |
| SPOT 03 | `(17.1, 0.6, -8.15)` | `(2.35, 0.16, 3.45)` | DOM東フィンガー前B737級 | `DOM 31-37 candidate` |
| SPOT 04 | `(26.2, 0.6, -8.65)` | `(3.7, 0.16, 4.85)` | INTL大型機級 | `INTL 41-46/51 candidate` |

現状のSPOT 01から04はゲーム用の仮番号として残す。将来AIP番号へ寄せる場合は、SPOT 01から03をDOM側の実スポット群、SPOT 04をINTL側の実スポット群へ置換するのが自然。

## Spot ID Data Policy

Phase 3.9-4C以降、SPOT 01から04は `AirportSpotDefinition` で管理する。

| Field | Meaning |
| --- | --- |
| `TutorialId` | 既存チュートリアル、Flight Strip、Taxi to Spotで使う表示ID。現段階では `SPOT 01` から `SPOT 04` を維持する |
| `RealWorldStyleId` | 将来AIP風の実スポット番号へ寄せるための候補名。現段階では確定番号ではなくDOM/INTLの候補エリアとして扱う |
| `Area` | `DOM` / `INTL` / `BASE` / `OTHER` の大分類 |
| `AircraftSizeClass` | `Narrowbody` / `Widebody` / `Any` の対応機体サイズ |
| `HasBoardingBridge` | ボーディングブリッジ接続対象かどうか |
| `Position` | 現在のUnity座標。`gatePositions` の生成元にもなる |
| `StandScale` | SPOT区画の見た目サイズ |
| `BoardingBridgeObjectName` | 将来SPOTとボーディングブリッジを紐づけるためのObject名 |

現段階では `TutorialId` をゲーム進行用の安定ID、`RealWorldStyleId` を将来の那覇空港風表示番号候補として分ける。AIP上の全SPOT番号はまだ実装しない。

## SPOT Size Policy

推定基準:

| Type | Real estimate | Current Unity target |
| --- | --- | --- |
| B737級SPOT | 約55m x 100m | 約`2.35 x 3.45` |
| B787/B777級SPOT | 約85m x 150m | 約`3.7 x 4.85` |

航空機の見た目スケールはB737級が長さ約1.25、翼幅約1.35、B787/B777級が長さ約1.75から2.08、翼幅約2.05から2.22。現在のSPOTは機体に対して窮屈すぎないが、Taxi routeと一致させる段階では停止線位置と機首方向を再調整する必要がある。

## Boarding Bridge Policy

| Bridge | Position | Size | Target |
| --- | --- | --- | --- |
| Boarding Bridge SPOT 01 | `(7.05, 0.78, -10.55)` | `(0.22, 0.2, 1.95)` | DOM西側 |
| Boarding Bridge SPOT 02 | `(10, 0.78, -10.45)` | `(0.22, 0.2, 2.15)` | DOM西フィンガー |
| Boarding Bridge SPOT 03 | `(17.05, 0.78, -10.45)` | `(0.22, 0.2, 2.15)` | DOM東フィンガー |
| Boarding Bridge SPOT 04 | `(26.2, 0.82, -10.35)` | `(0.28, 0.22, 2.8)` | INTL大型機 |

方針:

- まずは2から4本に絞り、全ブリッジ再現はしない。
- DOM側はB737級向けに短め・細め、INTL側はB787/B777級向けにやや長め・太めにする。
- 機種別ドア位置対応は後続Phaseで扱う。

## Taxi Route Alignment Notes

- 現在のTaxi routeはゲーム進行優先の仮ルートであり、SPOT停止位置とは完全一致していない。
- `gatePositions` はスポーン/到着先にも使われるため、見た目のSPOTだけ動かすとチュートリアルやTaxi to Spotの到着位置に影響する。
- Taxiway Mainは `z=-5`、旅客エプロンPushback Lineは `z=-6.2` から `z=-6.45` 付近にある。次に合わせるなら、Taxiway MainからPushback Line、各SPOT停止線へ分岐する二段構造にする。
- A滑走路安全チェック、Hold/Resume、Tutorialの進行を壊さないため、既存ルートを一度に全置換せず、表示用誘導線と移動ルートを段階的に近づける。

## Evaluation Against Estimates

良い点:

- DOM/INTL本体の長さ比は推定基準650:300に近い。
- DOMフィンガーの形状比は160m x 45mに近い。
- フィンガー間にPushback Line / reserveがあり、SPOTで埋まっていない。
- INTL側はDOM側より大型機向けの広い余白を持っている。

ズレている点:

- A滑走路長を基準に厳密換算すると、旅客ターミナル全体は大きすぎる。
- ターミナル本体とSPOTの相対距離は、航空機見た目優先でやや誇張されている。
- SPOT 01から04はAIP実番号との対応が未確定。
- Taxi route、Pushback Line、SPOT停止線はまだ一致していない。
- ミニマップ側のSPOT表示は、今回の詳細配置とはまだ完全連動していない。

## Next Phase Tasks

1. AIP実SPOT番号へ寄せる前に、DOM側・INTL側の仮SPOT群をID付きデータとして分離する。
2. SPOT 01から04の仮番号を、ゲーム内チュートリアル用IDと実空港風表示番号に分ける。
3. Taxiway Main、Pushback Line、SPOT停止線を段階的に接続する。
4. DOM側は2本フィンガー周辺にB737級12から16機程度の将来配置枠を作る。
5. INTL側はB787/B777級4機程度を置ける大型機SPOT枠を作る。
6. ボーディングブリッジは機体ドア位置対応前に、SPOTごとの接続元だけを整理する。
7. ミニマップを、詳細SPOT座標を直接読む形へ寄せるか、簡略表示として割り切るか決める。
