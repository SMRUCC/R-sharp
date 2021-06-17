﻿#Region "Microsoft.VisualBasic::f188cd41cf02158302ee6899f3466022, studio\Rsharp_kit\webKit\GraphQuery.vb"

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

    ' Module HtmlGraphQuery
    ' 
    '     Function: parseQuery, query
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.GraphQuery
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime

<Package("graphquery")>
Public Module HtmlGraphQuery

    <ExportAPI("parseQuery")>
    Public Function parseQuery(graphquery As String) As Query
        Return QueryParser.GetQuery(graphquery)
    End Function

    <ExportAPI("query")>
    Public Function query(document As Object, graphquery As Object,
                          Optional raw As Boolean = False,
                          Optional env As Environment = Nothing) As Object

        If TypeOf graphquery Is String Then
            graphquery = QueryParser.GetQuery(graphquery)
        End If
        If TypeOf document Is String Then
            document = HtmlDocument.LoadDocument(document)
        End If

        Dim data As JsonElement = New Engine().Execute(DirectCast(document, HtmlDocument), graphquery)

        If raw Then
            Return data
        Else
            Return data.createRObj(env)
        End If
    End Function
End Module
