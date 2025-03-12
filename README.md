GenericPowershell ADFS Attribute Store
======================================

This is a simple ADFS Custom Attribute store which provides functionality to use Windows PowerShell scripts/cmdlets to transform and issue ADFS claims.

## Usage

### Issuing a new claim:
```
 => issue(store = "GenericPowershell", types = ("PwshDate"), query = "Get-Date");
```
This will issue a claim called "PwshDate" with the value of the current date

### Transforming an existing claim and issuing it as a new claim:
**NOTE:** You must represent double-quotes in the claims-language query with two backticks (configurable; see below)
```
c:[Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname"]
 => issue(store = "GenericPowershell", types = ("PwshAccountName"), query = "``$($args[0] -replace '^DOMAIN\\','')$($args[1])``", param = c.Value, param = "helloworld");
```
This will execute the following powershell:
```powershell
"$($args[0] -replace '^DOMAIN\\','')$($args[1])" #This returns a string with the domain stripped off and the string "helloworld" appended
```

The resulting string will be issued as a claim called "PwshAccountName" with value `<windowsaccountname>helloworld`

### Issuing multiple AttributeValue elements for a claim:
This attribute store supports issuing multiple AttributeValue elements for a claim (like an array of elements).
This is typically used when issuing a user's group memberships for example.
The GenericPowershell ADFS Attribute Store will issue one AttributeValue element for each PSObject output to the pipeline.

For example, to issue 3 groups (Group1, Group2, Group3) use the following claims-rule:
```powershell
 => issue(
     store = "GenericPowershell",
     types = ("Groups"),
     query = "@('Group1','Group2','Group3')"
    );
```

### Escaping double-quotes in the claims-language query:
You can escape double-quotes in the claims-language query by using two backticks (default behavior).
If you want to use an alternate escape character sequence, you can configure it in ADFS under:
`AD FS > Service > Attribute Stores > GenericPowershell > Properties > Optional initialization parameters`
and define a Name/Value pair called `doubleQuoteEscapeSequence` with the value being your desired escape sequence.
In theory, you could define a single-quote here to replace all "verbatim strings" with "expanded strings" but results may vary.

Alternatively (for ease of readability), you can use smart quotes in the claims-language query.
Example:  
```
“smart quotation marks”
```
would make the above transform query:
```
c:[Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname"]
   => issue(store = "GenericPowershell",
      types = ("PwshAccountName"),
      query = "“$($args[0] -replace '^DOMAIN\\','')$($args[1])”",
      param = c.Value, param = "helloworld");
```

## Installation
### If compiling without signing the assembly:
1. Copy the compiled `GenericPowershell.dll` to `C:\Windows\ADFS`
2. Add a custom Attribute Store to ADFS with the following settings:
     1. Display Name: `GenericPowershell`
     2. Custom attribute store class name: `Skryptek.GenericPowershell, GenericPowershell`

### If the compiled assembly is strongly signed:
1. Install to the GAC via PowerShell or GACUtil.
#### GACUtil Method:  
```powershell
.\gacutil.exe -if 'C:\Path\To\GenericPowershell.dll'

```
#### PowerShell Method:
```powershell
[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")

$publish = New-Object System.EnterpriseServices.Internal.Publish

// Install Adapter into GAC
$publish.GacInstall('C:\Path\To\GenericPowershell.dll')
```

### Adding the ADFS Attribute Store
Add a custom Attribute Store to ADFS with the following settings:
1. Display Name: `GenericPowershell`
2. Custom attribute store class name: `Skryptek.GenericPowershellGenericPowershell, Version=1.0.0.0, Culture=neutral,PublicKeyToken=<publickeytoken>` (where `<publickeytoken>` is the public keytoken of the compiled assembly - all assemblies in the GAC have to be stronglysigned)
