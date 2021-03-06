﻿#Region "Microsoft.VisualBasic::f7057892c4db4eac5ff9c5e8d66a15ac, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\If\IfPromise.vb"

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

    '     Class IfPromise
    ' 
    '         Properties: assignTo, Result, Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: DoValueAssign
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Blocks

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
                Case GetType(ValueAssignExpression)
                    Return DirectCast(assignTo, ValueAssignExpression).DoValueAssign(envir, Value)
                Case Else
                    Return Internal.debug.stop(New NotImplementedException, envir)
            End Select
        End Function
    End Class
End Namespace
