﻿#Region "Microsoft.VisualBasic::9583a565b39f3ad666c7138154b9a59c, R#\Interpreter\Syntax\SyntaxImplements\DeclareNewFunctionSyntax.vb"

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

    '     Module DeclareNewFunctionSyntax
    ' 
    '         Function: DeclareNewFunction, getParameters, ReturnValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module DeclareNewFunctionSyntax

        Public Function ReturnValue(value As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
            With value.ToArray
                ' just return
                ' no value
                If .Length = 0 Then
                    Return New ReturnValue(Literal.NULL)
                Else
                    Dim valueSyntax As SyntaxResult =
                        .DoCall(Function(tokens)
                                    Return Expression.CreateExpression(tokens, opts)
                                End Function)

                    If valueSyntax.isException Then
                        Return valueSyntax
                    Else
                        Return New ReturnValue(valueSyntax.expression)
                    End If
                End If
            End With
        End Function

        Public Function DeclareNewFunction(code As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim [declare] As Token() = code(4)
            Dim parts As List(Of Token()) = [declare].SplitByTopLevelDelimiter(TokenType.close)
            Dim paramPart As Token() = parts(Scan0).Skip(1).ToArray
            Dim bodyPart As SyntaxResult = parts(2).Skip(1) _
                .ToArray _
                .DoCall(Function(tokens)
                            Return SyntaxImplements.ClosureExpression(tokens, opts)
                        End Function)

            If bodyPart.isException Then
                Return bodyPart
            End If

            Dim funcName As String = code(1)(Scan0).text
            Dim params As New List(Of DeclareNewVariable)
            Dim err As SyntaxResult = getParameters(paramPart, params)

            If err IsNot Nothing AndAlso err.isException Then
                Return err
            Else
                Return New DeclareNewFunction(
                    funcName:=funcName,
                    body:=bodyPart.expression,
                    params:=params.ToArray
                )
            End If
        End Function

        Private Function getParameters(tokens As Token(), ByRef params As List(Of DeclareNewVariable)) As SyntaxResult
            Dim parts As Token()() = tokens.SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(t) Not t.isComma) _
                .ToArray

            For Each syntaxTemp As SyntaxResult In parts _
                .Select(Function(t)
                            Return SyntaxImplements.DeclareNewVariable(t)
                        End Function)

                If syntaxTemp.isException Then
                    Return syntaxTemp
                Else
                    params.Add(syntaxTemp.expression)
                End If
            Next

            Return Nothing
        End Function
    End Module
End Namespace