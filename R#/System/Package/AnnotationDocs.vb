﻿#Region "Microsoft.VisualBasic::e7cfaae9c4aa734db12cb13f62f80eea, R#\Runtime\Package\AnnotationDocs.vb"

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

    '     Class AnnotationDocs
    ' 
    '         Function: (+2 Overloads) GetAnnotations
    ' 
    '         Sub: PrintHelp
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime.Interop
Imports LibraryAssembly = System.Reflection.Assembly

Namespace Runtime.Package

    ''' <summary>
    ''' Parser of the <see cref="Project"/> assembly
    ''' </summary>
    Public Class AnnotationDocs

        ReadOnly projects As New Dictionary(Of String, Project)
        ReadOnly markdown As MarkdownRender = MarkdownRender.DefaultStyleRender

        Public Function GetAnnotations(package As Type) As ProjectType
            Dim assembly As LibraryAssembly = package.Assembly
            Dim project As Project
            Dim projectKey As String = assembly.ToString
            Dim docXml As String = assembly.Location.TrimSuffix & ".xml"
            Dim type As ProjectType

            ' get xml docs from app path or annotation folder
            ' in app home folder
            If Not docXml.FileExists Then
                docXml = assembly.Location.ParentPath
                docXml = docXml & $"/Annotation/{assembly.Location.BaseName}.xml"
            End If

            If Not projects.ContainsKey(projectKey) Then
                projects(projectKey) = ProjectSpace.CreateDocProject(docXml)
            End If

            project = projects(projectKey)
            type = project.GetType(package.FullName)

            Return type
        End Function

        Public Function GetAnnotations(func As MethodInfo) As ProjectMember
            Dim type As ProjectType = GetAnnotations(func.DeclaringType)

            If type Is Nothing Then
                ' 整个类型都没有xml注释
                Return Nothing
            Else
                ' 可能目标函数对象没有xml注释
                ' 在这里假设没有重载？
                Return type.GetMethods(func.Name).ElementAtOrDefault(Scan0)
            End If
        End Function

        ''' <summary>
        ''' Print help information about the given R api method 
        ''' </summary>
        ''' <param name="api"></param>
        Public Sub PrintHelp(api As RMethodInfo)
            Dim docs As ProjectMember = GetAnnotations(api.GetRawDeclares)

            If Not docs Is Nothing Then
                Call markdown.DoPrint(docs.Summary, 1)
                Call Console.WriteLine()

                For Each param As param In docs.Params
                    Call markdown.DoPrint($"``{param.name}:``  " & param.text.Trim(" "c, ASCII.CR, ASCII.LF), 3)
                Next
            End If

            Call Console.WriteLine()

            If Not docs.Returns.StringEmpty Then
                Call markdown.DoPrint(" [**returns**]: ", 0)
                Call markdown.DoPrint(docs.Returns, 1)
                Call Console.WriteLine()
            End If

            Dim contentLines = api.GetPrintContent.LineTokens.AsList

            Call markdown.DoPrint(contentLines(Scan0), 2)

            For Each line As String In contentLines.Skip(1).Take(7)
                Call markdown.DoPrint("# " & line.Trim, 6)
            Next

            Call markdown.DoPrint(contentLines(-2).Trim, 6)
            Call markdown.DoPrint(contentLines(-1).Trim, 2)

            Call Console.WriteLine()
        End Sub

    End Class
End Namespace