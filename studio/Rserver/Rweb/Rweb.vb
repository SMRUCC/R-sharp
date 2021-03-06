﻿#Region "Microsoft.VisualBasic::dcbc6c7a3d3f7c77c4c924f54c4cce88, studio\Rserver\Rweb\Rweb.vb"

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

    ' Class Rweb
    ' 
    '     Properties: NextRequestId, TcpPort
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: callback, getHttpProcessor, Run
    ' 
    '     Sub: handleGETRequest, handleOtherMethod, handlePOSTRequest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Net.Sockets
Imports System.Runtime.CompilerServices
Imports Flute.Http.Core
Imports Flute.Http.Core.HttpStream
Imports Flute.Http.Core.Message
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Runtime.Serialize

''' <summary>
''' Rweb is not design for general web programming, it is 
''' design for running a background data task.
''' </summary>
Public Class Rweb : Inherits HttpServer

    Dim socket As TcpServicesSocket
    Dim processor As RProcessor

    Public Shared ReadOnly Property NextRequestId As String
        Get
            Return App.GetNextUniqueName("web_request__")
        End Get
    End Property

    Public ReadOnly Property TcpPort As Integer
        Get
            Return socket.LocalPort
        End Get
    End Property

    Public Sub New(Rweb$, port As Integer, tcp As Integer, show_error As Boolean, Optional threads As Integer = -1)
        MyBase.New(port, threads)

        Me.processor = New RProcessor(Me, Rweb, show_error)
        Me.socket = New TcpServicesSocket(tcp) With {
            .ResponseHandler = AddressOf callback
        }
    End Sub

    Public Overrides Function Run() As Integer
        Call RunTask(AddressOf socket.Run)
        Return MyBase.Run()
    End Function

    Public Overrides Sub handleGETRequest(p As HttpProcessor)
        Call processor.RscriptHttpGet(p)
    End Sub

    Private Function callback(request As RequestStream, remoteAddress As System.Net.IPEndPoint) As BufferPipe
        Using bytes As New MemoryStream(request.ChunkBuffer)
            Dim data As IPCBuffer = IPCBuffer.ParseBuffer(bytes)

            Call $"accept callback data: {data.ToString}".__DEBUG_ECHO
            Call processor.SaveResponse(data.requestId, data.buffer)

            Return New DataPipe(NetResponse.RFC_OK)
        End Using
    End Function

    Public Overrides Sub handlePOSTRequest(p As HttpProcessor, inputData As String)
        Dim request As New HttpPOSTRequest(p, inputData)

        Select Case request.URL.path
            Case "callback"
                Dim requestId As String = request.URL("request")
                Dim data As HttpPostedFile = request.POSTData.files _
                    .TryGetValue("data") _
                   ?.FirstOrDefault

                Using file As FileStream = data.TempPath.Open
                    Call Buffer _
                        .ParseBuffer(raw:=file) _
                        .DoCall(Sub(buf)
                                    Call processor.SaveResponse(requestId, buf)
                                End Sub)
                End Using

                Call p.writeSuccess(0)
            Case Else
                Using response As New HttpResponse(p.outputStream, AddressOf p.writeFailure)
                    Call processor.RscriptHttpPost(request, response)
                End Using
        End Select
    End Sub

    Public Overrides Sub handleOtherMethod(p As HttpProcessor)
        Call p.writeFailure(404, "not allowed!")
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Protected Overrides Function getHttpProcessor(client As TcpClient, bufferSize%) As HttpProcessor
        Return New HttpProcessor(client, Me, MAX_POST_SIZE:=bufferSize)
    End Function
End Class
