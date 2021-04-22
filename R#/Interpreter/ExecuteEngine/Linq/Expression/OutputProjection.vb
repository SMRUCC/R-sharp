﻿Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' data projection: ``SELECT &lt;fields>``
    ''' </summary>
    Public Class OutputProjection : Inherits LinqKeywordExpression

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "SELECT"
            End Get
        End Property

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace