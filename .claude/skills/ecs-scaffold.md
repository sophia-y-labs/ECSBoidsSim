---
name: ecs-scaffold
description: 生成 Entities 1.0+ 样板代码（System / Component / Authoring）
# 用法: /ecs-scaffold <type> <Name> [options]
# type: system | component | authoring | all
# options: --folder <相对路径>
---

# ECS Scaffold - 生成 Entities 1.0+ 样板代码

当用户输入 `/ecs-scaffold` 时，根据类型生成 ECS 样板代码。

## 类型说明

### system — ISystem
生成带 BurstCompile 的 ISystem，包含 OnCreate / OnUpdate 模板，文件放入 `Assets/Scripts/Systems/`。

### component — IComponentData
生成 IComponentData 结构体，文件放入 `Assets/Scripts/Components/`。

### authoring — Authoring + Baker
生成 MonoBehaviour + Baker 组合，文件放入 `Assets/Scripts/Authoring/`。

### all — 同时生成三者
生成 Component + Authoring + System，使用相同的命名前缀。

## 生成的模板

### System (`Assets/Scripts/Systems/<Name>System.cs`)
```csharp
using Unity.Entities;
using Unity.Burst;

namespace ESCBoidsSim.Systems
{
    [BurstCompile]
    public partial struct {{Name}}System : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<{{Name}}Component>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            new {{Name}}Job { DeltaTime = deltaTime }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct {{Name}}Job : IJobEntity
    {
        public float DeltaTime;

        [BurstCompile]
        private void Execute(ref {{Name}}Component component)
        {
            // TODO: implement
        }
    }
}
```

### Component (`Assets/Scripts/Components/<Name>Component.cs`)
```csharp
using Unity.Entities;

namespace ESCBoidsSim.Components
{
    public struct {{Name}}Component : IComponentData
    {
        public float Value;
    }
}
```

### Authoring (`Assets/Scripts/Authoring/<Name>Authoring.cs`)
```csharp
using Unity.Entities;
using UnityEngine;

namespace ESCBoidsSim.Authoring
{
    public class {{Name}}Authoring : MonoBehaviour
    {
        public float Value;

        class Baker : Baker<{{Name}}Authoring>
        {
            public override void Bake({{Name}}Authoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new {{Name}}Component
                {
                    Value = authoring.Value
                });
            }
        }
    }
}
```
