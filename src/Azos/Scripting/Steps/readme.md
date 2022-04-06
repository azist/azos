# Step Script Runner

Facilitates execution of C# code snippets embodied id [`Step`](BaseSteps.cs)-derived classes.

```csharp
  script
  {
    //We do not want to repeat this part in various type references down below,
    //so we set it in the root of run step section for all child section down below
    type-path='Azos.Scripting.Steps, Azos'

    step{ type='See' text='Step number one'}
    step{ type='See' text='Step number two'}
  }
```