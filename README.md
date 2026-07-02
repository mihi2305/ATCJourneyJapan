# ATCJourneyJapan

Unity 6.5 / Universal 3D / URP で作る、航空管制ゲームの最小プレイアブル原型です。

## 今回実装した内容

- 既存の `Assets/Scenes/SampleScene.unity` をPlay時に自動セットアップするプロトタイプを追加
- 2.5D俯瞰視点の簡易空港レイアウトを生成
- 滑走路A、誘導路、ゲート2つ、待機位置1つを生成
- 到着機1機、出発機1機を生成
- 航空機クリック選択、便名・状態ラベル表示
- 選択中の航空機状態に応じた指示ボタン表示
- 滑走路占有競合の検知と警告表示
- `Safety`、`Delay`、`Handled Aircraft Count` の仮スコア表示
- 到着機がゲート到着、出発機が離陸完了したら `Stage Clear` 表示

## Unity Editorでの開き方

1. Unity Hubでこのリポジトリ直下を開く
2. Unity versionは `6000.5.2f1` を使用
3. Sceneは `Assets/Scenes/SampleScene.unity` を開く
4. Playボタンを押す

今回のプロトタイプは `GameBootstrapper` がPlay開始時に必要なGameObjectを自動生成します。
Scene上に手動でPrefabを配置する必要はありません。

## Playボタンを押したときの確認手順

1. 俯瞰カメラで簡易空港が表示される
2. 左上にスコア、左側に選択中航空機、右下に指示ボタンが表示される
3. 到着機 `AJJ101` と出発機 `AJJ202` が表示される
4. 航空機をクリックするとオレンジ色になり、選択状態になる
5. 選択した航空機の状態に合う指示だけが表示される

## 操作方法

- 航空機クリック: 選択
- `Clear Landing`: 到着機に着陸許可
- `Taxi to Gate`: 到着機をゲートへ誘導
- `Taxi to Hold`: 出発機を滑走路手前へ誘導
- `Hold Short`: 出発機を待機位置で停止
- `Line Up`: 出発機を滑走路上へ進入
- `Clear Takeoff`: 出発機に離陸許可
- `Stop`: 移動中の航空機を停止

## 作成した主要スクリプトの役割

- `GameManager.cs`: 全体進行、初期化、滑走路安全監視、クリア判定
- `GameBootstrapper.cs`: SampleSceneをPlayしたときにプロトタイプを自動生成
- `AirportManager.cs`: 空港レイアウト、ゲート、待機位置、経路データを管理
- `RunwayController.cs`: 滑走路ごとの占有状態と競合判定
- `AircraftSpawner.cs`: 初期到着機・出発機を生成
- `AircraftController.cs`: 航空機の状態、移動、ラベル、コマンド実行
- `AircraftState.cs`: 航空機状態の定義
- `AircraftCommand.cs`: プレイヤー指示の定義
- `SimpleRoute.cs`: Waypointベースの簡易移動
- `SelectionManager.cs`: 選択中航空機の管理
- `CommandSystem.cs`: UIから航空機への指示適用
- `UIManager.cs`: HUD、スコア、指示ボタン、警告、Stage Clear表示
- `ScoreManager.cs`: 仮スコアの管理

## 今後B滑走路を追加する場合の設計方針

- `RunwayController` を滑走路ごとに追加する
- `AirportManager.Runways` はすでに複数滑走路を扱える形にしている
- 到着・出発経路を滑走路IDごとに分ける
- `AircraftController` に割り当て滑走路を持たせる
- UIで「Runway A / B」の選択を追加する
- 競合判定は各 `RunwayController` 単位で維持する

## 今後別空港を追加する場合の設計方針

- `AirportManager` に直書きしている座標を `AirportData` / `StageData` に分離する
- Gate、Runway、Taxiway、HoldShort、Routeをデータとして定義する
- Sceneは共通のまま、Stage選択で空港データを切り替える
- 実在空港を完全再現せず、ゲームとして理解しやすい簡略化を優先する

## 次に実装すべき候補

- `AirportData` / `StageData` のScriptableObject化
- Prefab化した航空機・滑走路・ゲートの導入
- 経路選択と滑走路割り当て
- 到着機・出発機の追加スポーン
- Delay計算を状態別に分ける
- UIの見た目改善
- チュートリアル表示
- ステージごとのクリア条件
- 教官講評のためのイベントログ記録
