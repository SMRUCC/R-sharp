﻿#Region "Microsoft.VisualBasic::91a456afda83e0d0819e38e9a63b4ee6, Library\R.base\utils\stringr.vb"

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

    ' Module stringr
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: createRObj, fromXML, Levenshtein, unescapeRRawstring, unescapeRUnicode
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.application.xml
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports RHtml = SMRUCC.Rsharp.Runtime.Internal.htmlPrinter
Imports Rlang = Microsoft.VisualBasic.My.RlangInterop

''' <summary>
''' Simple, Consistent Wrappers for Common String Operations
''' 
''' stringr provide fast, correct implementations of common string manipulations. 
''' stringr focusses on the most important and commonly used string manipulation 
''' functions whereas stringi provides a comprehensive set covering almost anything 
''' you can imagine. 
''' </summary>
<Package("stringr", Category:=APICategories.UtilityTools)>
Module stringr

    Sub New()
        RHtml.AttachHtmlFormatter(Of DistResult)(AddressOf ResultVisualize.HTMLVisualize)
    End Sub

    ''' <summary>
    ''' Compute the edit distance between two strings is defined as the 
    ''' minimum number of edit operations required to transform one string 
    ''' into another.
    ''' </summary>
    ''' <param name="x$"></param>
    ''' <param name="y$"></param>
    ''' <returns></returns>
    <ExportAPI("levenshtein")>
    Public Function Levenshtein(x$, y$) As DistResult
        Return LevenshteinDistance.ComputeDistance(x, y)
    End Function

    ''' <summary>
    ''' parse XML text data into R object
    ''' </summary>
    ''' <param name="str">the xml text</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("fromXML")>
    Public Function fromXML(str As String, Optional env As Environment = Nothing) As Object
        Return XmlElement.ParseXmlText(str).createRObj(env)
    End Function

    <Extension>
    Private Function createRObj(xml As XmlElement, env As Environment) As Object
        Dim obj As New list
        Dim attrs As New list

        For Each attr In xml.attributes.SafeQuery
            attrs.add(attr.Key, attr.Value)
        Next

        obj.add("attributes", attrs)

        If Not xml.text.StringEmpty Then
            obj.add("value", xml.text)
        End If

        For Each ele As IGrouping(Of String, XmlElement) In xml.elements.SafeQuery.GroupBy(Function(x) x.name)
            If ele.Count = 1 Then
                obj.add(ele.Key, createRObj(ele.First, env))
            Else
                Dim array As New List(Of Object)

                For Each item As XmlElement In ele
                    array.Add(createRObj(item, env))
                Next

                obj.add(ele.Key, array.ToArray)
            End If
        Next

        Return obj
    End Function

    ''' <summary>
    ''' processing a unicode char like ``&lt;U+767D>`` 
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("decode.R_unicode")>
    Public Function unescapeRUnicode(input As Object, Optional env As Environment = Nothing) As Object
        Return env.EvaluateFramework(Of String, String)(input, AddressOf Rlang.ProcessingRUniCode)
    End Function

    ''' <summary>
    ''' processing a unicode char like ``&lt;aa>``
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="encoding"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("decode.R_rawstring")>
    Public Function unescapeRRawstring(input As Object, Optional encoding As Encodings = Encodings.Unicode, Optional env As Environment = Nothing) As Object
        Return env.EvaluateFramework(Of String, String)(input, Function(str) Rlang.ProcessingRRawUniCode(str, encoding))
    End Function
End Module
