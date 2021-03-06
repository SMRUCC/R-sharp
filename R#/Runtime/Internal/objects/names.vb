﻿#Region "Microsoft.VisualBasic::37c9c7495ad4fea313af7c2ce5f603c4, R#\Runtime\Internal\objects\names.vb"

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

    '     Module names
    ' 
    '         Function: checkChar, getColNames, getNames, getRowNames, makeNames
    '                   setColNames, setNames, setRowNames, uniqueNames
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline

Namespace Runtime.Internal.Object

    Public Module names

        <Extension>
        Private Function checkChar(c As Char, allow_ As Boolean) As Char
            If c = "_"c AndAlso allow_ Then
                Return c
            End If

            If c >= "a"c AndAlso c <= "z"c Then
                Return c
            ElseIf c >= "A"c AndAlso c <= "Z"c Then
                Return c
            ElseIf c >= "0"c AndAlso c <= "9"c Then
                Return c
            Else
                Return "."c
            End If
        End Function

        <Extension>
        Public Function makeNames(nameList As IEnumerable(Of String), Optional unique As Boolean = False, Optional allow_ As Boolean = True) As String()
            Dim nameAll As New List(Of String)

            For Each name As String In nameList
                If name Is Nothing Then
                    name = "X"
                End If

                name = name _
                    .Select(Function(c) c.checkChar(allow_)) _
                    .CharString

                Call nameAll.Add(name)
            Next

            If unique Then
                Return nameAll.uniqueNames
            Else
                Return nameAll.ToArray
            End If
        End Function

        <Extension>
        Public Function uniqueNames(names As IEnumerable(Of String)) As String()
            Dim nameUniques As New Dictionary(Of String, Counter)

            For Each name As String In names
RE0:
                If nameUniques.ContainsKey(name) Then
                    nameUniques(name).Hit()
                    name = name & "_" & nameUniques(name).Value
                    GoTo RE0
                Else
                    nameUniques.Add(name, Scan0)
                End If
            Next

            Return nameUniques.Keys.ToArray
        End Function

        Public Function getNames([object] As Object, envir As Environment) As Object
            Dim type As Type = [object].GetType

            ' get names
            Select Case type
                Case GetType(list), GetType(dataframe)
                    Return DirectCast([object], RNames).getNames
                Case GetType(vbObject)
                    Return DirectCast([object], vbObject).getNames
                Case GetType(vector)
                    Return DirectCast([object], vector).getNames
                Case Else
                    If type.IsArray Then
                        Dim objVec As Array = Runtime.asVector(Of Object)([object])

                        If objVec _
                            .AsObjectEnumerator _
                            .All(Function(o)
                                     Return o.GetType Is GetType(Group)
                                 End Function) Then

                            Return objVec.AsObjectEnumerator _
                                .Select(Function(g)
                                            Return Scripting.ToString(DirectCast(g, Group).key, "NULL")
                                        End Function) _
                                .ToArray
                        ElseIf objVec _
                            .AsObjectEnumerator _
                            .All(Function(o)
                                     Return o.GetType.ImplementInterface(GetType(INamedValue))
                                 End Function) Then

                            Return objVec.AsObjectEnumerator _
                                .Select(Function(o)
                                            Return DirectCast(o, INamedValue).Key
                                        End Function) _
                                .ToArray
                        End If
                    ElseIf type.ImplementInterface(GetType(IDictionary)) Then
                        Dim keys As vector = DirectCast([object], IDictionary) _
                            .Keys _
                            .DoCall(Function(a)
                                        Return New vector(GetType(String), a, env:=envir)
                                    End Function)

                        Return keys
                    End If

                    Return Internal.debug.stop({"unsupported!", "func: names", "type: " & type.FullName}, envir)
            End Select
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="[object]"></param>
        ''' <param name="namelist">This method will ensure that the value is a string vector</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Function setNames([object] As Object, namelist As Array, env As Environment) As Object
            namelist = Runtime.asVector(Of String)(namelist)

            ' set names
            Select Case [object].GetType
                Case GetType(list), GetType(dataframe)
                    Return DirectCast([object], RNames).setNames(namelist, env)
                Case GetType(vector)
                    Return DirectCast([object], vector).setNames(namelist, env)
                Case Else
                    Return Internal.debug.stop({
                        "set names for the given type of object is unsupported! please consider convert it as vector at first...",
                        "func: names",
                        "objtype: " & [object].GetType.FullName
                    }, env)
            End Select
        End Function

        Public Function getRowNames([object] As Object, envir As Environment) As Object
            If [object].GetType Is GetType(dataframe) Then
                Return DirectCast([object], dataframe).rownames
            Else
                Return getNames([object], envir)
            End If
        End Function

        Public Function setRowNames([object] As Object, namelist As Array, envir As Environment) As Object
            If TypeOf [object] Is dataframe Then
                Dim data As dataframe = DirectCast([object], dataframe)

                If data.nrows <> namelist.Length Then
                    Return Internal.debug.stop({
                        "row size is not matched!",
                        "nrows: " & data.nrows,
                        "given size: " & namelist.Length
                    }, envir)
                Else
                    data.rownames = asVector(Of String)(namelist)
                    Return data.rownames
                End If
            Else
                Throw New NotImplementedException
            End If
        End Function

        Public Function getColNames([object] As Object, envir As Environment) As Object
            If [object].GetType Is GetType(dataframe) Then
                Return DirectCast([object], dataframe).columns.Keys.ToArray
            Else
                Return getNames([object], envir)
            End If
        End Function

        Public Function setColNames([object] As Object, namelist As Array, envir As Environment) As Object
            If TypeOf [object] Is dataframe Then
                With DirectCast([object], dataframe)
                    Dim [raw] = .columns.ToArray
                    Dim [new] As New Dictionary(Of String, Array)
                    Dim names As String() = asVector(Of String)(namelist)

                    For i As Integer = 0 To names.Length - 1
                        [new].Add(names(i), raw(i).Value)
                    Next

                    .columns = [new]
                End With
            Else
                Throw New NotImplementedException
            End If

            Return [object]
        End Function
    End Module
End Namespace
