﻿Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class QuerySource

        Protected Friend ReadOnly sequence As Expression
        Protected Friend ReadOnly symbol As SymbolDeclare

        Sub New(symbol As SymbolDeclare, sequence As Expression)
            Me.symbol = symbol
            Me.sequence = sequence
        End Sub

        Private Function getSymbolName(varX As Expression) As String
            Dim name As String = varX.ToString

            If TypeOf varX Is Literal Then
                name = any.ToString(DirectCast(varX, Literal).value)
            ElseIf TypeOf varX Is SymbolReference Then
                name = DirectCast(varX, SymbolReference).symbolName
            End If

            Return name
        End Function

        Public Iterator Function EnumerateFields(addSymbol As Boolean) As IEnumerable(Of NamedValue(Of Expression))
            If symbol.isTuple Then
                Dim sourceSymbol As String = getSymbolName(sequence)

                For Each varX In DirectCast(symbol.symbol, VectorLiteral).elements
                    Dim name As String = getSymbolName(varX)

                    If addSymbol Then
                        name = $"{sourceSymbol}.{name}"
                    End If

                    Yield New NamedValue(Of Expression)(name, varX)
                Next
            ElseIf addSymbol Then
                Yield New NamedValue(Of Expression)(any.ToString(symbol.symbol), symbol.symbol)
            Else
                Yield New NamedValue(Of Expression)("*", symbol.symbol)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"FROM {symbol} IN {sequence}"
        End Function
    End Class
End Namespace