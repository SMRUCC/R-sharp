﻿#Region "Microsoft.VisualBasic::2f6f4634d324be040979ee5acaa4517a, R#\Runtime\Core.vb"

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

    '     Module Core
    ' 
    '         Function: [Module], Add, asLogical, BinaryCoreInternal, Divide
    '                   Minus, Multiply, op_In, Power, UnaryCoreInternal
    '         Class typedefine
    ' 
    '             Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq

Namespace Runtime

    Public Module Core

        ''' <summary>
        ''' Type cache module
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        Public NotInheritable Class typedefine(Of T)

            ''' <summary>
            ''' The vector based type
            ''' </summary>
            Public Shared ReadOnly baseType As Type
            ''' <summary>
            ''' The abstract vector type
            ''' </summary>
            Public Shared ReadOnly enumerable As Type

            Private Sub New()
            End Sub

            Shared Sub New()
                baseType = GetType(T)
                enumerable = GetType(IEnumerable(Of T))
            End Sub
        End Class

        ReadOnly numericTypes As Index(Of Type) = {GetType(Integer), GetType(Long), GetType(Double), GetType(Single)}

        Public Function asLogical(x As Object) As Boolean()
            If x Is Nothing Then
                Return {False}
            End If

            Dim vector As Array = Runtime.asVector(Of Object)(x)
            Dim type As Type = vector.GetValue(Scan0).GetType

            If type Is GetType(Boolean) Then
                Return vector.AsObjectEnumerator _
                    .Select(Function(b) DirectCast(b, Boolean)) _
                    .ToArray
            ElseIf type Like numericTypes Then
                Return vector.AsObjectEnumerator _
                    .Select(Function(num) CBool(num <> 0)) _
                    .ToArray
            ElseIf type Is GetType(String) Then
                Return vector.AsObjectEnumerator _
                    .Select(Function(o)
                                Return Not DirectCast(o, String).StringEmpty
                            End Function) _
                    .ToArray
            Else
                Return vector.AsObjectEnumerator _
                    .Select(Function(o) Not o Is Nothing) _
                    .ToArray
            End If
        End Function

        ''' <summary>
        ''' The ``In`` operator
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="collection"></param>
        ''' <returns></returns>
        Public Function op_In(Of T)(x As Object, collection As IEnumerable(Of T)) As IEnumerable(Of Boolean)

            If x Is Nothing Then
                Return {}
            End If

            Dim type As Type = x.GetType

            With collection.AsList

                If type Is typedefine(Of T).baseType Then

                    ' Just one element in x, using list indexof is faster than using hash table
                    Return { .IndexOf(DirectCast(x, T)) > -1}

                ElseIf type.ImplementInterface(typedefine(Of T).enumerable) Then

                    ' This can be optimised by using hash table if the x and collection are both a large collection. 
                    Dim xVector = DirectCast(x, IEnumerable(Of T)).ToArray

                    If x.Vector.Length > 500 AndAlso .Count > 1000 Then

                        ' Using hash table optimised for large n*m situation          
                        With .AsHashSet()
                            Return xVector _
                                .Select(Function(n) .HasKey(n)) _
                                .ToArray
                        End With
                    Else

                        Return xVector _
                            .Select(Function(n) .IndexOf(n) > -1) _
                            .ToArray

                    End If

                Else
                    Throw New InvalidOperationException(type.FullName)
                End If
            End With
        End Function

        Public ReadOnly op_Add As Func(Of Object, Object, Object) = Function(x, y) x + y
        Public ReadOnly op_Minus As Func(Of Object, Object, Object) = Function(x, y) x - y
        Public ReadOnly op_Multiply As Func(Of Object, Object, Object) = Function(x, y) x * y
        Public ReadOnly op_Divided As Func(Of Object, Object, Object) = Function(x, y) x / y
        Public ReadOnly op_Mod As Func(Of Object, Object, Object) = Function(x, y) x Mod y
        Public ReadOnly op_Power As Func(Of Object, Object, Object) = Function(x, y) CDbl(x) ^ CDbl(y)

        ''' <summary>
        ''' Generic unary operator core for primitive type.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="[do]"></param>
        ''' <returns></returns>
        Public Function UnaryCoreInternal(Of T As IComparable(Of T), TOut)(x As Object, [do] As Func(Of Object, Object)) As IEnumerable(Of TOut)
            Dim type As Type = x.GetType

            If type Is typedefine(Of T).baseType Then
                Return {DirectCast([do](x), TOut)}
            ElseIf type.ImplementInterface(typedefine(Of T).enumerable) Then
                Return DirectCast(x, IEnumerable(Of T)) _
                    .Select(Function(o) DirectCast([do](o), TOut)) _
                    .ToArray
            Else
                Throw New InvalidCastException(type.FullName)
            End If
        End Function

        ''' <summary>
        ''' [Vector core] Generic binary operator core for numeric type.
        ''' </summary>
        ''' <typeparam name="TX"></typeparam>
        ''' <typeparam name="TY"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="[do]"></param>
        ''' <returns></returns>
        Public Function BinaryCoreInternal(Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x As Object, y As Object, [do] As Func(Of Object, Object, Object)) As IEnumerable(Of TOut)
            Dim xtype As Type = x.GetType
            Dim ytype As Type = y.GetType

            If xtype Is typedefine(Of TX).baseType Then

                If ytype Is typedefine(Of TY).baseType Then
                    Return {DirectCast([do](x, y), TOut)}

                ElseIf ytype.ImplementInterface(typedefine(Of TY).enumerable) Then

                    Return DirectCast(y, IEnumerable(Of TY)) _
                        .Select(Function(yi) DirectCast([do](x, yi), TOut)) _
                        .ToArray

                Else
                    Throw New InvalidCastException(ytype.FullName)
                End If

            ElseIf xtype.ImplementInterface(typedefine(Of TX).enumerable) Then

                If ytype Is typedefine(Of TY).baseType Then
                    Return DirectCast(x, IEnumerable(Of TX)) _
                        .Select(Function(xi) DirectCast([do](xi, y), TOut)) _
                        .ToArray

                ElseIf ytype.ImplementInterface(typedefine(Of TY).enumerable) Then

                    Dim xlist = DirectCast(x, IEnumerable(Of TX)).ToArray
                    Dim ylist = DirectCast(y, IEnumerable(Of TY)).ToArray

                    If xlist.Length = 1 Then
                        x = xlist(0)
                        Return DirectCast(y, IEnumerable(Of TY)) _
                            .Select(Function(yi) DirectCast([do](x, yi), TOut)) _
                            .ToArray
                    ElseIf ylist.Length = 1 Then
                        y = ylist(0)
                        Return DirectCast(x, IEnumerable(Of TX)) _
                            .Select(Function(xi) DirectCast([do](xi, y), TOut)) _
                            .ToArray
                    ElseIf xlist.Length = ylist.Length Then
                        Return xlist _
                            .SeqIterator _
                            .Select(Function(xi)
                                        Return DirectCast([do](CObj(xi.value), CObj(ylist(xi))), TOut)
                                    End Function) _
                            .ToArray
                    Else
                        Throw New InvalidOperationException("Vector length between the X and Y should be equals!")
                    End If
                Else
                    Throw New InvalidCastException(ytype.FullName)
                End If

            Else
                Throw New InvalidCastException(xtype.FullName)
            End If
        End Function

        ''' <summary>
        ''' Generic ``+`` operator for numeric type
        ''' </summary>
        ''' <typeparam name="TX"></typeparam>
        ''' <typeparam name="TY"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Add(Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x, y) As IEnumerable(Of TOut)
            Return BinaryCoreInternal(Of TX, TY, TOut)(x, y, Function(a, b) a + b)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Minus(Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x, y) As IEnumerable(Of TOut)
            Return BinaryCoreInternal(Of TX, TY, TOut)(x, y, Function(a, b) a - b)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Multiply(Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x, y) As IEnumerable(Of TOut)
            Return BinaryCoreInternal(Of TX, TY, TOut)(x, y, Function(a, b) a * b)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Divide(Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x, y) As IEnumerable(Of TOut)
            Return BinaryCoreInternal(Of TX, TY, TOut)(x, y, Function(a, b) a / b)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Power(Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x, y) As IEnumerable(Of TOut)
            Return BinaryCoreInternal(Of TX, TY, TOut)(x, y, Function(a, b) a ^ b)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [Module](Of TX As IComparable(Of TX), TY As IComparable(Of TY), TOut)(x, y) As IEnumerable(Of TOut)
            Return BinaryCoreInternal(Of TX, TY, TOut)(x, y, Function(a, b) a Mod b)
        End Function
    End Module
End Namespace
