# Alchemon

Alchemon is a game developed with **Godot (4.5)** and **C#**, inspired by pokemon-like games and fusion mechanics. The project features a universe where players collect items and fuse them to create creatures to battle with in turn based battles.

---

## Core Features

- **Items Collection**  
  Collect a wide variety of items with different rarities and qualities.

- **Fusion System**  
  Combine items to create different kind of creatures each battle.

- **Battle System**  
  Engage in strategic battles against other creatures using tactical decision-making.

---

## Planned / Future Features

- Progression system (levels, experience, scaling difficulty)
- Save/Load system improvements
- UI/UX refinements
- Create map and story
- AI improvements for battles
- Balancing and optimization passes

---

## Project Structure

The project follows a modular structure organized by gameplay domains.

.  
├───Assets  
│   ├───Creatures  
│   └───MergeableItems  
├───Data  
├───Resources  
│   ├───Abilities  
│   ├───CreatureDatas  
│   ├───MergeableItemDatas  
│   └───MergeRecipes  
├───Scenes  
│   ├───Autoload  
│   ├───Managers  
│   └───UI  
├───Scripts  
│   ├───Battle  
│   │   ├───Domain  
│   │   └───UI  
│   ├───Core  
│   │   ├───Autoload  
│   │   └───GameLoop  
│   ├───Creatures  
│   │   ├───Data  
│   │   ├───Domain  
│   │   └───Presentation  
│   ├───Items  
│   │   ├───Data  
│   │   ├───Domain  
│   │   └───Presentation  
│   ├───Managers  
│   ├───Merge  
│   │   ├───Data  
│   │   └───Domain  
│   ├───Player  
│   ├───UI  
│   ├───Utils  
│   └───World  
└───README.md  

### Architectural Principles

- Separation of concerns between systems
- Domain-based script organization
- Reusable manager-based systems (inventory, battle, recipes, etc.)

---

## Technical Stack

- **Engine:** Godot 4.5  
- **Language:** C# (.NET)  
- **Data Format:** .tres (Godot Resources format)  
- **Version Control:** Git  

---

## Getting Started

### Prerequisites

- [Godot Engine 4.5 (Mono/.NET version)](https://godotengine.org/)
- Compatible .NET SDK (matching the Godot version)
- Git

Game will be published later (probably on itch.io)
