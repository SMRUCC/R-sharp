﻿#Region "Microsoft.VisualBasic::bc894339a70050841194c60f928e97a4, Library\R.math\math.vb"

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

' Module math
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: create_deSolve_DataFrame, Hist, lm, (+2 Overloads) RK4, ssm
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.Multivariate
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Public Class lmCall

    Public Property lm As IFitted
    Public Property formula As FormulaExpression
    Public Property name As String
    Public Property variables As String()

    Sub New(name As String, variables As String())
        Me.name = name
        Me.variables = variables
    End Sub

    Public Overrides Function ToString() As String
        Return CreateFormulaCall.ToString
    End Function

    Public Function CreateFormulaCall() As Expression
        If TypeOf lm Is MLRFit Then
            Dim poly = DirectCast(lm.Polynomial, MultivariatePolynomial)
            Dim exp As Expression = New Literal(poly.Factors(Scan0))

            For i As Integer = 1 To poly.Factors.Length - 1
                exp = New BinaryExpression(
                    exp,
                    New BinaryExpression(
                        New Literal(poly.Factors(i)),
                        New SymbolReference(variables(i - 1)),
                        "*"),
                    "+")
            Next

            Return exp
        Else
            Dim linear = DirectCast(lm.Polynomial, Polynomial)
            Dim exp As Expression = New Literal(linear.Factors(Scan0))
            Dim singleSymbol As String = variables(Scan0)

            For i As Integer = 1 To linear.Factors.Length - 1
                exp = New BinaryExpression(
                    exp,
                    New BinaryExpression(
                        New Literal(linear.Factors(i)),
                        New BinaryExpression(
                            New SymbolReference(singleSymbol),
                            New Literal(i),
                            "^"),
                        "*"),
                    "+")
            Next

            Return exp
        End If
    End Function
End Class
