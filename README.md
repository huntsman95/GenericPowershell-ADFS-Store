GenericPowershell ADFS Attribute Store
======================================

This is a simple ADFS Custom Attribute store which provides functionality to use Windows PowerShell scripts/cmdlets to transform and issue ADFS claims.

## Usage

### Issuing a new claim:
```
 => issue(store = "GenericPowershell", types = ("PwshDate"), query = "Get-Date", param = "");
```
This will issue a claim called "PwshDate" with the value of the current date

### Transforming an existing claim and issuing it as a new claim:
```
c:[Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname"]
 => issue(store = "GenericPowershell", types = ("PwshAccountName"), query = "'{0}{1}' -replace '^DOMAIN\\',''", param = c.Value, param = "helloworld");
```
This will execute the following powershell:
```powershell
## The GenericPowershell attribute store will do a String.Format on {0} and {1} with the provided parameters
'{0}{1}' -replace '^DOMAIN\\','' #This returns a string with the domain stripped off and the string "helloworld" appended
```

The resulting string will be issued as a claim called "PwshAccountName" with value `<windowsaccountname>helloworld`


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
