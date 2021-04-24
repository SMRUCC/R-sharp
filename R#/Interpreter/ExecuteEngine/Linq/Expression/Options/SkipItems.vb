﻿Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class SkipItems : Inherits PipelineKeyword

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "Skip"
            End Get
        End Property

        ''' <summary>
        ''' this expression should produce an integer value
        ''' </summary>
        Dim n As Expression

        Sub New(n As Expression)
            Me.n = n
        End Sub

        Public Overrides Function Exec(result As IEnumerable(Of JavaScriptObject), context As ExecutableContext) As IEnumerable(Of JavaScriptObject)
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace