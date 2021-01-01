﻿#Region "Microsoft.VisualBasic::d2e347005ba36fdac466c8ec986172b1, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\IfBranch.vb"

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

    '     Class IfBranch
    ' 
    '         Properties: expressionName, ifTest, stackFrame, trueClosure, type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Evaluate, ToString
    '         Class IfPromise
    ' 
    '             Properties: assignTo, Result, Value
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: DoValueAssign
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Development.Package.File
Imports devtools = Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

    Public Class IfBranch : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return trueClosure.type
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.If
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Public ReadOnly Property ifTest As Expression
        Public ReadOnly Property trueClosure As DeclareNewFunction

        Friend Class IfPromise

            Public ReadOnly Property Result As Boolean
            Public ReadOnly Property Value As Object
            Public Property assignTo As Expression

            Sub New(value As Object, result As Boolean)
                Me.Value = value
                Me.Result = result
            End Sub

            Public Function DoValueAssign(envir As Environment) As Object
                ' 没有变量需要进行closure的返回值设置
                ' 则跳过
                If assignTo Is Nothing Then
                    Return Value
                End If

                Select Case assignTo.GetType
                    Case GetType(ValueAssign)
                        Return DirectCast(assignTo, ValueAssign).DoValueAssign(envir, Value)
                    Case Else
                        Return Internal.debug.stop(New NotImplementedException, envir)
                End Select
            End Function
        End Class

        Sub New(ifTest As Expression, trueClosure As DeclareNewFunction, stackframe As StackFrame)
            Me.ifTest = ifTest
            Me.trueClosure = trueClosure
            Me.stackFrame = stackframe
        End Sub

        Sub New(ifTest As Expression, trueClosure As ClosureExpression, stackframe As StackFrame)
            Call Me.New(
                ifTest:=ifTest,
                trueClosure:=New DeclareNewFunction(
                    funcName:="if_closure_internal",
                    params:={},
                    body:=trueClosure,
                    stackframe:=stackframe
                ),
                stackframe:=stackframe
            )
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim test As Object = ifTest.Evaluate(envir)

            If test Is Nothing Then
                Return New Message With {
                    .message = {
                        $"missing value where TRUE/FALSE needed"
                    },
                    .level = MSG_TYPES.ERR,
                    .environmentStack = debug.getEnvironmentStack(envir),
                    .trace = devtools.ExceptionData.GetCurrentStackTrace
                }
            ElseIf Program.isException(test) Then
                Return test
            End If

            Dim flags As Boolean() = REnv.asLogical(test)

            If flags.Length = 0 Then
                Return Internal.debug.stop({
                    "argument is of length zero",
                    "test: " & ifTest.ToString
                }, envir)
            End If

            If True = flags(Scan0) Then
                Dim env As New Environment(envir, stackFrame, isInherits:=False)
                Dim resultVal As Object = trueClosure.Invoke(env, {})

                If Program.isException(resultVal) Then
                    Return resultVal
                Else
                    Return New IfPromise(resultVal, True)
                End If
            Else
                Return New IfPromise(Nothing, False)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"if ({ifTest}) then {{
    {trueClosure}
}}"
        End Function
    End Class
End Namespace
