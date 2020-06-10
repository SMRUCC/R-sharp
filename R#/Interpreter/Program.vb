﻿#Region "Microsoft.VisualBasic::3e62d90f8b7c67d38511d4ff550769e4, R#\Interpreter\Program.vb"

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

    '     Class Program
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: BuildProgram, CreateProgram, EndWithFuncCalls, Execute, GetEnumerator
    '                   IEnumerable_GetEnumerator, isException, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter

    Public Class Program : Implements IEnumerable(Of Expression)

        Friend execQueue As Expression()
        Friend Rscript As Rscript

        Sub New()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Execute(envir As Environment) As Object
            Return ExecutableLoop.Execute(execQueue, envir)
        End Function

        Public Function EndWithFuncCalls(ParamArray anyFuncs As String()) As Boolean
            Dim last As Expression = execQueue.LastOrDefault

            If last Is Nothing Then
                Return False
            ElseIf Not TypeOf last Is FunctionInvoke Then
                Return False
            End If

            Dim funcName As Expression = DirectCast(last, FunctionInvoke).funcName

            If Not TypeOf funcName Is Literal Then
                Return False
            End If

            Dim strName As String = CStr(DirectCast(funcName, Literal).value)

            Return anyFuncs.Any(Function(a) a = strName)
        End Function

        Public Overrides Function ToString() As String
            Return execQueue _
                .Select(Function(exp) exp.ToString & ";") _
                .JoinBy(vbCrLf)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateProgram(Rscript As Rscript, Optional debug As Boolean = False, Optional ByRef error$ = Nothing) As Program
            Dim opts As New SyntaxBuilderOptions With {.debug = debug, .source = Rscript}
            Dim exec As Expression() = Rscript _
                .GetExpressions(opts) _
                .ToArray

            If opts.haveSyntaxErr Then
                [error] = opts.error
                Return Nothing
            Else
                Return New Program With {
                    .execQueue = exec,
                    .Rscript = Rscript
                }
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function isException(ByRef result As Object, Optional envir As Environment = Nothing, ByRef Optional isDotNETException As Boolean = False) As Boolean
            If result Is Nothing Then
                Return False
            ElseIf result.GetType Is GetType(Message) Then
                Return DirectCast(result, Message).level = MSG_TYPES.ERR
            ElseIf Not envir Is Nothing AndAlso result.GetType.IsInheritsFrom(GetType(Exception)) Then
                isDotNETException = True
                result = Internal.debug.stop(result, envir)

                Return True
            Else
                Return False
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Function BuildProgram(scriptText As String, Optional debug As Boolean = False, Optional ByRef error$ = Nothing) As Program
            Dim script = Rscript.AutoHandleScript(scriptText)
            Dim program As Program = CreateProgram(script, debug, [error])

            Return program
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Expression) Implements IEnumerable(Of Expression).GetEnumerator
            For Each line As Expression In execQueue
                Yield line
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
