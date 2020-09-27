# UniAllGameObjectTester

すべてのシーンやプレハブに存在するゲームオブジェクトに対するテストを簡単に実装できるパッケージ

## 使用例

### すべてのゲームオブジェクトに対するテスト

```cs
using Kogane;
using NUnit.Framework;

public class Example
{
    [Test]
    public void すべてのゲームオブジェクトの名前が_10_文字以下か()
    {
        AllGameObjectTester.Test
        (
            isValid: gameObject => gameObject.name.Length <= 10
        );
    }
}
```

### 特定のゲームオブジェクトに対するテスト

```cs
using Kogane;
using NUnit.Framework;

public class Example
{
    [Test]
    public void すべてのゲームオブジェクトの名前が_10_文字以下か()
    {
        AllGameObjectTester.Test
        (
            isValid: gameObject => gameObject.name.Length <= 10,

            // 「Assets/@Project」フォルダ以下のシーンに存在するゲームオブジェクトを対象にテストする
            isTargetScene: scenePath => scenePath.StartsWith( "Assets/@Project" ),

            // 「Assets/@Project」フォルダ以下のプレハブに存在するゲームオブジェクトを対象にテストする
            isTargetPrefab: prefabPath => prefabPath.StartsWith( "Assets/@Project" )
        );
    }
}
```
