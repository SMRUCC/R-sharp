﻿#Region "Microsoft.VisualBasic::432935abded6e0458f447eabd702b4e4, R#\Interpreter\Syntax\SyntaxImplements\DeclareNewSymbolSyntax.vb"

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

    '     Module DeclareNewSymbolSyntax
    ' 
    '         Function: (+4 Overloads) DeclareNewSymbol, getNames, ModeOf
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module DeclareNewSymbolSyntax

        Public Function ModeOf(keyword$, target As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim ObjTarget = Expression.CreateExpression(target, opts)

            If ObjTarget.isException Then
                Return ObjTarget
            Else
                Return New ModeOf(keyword, ObjTarget.expression)
            End If
        End Function

        <Extension>
        Public Function DeclareNewSymbol(code As List(Of Token()), [readonly] As Boolean, opts As SyntaxBuilderOptions) As SyntaxResult
            Dim valSyntaxtemp As SyntaxResult = Nothing

            ' 0   1    2   3    4 5
            ' let var [as type [= ...]]
            Dim symbolNames = getNames(code(1))
            Dim type As TypeCodes
            Dim value As Expression = Nothing

            If code = 2 Then
                type = TypeCodes.generic
            ElseIf code(2).isKeyword("as") Then
                type = code(3)(Scan0).text.GetRTypeCode

                If code.Count > 4 AndAlso code(4).isOperator("=", "<-") Then
                    valSyntaxtemp = code.Skip(5).AsList.ParseExpression(opts)
                End If
            Else
                type = TypeCodes.generic

                If code > 2 AndAlso code(2).isOperator("=", "<-") Then
                    valSyntaxtemp = code.Skip(3).AsList.ParseExpression(opts)
                End If
            End If

            If (Not valSyntaxtemp Is Nothing) AndAlso valSyntaxtemp.isException Then
                Return valSyntaxtemp
            Else
                value = valSyntaxtemp?.expression
            End If

            Dim symbol As New DeclareNewSymbol(
                names:=symbolNames,
                value:=value,
                type:=type,
                [readonly]:=[readonly]
            )

            Return New SyntaxResult(symbol)
        End Function

        Public Function DeclareNewSymbol(code As List(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
            Return code _
                .SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True) _
                .DeclareNewSymbol(False, opts)
        End Function

        ''' <summary>
        ''' declare a parameter symbol
        ''' </summary>
        ''' <param name="singleToken"></param>
        ''' <returns></returns>
        Public Function DeclareNewSymbol(singleToken As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim symbolNames = getNames(singleToken)
            Dim type As TypeCodes

            If symbolNames Like GetType(SyntaxErrorException) Then
                Return New SyntaxResult(symbolNames.TryCast(Of SyntaxErrorException), opts.debug)
            End If

            If singleToken.Length > 1 AndAlso symbolNames.TryCast(Of String()).Length = 1 Then
                type = singleToken(2).text.GetRTypeCode
            Else
                type = TypeCodes.generic
            End If

            Return New DeclareNewSymbol(
                names:=symbolNames,
                value:=Nothing,
                type:=type,
                [readonly]:=False
            )
        End Function

        ''' <summary>
        ''' declare a new parameter with optional default value
        ''' </summary>
        ''' <param name="symbol"></param>
        ''' <param name="value"></param>
        ''' <param name="opts"></param>
        ''' <param name="funcParameter"></param>
        ''' <returns></returns>
        Public Function DeclareNewSymbol(symbol As Token(), value As Token(), opts As SyntaxBuilderOptions, funcParameter As Boolean) As SyntaxResult
            Dim valSyntaxTemp As SyntaxResult = Expression.CreateExpression(value, opts)

            If valSyntaxTemp.isException Then
                Return valSyntaxTemp
            End If

            Dim symbolNames = getNames(symbol)

            If symbolNames Like GetType(SyntaxErrorException) Then
                Return New SyntaxResult(symbolNames.TryCast(Of SyntaxErrorException), opts.debug)
            End If

            Dim type As TypeCodes

            If funcParameter Then
                type = TypeCodes.generic
            Else
                type = valSyntaxTemp.expression.type
            End If

            Return New DeclareNewSymbol(
                names:=symbolNames,
                value:=valSyntaxTemp.expression,
                type:=type,
                [readonly]:=False
            )
        End Function

        ''' <summary>
        ''' get tuple names or a single symbol name
        ''' </summary>
        ''' <param name="code"></param>
        ''' <returns></returns>
        Friend Function getNames(code As Token()) As [Variant](Of String(), SyntaxErrorException)
            If code.Length > 1 Then
                If code(Scan0) = (TokenType.open, "[") AndAlso code.Last = (TokenType.close, "]") Then
                    ' [a,b,c]
                    ' tuple symbol names
                    Dim symbols = code.Skip(1) _
                        .Take(code.Length - 2) _
                        .Where(Function(token) Not token.name = TokenType.comma) _
                        .ToArray
                    Dim names As New List(Of String)

                    For Each symbol In symbols
                        ' allowes using keyword as symbol 
                        If symbol.name <> TokenType.identifier AndAlso symbol.name <> TokenType.keyword Then
                            Return New SyntaxErrorException(code.Select(Function(a) a.text).JoinBy(" "))
                        Else
                            names.Add(symbol.text)
                        End If
                    Next

                    Return names.ToArray
                ElseIf code(1) = (TokenType.keyword, "as") Then
                    ' a as type
                    Return {code(Scan0).text}
                Else
                    Return New SyntaxErrorException(code.Select(Function(a) a.text).JoinBy(" "))
                End If
            Else
                ' single symbol
                Return {code(Scan0).text}
            End If
        End Function
    End Module
End Namespace
