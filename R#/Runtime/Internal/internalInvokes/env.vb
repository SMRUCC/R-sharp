﻿#Region "Microsoft.VisualBasic::20942b3745f88f32bb1e171154b46a49, R#\Runtime\Internal\internalInvokes\env.vb"

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

'     Module env
' 
'         Function: [get], [typeof], CallInternal, doCall, environment
'                   getOutputDevice, globalenv, lockBinding, ls, objects
'                   objectSize, traceback, unlockBinding
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Development.Package
Imports REnv = SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports Microsoft.VisualBasic.Language
Imports System.Runtime.CompilerServices

Namespace Runtime.Internal.Invokes

    Module env

        ''' <summary>
        ''' # Return the Value of a Named Object
        ''' 
        ''' Search by name for an object (get) or zero or more objects (mget).
        ''' </summary>
        ''' <param name="x">For get, an object name (given as a character string).
        ''' For mget, a character vector of object names.</param>
        ''' <param name="envir">where to look for the object (see ‘Details’); if omitted search as if the name of the object appeared unquoted in an expression.</param>
        ''' <param name="inherits">
        ''' should the enclosing frames of the environment be searched?
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("get")>
        Public Function [get](x As Object, envir As Environment, Optional [inherits] As Boolean = True) As Object
            Dim name As String = REnv.asVector(Of Object)(x) _
                .DoCall(Function(o)
                            Return Scripting.ToString(REnv.getFirst(o), null:=Nothing)
                        End Function)

            If name.StringEmpty Then
                Return Internal.debug.stop("NULL value provided for object name!", envir)
            Else
                Return SymbolReference.GetReferenceObject(name, envir, [inherits])
            End If
        End Function

        ''' <summary>
        ''' Get global environment
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("globalenv")>
        <DebuggerStepThrough>
        Private Function globalenv(env As Environment) As Object
            Return env.globalEnvironment
        End Function

        ''' <summary>
        ''' Get current environment
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("environment")>
        <DebuggerStepThrough>
        Private Function environment(env As Environment) As Object
            Return env
        End Function

        ''' <summary>
        ''' Get the standard output device name string
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("output_device")>
        Private Function getOutputDevice(env As Environment) As String
            Return env.globalEnvironment.stdout.env.ToString.ToLower
        End Function

        ''' <summary>
        ''' # List Objects
        ''' 
        ''' ``ls`` and ``objects`` return a vector of character strings giving 
        ''' the names of the objects in the specified environment. When invoked 
        ''' with no argument at the top level prompt, ls shows what data sets 
        ''' and functions a user has defined. When invoked with no argument inside 
        ''' a function, ls returns the names of the function's local variables: 
        ''' this is useful in conjunction with ``browser``.
        ''' </summary>
        ''' <param name="name">The package name, which environment to use in 
        ''' listing the available objects. Defaults to the current environment. 
        ''' Although called name for back compatibility, in fact this argument 
        ''' can specify the environment in any form.</param>
        ''' <param name="env">
        ''' an alternative argument to name for specifying the environment. 
        ''' Mostly there for back compatibility.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("ls")>
        <RApiReturn(GetType(String))>
        Private Function ls(<RSymbolTextArgument> Optional name$ = Nothing, Optional env As Environment = Nothing) As Object
            Dim opt As NamedValue(Of String) = name.GetTagValue(":", trim:=True)
            Dim globalEnv As GlobalEnvironment = env.globalEnvironment
            Dim pkgMgr As PackageManager = globalEnv.packages

            If name.StringEmpty Then
                ' list all of the objects in current 
                ' R# runtime environment
                Return env.symbols.Keys.ToArray
            ElseIf opt.Name.StringEmpty Then
                Return opt.listOptionItems(name, env)
            End If

            Select Case opt.Name.ToLower
                Case "package"
                    ' list all of the function api names in current package
                    Dim package As Package = pkgMgr.FindPackage(opt.Value, Nothing)

                    If package Is Nothing Then
                        Return debug.stop({"missing required package for query...", "package: " & opt.Value}, env)
                    Else
                        Return package.ls
                    End If
                Case Else
                    Return debug.stop(New NotSupportedException(name), env)
            End Select
        End Function

        <Extension>
        Private Function listOptionItems(opt As NamedValue(Of String), name$, env As Environment)
            Dim globalEnv As GlobalEnvironment = env.globalEnvironment
            Dim pkgMgr As PackageManager = globalEnv.packages

            If opt.Value = "REnv" Then
                Return Internal.invoke.ls
            ElseIf opt.Value = "Activator" Then
                Dim names As Array = globalEnv.types.Keys.ToArray
                Dim fullName As Array = DirectCast(names, String()) _
                    .Select(Function(key)
                                Return globalEnv.types(key).fullName
                            End Function) _
                    .ToArray

                Return New dataframe With {
                    .columns = New Dictionary(Of String, Array) From {
                        {"name", names},
                        {"fullName", fullName}
                    },
                    .rownames = names
                }
            ElseIf opt.Value.DirectoryExists Then
                ' list dir?
                Return opt.Value _
                    .ListFiles _
                    .Select(AddressOf FileName) _
                    .ToArray
            ElseIf pkgMgr.hasLibFile(name.FileName) Then
                ' list all of the package names in current dll module
                Return PackageLoader _
                    .ParsePackages(dll:=name) _
                    .Select(Function(pkg) pkg.namespace) _
                    .ToArray
            Else
                Dim func = env.enumerateFunctions _
                    .Where(Function(fun)
                               Return TypeOf fun.value Is RMethodInfo AndAlso DirectCast(fun.value, RMethodInfo).GetPackageInfo.namespace = name
                           End Function) _
                    .Select(Function(fun) DirectCast(fun.value, RMethodInfo).name) _
                    .ToArray

                If func.IsNullOrEmpty Then
                    Return debug.stop({"invalid query term!", "term: " & name}, env)
                Else
                    Return func
                End If
            End If
        End Function

        <ExportAPI("objects")>
        Private Function objects(env As Environment) As String()
            Return env.symbols.Keys.ToArray
        End Function

        ''' <summary>
        ''' # Report the Space Allocated for an Object
        ''' 
        ''' Provides an estimate of the memory that is being used to store 
        ''' an R# object.
        ''' 
        ''' Exactly which parts of the memory allocation should be attributed 
        ''' to which object is not clear-cut. This function merely provides 
        ''' a rough indication: it should be reasonably accurate for atomic 
        ''' vectors, but does not detect if elements of a list are shared, for 
        ''' example. (Sharing amongst elements of a character vector is taken 
        ''' into account, but not that between character vectors in a single 
        ''' object.)
        '''
        ''' The calculation Is Of the size Of the Object, And excludes the 
        ''' space needed To store its name In the symbol table.
        '''
        ''' Associated space(e.g., the environment of a function And what the 
        ''' pointer in a EXTPTRSXP points to) Is Not included in the 
        ''' calculation.
        '''
        ''' Object sizes are larger On 64-bit builds than 32-bit ones, but will 
        ''' very likely be the same On different platforms With the same word 
        ''' length And pointer size.
        ''' </summary>
        ''' <param name="x">an R# object.</param>
        ''' <returns>
        ''' An object of class "object_size" with a length-one double value, 
        ''' an estimate of the memory allocation attributable to the object 
        ''' in bytes.
        ''' </returns>
        <ExportAPI("object.size")>
        Public Function objectSize(<RRawVectorArgument> x As Object) As Long
            Return HeapSizeOf.MeasureSize(x)
        End Function

        ''' <summary>
        ''' # Execute a Function Call
        ''' 
        ''' ``do.call`` constructs and executes a function call from a name or 
        ''' a function and a list of arguments to be passed to it.
        ''' </summary>
        ''' <param name="what"></param>
        ''' <param name="calls">
        ''' either a function or a non-empty character string naming the function 
        ''' to be called.
        ''' </param>
        ''' <param name="args">
        ''' a list of arguments to the function call. The names attribute of 
        ''' args gives the argument names.
        ''' </param>
        ''' <param name="envir">
        ''' an environment within which to evaluate the call. This will be most 
        ''' useful if what is a character string and the arguments are symbols 
        ''' or quoted expressions.
        ''' </param>
        ''' <returns>The result of the (evaluated) function call.</returns>
        <ExportAPI("do.call")>
        Public Function doCall(what As Object,
                               Optional calls$ = Nothing,
                               <RListObjectArgument>
                               Optional args As Object = Nothing,
                               Optional envir As Environment = Nothing) As Object

            If TypeOf what Is String AndAlso calls.StringEmpty Then
                ' call static api by name
                Return CallInternal(what, args, envir)
            ElseIf what Is Nothing AndAlso calls.StringEmpty Then
                Return Internal.debug.stop("Nothing to call!", envir)
            End If

            Dim targetType As Type = what.GetType

            ' call api from an object instance 
            If targetType Is GetType(vbObject) Then
                Dim Robj As vbObject = DirectCast(what, vbObject)
                Dim member As Object

                If Not Robj.existsName(calls) Then
                    ' throw exception for invoke missing member from .NET object?
                    Return Internal.debug.stop({$"Missing member '{calls}' in target {what}", "type: " & Robj.type.fullName, "member name: " & calls}, envir)
                Else
                    member = Robj.getByName(name:=calls)
                End If

                ' invoke .NET API / property getter
                If member.GetType Is GetType(RMethodInfo) Then
                    Dim arguments As InvokeParameter() = args
                    Dim api As RMethodInfo = DirectCast(member, RMethodInfo)

                    Return api.Invoke(envir, arguments)
                Else
                    Return member
                End If
            ElseIf targetType Is GetType(list) Then
                Throw New NotImplementedException
            Else
                Return Internal.debug.stop(New NotImplementedException(targetType.FullName), envir)
            End If
        End Function

        Public Function CallInternal(call$, args As Object, envir As Environment) As Object
            Dim ref As NamedValue(Of String) = [call].GetTagValue("::")
            Dim callName As String = ref.Value
            Dim [namespace] As String = ref.Name
            Dim func As Object = FunctionInvoke.GetFunctionVar(New Literal(callName), envir, [namespace]:=[namespace])

            If Program.isException(func) Then
                Return func
            End If

            Dim invoke As RFunction = DirectCast(func, RFunction)
            Dim arguments As New List(Of InvokeParameter)

            If TypeOf args Is list Then
                Dim i As i32 = Scan0

                For Each item In DirectCast(args, list).slots
                    Call New InvokeParameter(name:=item.Key, item.Value, ++i).DoCall(AddressOf arguments.Add)
                Next
            ElseIf TypeOf args Is InvokeParameter() Then
                arguments.AddRange(DirectCast(args, InvokeParameter()))
            End If

            Dim result As Object = invoke.Invoke(envir, arguments.ToArray)

            Return FunctionInvoke.HandleResult(result, envir)
        End Function

        ''' <summary>
        ''' ### Get and Print Call Stacks
        ''' 
        ''' By default traceback() prints the call stack of the last uncaught 
        ''' error, i.e., the sequence of calls that lead to the error. This 
        ''' is useful when an error occurs with an unidentifiable error message. 
        ''' It can also be used to print the current stack or arbitrary lists 
        ''' of deparsed calls.
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("traceback")>
        Public Function traceback(Optional env As Environment = Nothing) As ExceptionData
            Dim exception As Message = env.globalEnvironment.lastException

            If exception Is Nothing Then
                ' 如果错误消息不存在
                ' 则返回当前的调用栈信息
                Return New ExceptionData With {
                    .StackTrace = debug.getEnvironmentStack(env),
                    .Message = {"n/a"},
                    .TypeFullName = "n/a"
                }
            Else
                Return New ExceptionData With {
                    .StackTrace = exception.environmentStack,
                    .Message = exception.message,
                    .TypeFullName = GetType(Message).FullName
                }
            End If
        End Function

        ''' <summary>
        ''' Binding and Environment Locking, Active Bindings
        ''' </summary>
        ''' <param name="sym">a name object or character string.</param>
        ''' <param name="env">an environment.</param>
        ''' <returns></returns>
        <ExportAPI("lockBinding")>
        Public Function lockBinding(sym As String(), Optional env As Environment = Nothing) As Object
            Dim symbolObj As Symbol

            For Each name As String In sym
                symbolObj = env.FindSymbol(name)

                If symbolObj Is Nothing Then
                    Return Message.SymbolNotFound(env, name, TypeCodes.NA)
                Else
                    symbolObj.readonly = True
                End If
            Next

            Return Nothing
        End Function

        <ExportAPI("unlockBinding")>
        Public Function unlockBinding(sym As String(), Optional env As Environment = Nothing) As Object
            Dim symbolObj As Symbol

            For Each name As String In sym
                symbolObj = env.FindSymbol(name)

                If symbolObj Is Nothing Then
                    Return Message.SymbolNotFound(env, name, TypeCodes.NA)
                Else
                    symbolObj.readonly = False
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' get a .NET type model from a given VB.NET type full name
        ''' </summary>
        ''' <param name="fullName"></param>
        ''' <returns></returns>
        <ExportAPI("export")>
        Public Function [typeof](fullName As String) As RType
            Return RType.GetRSharpType(Type.GetType(fullName))
        End Function
    End Module
End Namespace
