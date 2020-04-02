﻿#Region "Microsoft.VisualBasic::6a9dc6d697806b73323cd77d77e734ce, Library\R.base\rawSerializer.vb"

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

' Module rawSerializer
' 
'     Sub: writeList, writeObject, writeString
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports cdfAttribute = Microsoft.VisualBasic.Data.IO.netCDF.Components.attribute
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe

Module rawSerializer

    <Extension>
    Public Sub writeObject(cdf As CDFWriter, symbolRef$, obj As Object)
        If obj Is Nothing Then
            ' skip null?
            Return
        ElseIf TypeOf obj Is list Then
            Call cdf.writeList(symbolRef, DirectCast(obj, list).slots)
        ElseIf TypeOf obj Is Rdataframe Then
            Call cdf.writeDataframe(symbolRef, DirectCast(obj, Rdataframe))
        ElseIf TypeOf obj Is Dictionary(Of String, Object) Then
            Call cdf.writeList(symbolRef, obj)
        ElseIf obj.GetType.ImplementInterface(Of IDictionary) Then
            Dim list As New Dictionary(Of String, Object)
            Dim raw As IDictionary = obj

            For Each key As Object In raw.Keys
                list(Scripting.ToString(key)) = raw.Item(key)
            Next

            Call cdf.writeList(symbolRef, list)
        Else
            Dim vector As Array = Runtime.asVector(Of Object)(obj)
            Dim elTypes = vector.AsObjectEnumerator _
                .Select(Function(o) o.GetType) _
                .GroupBy(Function(t) t.FullName) _
                .ToArray _
                .OrderByDescending(Function(g) g.Count) _
                .First _
                .First

            If elTypes Is GetType(String) Then
                Call cdf.writeString(symbolRef, Runtime.asVector(Of String)(vector))
            Else
                Dim value As CDFData = (CObj(vector), elTypes.GetCDFTypeCode)
                Dim attributes As cdfAttribute() = {
                    New cdfAttribute With {.name = "length", .type = CDFDataTypes.INT, .value = vector.Length},
                    New cdfAttribute With {.name = "type", .type = CDFDataTypes.INT, .value = CInt(elTypes.GetRTypeCode)}
                }
                Dim dims As Dimension() = {cdf.getDimension(elTypes.FullName)}

                Call cdf.AddVariable(symbolRef, value, dims, attributes)
            End If
        End If
    End Sub

    <Extension>
    Private Sub writeString(cdf As CDFWriter, symbolRef$, strings As String())
        Using buffer As New MemoryStream, sb As New BinaryDataWriter(buffer)
            If strings Is Nothing Then
                strings = {}
            End If

            For Each str As String In strings
                Call sb.Write(str, BinaryStringFormat.DwordLengthPrefix)
            Next

            Call sb.Flush()

            Dim value As New CDFData With {.chars = buffer.ToArray.ToBase64String}
            Dim attributes As cdfAttribute() = {
                New cdfAttribute With {.name = "length", .type = CDFDataTypes.INT, .value = strings.Length},
                New cdfAttribute With {.name = "type", .type = CDFDataTypes.INT, .value = CInt(TypeCodes.string)}
            }

            cdf.AddVariable(symbolRef, value, {cdf.getDimension(GetType(String).FullName)}, attributes)
        End Using
    End Sub

    <Extension>
    Private Sub writeDataframe(cdf As CDFWriter, symbolRef$, table As Rdataframe)
        ' write symbol
        Dim attributes As cdfAttribute() = {
            New cdfAttribute With {.name = "length", .type = CDFDataTypes.INT, .value = table.ncols}，
            New cdfAttribute With {.name = "type", .type = CDFDataTypes.INT, .value = CInt(TypeCodes.dataframe)},
            New cdfAttribute With {.name = "ncols", .type = CDFDataTypes.INT, .value = table.ncols},
            New cdfAttribute With {.name = "nrows", .type = CDFDataTypes.INT, .value = table.nrows}
        }
        Dim symbolVal As CDFData = {CInt(TypeCodes.dataframe)}

        Call cdf.AddVariable(symbolRef, symbolVal, {cdf.getDimension(GetType(Integer).FullName)}, attributes)

        ' write colnames
        Call cdf.writeString($"{symbolRef}\colnames", table.columns.Keys.ToArray)
        ' write rownames
        Call cdf.writeString($"{symbolRef}\rownames", table.rownames)

        ' write column values
        For Each name As String In table.columns.Keys
            Call cdf.writeObject($"{symbolRef}\slots\{name}", table.columns(name))
        Next
    End Sub

    <Extension>
    Private Sub writeList(cdf As CDFWriter, symbolRef$, list As Dictionary(Of String, Object))
        ' write symbol
        Dim attributes As cdfAttribute() = {
            New cdfAttribute With {.name = "length", .type = CDFDataTypes.INT, .value = list.Count}，
            New cdfAttribute With {.name = "type", .type = CDFDataTypes.INT, .value = CInt(TypeCodes.list)}
        }
        Dim symbolVal As CDFData = {CInt(TypeCodes.list)}

        Call cdf.AddVariable(symbolRef, symbolVal, {cdf.getDimension(GetType(Integer).FullName)}, attributes)

        ' write names
        Call cdf.writeString($"{symbolRef}\names", list.Keys.ToArray)

        ' write values
        For Each name As String In list.Keys
            Call cdf.writeObject($"{symbolRef}\slots\{name}", list(name))
        Next
    End Sub
End Module

