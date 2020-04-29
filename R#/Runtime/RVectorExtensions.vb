﻿#Region "Microsoft.VisualBasic::9e6f626efa30586f027fbcc4f46f8195, R#\Runtime\RVectorExtensions.vb"

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

    '     Module RVectorExtensions
    ' 
    '         Function: [single], (+2 Overloads) asVector, createArray, CTypeOfList, fromArray
    '                   getFirst, isVector
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts

Namespace Runtime

    <HideModuleName> Public Module RVectorExtensions

        ''' <summary>
        ''' Object ``x`` is an array of <typeparamref name="T"/>?
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function isVector(Of T)(x As Object) As Boolean
            If x Is Nothing Then
                Return False
            ElseIf Not x.GetType.IsArray Then
                If x.GetType Is GetType(T) Then
                    Return True
                Else
                    Return False
                End If
            Else
                Dim type As Type = x.GetType

                If type Is GetType(T()) OrElse type.ImplementInterface(GetType(IEnumerable(Of T))) Then
                    Return True
                Else
                    Return DirectCast(x, Array).GetValue(Scan0).GetType Is GetType(T)
                End If
            End If
        End Function

        ''' <summary>
        ''' Get first element in the input <paramref name="value"/> sequence
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function getFirst(value As Object, Optional nonNULL As Boolean = False) As Object
            Dim valueType As Type

            If value Is Nothing Then
                Return Nothing
            Else
                If value.GetType Is GetType(vector) Then
                    value = DirectCast(value, vector).data
                End If

                valueType = value.GetType
            End If

            If valueType.IsArray Then
                With DirectCast(value, Array)
                    If .Length = 0 Then
                        Return Nothing
                    ElseIf nonNULL Then
                        For i As Integer = 0 To .Length - 1
                            If Not .GetValue(i) Is Nothing Then
                                Return .GetValue(i)
                            End If
                        Next

                        Return Nothing
                    Else
                        Return .GetValue(Scan0)
                    End If
                End With
            Else
                Return value
            End If
        End Function

        ''' <summary>
        ''' Try get a single element
        ''' </summary>
        ''' <param name="x">
        ''' If the input object x is an array with just one element, 
        ''' then the single value will be populate, otherwise will 
        ''' populate the input x
        ''' </param>
        ''' <returns></returns>
        Public Function [single](x As Object) As Object
            If Not x Is Nothing Then
                If x.GetType.IsArray AndAlso DirectCast(x, Array).Length = 1 Then
                    Return DirectCast(x, Array).GetValue(Scan0)
                End If
            End If

            Return x
        End Function

        ''' <summary>
        ''' Ensure that the input <paramref name="value"/> object is a sequence. 
        ''' (This method will decouple the object instance value from vbObject 
        ''' container unless the required <paramref name="type"/> is 
        ''' <see cref="vbObject"/>.)
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Function asVector(value As Object, type As Type, env As Environment) As Array
            Dim arrayType As Type = type.MakeArrayType
            Dim valueType As Type

            If value Is Nothing Then
                Return Nothing
            Else
                If value.GetType Is GetType(vector) Then
                    value = DirectCast(value, vector).data
                End If

                valueType = value.GetType
            End If

            'If valueType Is GetType(list) Then
            '    ' list value as vector?
            'End If

            If Not valueType Is arrayType Then
                If valueType.IsArray Then
                    Return type.createArray(value, env)
                ElseIf valueType Is GetType(Group) Then
                    Return type.createArray(DirectCast(value, Group).group, env)
                Else
                    Dim array As Array = Array.CreateInstance(type, 1)
                    array.SetValue(RCType.CTypeDynamic(value, type, env), Scan0)
                    Return array
                End If
            Else
                Return value
            End If
        End Function

        <Extension>
        Private Function createArray(type As Type, value As Object, env As Environment) As Object
            Dim src As Array = value
            Dim array As Array = Array.CreateInstance(type, src.Length)

            For i As Integer = 0 To array.Length - 1
                array.SetValue(RCType.CTypeDynamic(src.GetValue(i), type, env), i)
            Next

            Return array
        End Function

        Public Function CTypeOfList(Of T)(list As IDictionary, env As Environment) As Dictionary(Of String, T)
            Dim ofList As New Dictionary(Of String, T)
            Dim elementType As Type = GetType(T)

            For Each key As Object In list.Keys
                ofList(Scripting.ToString(key)) = RCType.CTypeDynamic(list.Item(key), elementType, env)
            Next

            Return ofList
        End Function

        ''' <summary>
        ''' 这个函数会确保返回的输出值都是一个数组
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function asVector(Of T)(value As Object) As Array
            Dim valueType As Type
            Dim typeofT As Type = GetType(T)

            If value Is Nothing Then
                Return New T() {}
            Else
                If value.GetType Is GetType(vector) Then
                    value = DirectCast(value, vector).data
                End If

                valueType = value.GetType
            End If

            If valueType Is typeofT Then
                Return {DirectCast(value, T)}
            ElseIf valueType Is GetType(T()) Then
                Return DirectCast(value, T())
            ElseIf valueType.IsArray Then
                Return typeofT.fromArray(Of T)(value)
            ElseIf valueType Is GetType(Group) Then
                Return typeofT.fromArray(Of T)(DirectCast(value, Group).group)
            ElseIf valueType.IsInheritsFrom(GetType(IEnumerable(Of T))) Then
                Return DirectCast(value, IEnumerable(Of T)).ToArray
            Else
                If typeofT Is GetType(Object) Then
                    Return {DirectCast(value, T)}
                ElseIf typeofT Is GetType(Boolean) AndAlso valueType Is GetType(String) Then
                    Return {DirectCast(value, String).ParseBoolean}
                Else
                    Return {Conversion.CTypeDynamic(Of T)(value)}
                End If
            End If
        End Function

        <Extension>
        Private Function fromArray(Of T)(typeofT As Type, value As Object) As Object
            If DirectCast(value, Array) _
                .AsObjectEnumerator _
                .All(Function(i)
                         If Not i.GetType.IsInheritsFrom(GetType(Array)) Then
                             Return True
                         Else
                             Return DirectCast(i, Array).Length = 1
                         End If
                     End Function) Then

                value = DirectCast(value, Array) _
                    .AsObjectEnumerator _
                    .Select(Function(o)
                                If Not o.GetType Is typeofT Then
                                    If o.GetType.IsInheritsFrom(GetType(Array)) Then
                                        o = DirectCast(o, Array).GetValue(Scan0)
                                    End If
                                End If
                                If Not o.GetType Is typeofT Then
                                    ' 进行一些类型转换

                                    ' if apply the RConversion.CTypeDynamic
                                    ' then it may decouple object from vbObject container
                                    o = Conversion.CTypeDynamic(o, typeofT)
                                End If

                                Return DirectCast(o, T)
                            End Function) _
                    .ToArray
            End If

            Return value
        End Function
    End Module
End Namespace
