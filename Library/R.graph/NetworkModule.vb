﻿#Region "Microsoft.VisualBasic::14226754c166ead7a9dab320f32b0130, Library\R.graph\NetworkModule.vb"

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

    ' Module NetworkModule
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: addEdge, addEdges, addNode, addNodes, computeNetwork
    '               degree, emptyNetwork, getByGroup, getElementByID, LoadNetwork
    '               printGraph, SaveNetwork, setAttributes, typeGroupOfNodes
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports node = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("igraph", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@gcmodeller.org")>
<RTypeExport("graph", GetType(NetworkGraph))>
Public Module NetworkModule

    Sub New()
        REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of NetworkGraph)(AddressOf printGraph)
    End Sub

    Private Function printGraph(obj As Object) As String
        Dim g As NetworkGraph = DirectCast(obj, NetworkGraph)

        Return $"Network graph with {g.vertex.Count} vertex nodes and {g.graphEdges.Count} edges."
    End Function

    <ExportAPI("save.network")>
    Public Function SaveNetwork(g As Object, file$, Optional properties As String() = Nothing) As Boolean
        If g Is Nothing Then
            Throw New ArgumentNullException("g")
        End If

        Dim tables As NetworkTables

        If g.GetType Is GetType(NetworkGraph) Then
            tables = DirectCast(g, NetworkGraph).Tabular(properties)
        ElseIf g.GetType Is GetType(NetworkTables) Then
            tables = g
        Else
            Throw New InvalidProgramException(g.GetType.FullName)
        End If

        Return tables.Save(file)
    End Function

    <ExportAPI("read.network")>
    Public Function LoadNetwork(directory$, Optional defaultNodeSize As Object = "20,20") As NetworkGraph
        Return NetworkFileIO.Load(directory.GetDirectoryFullPath).CreateGraph(defaultNodeSize:=InteropArgumentHelper.getSize(defaultNodeSize))
    End Function

    ''' <summary>
    ''' Create a new network graph or clear the given network graph
    ''' </summary>
    ''' <param name="g"></param>
    ''' <returns></returns>
    <ExportAPI("empty.network")>
    Public Function emptyNetwork(Optional g As NetworkGraph = Nothing) As NetworkGraph
        If g Is Nothing Then
            g = New NetworkGraph
        Else
            g.Clear()
        End If

        Return g
    End Function

    ''' <summary>
    ''' Calculate node degree in given graph
    ''' </summary>
    ''' <param name="g"></param>
    ''' <returns></returns>
    <ExportAPI("degree")>
    Public Function degree(g As NetworkGraph) As Dictionary(Of String, Integer)
        Return g.ComputeNodeDegrees
    End Function

    ''' <summary>
    ''' compute network properties' data
    ''' </summary>
    ''' <param name="g"></param>
    ''' <returns></returns>
    <ExportAPI("compute.network")>
    Public Function computeNetwork(g As NetworkGraph) As NetworkGraph
        Call g.ComputeNodeDegrees
        Return g
    End Function

    <ExportAPI("add.nodes")>
    Public Function addNodes(g As NetworkGraph, labels$()) As NetworkGraph
        For Each label As String In labels
            Call g.CreateNode(label)
        Next

        Return g
    End Function

    <ExportAPI("add.node")>
    Public Function addNode(g As NetworkGraph, label$,
                            <RListObjectArgument>
                            Optional attrs As Object = Nothing,
                            Optional env As Environment = Nothing) As node

        Dim node As node = g.CreateNode(label)

        For Each attribute As NamedValue(Of Object) In RListObjectArgumentAttribute.getObjectList(attrs, env)
            node.data.Add(attribute.Name, Scripting.ToString(attribute.Value))
        Next

        Return node
    End Function

    ''' <summary>
    ''' Set node attribute data
    ''' </summary>
    ''' <param name="nodes"></param>
    ''' <param name="attrs"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("attrs")>
    Public Function setAttributes(<RRawVectorArgument> nodes As Object,
                                  <RListObjectArgument> attrs As Object,
                                  Optional env As Environment = Nothing) As Object

        Dim attrValues As NamedValue(Of String)() = RListObjectArgumentAttribute _
            .getObjectList(attrs, env) _
            .Select(Function(a)
                        Return New NamedValue(Of String) With {
                            .Name = a.Name,
                            .Value = Scripting.ToString(a.Value)
                        }
                    End Function) _
            .ToArray

        For Each node As node In REnv.asVector(Of node)(nodes)
            For Each a In attrValues
                node.data(a.Name) = a.Value
            Next
        Next

        Return nodes
    End Function

    <ExportAPI("add.edge")>
    Public Function addEdge(g As NetworkGraph, u$, v$) As Edge
        Return g.CreateEdge(u, v)
    End Function

    ''' <summary>
    ''' Add edges by a given node label tuple list
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="tuples">a given node label tuple list</param>
    ''' <returns></returns>
    <ExportAPI("add.edges")>
    Public Function addEdges(g As NetworkGraph, tuples As Object, <RRawVectorArgument> Optional weight As Object = Nothing) As NetworkGraph
        Dim nodeLabels As String()
        Dim edge As Edge
        Dim i As i32 = 1
        Dim weights As Double() = REnv.asVector(Of Double)(weight)
        Dim w As Double

        For Each tuple As NamedValue(Of Object) In list.GetSlots(tuples).IterateNameValues
            nodeLabels = REnv.asVector(Of String)(tuple.Value)
            w = weights.ElementAtOrDefault(CInt(i) - 1)
            edge = g.CreateEdge(nodeLabels(0), nodeLabels(1))
            edge.weight = w

            ' 20191226
            ' 如果使用数字作为边的编号的话
            ' 极有可能会出现重复的边编号
            ' 所以在这里判断一下
            ' 尽量避免使用数字作为编号
            If ++i = tuple.Name.ParseInteger Then
                edge.ID = $"{edge.U.label}..{edge.V.label}"
            Else
                edge.ID = tuple.Name
            End If
        Next

        Return g
    End Function

    <ExportAPI("getElementByID")>
    Public Function getElementByID(g As NetworkGraph, id As Object, Optional env As Environment = Nothing) As Object
        Dim array As Array

        If id Is Nothing Then
            Return Nothing
        End If

        Dim idtype As Type = id.GetType

        If idtype Is GetType(Integer) Then
            Return g.GetElementByID(DirectCast(id, Integer))
        ElseIf idtype Is GetType(String) Then
            Return g.GetElementByID(DirectCast(id, String))
        ElseIf REnv.isVector(Of Integer)(id) Then
            array = REnv.asVector(Of Integer)(id) _
                .AsObjectEnumerator _
                .Select(Function(i)
                            Return g.GetElementByID(DirectCast(i, Integer))
                        End Function) _
                .ToArray
        ElseIf REnv.isVector(Of String)(id) Then
            array = REnv.asVector(Of String)(id) _
                .AsObjectEnumerator _
                .Select(Function(i)
                            Return g.GetElementByID(DirectCast(i, String))
                        End Function) _
                .ToArray
        Else
            Return Message.InCompatibleType(GetType(String), id.GetType, env)
        End If

        Return array
    End Function

    ''' <summary>
    ''' Make node groups by given type name
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="type"></param>
    ''' <param name="nodes"></param>
    ''' <returns></returns>
    <ExportAPI("groups")>
    Public Function typeGroupOfNodes(g As NetworkGraph, type$, nodes As String()) As NetworkGraph
        Call nodes _
            .Select(AddressOf g.GetElementByID) _
            .DoEach(Sub(n)
                        n.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = type
                    End Sub)
        Return g
    End Function

    ''' <summary>
    ''' Node select by group or other condition
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="typeSelector$"></param>
    ''' <returns></returns>
    <ExportAPI("select")>
    Public Function getByGroup(g As NetworkGraph, typeSelector As Object, Optional env As Environment = Nothing) As Object
        If typeSelector Is Nothing Then
            Return {}
        ElseIf typeSelector.GetType Is GetType(String) Then
            Dim typeStr$ = typeSelector.ToString

            Return g.vertex _
                .Where(Function(n)
                           Return n.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = typeStr
                       End Function) _
                .ToArray
        ElseIf REnv.isVector(Of String)(typeSelector) Then
            Dim typeIndex As Index(Of String) = REnv _
                .asVector(Of String)(typeSelector) _
                .AsObjectEnumerator(Of String) _
                .ToArray

            Return g.vertex _
                .Where(Function(n)
                           Return n.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) Like typeIndex
                       End Function) _
                .ToArray
        ElseIf typeSelector.GetType.ImplementInterface(GetType(RFunction)) Then
            Dim selector As RFunction = typeSelector

            Return g.vertex _
                .Where(Function(n)
                           Dim test As Object = selector.Invoke(env, InvokeParameter.Create(n))
                           ' get test result
                           Return REnv _
                               .asLogical(test) _
                               .FirstOrDefault
                       End Function) _
                .ToArray
        Else
            Return Message.InCompatibleType(GetType(RFunction), typeSelector.GetType, env)
        End If
    End Function

    ''' <summary>
    ''' Decompose a graph into components, Creates a separate graph for each component of a graph.
    ''' </summary>
    ''' <param name="graph">The original graph.</param>
    ''' <param name="weakMode">
    ''' Character constant giving the type of the components, wither weak for weakly connected 
    ''' components or strong for strongly connected components.
    ''' </param>
    ''' <param name="minVertices">The minimum number of vertices a component should contain in 
    ''' order to place it in the result list. Eg. supply 2 here to ignore isolate vertices.
    ''' </param>
    ''' <returns>A list of graph objects.</returns>
    <ExportAPI("decompose")>
    Public Function DecomposeGraph(graph As NetworkGraph, Optional weakMode As Boolean = True, Optional minVertices As Integer = 5) As NetworkGraph()
        Return graph.DecomposeGraph(weakMode, minVertices).ToArray
    End Function
End Module
