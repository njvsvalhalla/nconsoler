# NConsolerStandard

[![Build status](https://ci.appveyor.com/api/projects/status/e0ijh5fcmsr5xsf0?svg=true)](https://ci.appveyor.com/project/njvsvalhalla/nconsoler)

## Library for parsing console arguments in C

This is forked from the original [nconsoler](https://github.com/csharpus/nconsoler) to update it to be a .NET Standard library. This can be used in core. According to [google code archive](https://code.google.com/archive/p/nconsoler/#!) this was licensed as MPL 1.1 so I will be updating the repository to actually make it compliant (the original source didn't even have the license file that I saw). I'll try to work on some of the leftover issues from the original repository.

I tested this on Linux with the examples and works great.

The following is a markdown conversion of the old website located [here](http://web.archive.org/web/20120117045940/http://nconsoler.csharpus.com/manual)

If there is any mistake in this readme, please feel free to create an issue or make a pull request.

Nuget package [is located here](https://www.nuget.org/packages/NConsolerStandard/)

## Changes

 - Added License file
 - Included copyright
 - Updated tests to Moq instead of RhinoMock
 - Removed all the build files and bats
 - Added this readme
 - Removed the "Dependencies" folder and used nuget packages for the applicable libraries
 - Formatted some of the ugly code, and variable names

## QuickStart

The following creates a program that has an "Action", with a required parameter "count" and optional parameter "flag"    

    class Program {
          static void Main(string[] args) {
                Consolery.Run(typeof(Program), args);
          }
    
          [Action]
          public static void DoWork(
                [Required] int count,
                [Optional(false)] bool flag) {
                     Console.WriteLine("DoWork {0} {1}", count, flag);
                }
    }
## Cheat Sheet

## Code examples

To add an action:

```
[Action]
public static void Method(...
```
To add a required parameter:
```
[Required(Description = "Parameter name")] string name
```
To add an optional one:
```
[Optional("Parameter name")] string optional
```
To add an alias for parameter:
```
[Optional("Default value", "anAlias")] string value
```

it can be used either `program.exe ... /value:someValue` or `program.exe ... /anAlias:someValue`

## Command Line examples

to run program  
`program.exe "required" /optional:"value" /-boolean`

to pass true for boolean  
`/paramName`

to pass false for boolean  
`/-paramName`

to pass an integer array  
`/paramName:1+2+3`

to pass Date  
`/paramName:21-01-2008`

To get help information  
`program.exe [/help | /h | /?]` or just `program.exe`

##  Parameters Passed to Command Line

NConsoler has two types of parameters:

-   `Required` (always go before optional in command line as well as declared in action method)
-   `Optional` (is not necessary and could be skipped in command line)

Form of optional parameter is next: `/param_name:param_value` where `param_name` - name of parameter in source code.

NConsoler provides aliases to optional parameters. For example if parameter name is `flag`, and you want to use it in command line as `/f`, you can specify one or more aliases in `OptionalAttribute` arguments.

Boolean optional parameters are used without param value: `/param_name` or `/-param_name` in first case true will be sent to the method and false in second one. Parameters `/?`, `/help` and `/h` are reserved for requesting help.

NConsoler supports next types of parameters:

-   `string`
-   `int`
-   `bool`
-   `string[]`
-   `int[]`
-   `DateTime`

Example:

```
[Action]
public static void Method(
    [Required] string p1,
    [Required] int p2,
    [Required] string[] p3,
    [Required] int[] p4,
    [Required] DateTime p5,
    [Optional(false)] bool flag)
```

to run this method: `program.exe "first" 2 "a"+"b" 4+5+6 20-01-2008 /flag`

## Multiple Actions
It's easy to create an application with more than one action method, just add new method and mark it with `Action` attribute. In this case action method name should be specified as first console argument parameter. This parameter is case insensitive.

Example:

```
[Action]
public static void Add(
    [Required] string userName)...

[Action]
public static void Remove(
    [Required] string userName)...
```

to run first method: `program.exe add "user1"` to run second one: `program.exe remove "user1"`

## Validation of Metadata and User Input
It's easy to create an application with more than one action method, just add new method and mark it with `Action` attribute. In this case action method name should be specified as first console argument parameter. This parameter is case insensitive.

Example:

```
[Action]
public static void Add(
    [Required] string userName)...

[Action]
public static void Remove(
    [Required] string userName)...
```

to run first method: `program.exe add "user1"` to run second one: `program.exe remove "user1"`

## Unit Testing Metadata

If you want to create unit test that validates whether your action methods configured correctly, create a simple unit test that just calls `Validate` method:

```
[Test]
public void NConsolerConfigurationShouldBeCorrect() {
    Consolery.Validate(typeof(Program));
}
```

If configuration is incorrect, an exception with error description will be thrown and test will not pass.

## Help Messages
Run your console program with `/h`, `/help` or `/?` parameter. NConsoler constructs help message automatically based on action methods meta information. Use description parameter in `RequiredAttribute` or `OptionalAttribute` constructor.

```
[Action]
public static void Method(
    [Required(Description = "Applies some magic")] string flag)...
```

Returns you a message:

 usage: NConsolerTest flag
  flag Applies some magic

## Examples

Examples are included in the solution. To run them:

HelloWorld:

    dotnet HelloWorld.dll "Hello World"

Multiplier:

    dotnet Multiplier.dll 2 2
Quickstart:

    dotnet Quickstart.dll 10 /flag
Rich:

    dotnet Rich.dll 10 "description" /-book /comment:"details" /length:100

