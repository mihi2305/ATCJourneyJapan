# Codex Workflow

CodexがATCJourneyJapanで作業する時のルールです。
作業前に必ずこのファイルを確認します。

## 作業前

- 必ず `pwd` を確認する
- 作業対象は `/Users/ogatamihiro/Projects/ATCJourneyJapan`
- 古い `/Users/ogatamihiro/Documents/ATCJourneyJapan` は使わない
- `git status` を確認する
- `ROADMAP.md`、`CURRENT_PHASE.md`、`UI_DESIGN_PRINCIPLES.md` を読む
- 現在の依頼が `CURRENT_PHASE.md` の範囲内か確認する

## 作業中

- `CURRENT_PHASE.md` の範囲外の機能は実装しない
- 必要な気づきがあれば、実装せずROADMAPに後続Phaseとして追記する
- Unity batchmode検証は実行しない
- 破壊的操作をしない
- `Library`, `Temp`, `Obj`, `Logs`, `UserSettings`, `Assets/_Recovery/` は触らない
- `main` にはmergeしない
- Scripts配下を触るかどうかは、依頼内容で明示されている場合だけ判断する

## 作業後

- `git status` を確認する
- 変更ファイルを報告する
- commit/pushする場合は、対象ファイルを明示する
- `main` にはmergeしない
- 最後にUnityで確認すべき手順を報告する

## Commit / Push

- commit前に `git diff --check` を確認する
- commit前に `git diff --cached --name-only` で対象ファイルを確認する
- `Library`, `Temp`, `Obj`, `Logs`, `UserSettings`, `Assets/_Recovery/` をcommitしない
- push先は作業ブランチ `codex/playable-atc-prototype` を基本にする
