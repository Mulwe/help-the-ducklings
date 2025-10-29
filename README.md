<div align="center"> 
 
# 🦆 Help The Duckling

**Protect your ducks from enemies trying to steal them!**
 
### 🎮 [**► PLAY NOW ON ITCH.IO**](https://mulwe.itch.io/help-the-ducklings) 🎮

[![Play on Itch.io](https://img.shields.io/badge/🎯%20CLICK%20TO%20PLAY-FA5C5C?style=for-the-badge&logo=itch.io&logoColor=white&labelColor=000000)](https://mulwe.itch.io/help-the-ducklings)

<br>

![Unity](https://img.shields.io/badge/Unity%206-000000?style=flat-square&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=c-sharp&logoColor=white)
![WebGL](https://img.shields.io/badge/WebGL-990000?style=flat-square&logo=webgl&logoColor=white)
![Windows](https://img.shields.io/badge/Windows-0078D6?style=flat-square&logo=windows&logoColor=white)

</div>

---

## 🎮 About The Game

Help The Ducklings is an action-packed 2D platformer where you must protect your adorable duck companions from relentless enemies. Push back the threats, survive as long as possible, and keep your flock safe!

Created in ~2 days for the **LowRezz Jam**, this post-jam version includes improvements, polish, and additional features that enhance the core gameplay loop.

**🎮 Controls:**
- **A / D** – Move left/right  
- **W / Space** – Jump

**🎯 Objective:**  
Enemies are trying to steal your ducks! Use your platforming skills to push them away and protect your flock for as long as you can.

<div align="center">
  <img src="https://img.itch.zone/aW1nLzIyMTM5NTM1LnBuZw==/original/Gnm1NI.png" alt="Help The Ducklings Gameplay" width="700">
  <p><em>🎮 Play the WebGL version directly in your browser!</em></p>
</div>

---

## ✨ Technical Highlights

### 🦆 Smart Follower System
Implemented a dynamic chain system where ducklings attach to the player in sequence. Each duckling follows the one ahead of it, creating a smooth, organic movement pattern that responds to player actions.

### 🤖 Enemy AI with FSM
Designed an enemy AI system using a Finite State Machine pattern with three distinct states:
- **Patrol**: Enemy follows waypoints
- **Chase**: Enemy pursues the player when detected
- **Attack**: Enemy engages when in range

### 🔄 Intelligent Object Pooling
Built a custom object pooling system for projectiles and particles, significantly reducing garbage collection overhead and maintaining stable frame rates during intense gameplay.

### 🎯 Fail-Safe Recovery System
Ducklings that fall out of bounds automatically respawn at their original positions, preventing frustrating game-breaking scenarios while maintaining challenge.

### 🎨 Event-Driven Architecture
Implemented a singleton GameManager with C# events for clean, decoupled communication between systems (UI, score tracking, game state management).

---

## 🏆 Game Jam Project

This game was created for the **[LowRezz Jam](https://itch.io/jam/lowrezjam)** with the following constraints:
- ⏱️ **Development Time**: ~2 days (jam period)
- 🎨 **Resolution Constraint**: Low-resolution pixel art style
- 🚀 **Post-Jam Improvements**: Additional polish, bug fixes, and feature enhancements

The jam version focused on core mechanics, while the current version on Itch.io includes refined gameplay and better visual feedback.

---

## 🛠️ Tech Stack

| Category | Technology |
|----------|-----------|
| **Engine** | Unity 6 (6000.0.59f2) |
| **Language** | C# |
| **Render Pipeline** | Universal Render Pipeline (URP) |
| **Input** | New Input System |
| **Camera** | Cinemachine |
| **Async Operations** | [UniTask](https://github.com/Cysharp/UniTask) (WebGL-optimized) |

---

## 🚀 Project Evolution

This project showcases my growth as a Unity developer:

- **Migration Success**: Upgraded from Unity 202X to Unity 6, resolving breaking API changes and ensuring compatibility
- **Performance Optimization**: Implemented object pooling and optimized collision detection
- **WebGL Compatibility**: Adapted async patterns using UniTask for seamless browser performance
- **Code Architecture**: Refactored codebase to use event-driven patterns and SOLID principles

---

## 💻 Running Locally

### Prerequisites
- Unity Hub installed
- Unity 6 (6000.0.59f2) or later

### Setup
```bash
# Clone the repository
git clone https://github.com/Mulwe/help-the-ducklings

# Open in Unity Hub
# Navigate to: Assets/_Project/Scenes/Bootstrap.unity
# Press Play
```

> **Note**: Any scene can be played directly. The bootstrap scene loads automatically via a custom script loader.

---

## 🎯 What I Learned

- **Rapid Prototyping**: Developing a complete game in 2 days taught me to prioritize core mechanics and iterate quickly
- **Design Patterns**: Implementing Singleton, FSM, and Object Pooling patterns in a time-constrained environment
- **Unity Migration**: Handling version upgrades and API deprecations (Unity 202X → Unity 6)
- **Performance**: Optimizing for WebGL builds and managing memory efficiently
- **Scope Management**: Making strategic decisions about features to include within jam time limits
- **Post-Jam Polish**: Improving and refining based on player feedback

---

## 📝 License

This project is available for portfolio and educational purposes.

---

<div align="center">
  
**Made with 💙 by [Mulwe](https://github.com/Mulwe)**

[⬆ Back to Top](#-help-the-duckling)

</div>
