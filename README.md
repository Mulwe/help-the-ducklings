# 🦆 Help The Duckling

A 2D puzzle-platformer where you guide lost ducklings back to safety

 <div align="center">

<h3>
  <a href="https://yourgame.itch.io/" style="text-decoration: none;">
    🎮 <strong>PLAY NOW - FREE WEBGL BUILD</strong> 🎮
  </a>
</h3>

<a href="https://yourgame.itch.io/">
  <img src="https://img.shields.io/badge/🦆%20AVAILABLE%20ON%20ITCH.IO-FA5C5C?style=for-the-badge&logo=itch.io&logoColor=white&labelColor=000000" alt="Play on Itch.io" height="50">
</a>

</div> 

 
---

## 🎮 About The Game

Help The Duckling is a charming 2D puzzle-platformer where players navigate through environmental challenges while managing a growing chain of adorable ducklings. Each level presents unique obstacles that require strategic thinking and precise timing to safely escort all ducklings to their destination.
The game combines classic platforming mechanics with a dynamic follower system, creating engaging gameplay where every duckling matters.

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

- **Design Patterns**: Implementing Singleton, FSM, and Object Pooling patterns
- **Unity Migration**: Handling version upgrades and API deprecations
- **Performance**: Optimizing for WebGL builds and managing memory efficiently
- **Architecture**: Building scalable, maintainable code with event-driven design

---

## 📸 Screenshots

<div align="center">
  <img src="https://via.placeholder.com/400x250?text=Level+1" alt="Level 1" width="400">
  <img src="https://via.placeholder.com/400x250?text=Enemy+AI" alt="Enemy AI" width="400">
</div>

---

## 📝 License

This project is available for portfolio and educational purposes.

---

<div align="center">
  
**Made with 💙 by [Mulwe](https://github.com/Mulwe)**

[⬆ Back to Top](#-help-the-duckling)

</div>
