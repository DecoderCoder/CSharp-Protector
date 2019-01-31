# CSharp-Protector

```
using TypeName=TypeAssemblyQualifiedName;

Global
{
var GlobalVar1
var GlobalVar2
}

void Main
{
var LocalVar
LdStr "Text"
LdInt 20
Ceq
Cgt
Cls
LdByte 0x34
Call VoidName
CallCSharp TypeName:MethodName(Argument1, Argument2)
Br +1
BrTrue -2
BrFalse 1
Pop
StGv GlobalVar1
LdGv GlobalVar1
StLv LocalVar
LdLv LocalVar
Ret
}
```

