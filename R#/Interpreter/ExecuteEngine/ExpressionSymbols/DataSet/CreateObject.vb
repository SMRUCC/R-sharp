﻿#Region "Microsoft.VisualBasic::b633137b0ca442216ef2e3c8660ad029, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\CreateObject.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class CreateObject
    ' 
    '         Properties: constructor, name, stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' ``new xxx(...)``
    ''' </summary>
    Public Class CreateObject : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        Public ReadOnly Property name As String
        Public ReadOnly Property constructor As Expression()
        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Sub New(name$, constructor As Expression(), stackframe As StackFrame)
            Me.name = name
            Me.stackFrame = stackframe
            Me.constructor = constructor
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim type As RType = envir.globalEnvironment.types.TryGetValue(name)
            Dim obj As vbObject

            If type Is Nothing Then
                Return Internal.debug.stop({"missing required type information...", "type: " & name}, envir)
            Else
                obj = vbObject.CreateInstance(type.raw)
            End If

            Dim err As New Value(Of Object)

            ' initialize the property
            For Each prop As Expression In constructor
                If Not TypeOf prop Is ValueAssign Then
                    Return Internal.debug.stop({
                         $"invalid expression: {prop} !",
                         $"require: " & GetType(ValueAssign).Name,
                         $"but given: " & prop.expressionName
                    }, envir)
                Else
                    With DirectCast(prop, ValueAssign)
                        Dim name = .targetSymbols(Scan0).Evaluate(envir)
                        Dim value = .value.Evaluate(envir)

                        If TypeOf (err = obj.setByName(name, value, envir)) Is Message Then
                            Return err.Value
                        End If
                    End With
                End If
            Next

            Return obj
        End Function

        Public Overrides Function ToString() As String
            Return $"new {name}({constructor.JoinBy(", ")})"
        End Function
    End Class
End Namespace