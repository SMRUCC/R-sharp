﻿Imports System.IO
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

Namespace System.Package.File.Expressions

    Public Class RSymbolIndex : Inherits RExpression

        Public Sub New(context As Writer)
            MyBase.New(context)
        End Sub

        Public Overrides Sub WriteBuffer(ms As MemoryStream, x As Expression)
            Call WriteBuffer(ms, DirectCast(x, SymbolIndexer))
        End Sub

        Public Overloads Sub WriteBuffer(ms As MemoryStream, x As SymbolIndexer)
            Using outfile As New BinaryWriter(ms)
                Call outfile.Write(CInt(ExpressionTypes.SymbolIndex))
                Call outfile.Write(0)
                Call outfile.Write(CByte(x.type))

                Call outfile.Write(x.indexType)
                Call outfile.Write(context.GetBuffer(x.symbol))
                Call outfile.Write(context.GetBuffer(x.index))

                Call outfile.Flush()
                Call saveSize(outfile)
            End Using
        End Sub

        Public Overrides Function GetExpression(buffer As MemoryStream, type As ExpressionTypes, desc As DESCRIPTION) As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace