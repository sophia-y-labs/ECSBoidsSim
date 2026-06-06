---
name: build
description: 构建 Unity 项目到本地目录
# 用法: /build [target]
# target: windows | linux | all  (默认 windows)
---

# Build Unity Project

当用户输入 `/build` 时执行 Unity 批处理构建。

## 构建目标

| 参数 | 目标平台 | 输出路径 |
|------|----------|----------|
| `windows` (默认) | StandaloneWindows64 | `Builds/Windows/` |
| `linux` | StandaloneLinux64 | `Builds/Linux/` |
| `all` | 两个平台 | 各自目录 |

## 执行步骤

1. 解析参数：如果未提供参数，默认 `windows`
2. 运行 Unity 批处理命令：

```
"D:\Dev\2022.3.62f3c1\Editor\Unity.exe" -quit -batchmode -logFile - -projectPath "i:\ESCBoidsSim" -executeMethod ESCBoidsSim.BuildTools.PerformBuild -buildTarget <target>
```

3. 检查输出中是否有 `Build succeeded` 或 `Build failed`
4. 如果失败，读取日志并报告错误
