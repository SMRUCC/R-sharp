﻿Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' the linq expression of a specific keyword 
    ''' </summary>
    Public MustInherit Class LinqKeywordExpression : Inherits Expression

        Public MustOverride ReadOnly Property keyword As String

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.LinqDeclare
            End Get
        End Property
    End Class
End Namespace