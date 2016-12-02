﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Metrolib.Physics;

namespace Metrolib.Controls.Charts.Network.Algorithms
{
	/// <summary>
	///     Algorithm that performs a physical simulation where nodes repulse each other and edges are springs.
	/// </summary>
	/// <remarks>
	///     http://profs.etsmtl.ca/mMcGuffin/research/2012-mcguffin-simpleNetVis/mcguffin-2012-simpleNetVis.pdf.
	/// </remarks>
	internal sealed class ForceDirectedLayoutAlgorithm
		: INodeLayoutAlgorithm
	{
		private readonly ForceDirectedLayout _layout;
		private readonly List<IEdge> _edges;
		private readonly Dictionary<object, Node> _nodesByDataContext;
		private readonly Random _rng;

		#region Simulation

		private readonly EulerIntegrator _integrator;
		private readonly Attractor _attractor;
		private readonly Spring _spring;

		#endregion

		public ForceDirectedLayoutAlgorithm(ForceDirectedLayout layout)
		{
			if (layout == null)
				throw new ArgumentNullException("layout");

			_layout = layout;
			_nodesByDataContext = new Dictionary<object, Node>();
			_edges = new List<IEdge>();
			_rng = new Random(42);

			_integrator = new EulerIntegrator();
			_attractor = new Attractor();
			_spring = new Spring(_rng);
		}

		public void Dispose()
		{
		}

		public List<NodePosition> Update(TimeSpan elapsed)
		{
			if (elapsed <= TimeSpan.Zero)
				return new List<NodePosition>();

			double dt = Math.Min(0.06, elapsed.TotalSeconds);

			Repulse();
			Attract();
			UpdatePositions(dt);

			var nodes = new List<NodePosition>(_nodesByDataContext.Count);
			foreach (var node in _nodesByDataContext.Values)
			{
				nodes.Add(new NodePosition
					{
						Node = node.DataContext,
						Position = node.Body.Position
					});
			}
			return nodes;
		}

		private void Repulse()
		{
			_attractor.Force = _layout.Repulsiveness;

			foreach (Node node1 in _nodesByDataContext.Values)
			{
				foreach (Node node2 in _nodesByDataContext.Values)
				{
					if (ReferenceEquals(node1, node2))
						continue;

					_attractor.ApplyForces(node2.Body, node1.Body);
				}
			}
		}

		private void Attract()
		{
			_spring.Length = _layout.Distance;
			_spring.Stiffness = _layout.SpringStiffness;
			_spring.Dampening = _layout.SpringDampening;

			foreach (IEdge edge in _edges)
			{
				Node node1 = GetNode(edge.Node1);
				if (node1 == null)
					continue;
				Node node2 = GetNode(edge.Node2);
				if (node2 == null)
					continue;

				_spring.ApplyForces(node1.Body, node2.Body);
			}
		}

		private void UpdatePositions(double dt)
		{
			_integrator.Update(_nodesByDataContext.Values.Select(x => x.Body), dt);
		}

		public void AddNodes(IEnumerable nodes)
		{
			if (nodes == null)
				return;

			foreach (object node in nodes)
			{
				AddNode(node);
			}
		}

		public void RemoveNodes(IEnumerable nodes)
		{
			if (nodes == null)
				return;

			foreach (object node in nodes)
			{
				RemoveNode(node);
			}
		}

		public void ClearNodes()
		{
			_nodesByDataContext.Clear();
		}

		public void AddEdges(IEnumerable<IEdge> edges)
		{
			if (edges == null)
				return;

			_edges.AddRange(edges);
		}

		public void RemoveEdges(IEnumerable<IEdge> edges)
		{
			if (edges == null)
				return;

			foreach (IEdge edge in edges)
			{
				RemoveEdge(edge);
			}
		}

		public void ClearEdges()
		{
			_edges.Clear();
		}

		[Pure]
		private Node GetNode(object dataContext)
		{
			if (dataContext == null)
				return null;

			Node node;
			if (!_nodesByDataContext.TryGetValue(dataContext, out node))
				return null;

			return node;
		}

		public void AddNode(object node)
		{
			if (node == null)
				return;

			_nodesByDataContext.Add(node, new Node(node));
		}

		public void RemoveNode(object node)
		{
			if (node == null)
				return;

			_nodesByDataContext.Remove(node);
		}

		private void RemoveEdge(IEdge edge)
		{
			if (edge == null)
				return;

			_edges.Remove(edge);
		}
	}
}