# BackPlugin for Terraria

## 简介
BackPlugin 是一个为 Terraria 游戏服务器设计的插件，由 Megghy 开发，熙恩改。这个插件允许玩家在死亡后使用 `/back` 命令传送回他们最后一次死亡的位置。

## 主要功能
- **死亡点记录**：在玩家死亡时，插件会记录他们的死亡位置。
- **传送回死亡点**：玩家可以通过输入 `/back` 命令来传送回他们的死亡点。
- **冷却时间**：使用 `/back` 命令后，玩家需要等待一段时间（默认20秒）才能再次使用该命令。

## 安装与使用
1. 下载 BackPlugin 插件。
2. 将插件文件放置在 Terraria 服务器的 `ServerPlugins` 文件夹中。
3. 启动服务器，插件会自动加载。
4. 在游戏内，玩家可以通过输入 `/back` 命令来使用插件功能。

## 配置
插件的配置文件位于 `TShock.SavePath\back.json`。你可以修改 `BackCooldown` 属性来设置冷却时间。

## 支持与反馈
- 如果您在使用过程中遇到问题或有任何建议，欢迎在官方论坛或社区中提出issues或pr。
- github仓库：https://github.com/THEXN/back
