# Help The Duckling

> A 2D puzzle-platformer where you must guide lost ducklings back to safety by solving environmental puzzles.
<p align="center">
  <img src="https://i.imgur.com/link-to-your.gif" alt="Help The Duckling Gameplay" width="800">
  </p>

<p align="center">
  <strong>You can play the game live in your browser (WebGL):</strong>
  <br>
  <a href="https://yourgame.itch.io/" title="Play on Itch.io">
    <img src="https://img.shields.io/badge/▶%20Play%20on%20Itch.io-FA5C5C?style=for-the-badge&logo=itch.io&logoColor=white" alt="Play on Itch.io">
  </a>
  </p>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-6-blueviolet?style=flat-square&logo=unity" alt="Unity 6">
  <img src="https://img.shields.io/badge/C%23-blue?style=flat-square&logo=c-sharp" alt="C#">
  <img src="https://img.shields.io/badge/WebGL-orange?style=flat-square&logo=webassembly" alt="WebGL">
</p>

---

## 📖 About The Project

> A one-sentence pitch. What is this game?
> e.g., "A 3D physics-based puzzle-platformer where you control a [character]
> to solve [problem]."

This project is a [Genre, e.g., 2D Platformer] built in Unity. The primary goal was to [Your objective, e.g., "build a custom physics-based character controller" or "implement a finite state machine for enemy AI"].

---

## ✨ Key Features Implemented

* **Duck Follower System:**
  > Ducks dynamically attach to the player, forming a following chain. Each new duck joins the end of the queue regardless of pickup order.

* **Out of Bounds Recovery:**
  > If ducks go out of bounds, they automatically return to their original positions.

* **Enemy AI:**
  > Used a Finite State Machine (FSM) to manage enemy states (Patrol, Chase, Attack).

* **Game/UI Management:**
  > Singleton `GameManager` to handle game state, score, and UI updates via C# events.

* **Object Pooling:**
  > Implemented an object pooling system for projectiles to optimize performance.

* **Project Migration:**
  > Successfully migrated the project from Unity `202X.X.X` to the latest **Unity 6** (`6000.0.59f2`), resolving API changes and ensuring build stability.
---

## 🛠️ Tech Stack & Tools

* **Engine:** Unity `6000.0.59f2` (migrated from Unity 202X)
* **Language:** `C#`
* **Render Pipeline:** `URP`
* **Key Packages:**
    * `Input System` (for player controls)
    * `Cinemachine` (for camera management)
    * [UniTask](https://github.com/Cysharp/UniTask)  (used instead of `async/await` for better compatibility with WebGL builds) 

---


## 📂 How To Run Locally (For Developers)

1.  Clone the repository: `git clone https://github.com/Mulwe/help-the-ducklings` 
2.  Open the project in **Unity Hub**.
3.  Ensure you are using Unity version `202X.X.X` or higher.
4.  Open the `Assets/_Project/Scenes/Bootstrap.unity` scene and press "Play".

> Note: You can start from any scene — the project includes a script that ensures `Bootstrap.unity` is loaded automatically at runtime.
