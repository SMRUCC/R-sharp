﻿Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace System.Package.File.Expressions

    Public Class RFunction : Inherits RExpression
        Implements INamedValue

        Public Property name As String Implements INamedValue.Key
        Public Property parameters As RSymbol()
        Public Property body As RExpression()
        Public Property sourceMap As StackFrame

        Public Overrides Function GetExpression(desc As DESCRIPTION) As Expression
            Dim params As DeclareNewSymbol() = parameters.Select(Function(v) v.GetExpression(desc)).ToArray
            Dim closure As New ClosureExpression(body.Select(Function(exec) exec.GetExpression(desc)).ToArray)
            Dim stackframe As StackFrame = sourceMap

            sourceMap.Method.Namespace = desc.Package

            Return New DeclareNewFunction(name, params, closure, stackframe) With {
                .[Namespace] = desc.Package
            }
        End Function

        Public Shared Function FromSymbol(symbol As DeclareNewFunction) As RExpression
            Dim name As String = symbol.funcName
            Dim params As RExpression() = symbol.params _
                .Select(AddressOf RSymbol.GetSymbol) _
                .ToArray
            Dim bodyLines As New List(Of RExpression)

            For Each item As RExpression In params
                If TypeOf item Is ParserError Then
                    Return item
                End If
            Next

            For Each exec As Expression In symbol.body.EnumerateCodeLines
                Dim line As RExpression = RExpression.CreateFromSymbolExpression(exec)

                If TypeOf line Is ParserError Then
                    Return line
                Else
                    bodyLines.Add(line)
                End If
            Next

            Return New RFunction With {
                .name = name,
                .parameters = params _
                    .Select(Function(a) DirectCast(a, RSymbol)) _
                    .ToArray,
                .sourceMap = symbol.stackFrame,
                .body = bodyLines.ToArray
            }
        End Function
    End Class
End Namespace