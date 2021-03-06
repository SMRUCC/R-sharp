﻿#Region "Microsoft.VisualBasic::d253274ae17423a14b3665c50f701f53, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\ValueAssignOperator\ValueAssignExpression.vb"

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

    '     Class ValueAssignExpression
    ' 
    '         Properties: expressionName, symbolSize, targetSymbols, type, value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: assignSymbol, assignTuples, doValueAssign, DoValueAssign, Evaluate
    '                   GetSymbol, setByNameIndex, setFromObjectList, setFromVector, setVectorElements
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Development.Package.File
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    ''' <summary>
    ''' Set variable or tuple
    ''' </summary>
    Public Class ValueAssignExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return value.type
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.SymbolAssign
            End Get
        End Property

        ''' <summary>
        ''' 可能是对tuple做赋值
        ''' 所以应该是多个变量名称
        ''' </summary>
        Public ReadOnly Property targetSymbols As Expression()
        Public ReadOnly Property value As Expression

        Friend isByRef As Boolean

        Public ReadOnly Property symbolSize As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return targetSymbols.Length
            End Get
        End Property

        Sub New(targetSymbols$(), value As Expression)
            Me.targetSymbols = targetSymbols _
                .Select(Function(name) New Literal(name)) _
                .ToArray
            Me.value = value
        End Sub

        Sub New(target As Expression(), value As Expression)
            Me.targetSymbols = target
            Me.value = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return DoValueAssign(envir, value.Evaluate(envir))
        End Function

        Public Function DoValueAssign(envir As Environment, value As Object) As Object
            If Not value Is Nothing AndAlso value.GetType Is GetType(IfPromise) Then
                ' 如果是if分支返回的结果，则将if分支的赋值对象设置为
                ' 当前的赋值操作的目标对象符号
                DirectCast(value, IfPromise).assignTo = Me
                Return value
            Else
                If Not value Is Nothing AndAlso TypeOf value Is invisible Then
                    ' value assign will break the invisible
                    value = DirectCast(value, invisible).value
                End If

                Return doValueAssign(envir, targetSymbols, isByRef, value)
            End If
        End Function

        Friend Shared Function doValueAssign(envir As Environment, targetSymbols As Expression(), isByRef As Boolean, value As Object) As Object
            Dim message As Message

            If targetSymbols.Length = 1 Then
                message = assignSymbol(envir, targetSymbols(Scan0), isByRef, value)
            Else
                ' assign tuples
                message = assignTuples(envir, targetSymbols, isByRef, value)
            End If

            If message Is Nothing Then
                Return value
            Else
                Return message
            End If
        End Function

        Public Overrides Function ToString() As String
            If symbolSize = 1 Then
                Return $"{targetSymbols(0)} <- {value.ToString}"
            Else
                Return $"[{targetSymbols.JoinBy(", ")}] <- {value.ToString}"
            End If
        End Function

        Private Shared Function setFromVector(envir As Environment, targetSymbols As Expression(), isByRef As Boolean, value As Object) As Message
            Dim message As New Value(Of Message)
            Dim array As Array

            If value.GetType Is GetType(vector) Then
                array = DirectCast(value, vector).data
            Else
                array = DirectCast(value, Array)
            End If

            If array.Length = 1 Then
                ' all assign the same value result
                For Each name As Expression In targetSymbols
                    If Not (message = assignSymbol(envir, name, isByRef, value)) Is Nothing Then
                        Return message.Value
                    End If
                Next
            ElseIf array.Length = targetSymbols.Length Then
                ' one by one
                For i As Integer = 0 To array.Length - 1
                    If Not (message = assignSymbol(envir, targetSymbols(i), isByRef, array.GetValue(i))) Is Nothing Then
                        Return message.Value
                    End If
                Next
            Else
                ' 数量不对
                Return Internal.debug.stop(New InvalidCastException, envir)
            End If

            Return Nothing
        End Function

        Private Shared Function setFromObjectList(envir As Environment, targetSymbols As Expression(), isByRef As Boolean, value As Object) As Message
            Dim list As list = DirectCast(value, list)
            Dim message As New Value(Of Message)

            If list.length = 1 Then
                ' 设置tuple的值的时候
                ' list必须要有相同的元素数量
                Return Internal.debug.stop("Number of list element is not identical to the tuple elements...", envir)
            ElseIf list.length = targetSymbols.Length Then
                Dim name$

                ' one by one
                For i As Integer = 0 To list.length - 1
                    name = GetSymbol(targetSymbols(i))

                    If list.slots.ContainsKey(name) Then
                        value = list.slots(name)
                    Else
                        ' R中的元素下标都是从1开始的
                        value = list.slots($"{i + 1}")
                    End If

                    If Not (message = assignSymbol(envir, targetSymbols(i), isByRef, value)) Is Nothing Then
                        Return message.Value
                    End If
                Next
            Else
                ' 数量不对
                Return Internal.debug.stop(New InvalidCastException, envir)
            End If

            Return Nothing
        End Function

        Private Shared Function assignTuples(envir As Environment, targetSymbols As Expression(), isByRef As Boolean, value As Object) As Message
            Dim message As New Value(Of Message)
            Dim type As Type

            If value Is Nothing Then
                ' all assign the same value result
                ' literal NULL
                For Each name As Expression In targetSymbols
                    If Not (message = assignSymbol(envir, name, isByRef, value)) Is Nothing Then
                        Return message.Value
                    End If
                Next

                Return Nothing
            Else
                type = value.GetType
            End If

            If type.IsInheritsFrom(GetType(Array)) OrElse type Is GetType(vector) Then
                Return setFromVector(envir, targetSymbols, isByRef, value)

            ElseIf type Is GetType(list) Then
                Return setFromObjectList(envir, targetSymbols, isByRef, value)

            Else
                Return Internal.debug.stop(New NotImplementedException, envir)
            End If

            Return Nothing
        End Function

        Friend Shared Function GetSymbol(symbolName As Expression) As String
            Select Case symbolName.GetType
                Case GetType(Literal)
                    Return CStr(DirectCast(symbolName, Literal).value)
                Case GetType(SymbolReference)
                    Return DirectCast(symbolName, SymbolReference).symbol
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Shared Function setByNameIndex(symbolName As Expression, envir As Environment, value As Object) As Message
            Dim symbolIndex As SymbolIndexer = DirectCast(symbolName, SymbolIndexer)
            Dim targetObj As Object = symbolIndex.symbol.Evaluate(envir)
            Dim index As Object = symbolIndex.index.Evaluate(envir)

            If Program.isException(index) Then
                Return index
            ElseIf True = CBool(base.isEmpty(index)) Then
                Return SymbolIndexer.emptyIndexError(symbolIndex, envir)
            End If

            If targetObj Is Nothing Then
                Return Internal.debug.stop({"Target symbol is nothing!", $"SymbolName: {symbolIndex.symbol}"}, envir)
            ElseIf TypeOf targetObj Is Message Then
                Return targetObj
            End If

            If symbolIndex.indexType = SymbolIndexers.vectorIndex AndAlso index.GetType Like RType.integers Then
                Return setVectorElements(targetObj, DirectCast(Runtime.asVector(Of Integer)(index), Integer()), value, envir)
            ElseIf symbolIndex.indexType = SymbolIndexers.vectorIndex AndAlso index.GetType Like RType.logicals Then
                Dim flags As Boolean() = DirectCast(Runtime.asVector(Of Boolean)(index), Boolean())
                Dim indexVals As Integer() = flags _
                    .Select(Function(b, i) (b, i + 1)) _
                    .Where(Function(flag) flag.b) _
                    .Select(Function(flag) flag.Item2) _
                    .ToArray

                Return setVectorElements(targetObj, indexVals, value, envir)
            End If

            Dim indexStr As String() = DirectCast(Runtime.asVector(Of String)(index), String())
            Dim result As Object

            If Not targetObj.GetType.ImplementInterface(GetType(RNameIndex)) Then
                If targetObj.GetType Is GetType(dataframe) Then
                    Return dataframeValueAssign.ValueAssign(symbolIndex, indexStr, DirectCast(targetObj, dataframe), value, envir)
                Else
                    Return Internal.debug.stop({"Target symbol can not be indexed by name!", $"SymbolName: {symbolIndex.symbol}"}, envir)
                End If
            End If

            If indexStr.IsNullOrEmpty Then
                Return SymbolIndexer.emptyIndexError(symbolIndex, envir)
            End If

            If symbolIndex.indexType = SymbolIndexers.nameIndex Then
                ' a[[x]] <- v
                ' a$x <- v
                result = DirectCast(targetObj, RNameIndex).setByName(indexStr(Scan0), value, envir)

                If indexStr.Length > 1 Then
                    envir.AddMessage($"'{symbolIndex.index}' contains multiple index, only use first index key value...")
                End If
            Else
                result = DirectCast(targetObj, RNameIndex).setByName(indexStr, DirectCast(value, Array), envir)
            End If

            If Not result Is Nothing AndAlso result.GetType Is GetType(Message) Then
                Return DirectCast(result, Message)
            Else
                Return Nothing
            End If
        End Function

        Private Shared Function assignSymbol(envir As Environment, symbolName As Expression, isByRef As Boolean, value As Object) As Message
            Dim target As Symbol = Nothing

            Select Case symbolName.GetType
                Case GetType(Literal)
                    Dim symbol As String = any.ToString(DirectCast(symbolName, Literal).value)

                    Select Case symbol
                        Case "NA", "NULL", "TRUE", "FALSE", "NaN"
                            Return Internal.debug.stop({"invalid (do_set) left-hand side to assignment", "constant symbol is not allowed!", "symbol: " & symbol}, envir)
                        Case Else
                            If Not symbol.IsPattern(Scanner.RSymbol) Then
                                Return Internal.debug.stop({"invalid (do_set) left-hand side to assignment", "constant literal is not allowed!", "literal: " & symbol}, envir)
                            End If
                    End Select

                    target = envir.FindSymbol(symbol)
                Case GetType(SymbolReference)
                    target = envir.FindSymbol(DirectCast(symbolName, SymbolReference).symbol)
                Case GetType(SymbolIndexer)
                    Return setByNameIndex(symbolName, envir, value)
                Case Else
                    Return Internal.debug.stop(New InvalidExpressionException, envir)
            End Select

            If target Is Nothing Then
                If TypeOf symbolName Is Literal AndAlso Not envir.globalEnvironment.Rscript.strict Then
                    envir.Push(DirectCast(symbolName, Literal).value.ToString, value, [readonly]:=False)
                    Return Nothing
                Else
                    Return Message.SymbolNotFound(envir, symbolName.ToString, TypeCodes.generic)
                End If
            End If

            If isByRef Then
                Return target.SetValue(value, envir)
            Else
                If Not value Is Nothing AndAlso value.GetType.IsInheritsFrom(GetType(Array)) Then
                    Return target.SetValue(DirectCast(value, Array).Clone, envir)
                Else
                    Return target.SetValue(value, envir)
                End If
            End If
        End Function

        Private Shared Function setVectorElements(ByRef target As Object, index As Integer(), value As Object, env As Environment) As Message
            If index.IsNullOrEmpty Then
                ' vector content no changed
                Return Nothing
            End If

            If target.GetType Is GetType(vector) Then
                target = DirectCast(target, vector).data
            End If

            Dim targetVector As Array = DirectCast(target, Array)
            Dim getValue As Func(Of Object)

            If value Is Nothing Then
                getValue = Function() Nothing
            Else
                Dim valueVec As Array = Runtime.asVector(Of Object)(value)
                Dim i As i32 = Scan0

                If valueVec.Length = 1 Then
                    value = valueVec.GetValue(Scan0)
                    getValue = Function() value
                Else

                    getValue = Function() valueVec.GetValue(++i)
                End If
            End If

            Dim elementType As Type = Runtime.MeasureArrayElementType(targetVector)

            For Each i As Integer In DirectCast(index, Integer())
                ' 动态调整数组的大小
                If targetVector.Length >= i Then
                    targetVector.SetValue(getValue(), i - 1)
                Else
                    Dim newVec As Array = Array.CreateInstance(elementType, i - 1)

                    Array.ConstrainedCopy(targetVector, Scan0, newVec, Scan0, targetVector.Length)
                    newVec.SetValue(getValue(), i - 1)
                    targetVector = newVec
                End If
            Next

            If target.GetType Is GetType(vector) Then
                DirectCast(target, vector).data = targetVector
            Else
                target = targetVector
            End If

            Return Nothing
        End Function
    End Class
End Namespace
