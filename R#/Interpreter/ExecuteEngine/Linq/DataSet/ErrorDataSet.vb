﻿Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class ErrorDataSet : Inherits DataSet

        Public Property message As Message

        Friend Overrides Iterator Function PopulatesData() As IEnumerable(Of Object)
        End Function
    End Class
End Namespace