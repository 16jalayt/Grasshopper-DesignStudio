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
Cricut_Design_Studio -> SceneGroup.cs -> add()
Replace function with:

```cSharp
public static void add(SceneGroup sg, PcControl pc, string fontName)
{ 
    CutCartRecord cutCartRecord = new CutCartRecord(fontName);
    cutCartRecord.skipped = false;
    cutCartRecords.Add(cutCartRecord);
}
```

To remove the popup about the jukebox:
Cricut_Design_Studio -> SceneGroup.cs -> search()
Replace with:

```cSharp
public static CutCartRecord find(string fontName, PcControl pc)
{
    if (fontName == null || fontName.Length < 5)
    {
        return null;
    }
    if (fontName.CompareTo("Makin the Grade") == 0)
    {
        fontName = "Makin' the Grade";
    }
    if (isReset)
    {
        isReset = false;
    }
    //cutCartRecords are currently inserted in machine
    //fontName is requested font
    foreach (CutCartRecord cutCartRecord2 in cutCartRecords)
    {
        if (fontName.CompareTo(cutCartRecord2.fontName) == 0)
        {
            return cutCartRecord2;
        }
    }
    return null;
}
```

## C# updates

replace:

```cSharp
[DllImport("Cricut USB.dll")]
```

with:

```cSharp
[DllImport("Cricut USB.dll", CallingConvention = CallingConvention.Cdecl)]
```
