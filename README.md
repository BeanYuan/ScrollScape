# ScrollScape

[🇨🇳 中文说明](#scrollscape-中文说明) | [🇺🇸 English README](#scrollscape-english-readme)

---

# ScrollScape (English README)

**ScrollScape** is a 2D side-scrolling puzzle-platformer where the player controls both the character and the *windows* of the world.

Scrolling, resizing, hiding, and manipulating windows reshape the level in real-time—turning interface manipulation into core gameplay.

---

## 🎮 Game Overview

In ScrollScape, the level is built inside interactive windows.  
Players can:

- Drag **scroll handles** to shift platform positions  
- **Resize windows** to alter paths  
- **Hide/Unhide** content to reveal hidden routes  
- Trigger **grid-based camera shifts**  
- Use **window clipping** to modify platform colliders  

Every UI action has real gameplay impact.

---

## 🌟 Key Features

### 🖼 1. Fully Interactive Window Mechanics
- Draggable scroll handles  
- Resizable windows  
- Closable windows  
- Hidden content = missing platforms  

### 🧩 2. Window-Based Puzzle Level Design
- SpriteMask controls visual clipping  
- Collider auto-shrinks based on window intersection  
- Scrolling changes the terrain live  

### 🚶 3. Polished Character Controller
- Coyote time  
- Jump buffering  
- Multi-jump smoothing  
- Anti-wall sticking  
- Player stays on moving platforms  

### 🎥 4. Grid Camera System
- Camera shifts one “screen cell” at a time  
- Background moves 1/4 distance (clean parallax)  
- Smooth transitions using SmoothDamp  

### 🛠 5. Level Design Toolset
- Scroll Window Prefab  
- Resize Window Prefab  
- OneShotActivator (with flying animation)  
- Tilemap clipping collider system  
- Respawn + Checkpoint  
- Debug movement buttons  

### 🎵 6. Original Audio
- Scroll start / loop / end  
- Click / jump / death SFX  
- BGM with correct mixing priority  

---

## 👥 Team Members

| Member | Role |
|--------|------|
| **Jiaze Li** | Project Manager / Game Designer |
| **Lizhuoyuan Wan** | Programmer |
| **Peiyuan Huang** | Level Designer |
| **Yiang Fan** | Audio & Music |
| **Yue Kou** | 2D Artist |

---

## 🛠 Tech Stack

- Unity 2022+  
- C#  
- Tilemap + Composite Collider  
- Custom non-Canvas window system  
- Custom scroll/resize/hide mechanics  
- SmoothDamp camera system  
- Sprite Mask + Collider clipping  

---

## 🚀 Development Status

- ✔ Core mechanics complete  
- ✔ Scroll / Resize / Hide systems finished  
- ✔ Window clipping collider system complete  
- ✔ Checkpoint + Respawn done  
- ✔ Camera grid shift finalized  
- ✔ Audio system mostly complete  
- ✔ Art & level content in production  

---

## 🔮 Roadmap

- ☐ Full tutorial stage  
- ☐ Boss & advanced window puzzle levels  
- ☐ UI animation polish  
- ☐ Character animation + cutscenes  
- ☐ Dynamic music system  
- ☐ Public demo release  

---

---

# ScrollScape （中文说明）

**ScrollScape** 是一款 2D 横板跳跃解谜游戏。  
玩家不仅操作角色，还能直接操控「窗口」本身。

**滑动、缩放、隐藏窗口会实时改变关卡结构**，让界面操作成为游戏玩法的一部分。

---

## 🎮 游戏简介

ScrollScape 的关卡全部存在于可操控窗口中。玩家可以：

- 拖动 **滚动条** 改变平台位置  
- **缩放窗口** 改变道路结构  
- **隐藏/显示** 内容创造新路线  
- 触发 **分格镜头移动**  
- 利用 **窗口裁切** 改变平台碰撞  

所有 UI 操作都与解谜紧密结合。

---

## 🌟 核心特色

### 🖼 1. 可操控窗口系统
- 可拖拽滚动条  
- 可缩放窗口  
- 可关闭窗口  
- 内容隐藏 = 平台消失  

### 🧩 2. 窗口式关卡设计
- SpriteMask 控制显示范围  
- Collider 自动裁切  
- 窗口滚动即改变地形  

### 🚶 3. 精细角色操作手感
- Coyote Time  
- 跳跃缓冲  
- 多段跳优化  
- 避免吸墙  
- 随平台移动  

### 🎥 4. 分屏式镜头移动
- 玩家离开当前视区 → 镜头移动一格  
- 背景移动 1/4 实现视差  
- SmoothDamp 平滑过渡  

### 🛠 5. 关卡构建工具
- ScrollWindow 预制体  
- ResizeWindow 预制体  
- OneShotActivator 按钮（带飞行动画）  
- 平台裁切系统  
- 检查点 + 重生  
- Debug 模拟移动按钮  

### 🎵 6. 原创音效与音乐
- 滚动开始/循环/结束  
- 按钮点击  
- 跳跃  
- 死亡音效  
- 背景音乐混音优先级优化  

---

## 👥 团队成员

| 成员 | 职责 |
|------|------|
| **Jiaze Li** | 项目管理 / 策划 |
| **Lizhuoyuan Wan** | 程序开发 |
| **Peiyuan Huang** | 关卡策划 |
| **Yiang Fan** | 音效与音乐 |
| **Yue Kou** | 2D 美术 |

---

## 🛠 技术栈

- Unity 2022+  
- C#  
- Tilemap + Composite Collider  
- 自定义非 Canvas UI 窗口系统  
- 自定义滚动/缩放/隐藏逻辑  
- SmoothDamp 摄像机  
- Sprite Mask + Collider 裁切  

---

## 🚀 当前进度

- ✔ 核心功能完成  
- ✔ 窗口滚动 / 缩放 / 隐藏  
- ✔ 平台裁切系统  
- ✔ 检查点 & 重生  
- ✔ 分格镜头系统  
- ✔ 音效系统稳定  
- ✔ 美术与关卡制作中  

---

## 🔮 未来计划

- ☐ 完整新手教学关  
- ☐ Boss & 高级窗口谜题关  
- ☐ UI 动效完善  
- ☐ 角色动画与剧情演出  
- ☐ 动态音乐系统  
- ☐ Demo 公测  

---

_End of README_
