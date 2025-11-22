# AbonConsole - Command Console for Unity Games
![Title Demo](ReadmeImages/AbonConsoleTitleDemo.gif) 

AbonConsole is a powerful command interpreter and uGUI interface designed to be easy to integrate and expand upon. 

## Pardon Our (My) Dust
AbonConsole currently does not compile when placed in an empty Unity project. It's been directly copied from an ongoing game project, and has some unresolved dependencies despite my best efforts to make it self contained. I'm currently working to fix these.

## Roadmap
- [ ] Get it to compile in an empty project (important!)
- [ ] Set up repository as a Unity package and reorganize it according to Unity's [package layout conventions](https://docs.unity3d.com/6000.2/Documentation/Manual/cus-layout.html).

## Syntax
AbonConsole uses an ANTLR-generated parser to provide an intuitive syntax suited to quick command entry, with some extra features not often found in command consoles.
### Method Calls
Method commands map directly to a C# static method, so they can be called in the familiar way:
```
> method(arg1, arg2, ...)
```
0 and 1 parameter methods can drop the parenthesis for easier entry:
```
> one_param arg1; zero_param
```
is the same as
```
> one_param(arg1); zero_param()
```
#### Dot Calls
A `.` character before a method call will insert the preceding expression as the first argument of the call:
```
> expression.method(arg1, arg2)
```
is the same as
```
> method(expression, arg1, arg2)
```

### Fields and Properties
Fields and properties can be read and assigned to in the familiar way:
```
> field_or_prop = 123
> field_or_prop
< 123
```
The `=` for assignment is optional.

### Literals
AbonConsole supports the following literal expressions:

| Literal Format | .NET type | Notes |
| ---: | --- | --- |
| `1234` | `int` |
| `1234l` | `long` | 
| `1234.5` | `float` | Numbers after decimal point are optional. |
| `1234.5d` | `double` | Numbers after decimal point are optional. |
| `"abcd"` | `string` | Most common escape sequences are supported. |
| `'a'` | `char` | Most common escape sequences are supported. |
| `true` or `false` | `bool` |

### Nested Expressions
Enclosing any expression with `$()` will return a value of type `ConsoleExpression` that represents the enclosed expression. This is the base type of all expressions that can be executed by the interpreter.

```
> $( method_name(arg1, arg2) )
< ConsoleMethodInvocationExpression: method_name(Type1 arg1, Type2 arg2)
```