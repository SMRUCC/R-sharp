﻿#Region "Microsoft.VisualBasic::94ce193e68dcc6c9421c930177deab3e, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\BinaryInExpression.vb"

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

    '     Class BinaryInExpression
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, findTest, getIndex, testListIndex, testVectorIndexOf
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    ''' <summary>
    ''' + 如果右边参数为序列，则是进行对值的indexOf操作
    ''' + 如果右边参数为列表，则是对key进行查找操作
    ''' </summary>
    Public Class BinaryInExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.boolean
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Binary
            End Get
        End Property

        ''' <summary>
        ''' left
        ''' </summary>
        Friend a As Expression
        ''' <summary>
        ''' right
        ''' </summary>
        Friend b As Expression

        Sub New(a As Expression, b As Expression)
            Me.a = a
            Me.b = b
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim sequence As Object = b.Evaluate(envir)
            Dim testLeft As Object() = getIndex(a.Evaluate(envir))

            If sequence Is Nothing Then
                Return {}
            ElseIf RProgram.isException(sequence) Then
                Return sequence
            End If

            Dim flags As Boolean()

            If TypeOf sequence Is list AndAlso REnv.MeasureRealElementType(testLeft) Is GetType(String) Then
                flags = testListIndex(sequence, REnv.TryCastGenericArray(testLeft, env:=envir))
            Else
                flags = testVectorIndexOf(getIndex(sequence).Indexing, testLeft)
            End If

            Return flags
        End Function

        Private Shared Function testListIndex(sequence As list, testLeft As String()) As Boolean()
            Return testLeft.Select(Function(a) sequence.slots.ContainsKey(a)).ToArray
        End Function

        Private Shared Function testVectorIndexOf(index As Index(Of Object), testLeft As Object()) As Boolean()
            Dim rawIndexObjects As Object() = index.Objects
            Dim isComparable As Boolean = rawIndexObjects.All(Function(a) a.GetType.ImplementInterface(GetType(IComparable)))
            Dim findTest As Boolean() = testLeft _
                .Select(Function(x)
                            Return BinaryInExpression.findTest(x, isComparable, index, rawIndexObjects)
                        End Function) _
                .ToArray

            Return findTest
        End Function

        Private Shared Function findTest(x As Object, isComparable As Boolean, index As Index(Of Object), rawIndexObjects As Object()) As Boolean
            If x Like index Then
                Return True
            ElseIf isComparable AndAlso x.GetType.ImplementInterface(GetType(IComparable)) Then
                For Each y As Object In rawIndexObjects
                    Dim test = BinaryBetweenExpression.compareOf(x, y)

                    If test Like GetType(Exception) Then
                        ' can not compare between different type!
                        ' ignore
                    ElseIf test.TryCast(Of Integer) = 0 Then
                        Return True
                    End If
                Next

                Return False
            Else
                Return False
            End If
        End Function

        Private Shared Function getIndex(src As Object) As Object()
            Dim isList As Boolean = False
            Dim seq = LinqQuery.produceSequenceVector(src, isList)

            If isList Then
                Return DirectCast(seq, KeyValuePair(Of String, Object)()) _
                    .Select(Function(t) t.Value) _
                    .ToArray
            Else
                Return DirectCast(seq, Object())
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"({a} %in% index<{b}>)"
        End Function
    End Class
End Namespace
