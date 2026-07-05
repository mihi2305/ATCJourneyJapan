# Aircraft Ground Movement and Takeoff Attitude Spec

## Purpose

Phase 3.9-5A時点の航空機移動、heading、facingDirection、visual rotationの現状を整理し、次Phase以降のheading / facingDirection QA修正、Takeoff pitch演出、機種別挙動差分の基準にする。

今回は挙動変更を行わず、仕様メモとして扱う。

## Current Implementation

### Main Files

| File | Current responsibility |
| --- | --- |
| `AircraftController.cs` | 状態遷移、コマンド実行、速度指定、heading / facingDirection / Transform rotation同期 |
| `SimpleRoute.cs` | waypointキュー、一定速度の `Vector3.MoveTowards` 移動。Phase 3.9-5B以降はTransform rotationを直接更新しない |
| `AircraftVisualSpec.cs` | 機種別の実寸、見た目サイズ、クリック判定、ラベル高さ |
| `AirportManager.cs` | Pushback / Taxi / Line Up / Takeoff / Landing Rollout の仮route waypoint |
| `AircraftData.cs` | UI表示用のruntime heading / facingDirection / stateを保持 |

### Current Speed Handling

`AircraftController` は以下の固定値を持つ。

| Field | Current value | Current use |
| --- | --- | --- |
| `groundSpeed` | `4f` | Taxi to gate, Taxi to hold, Line up, Vacate runway |
| `airborneSpeed` | `8f` | Final approach, Takeoff route |
| Pushback speed | `groundSpeed * 0.65f` = `2.6f` | Pushback |
| Landing rollout speed | `groundSpeed + 1f` = `5f` | Landing roll |

`SimpleRoute` は加速・減速を持たず、`Speed * deltaTime` で一定速度移動する。

### Current Heading And Rotation Handling

現在の役割は以下。

| Concept | Current owner | Current meaning |
| --- | --- | --- |
| `headingDegrees` | `AircraftController` | UI / minimap向け角度。`-Atan2(x, z)` で算出 |
| `facingDirection` | `AircraftController` | X/Z平面の向きベクトル。`East/West/North/South`表示の元 |
| Parent transform yaw | `AircraftController` | 機体全体の見た目回転 |
| Visual child rotation | なし | 現状は親Transformと同じ。pitch/roll専用制御は未実装 |
| Visual pitch | なし | Takeoff rotation / climb pitchは未実装 |
| Visual roll/bank | なし | 常に0相当 |

注意点:

- Phase 3.9-5B前は `SimpleRoute.Tick` が `target.rotation = Quaternion.LookRotation(...)` を実行していた。
- Phase 3.9-5B以降は `SimpleRoute` からrotation更新を外し、`AircraftController.UpdateHeadingFromMovement` / runway heading lock側に寄せた。
- これにより、Taxi route由来のrotationがLine Up / Takeoff headingを上書きしない。
- Hold / Stopなど移動差分が小さい時は `MinHeadingMovementSqrMagnitude` によりheading更新しないため、停止中の向き保持はある程度できている。

## State Speed And Attitude Targets

### Ground Movement

| State / action | Current speed | Target game feel | Target heading | Target pitch / roll |
| --- | --- | --- | --- | --- |
| Pushback | `2.6f` | 2〜4 kt相当。ゆっくり、一定でも可 | Pushback route方向。後退演出を入れるなら機首方向と移動方向を分ける | pitch 0, roll 0 |
| Apron taxi | `4f` | 5〜10 kt相当。SPOT周辺は遅め | 次waypoint方向 | pitch 0, roll 0 |
| Taxiway taxi | `4f` | 10〜20 kt相当。将来はapronより速く | 次waypoint方向 | pitch 0, roll 0 |
| Hold Taxi | 0 | 現在位置停止 | 最後の有効headingを保持 | pitch 0, roll 0 |
| Resume Taxi | route speed再開 | 停止前routeを継続 | 次waypoint方向へ向き直してから再開 | pitch 0, roll 0 |
| Holding Point / Holding Short | 0 | 停止 | 滑走路進入方向または停止線方向 | pitch 0, roll 0 |
| Line Up | `4f` | 5〜10 kt相当。ゆっくり滑走路へ入る | runway centerline方向 | pitch 0, roll 0 |

### Arrival

| State / action | Current speed | Target game feel | Target heading | Target pitch / roll |
| --- | --- | --- | --- | --- |
| Final Approach | `8f` | 進入中として速め | runway approach方向 | 将来は軽い降下姿勢。現Phaseではpitch 0でも可 |
| Landing Rollout | `5f` | 着地後、Taxi速度へ減速していく | runway centerline方向 | pitch 0へ戻す、roll 0 |
| Vacating Runway | `4f` | Taxi速度 | exit taxiway方向 | pitch 0, roll 0 |

## Takeoff Attitude Target

Phase 3.9-5H時点の `GetTakeoffRoute` は `RunwayGeometry` のLine Up点、使用方向、departure endから生成する。
RWY 18Lの場合、Unity上では `-X` 側thresholdから `+X` 方向へ離陸するため、現状のゲーム用routeは概ね以下になる。

```text
(-8, 0.6, 0) -> (19, 0.6, 0) -> (26, 2.4, 0)
```

これはまだ「離陸滑走」と「浮き上がり」を1つの一定速度routeで表現している。今後は状態を分ける。

| Future sub-state | Target movement | Target pitch |
| --- | --- | --- |
| Takeoff roll | 滑走路中心線上を0から徐々に加速 | 0度 |
| Rotation | lift-off point手前から機首上げ開始 | 3〜5度 |
| Liftoff | 機体高度が地上から離れる | 8〜12度 |
| Initial climb | 滑走路端方向へ上昇 | 10〜15度 |

機種別目安:

| Aircraft class | Liftoff distance target on A runway | Rotation feel | Pitch target |
| --- | --- | --- | --- |
| B737 / A320 class | A滑走路3000mの40〜55% | やや軽快 | Rotation 3〜5度、liftoff 8〜12度 |
| B787 class | A滑走路3000mの60〜80% | やや重め | Rotation 3〜4度、liftoff 8〜11度 |
| B777 class | A滑走路3000mの70〜90% | 重め・ゆっくり | Rotation 3〜4度、liftoff 8〜10度 |

## Recommended Role Split

次Phase以降は以下の役割分担に寄せる。

| Data / transform | Role |
| --- | --- |
| `headingDegrees` | 管制UI、ミニマップ矢印、ログ表示向けの2D方位 |
| `facingDirection` | X/Z平面の論理的な機首方向 |
| `movementDirection` | 実際に移動している方向。Pushbackでは機首方向と異なる可能性がある |
| parent transform yaw | 機体全体の水平機首方向 |
| visual child local pitch | Rotation / liftoff / climbの機首上げ |
| visual child local roll | 原則0。将来turn bankingを入れる場合のみ使用 |

重要:

- 地上走行中はpitch/rollを0に固定する。
- yawは原則 `AircraftController` が一元管理する。
- `SimpleRoute` は移動だけを担当し、Transform rotation更新は外すのが望ましい。
- Pushbackをリアルにする場合は、`movementDirection` と `facingDirection` を分ける。

## Aircraft Movement Profile Data Proposal

`AircraftVisualSpec` は見た目寸法の責務を持っているため、速度・加速・離陸姿勢は別データに分けるのが自然。

候補:

```csharp
public class AircraftMovementProfile
{
    public string AircraftType { get; }
    public float PushbackSpeed { get; }
    public float ApronTaxiSpeed { get; }
    public float TaxiwaySpeed { get; }
    public float LineUpSpeed { get; }
    public float TakeoffAcceleration { get; }
    public float LiftoffRunwayFractionMin { get; }
    public float LiftoffRunwayFractionMax { get; }
    public float RotationPitchDegrees { get; }
    public float LiftoffPitchDegrees { get; }
    public float InitialClimbPitchDegrees { get; }
}
```

初期値案:

| Type | Pushback | Apron taxi | Taxiway | Line up | Liftoff fraction |
| --- | --- | --- | --- | --- | --- |
| B737 / A320 | slow | medium | medium-fast | slow | 0.40〜0.55 |
| B787 | slow | medium | medium | slow | 0.60〜0.80 |
| B777 | slow | medium-slow | medium | slow | 0.70〜0.90 |

ゲーム内速度値は現行 `groundSpeed=4` を基準に、最初は以下のように置くと安全。

| Meaning | Suggested Unity speed |
| --- | --- |
| Pushback | `2.0f`〜`2.8f` |
| Apron taxi | `3.0f`〜`4.0f` |
| Taxiway taxi | `4.5f`〜`5.5f` |
| Line up | `3.0f`〜`4.0f` |
| Landing rollout initial | `5.0f` からTaxi速度へ減速 |
| Takeoff roll | 加速式。初期は `0` から `8f` 相当へ |

## Minimal Next Phase Fix

次Phaseで実装すべき最小修正:

1. `SimpleRoute.Tick` から `target.rotation = Quaternion.LookRotation(...)` を外し、rotation更新を `AircraftController` に一元化する。
2. `AircraftController` に `SetVisualYawFromHeading` 相当を設け、`headingDegrees` / `facingDirection` / parent yawを同じ入力から同期する。
3. Hold / Stop / TaxiHeldでは、状態変更時にdefault headingを上書きしない方針を維持する。
4. Resume時は、現状通り次waypoint方向へ向き直してから移動再開する。
5. Takeoff pitch本実装はまだ入れず、地上状態ではvisual child pitchを0に戻す土台だけ検討する。
6. 速度の機種別化は、まず `AircraftMovementProfile` 追加のみ。routeやwaypointの大規模再設計は後続。

Phase 3.9-5Bでは、出発機のLine Up / Takeoff中だけ使用滑走路方向のheadingを固定し、`SimpleRoute` のTransform rotation更新を外す。
現在のRWY 18LはUnity座標の-X側thresholdから+X方向、RWY 36Rは+X側thresholdから-X方向として扱う。

Phase 3.9-5Cでは、出発機routeを現在位置ベースにして、SPOT -> Pushback Line -> Taxiway Main -> Holding Short -> RWY 18L centerlineへつなぐ。
Pushback中は機首をターミナル側へ保持し、Line Up完了後にRWY 18Lの+X方向へ揃える。

Phase 3.9-5Dでは、RWY 18L/36Rを `RunwayGeometry`、主要誘導路を `TaxiwayRouteDefinition` として定義し、Line Up / Takeoff / Landing Rollout / Arrival exit routeが滑走路IDと使用方向から参照できる土台にした。

Phase 3.9-5Eでは、管制コマンド実行時に現在のPrimary Runway方向を `AircraftData.ActiveRunwayDesignator` へ明示的にバインドする。
Line Up / Cleared for Takeoff / Clear to Landは、バインド済みの方向をもとに `RunwayGeometry` からheadingとrouteを取得する。

Phase 3.9-5Gでは、出発機の使用滑走路方向をPushback後の `Taxi to RWY` で確定する。
Hold Short / Line Up / Cleared for Takeoffは、Taxi clearanceで保存済みの `ActiveRunwayDesignator` を再利用し、Line Up / Takeoff時に別滑走路を再選択させない。

Phase 3.9-5Hでは、A滑走路を物理滑走路 `A`、運用方向 `RWY 18L / 36R` として再整理した。
Unity空間ではA滑走路を横向きに描くため、`-X側 = 北端`、`+X側 = 南端` とする。
`RWY 18L` は北端thresholdから南向き、つまりUnity上では `-X -> +X`、`RWY 36R` は南端thresholdから北向き、つまりUnity上では `+X -> -X` として扱う。
Line Up位置とTakeoff / Landing方向は `RunwayGeometry` から取得する。
B滑走路は将来用の物理滑走路 `B`、運用方向 `RWY 18R / 36L` として登録するが、現Phaseでは運用しない。

Phase 3.9-5Iでは、到着機のFinal Approachを `RunwayGeometry` に接続する。
`Clear to Land` 時に指定designatorの中心線延長上にあるFinal Approach Startへ入り、Final Approach Fix、threshold、touchdown、rollout endの順で滑走路中心線上を進む。

次PhaseのSmooth Turn方針:

- 現在の `headingDegrees` を即時反映値から現在headingとして扱い、別に `targetHeadingDegrees` を持つ。
- 地上走行中は `turnRateDegPerSec` を低めにし、waypoint切り替え時に `Mathf.MoveTowardsAngle` でtargetへ追従する。
- Pushback中は機首方向と移動方向を分けられるよう、`movementDirection` と `facingDirection` の分離を維持する。

## Do Not Change Yet

- Taxi waypoint再設計
- Pushback Lineとの完全接続
- Takeoff pitch本格実装
- Liftoff / climb本格実装
- Landing rollout減速本格実装
- B787/B777の新規便追加
- B滑走路運用
