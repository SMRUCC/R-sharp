﻿Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' from ... select ...
    ''' </summary>
    Public Class ProjectionExpression : Inherits QueryExpression

        Dim opt As Options
        Dim project As OutputProjection

        Public Overrides ReadOnly Property name As String
            Get
                Return "from ... [select ...]"
            End Get
        End Property

        Sub New(symbol As SymbolDeclare, sequence As Expression, exec As IEnumerable(Of Expression), proj As OutputProjection, opt As Options)
            Call MyBase.New(symbol, sequence, exec)

            Me.opt = opt
            Me.project = proj
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns>
        ''' array of <see cref="JavaScriptObject"/>
        ''' </returns>
        Public Overrides Function Exec(context As ExecutableContext) As Object
            Dim projections As New List(Of JavaScriptObject)
            Dim closure As New ExecutableContext(New Environment(context, context.stackFrame, isInherits:=False))
            Dim skipVal As Boolean
            Dim dataset As DataSet = GetDataSet(context)

            Call closure.AddSymbol(symbol.symbol, TypeCodes.generic)

            For Each item As Object In dataset.PopulatesData()
                closure.SetSymbol(symbol.symbol, item)

                For Each line As Expression In executeQueue
                    If TypeOf line Is WhereFilter Then
                        skipVal = Not DirectCast(line.Exec(closure), Boolean)

                        If skipVal Then
                            Exit For
                        End If
                    End If
                Next

                If Not skipVal Then
                    projections.Add(project.Exec(closure))
                End If
            Next

            Return opt.RunOptionPipeline(projections, context).ToArray
        End Function
    End Class
End Namespace