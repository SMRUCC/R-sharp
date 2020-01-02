﻿#Region "Microsoft.VisualBasic::518fab9cf1540be5dcdab81dd6b12332, R#\Runtime\Interop\RInteropAttribute.vb"

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

'     Class RInteropAttribute
' 
' 
' 
'     Class RByRefValueAssignAttribute
' 
' 
' 
'     Class RRawVectorArgumentAttribute
' 
' 
' 
'     Class RSuppressPrintAttribute
' 
'         Function: IsPrintInvisible
' 
'     Class RListObjectArgumentAttribute
' 
'         Function: getObjectList
' 
' 
' /********************************************************************************/

#End Region

Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.All, AllowMultiple:=False, Inherited:=True)>
    Public Class RInteropAttribute : Inherits Attribute
    End Class

    ''' <summary>
    ''' 这个参数是接受``a(x) &lt;- y``操作之中的``y``结果值的
    ''' </summary>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RByRefValueAssignAttribute : Inherits RInteropAttribute
    End Class

    ''' <summary>
    ''' 表示这个参数是一个数组，环境系统不应该自动调用getFirst取第一个值
    ''' </summary>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RRawVectorArgumentAttribute : Inherits RInteropAttribute
    End Class

    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RParameterNameAliasAttribute : Inherits RInteropAttribute

        Public ReadOnly Property [alias] As String

        Sub New([alias] As String)
            Me.alias = [alias]
        End Sub

        Public Overrides Function ToString() As String
            Return [alias]
        End Function
    End Class
End Namespace
