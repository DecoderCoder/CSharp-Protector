# VM

[VMCompiler](https://github.com/DecoderCoder/CSharp-Protector/tree/master/VMCompiler/Samples)

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
Add
Sub
Mul
Div
Br 1
BrTrue 1
BrFalse 1
Pop
Dup
StGv GlobalVar1
LdGv GlobalVar1
StLv LocalVar
LdLv LocalVar
Nop
Ret
}
```
