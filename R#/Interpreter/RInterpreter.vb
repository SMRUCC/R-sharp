﻿#Region "Microsoft.VisualBasic::deccfc390d884546bb0ac9269d839e54, R#\Interpreter\RInterpreter.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:

'     Class RInterpreter
' 
'         Properties: debug, globalEnvir, Rsharp, strict, warnings
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: (+2 Overloads) Evaluate, finalizeResult, FromEnvironmentConfiguration, InitializeEnvironment, Invoke
'                   (+2 Overloads) LoadLibrary, Run, RunInternal, Source
' 
'         Sub: (+3 Overloads) Add, Print, PrintMemory
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System.Configuration
Imports REnv = SMRUCC.Rsharp.Runtime.Internal.Invokes

Namespace Interpreter

    Public Class RInterpreter

        ''' <summary>
        ''' Global runtime environment.(全局环境)
        ''' </summary>
        Public ReadOnly Property globalEnvir As GlobalEnvironment
        Public ReadOnly Property warnings As New List(Of Message)

        ''' <summary>
        ''' R# running in debug mode.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 调试模式下，除了输出表达式的字符串信息之外
        ''' 还会改变stop的行为，在非调试模式下，stop函数只会丢出错误消息并且终止脚本的运行
        ''' 但是在调试模式下面，stop函数则会令程序抛出异常方便开发人员进行错误的定位
        ''' </remarks>
        Public Property debug As Boolean = False

        ''' <summary>
        ''' 是否在严格模式下运行R#脚本？默认为严格模式，即：
        ''' 
        ''' 1. 所有的变量必须使用``let``关键词进行申明
        ''' </summary>
        ''' <returns></returns>
        Public Property strict As Boolean = True

        ''' <summary>
        ''' Get value of a <see cref="Symbol"/>
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property GetValue(name As String) As Object
            Get
                Return globalEnvir(name).value
            End Get
        End Property

        Public Const lastVariableName$ = "$"

        Public ReadOnly Property configFile As ConfigFile
            Get
                Return globalEnvir.options.file
            End Get
        End Property

        ''' <summary>
        ''' 直接无参数调用这个构造函数，则会使用默认的配置文件创建R#脚本解释器引擎实例
        ''' </summary>
        ''' <param name="envirConf"></param>
        Sub New(Optional envirConf As Options = Nothing)
            If envirConf Is Nothing Then
                envirConf = New Options(ConfigFile.localConfigs)
            End If

            globalEnvir = New GlobalEnvironment(Me, envirConf)
            globalEnvir.Push(lastVariableName, Nothing, False, TypeCodes.generic)
            globalEnvir.Push("PI", Math.PI, True, TypeCodes.double)
            globalEnvir.Push("E", Math.E, True, TypeCodes.double)
            globalEnvir.Push(".GlobalEnv", globalEnvir, True, TypeCodes.generic)

            ' config R# interpreter engine
            [strict] = envirConf.strict
        End Sub

        Public Sub PrintMemory(Optional dev As TextWriter = Nothing)
            Dim table$()() = globalEnvir _
                .Select(Function(v)
                            Dim value$ = Symbol.GetValueViewString(v)

                            Return {
                                v.name,
                                v.typeCode.ToString,
                                v.typeof.FullName,
                                $"[{v.length}] {value}"
                            }
                        End Function) _
                .ToArray

            With dev Or App.StdOut
                Call .DoCall(Sub(device)
                                 Call table.PrintTable(
                                    dev:=device,
                                    leftMargin:=3,
                                    title:={"name", "mode", "typeof", "value"}
                                 )
                             End Sub)
            End With
        End Sub

        ''' <summary>
        ''' A shortcut of ``print(expr)``
        ''' </summary>
        ''' <param name="expr"></param>
        Public Sub Print(expr As String)
            Dim result As Object = Evaluate(expr)

            ' do expression evaluation and then 
            ' print($expr)
            Call REnv.print(result, globalEnvir)
        End Sub

        ''' <summary>
        ''' Load packages from package name or dll module file
        ''' </summary>
        ''' <param name="packageName">
        ''' package namespace or dll module file path
        ''' </param>
        Public Function LoadLibrary(packageName As String) As RInterpreter
            If packageName.FileExists Then
                ' is a dll file
                Call [Imports].LoadLibrary(packageName, globalEnvir, {"*"})
            Else
                Dim result As Message = globalEnvir.LoadLibrary(packageName)

                If Not result Is Nothing Then
                    Call Internal.debug.PrintMessageInternal(result)
                End If
            End If

            Return Me
        End Function

        Public Function LoadLibrary(package As Type) As RInterpreter
            Call globalEnvir.LoadLibrary(package)
            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Sub Add(name$, value As Object, Optional type As TypeCodes = TypeCodes.generic)
            Call globalEnvir.Push(name, value, [readonly]:=False, mode:=type)
        End Sub

        <DebuggerStepThrough>
        Public Sub Add(name$, closure As [Delegate])
            globalEnvir.Push(name, New RMethodInfo(name, closure), [readonly]:=False, mode:=TypeCodes.closure)
        End Sub

        <DebuggerStepThrough>
        Public Sub Add(name$, closure As MethodInfo, Optional target As Object = Nothing)
            globalEnvir.Push(name, New RMethodInfo(name, closure, target), [readonly]:=False, mode:=TypeCodes.closure)
        End Sub

        Public Function Invoke(funcName$, ParamArray args As Object()) As Object
            Dim symbol = globalEnvir.FindSymbol(funcName)

            If symbol Is Nothing Then
                Throw New EntryPointNotFoundException($"No object named '{funcName}' could be found in global environment!")
            ElseIf symbol.typeCode <> TypeCodes.closure OrElse Not symbol.typeof.ImplementInterface(GetType(RFunction)) Then
                Throw New InvalidProgramException($"Object '{funcName}' is not a function!")
            End If

            Return DirectCast(symbol.value, RFunction).Invoke(globalEnvir, args)
        End Function

        ''' <summary>
        ''' Run R# script program from text data.
        ''' </summary>
        ''' <param name="script">The script text</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function Evaluate(script As String) As Object
            Return RunInternal(Rscript.FromText(script), {})
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function Run(program As Program) As Object
            Return finalizeResult(program.Execute(globalEnvir))
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="source">The script file name</param>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        Private Function InitializeEnvironment(source$, arguments As NamedValue(Of Object)()) As Environment
            Dim env As Environment

            If source Is Nothing OrElse InStr(source, "<in_memory_") = 1 Then
                env = globalEnvir
            Else
                env = New StackFrame With {
                    .File = source,
                    .Line = 0,
                    .Method = New Method With {
                        .Method = MethodBase.GetCurrentMethod.Name,
                        .[Module] = "n/a",
                        .[Namespace] = "SMRUCC/R#"
                    }
                }.DoCall(Function(stackframe)
                             Return New Environment(globalEnvir, stackframe, isInherits:=True)
                         End Function)
            End If

            Dim symbol$
            Dim obj As Object

            For Each var As NamedValue(Of Object) In arguments
                symbol = var.Name
                obj = var.Value

                Call env.Push(symbol, obj, [readonly]:=False)
            Next

            If debug AndAlso arguments.Length > 0 Then
                Call "Initialize of the environment with pre-define symbols:".__DEBUG_ECHO
                Call arguments.Keys.GetJson.__INFO_ECHO
            End If

            Return env
        End Function

        Friend Function finalizeResult(result As Object) As Object
            Dim last As Symbol = Me.globalEnvir(lastVariableName)

            ' set last variable in current environment
            Call last.SetValue(result, globalEnvir)

            If Program.isException(result) Then
                Call VBDebugger.WaitOutput()
                Call Internal.debug.PrintMessageInternal(message:=result)
            End If

            If globalEnvir.messages > 0 Then
                Call VBDebugger.WaitOutput()

                For Each message As Message In globalEnvir.messages
                    Call message.DoCall(AddressOf Internal.debug.PrintMessageInternal)
                Next

                Call globalEnvir.messages.Clear()
            End If

            Return result
        End Function

        Private Function RunInternal(Rscript As Rscript, arguments As NamedValue(Of Object)()) As Object
            Dim globalEnvir As Environment = InitializeEnvironment(Rscript.fileName, arguments)
            Dim error$ = Nothing
            Dim program As Program = Program.CreateProgram(Rscript, debug:=debug, [error]:=[error])
            Dim result As Object = program.Execute(globalEnvir)

            ' fix bugs of warning message populates
            ' to upper global environment
            If Not globalEnvir Is Me.globalEnvir Then
                Call globalEnvir.Dispose()
            End If

            Return finalizeResult(result)
        End Function

        ''' <summary>
        ''' Run R# script program from a given script file.
        ''' (运行脚本的时候调用的是<see cref="globalEnvir"/>全局环境)
        ''' </summary>
        ''' <param name="filepath">The script file path.</param>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Source(filepath$, ParamArray arguments As NamedValue(Of Object)()) As Object
            ' when source a given script by path
            ' then an object list variable with special name will be push into 
            ' the environment
            ' 
            ' let !script = list(dir = dirname, file = filename, fullName = filepath)
            Dim script As list = CreateMagicScriptSymbol(filepath)
            Dim result As Object

            If filepath.FileExists Then
                arguments = arguments _
                    .JoinIterates(New NamedValue(Of Object)("!script", script)) _
                    .ToArray
                result = RunInternal(Rscript.FromFile(filepath), arguments)
            Else
                result = Internal.debug.stop({
                    $"cannot open the connection.",
                    $"cannot open file '{filepath.FileName}': No such file or directory",
                    $"file: {filepath.GetFullPath}",
                    $"function: source"
                }, globalEnvir)
            End If

            Return result
        End Function

        Public Shared ReadOnly Property Rsharp As New RInterpreter

        Public Shared Function Evaluate(script$, ParamArray args As NamedValue(Of Object)()) As Object
            SyncLock Rsharp
                With Rsharp
                    If Not args.IsNullOrEmpty Then
                        Dim name$
                        Dim value As Object

                        For Each var As NamedValue(Of Object) In args
                            name = var.Name
                            value = var.Value

                            Call .globalEnvir.Push(name, value, [readonly]:=False, mode:=TypeCodes.generic)
                        Next
                    End If

                    Return .Evaluate(script)
                End With
            End SyncLock
        End Function

        <DebuggerStepThrough>
        Public Shared Function FromEnvironmentConfiguration(configs As String) As RInterpreter
            Return New RInterpreter(New Options(configs))
        End Function
    End Class
End Namespace
