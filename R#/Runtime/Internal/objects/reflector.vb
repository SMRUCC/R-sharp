﻿#Region "Microsoft.VisualBasic::24e1e1563ad5342db3d7ed6c9332dd0f, R#\Runtime\Internal\objects\reflector.vb"

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

'     Module reflector
' 
'         Function: dataframe, GetStructure, printSlots, strList, strVector
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object

    Module reflector

        ''' <summary>
        ''' a helper method for view data structure of the given object
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="env"></param>
        ''' <param name="indent$"></param>
        ''' <returns></returns>
        Public Function GetStructure(x As Object, env As GlobalEnvironment, indent$, list_len%) As String
            If x Is Nothing Then
                Return "<NULL>"
            ElseIf x.GetType Is GetType(list) Then
                x = DirectCast(x, list).slots
            End If

            If TypeOf x Is vbObject Then
                x = DirectCast(x, vbObject).target
            End If
            If TypeOf x Is MagicScriptSymbol Then
                x = DirectCast(x, MagicScriptSymbol).toList.slots
            End If

            Dim type As Type = x.GetType
            Dim code As TypeCodes = type.GetRTypeCode

            If TypeOf x Is vector Then
                type = DirectCast(x, vector).data.GetType
                code = type.GetRTypeCode
                x = DirectCast(x, vector).data
            End If

            If type.ImplementInterface(GetType(IDictionary)) Then
                Return strList(list:=x, env:=env, indent:=indent, list_len:=list_len)
            ElseIf Runtime.IsPrimitive(code, includeComplexList:=False) OrElse type.IsArray Then
                Return strVector(Runtime.asVector(Of Object)(x), type, env)
            ElseIf type Is GetType(dataframe) Then
                Return dataframe(x, env, indent, list_len)
            Else
                Return printer.ValueToString(x, env)
            End If
        End Function

        Private Function strList(list As IDictionary, env As GlobalEnvironment, indent$, list_len%) As String
            Dim value As Object
            Dim sb As New StringBuilder
            Dim i As i32 = 1
            Dim keyValues As New List(Of (key$, value$))

            If list.Count = 0 Then
                Return $"{indent}list()"
            End If

            Call sb.AppendLine("List of " & list.Count)

            For Each slotKey As Object In (From x In list.Keys.AsQueryable Select x Take list_len)
                value = list(slotKey)

                If ++i = CInt(Val(slotKey.ToString)) Then
                    slotKey = $"[{slotKey}]"
                End If

                slotKey = $"{indent}$ {slotKey}"
                value = GetStructure(value, env, indent & "..", list_len)
                keyValues.Add((slotKey, DirectCast(value, String)))
            Next

            Dim truncated$ = Nothing

            If list.Count > list_len Then
                truncated = $"{indent}[list output truncated]"
            End If

            Return sb.printSlots(keyValues, truncated)
        End Function

        <Extension>
        Private Function printSlots(sb As StringBuilder, keyValues As List(Of (key$, value$)), truncated$) As String
            Dim maxPrefixSize As Integer = keyValues _
                .Select(Function(s) s.key) _
                .MaxLengthString _
                .Length

            For Each slot In keyValues
                Call sb.AppendLine($"{slot.key}{New String(" "c, maxPrefixSize - slot.key.Length)} : {slot.value}")
            Next

            If Not truncated.StringEmpty Then
                Call sb.AppendLine(truncated)
            End If

            Return sb.ToString
        End Function

        Private Function dataframe(d As dataframe, env As GlobalEnvironment, indent$, list_len%) As String
            Dim sb As New StringBuilder()
            Dim value As Array
            Dim i As i32 = 1
            Dim slotKey As String
            Dim keyValues As New List(Of (key$, value$))

            Call sb.AppendLine($"'data.frame':	{d.nrows} obs. of  {d.ncols} variables:")

            For Each col As KeyValuePair(Of String, Array) In d.columns
                value = col.Value
                slotKey = col.Key

                If ++i = CInt(Val(slotKey.ToString)) Then
                    slotKey = $"[{slotKey}]"
                End If

                keyValues.Add(($"{indent}$ {slotKey}", GetStructure(value, env, indent & "..", list_len)))
            Next

            Return sb.printSlots(keyValues, Nothing)
        End Function

        ''' <summary>
        ''' inspect the vector data structure
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="type"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Private Function strVector(a As Array, type As Type, env As GlobalEnvironment) As String
            Dim typeCode$

            If type Like RType.integers Then
                typeCode = "int"
            ElseIf type Like RType.characters Then
                typeCode = "chr"
            ElseIf type Like RType.floats Then
                typeCode = "num"
            ElseIf type Like RType.logicals Then
                typeCode = "logical"
            Else
                typeCode = "any"
            End If

            If a.Length = 1 Then
                Return $"{typeCode} {printer.ValueToString(a.GetValue(Scan0), env)}"
            ElseIf a.Length <= 6 Then
                Return $"{typeCode} [1:{a.Length}] {printer.getStrings(a, Nothing, env).JoinBy(" ")}"
            Else
                Return $"{typeCode} [1:{a.Length}] {printer.getStrings(a, Nothing, env).Take(6).JoinBy(" ")} ..."
            End If
        End Function
    End Module
End Namespace
