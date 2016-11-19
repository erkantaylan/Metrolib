﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

// ReSharper disable CheckNamespace

namespace Metrolib
// ReSharper restore CheckNamespace
{
	/// <summary>
	/// </summary>
	public sealed class LineSeries
		: INotifyPropertyChanged
	{
		private List<Point> _actualValues;
		private int _count;
		private Brush _fill;
		private Pen _outline;
		private IEnumerable<Point> _values;

		private Range _xRange;
		private Range _yRange;

		public LineSeries()
		{
			_outline = new Pen(Brushes.DodgerBlue, 2);
		}

		public int Count
		{
			get { return _count; }
		}

		public List<Point> ActualValues
		{
			get { return _actualValues; }
			private set
			{
				if (value == _actualValues)
					return;

				_actualValues = value;
				EmitPropertyChanged();
			}
		}

		/// <summary>
		/// </summary>
		public Pen Outline
		{
			get { return _outline; }
			set
			{
				if (value == _outline)
					return;

				_outline = value;
				EmitPropertyChanged();
			}
		}

		/// <summary>
		/// </summary>
		public Brush Fill
		{
			get { return _fill; }
			set
			{
				if (value == _fill)
					return;
				_fill = value;
				EmitPropertyChanged();
			}
		}

		/// <summary>
		/// </summary>
		public IEnumerable<Point> Values
		{
			get { return _values; }
			set
			{
				if (value == _values)
					return;

				var notify = _values as INotifyCollectionChanged;
				if (notify != null)
					notify.CollectionChanged -= OnValuesChanged;

				_values = value;

				notify = Values as INotifyCollectionChanged;
				if (notify != null)
					notify.CollectionChanged += OnValuesChanged;

				EmitPropertyChanged();

				UpdateRanges();
			}
		}

		/// <summary>
		/// </summary>
		public Range XRange
		{
			get { return _xRange; }
			private set
			{
				if (value == _xRange)
					return;

				_xRange = value;
				EmitPropertyChanged();
			}
		}

		/// <summary>
		/// </summary>
		public Range YRange
		{
			get { return _yRange; }
			private set
			{
				if (value == _yRange)
					return;

				_yRange = value;
				EmitPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnValuesChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			UpdateRanges();
		}

		private void UpdateRanges()
		{
			_count = 0;
			if (Values != null)
			{
				var xRange = new Range(double.MaxValue, double.MinValue);
				var yRange = new Range(double.MaxValue, double.MinValue);
				foreach (Point point in Values)
				{
					xRange.Maximum = Math.Max(point.X, xRange.Maximum);
					xRange.Minimum = Math.Min(point.X, xRange.Minimum);

					yRange.Maximum = Math.Max(point.Y, yRange.Maximum);
					yRange.Minimum = Math.Min(point.Y, yRange.Minimum);

					++_count;
				}

				if (xRange.Maximum >= xRange.Minimum)
				{
					XRange = xRange;
					YRange = yRange;
				}
				else
				{
					XRange = new Range();
					YRange = new Range();
				}

				ActualValues = new List<Point>(Values);
				XRange = XRange;
				YRange = YRange;
			}
			else
			{
				ActualValues = new List<Point>();
				XRange = new Range();
				YRange = new Range();
			}
		}

		private void EmitPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}