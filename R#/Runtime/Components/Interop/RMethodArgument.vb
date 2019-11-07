﻿Imports System.Reflection

Namespace Runtime.Components

    Public Class RMethodArgument

        Public Property name As String
        Public Property type As RType
        Public Property [default] As Object
        Public Property isOptional As Boolean
        Public Property isObjectList As Boolean

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {type}"
        End Function

        Public Shared Function ParseArgument(p As ParameterInfo) As RMethodArgument
            Return New RMethodArgument With {
                .name = p.Name,
                .type = New RType(p.ParameterType),
                .[default] = p.DefaultValue,
                .isOptional = p.HasDefaultValue,
                .isObjectList = Not p.GetCustomAttribute(Of RListObjectArgumentAttribute) Is Nothing
            }
        End Function
    End Class
End Namespace