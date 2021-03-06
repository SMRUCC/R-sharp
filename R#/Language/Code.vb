﻿#Region "Microsoft.VisualBasic::05e71c28875faeb35f9d21f023b3de26, R#\Language\Code.vb"

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

    '     Module Code
    ' 
    '         Function: GetCodeSpan, joinNext, ParseScript
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language

    Module Code

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function ParseScript(script As String) As Token()
            Return New Scanner(script).GetTokens().ToArray
        End Function

        ''' <summary>
        ''' For parse the raw script text
        ''' </summary>
        ''' <param name="code"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetCodeSpan(code As IEnumerable(Of Token)) As IntRange
            With code.OrderBy(Function(t) t.span.start).ToArray
                Return New IntRange(.First.span.start, .Last.span.stops)
            End With
        End Function

        <Extension>
        Friend Function joinNext(tken As Token, token As String) As JoinToken
            Dim nextToken As Token = Scanner.MeasureToken(token)
            Dim join As New JoinToken With {
                .name = tken.name,
                .text = tken.text,
                .span = tken.span,
                .[next] = nextToken
            }

            Return join
        End Function
    End Module
End Namespace
