# BehaviorTree

C# behavior tree library with a fluent API. Build AI and game logic as composable trees of actions and control nodes, then tick them each frame with elapsed time.

Fork and continuation of [fluent-behaviour-tree](https://github.com/codecapers/fluent-behaviour-tree) by Code Capers. For background, see [Fluent behavior trees for AI and game logic](http://www.what-could-possibly-go-wrong.com/fluent-behavior-trees-for-ai-and-game-logic/).

## Features

- Fluent `BehaviorTreeBuilder` for readable tree construction
- Composite nodes: **Sequence**, **Selector**, **Parallel**, **Inverter**
- Leaf **Action** nodes and boolean **Condition** sugar
- **Splice** reusable sub-trees into a parent tree
- `TimeData` passed on every tick for delta-time aware logic
- **Async actions** via `Task` and `ValueTask` on supported target frameworks
- Full **XML documentation** shipped with the NuGet package
- Multi-target: .NET Framework 3.5 through .NET 10, plus `netstandard2.0` / `netstandard2.1`

## Installation

**NuGet (Package Manager Console):**

```powershell
Install-Package BehaviorTree
```

**.NET CLI:**

```bash
dotnet add package BehaviorTree
```

**From source:** clone this repository and reference `src/BehaviorTree.csproj`, or build the solution under `src/`.

## Quick start

A tree is built with `BehaviorTreeBuilder` and returned from `Build()`. The root must be a composite node (not a bare `Do`).

```csharp
using BehaviorTree;

IBehaviorTreeNode tree;

public void Startup()
{
    tree = new BehaviorTreeBuilder()
        .Sequence("my-sequence")
            .Do("action1", t =>
            {
                // Action 1.
                return BehaviorTreeStatus.Success;
            })
            .Do("action2", t =>
            {
                // Action 2.
                return BehaviorTreeStatus.Success;
            })
        .End()
        .Build();
}
```

Tick the tree on each update of your game loop:

```csharp
public void Update(float deltaTime)
{
    tree.Tick(new TimeData(deltaTime));
}
```

`TimeData` exposes `DeltaTime` (seconds since last tick).

## Behavior tree status

Nodes return one of:

| Status | Meaning |
|--------|---------|
| `Success` | The node finished and succeeded. |
| `Failure` | The node finished and failed. |
| `Running` | The node is still in progress; the tree will resume here on the next tick. |

## Node types

### Action (leaf)

Use `Do` for synchronous leaf actions. Query entities or the world, then return a status.

```csharp
.Do("do-something", t =>
{
    // ... do something ...
    return BehaviorTreeStatus.Success;
})
```

`Do` must be nested inside a parent composite node.

### Sequence

Runs children in order. Fails on the first child that returns `Failure`. Advances when the current child returns `Success`. Stays on the current child while it returns `Running`. Succeeds when all children succeed.

```csharp
.Sequence("my-sequence")
    .Do("action1", t => BehaviorTreeStatus.Success)
    .Do("action2", t => BehaviorTreeStatus.Success)
.End()
```

### Parallel

Runs all children each tick. Terminates when enough children have failed or succeeded (thresholds are configurable).

```csharp
int numRequiredToFail = 2;
int numRequiredToSucceed = 2;

.Parallel("my-parallel", numRequiredToFail, numRequiredToSucceed)
    .Do("action1", t => BehaviorTreeStatus.Running)
    .Do("action2", t => BehaviorTreeStatus.Running)
.End()
```

### Selector

Runs children in order until one succeeds. Fails if every child fails. Stays on a child while it returns `Running`.

```csharp
.Selector("my-selector")
    .Do("action1", t => BehaviorTreeStatus.Failure)  // try next
    .Do("action2", t => BehaviorTreeStatus.Success) // stop here
    .Do("action3", t => BehaviorTreeStatus.Success) // not reached
.End()
```

### Condition

Syntactic sugar over `Do`: a `bool` is mapped to `Success` or `Failure`. Often used with `Selector`.

```csharp
.Selector("my-selector")
    .Condition("condition1", t => SomeBooleanCondition())
    .Do("action1", t => SomeAction())
.End()
```

### Inverter

Inverts `Success` / `Failure` of its single child. Keeps `Running` while the child is `Running`.

```csharp
.Inverter("inverter1")
    .Do("action1", t => BehaviorTreeStatus.Success) // becomes Failure
.End()

.Inverter("inverter2")
    .Do("action1", t => BehaviorTreeStatus.Failure) // becomes Success
.End()
```

`Inverter` accepts only one child.

## Async actions

On supported target frameworks, leaf actions can return `Task<BehaviorTreeStatus>` or `ValueTask<BehaviorTreeStatus>`. The action is invoked once; the node stays `Running` until the task completes.

Use `TickAsync` to drive the tree asynchronously:

```csharp
public async Task UpdateAsync(float deltaTime)
{
    var status = await tree.TickAsync(new TimeData(deltaTime));
}
```

**Task-based actions** (`Do` overload, `Condition` with `Task<bool>`):

```csharp
await new BehaviorTreeBuilder()
    .Sequence("async-sequence")
        .Do("fetch-data", async t =>
        {
            await LoadDataAsync();
            return BehaviorTreeStatus.Success;
        })
        .Condition("has-target", async t => await HasTargetAsync())
    .End()
    .Build()
    .TickAsync(new TimeData(deltaTime));
```

**ValueTask-based actions** (`DoValue`, `Condition` with `ValueTask<bool>` — `netstandard2.1+` and .NET 5+ only):

```csharp
.DoValue("fast-path", t => new ValueTask<BehaviorTreeStatus>(BehaviorTreeStatus.Success))
```

Synchronous `Tick()` also works on async nodes (it blocks until a pending task completes), but prefer `TickAsync()` in async code.

### API availability by target framework

| API | Available on |
|-----|--------------|
| `Tick`, synchronous `Do` / `Condition` | All targets (`net35` … `net10`, `netstandard2.0/2.1`) |
| `TickAsync`, `Do` with `Task`, `Condition` with `Task<bool>` | `net452+`, `netstandard2.0+`, .NET 5+ |
| `DoValue`, `Condition` with `ValueTask<bool>` | `netstandard2.1+`, .NET 5+ |

## Nesting

Trees can nest to any depth:

```csharp
.Selector("parent")
    .Sequence("child-1")
        .Parallel("grand-child", 1, 1)
            .Do("leaf", t => BehaviorTreeStatus.Success)
        .End()
    .End()
    .Sequence("child-2")
        .Do("leaf", t => BehaviorTreeStatus.Success)
    .End()
.End()
```

## Splicing sub-trees

Build reusable pieces separately and attach them with `Splice`:

```csharp
private static IBehaviorTreeNode CreateSubTree()
{
    return new BehaviorTreeBuilder()
        .Sequence("my-sub-tree")
            .Do("action1", t => BehaviorTreeStatus.Success)
            .Do("action2", t => BehaviorTreeStatus.Success)
        .End()
        .Build();
}

public void Startup()
{
    tree = new BehaviorTreeBuilder()
        .Sequence("my-parent-sequence")
            .Splice(CreateSubTree())
            .Splice(CreateSubTree())
        .End()
        .Build();
}
```

## Building and tests

From the repository root:

```bash
dotnet build src/BehaviorTree.csproj
dotnet test tests/BehaviorTree.Tests.csproj
```

## Learn more

- [Behavior tree (Wikipedia)](https://en.wikipedia.org/wiki/Behavior_tree_(artificial_intelligence,_robotics_and_control))
- [Behavior trees for AI: How they work](http://www.gamasutra.com/blogs/ChrisSimpson/20140717/221339/Behavior_trees_for_AI_How_they_work.php)
- [Understanding Behaviour Trees](http://aigamedev.com/open/article/bt-overview/)
- [Introduction and implementation of Behaviour Trees](http://guineashots.com/2014/07/25/an-introduction-to-behavior-trees-part-1/)

## License

MIT — see [LICENSE](LICENSE). Copyright © 2015 Code Capers; copyright © 2015–2026 ema.
