# 3D Architecture Plan

Phase 3.5では、本格的な3D空港化の前に、現在の2D風俯瞰プロトタイプが将来の3D化を妨げない構造になっているかを確認します。

## 1. 現在の表示が2D風に見えている理由

- 空港レイアウトは `AirportManager` がCubeで滑走路、誘導路、SPOTを生成している
- 航空機は `AircraftSpawner` がCapsuleの簡易Objectとして生成している
- `GameManager` のカメラは高い位置からのOrthographic寄り俯瞰表示で、地形や建物の奥行きを見せる構成ではない
- waypointは `Vector3` の固定座標だが、主にX/Z平面上を動くため、見た目は2Dマップに近い
- UIはScreen Space OverlayのCanvas上にあり、空港ビューの上にストリップ、ミニマップ、ログを重ねている

## 2. 現在の構造で3D化後も維持できるもの

- 航空機、滑走路、誘導路、SPOT、waypointはUnityワールド座標のゲーム空間データとして扱えている
- `AircraftController` は状態遷移、コマンド可否、waypoint移動、heading更新を担当しており、特定の3Dモデル形状には強く依存していない
- `SimpleRoute` はTransformをwaypointへ動かす小さな移動処理なので、将来3Dモデルへ差し替えても再利用しやすい
- `AircraftData` は便名、機種、RWY、SPOT、状態、headingを持ち、ストリップ、右下詳細、ミニマップへ渡せる
- ミニマップは `WorldToMiniMap(Vector3)` でゲーム空間上の位置を2D UIへ投影しており、3Dビューとは別の状況把握UIとして維持できる
- `RunwayController` は見た目ではなく滑走路占有状態を管理しているため、3Dパーツ化後も安全管理ロジックとして使える

## 3. 3D化前に注意すべき設計原則

- 現在の2D風表示は、ゲームロジック検証用のプロトタイプとして扱う
- UI座標とゲーム空間座標を混ぜない
- `AircraftController` などのロジックは、見た目の3Dモデル、Prefab、Meshに依存させない
- waypoint、滑走路、誘導路、SPOTは、見た目Objectではなく空港データとして管理する
- ミニマップ用の座標変換範囲や滑走路/誘導路/Spot配置は、将来的に `AirportManager` またはStageData側へ寄せる
- カメラ変更でゲームロジックが変わらないように、入力、選択、UI表示、航空機移動を分離する
- 3Dモデル差し替え時も、便名、状態、heading、nextTarget、管制ログ、安全管理は現在のデータ構造を使う

## 4. 将来差し替える表示要素

- 現在のCapsule航空機は、将来的に簡易3D飛行機モデルへ置き換える
- Cubeの滑走路、誘導路、SPOTは、3DパーツまたはPrefabへ置き換える
- Gate表記はSPOT中心に整理し、将来的にはスポット番号、停止位置、誘導路接続点をデータ化する
- カメラは後続Phaseで、真上寄り俯瞰から管制塔風の斜め俯瞰へ移行する
- 那覇空港らしい海、ターミナル、管制塔、2本滑走路はさらに後続Phaseで扱う
- UIは左ストリップ、右上ミニマップ、右下詳細、下部ログの役割を維持し、3Dビュー中央を大きな常時UIで隠さない

## 5. Phase 3.6以降で行うべき次の実装案

- Phase 3.6: Simple 3D Airport View Prototype / 簡易3D空港ビュー試作
- まずは現在の空港座標を維持したまま、滑走路、誘導路、SPOTをPrefab化または生成責務を整理する
- 簡易3D航空機モデルを1種類だけ導入し、`AircraftController` がモデル差し替えに依存しないことを確認する
- カメラを斜め俯瞰へ切り替える小さな試作を行い、ストリップ、ミニマップ、右下詳細、下部ログと干渉しないか確認する
- `AirportManager` に散らばる固定座標と、`UIManager` のミニマップ座標範囲の重複を減らす設計を検討する
- 本格的な那覇空港再現、B滑走路、ターミナル、管制塔、海、空の作り込みは後続Phaseで扱う

## 6. Phase 3.6-1で実施した航空機表示の簡易3D化

- 航空機の親Objectは移動、クリック判定、状態管理、heading更新のために維持する
- 見た目は親Objectの子Objectとして、胴体、機首、主翼、尾翼をUnity標準Primitiveで構成する
- 胴体、主翼、尾翼はCube、機首はSphereで表現する
- 親ObjectのTransform rotationが既存のheadingに追従するため、子Objectの飛行機形状も同じ向きへ回転する
- 将来的には子Object群を本物の3D航空機Prefabへ差し替え、`AircraftController` のロジックは維持する
