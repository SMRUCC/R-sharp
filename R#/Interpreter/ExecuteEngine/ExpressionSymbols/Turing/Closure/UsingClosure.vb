﻿#Region "Microsoft.VisualBasic::6c3260a9f804a0d716b49e0266e8f166, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\UsingClosure.vb"

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

    '     Class UsingClosure
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class UsingClosure : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ReadOnly params As DeclareNewVariable
        ReadOnly closure As ClosureExpression

        Sub New(params As DeclareNewVariable, closure As ClosureExpression)
            Me.params = params
            Me.closure = closure
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Using env As New Environment(envir, params.ToString)
                Dim resource As Object = params.Evaluate(env)
                Dim result As Object

                If resource Is Nothing Then
                    Return Internal.stop("Target is nothing!", env)
                ElseIf Program.isException(resource) Then
                    Return resource
                ElseIf Not resource.GetType.ImplementInterface(GetType(IDisposable)) Then
                    Return Message.InCompatibleType(GetType(IDisposable), resource.GetType, env)
                End If

                ' run using closure and get result
                result = closure.Evaluate(env)

                ' then dispose the target
                ' release handle and clean up the resource
                Call DirectCast(resource, IDisposable).Dispose()

                Return result
            End Using
        End Function

        Public Overrides Function ToString() As String
            Return $"using {params} {{
    # using closure internal
    {closure}
}}"
        End Function
    End Class
End Namespace