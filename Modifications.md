# Changes

Form1.cs:

```cSharp
verifyActivation()
remove last if statement preserving contents

if (text.CompareTo(regNumStr) == 0)
{
    trialMode = false;
    PcCache.trialMode = false;
}
```

to:

```cSharp
trialMode = false;
PcCache.trialMode = false;
```

or change to ```!=```

Remove Font restriction:
Cricut_Design_Studio->SceneGroup.cs->add()
Replace function with:

```cSharp
public static void add(SceneGroup sg, PcControl pc, string fontName)
{ 
    CutCartRecord cutCartRecord = new CutCartRecord(fontName);
    cutCartRecord.skipped = false;
    cutCartRecords.Add(cutCartRecord);
}
```

C# updates:
replace:

```cSharp
[DllImport("Cricut USB.dll")]
```

with:

```cSharp
[DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
```
