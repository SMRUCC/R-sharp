﻿#Region "Microsoft.VisualBasic::1fc96a51f7e3270842ddd75b612e6e96, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\StringInterpolation.vb"

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

'     Class StringInterpolation
' 
'         Properties: type
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Evaluate, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.System.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    Public Class StringInterpolation : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.StringInterpolation
            End Get
        End Property

        ''' <summary>
        ''' 这些表达式产生的全部都是字符串结果值
        ''' </summary>
        Friend ReadOnly stringParts As Expression()

        Sub New(stringParts As Expression())
            Me.stringParts = stringParts
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim current As Array = Runtime.asVector(Of String)(stringParts(Scan0).Evaluate(envir))
            Dim [next] As Object

            For Each part As Expression In stringParts.Skip(1)
                [next] = part.Evaluate(envir)

                With Runtime.asVector(Of Object)([next])
                    If .Length = 1 Then
                        [next] = .GetValue(Scan0)
                    End If
                End With

                If Program.isException([next]) Then
                    Return [next]
                Else
                    current = StringBinaryExpression.DoStringBinary(Of String)(
                        a:=current,
                        b:=[next],
                        op:=Function(x, y) x & y
                    )
                End If
            Next

            Return current
        End Function

        Public Overrides Function ToString() As String
            Return stringParts.JoinBy(" & ")
        End Function
    End Class
End Namespace
