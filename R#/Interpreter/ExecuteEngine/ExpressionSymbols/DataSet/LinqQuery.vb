﻿Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.LINQ
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    Public Class LinqQuery : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.LinqQuery
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Dim LINQ As QueryExpression

        Sub New(query As QueryExpression, stackFrame As StackFrame)
            Me.LINQ = query
            Me.stackFrame = stackFrame
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim result As Object = LINQ.Exec(New ExecutableContext(envir))

            If TypeOf result Is Message Then
                Return result
            End If

            If TypeOf LINQ Is ProjectionExpression Then
                If TypeOf LINQ.dataset Is DataFrameDataSet Then
                    ' returns a new dataframe
                    Return newDataFrame(DirectCast(result, JavaScriptObject()), DirectCast(LINQ, ProjectionExpression).project.fields.Keys)
                Else
                    ' returns a new sequence
                    Return result
                End If
            Else
                ' scalar
                ' returns directly
                Return result
            End If
        End Function

        Private Shared Function newDataFrame(js As JavaScriptObject(), project As String()) As dataframe
            Dim table As New dataframe With {
                .columns = New Dictionary(Of String, Array)
            }

            For Each name As String In project
                table.columns.Add(name, Array.CreateInstance(GetType(Object), js.Length))
            Next

            If js.Length = 0 Then
                Return table
            End If

            For Each name As String In project
                Dim vec As Array = table.columns(name)

                For i As Integer = 0 To vec.Length - 1
                    vec.SetValue(js(i)(name), i)
                Next
            Next

            Return table
        End Function

        Friend Shared Function produceSequenceVector(sequence As Object, ByRef isList As Boolean) As Object
            If sequence.GetType Is GetType(list) Then
                sequence = DirectCast(sequence, list).slots
            End If

            If sequence.GetType Is GetType(Dictionary(Of String, Object)) Then
                sequence = DirectCast(sequence, Dictionary(Of String, Object)).ToArray
                isList = True
            ElseIf sequence.GetType.ImplementInterface(GetType(IDictionary)) Then
                With DirectCast(sequence, IDictionary)
                    sequence = (From key In .Keys.AsQueryable
                                Let keyStr As String = any.ToString(key)
                                Let keyVal As Object = .Item(key)
                                Select New KeyValuePair(Of String, Object)(keyStr, keyVal)).ToArray
                    isList = True
                End With
            Else
                sequence = Runtime.asVector(Of Object)(sequence)
            End If

            Return sequence
        End Function
    End Class
End Namespace