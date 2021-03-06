﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

// ReSharper disable CheckNamespace

namespace Metrolib.Controls
// ReSharper restore CheckNamespace
{
	/// <summary>
	/// </summary>
	public class FlatTreeViewItem
		: TreeViewItem
	{
		/// <summary>
		///     Definition of the <see cref="IsExpandable" /> dependency property.
		/// </summary>
		public static readonly DependencyProperty IsExpandableProperty =
			DependencyProperty.Register("IsExpandable", typeof(bool), typeof(FlatTreeViewItem), new PropertyMetadata(true));

		private ITreeViewItemViewModel _model;

		static FlatTreeViewItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof (FlatTreeViewItem),
			                                         new FrameworkPropertyMetadata(typeof (FlatTreeViewItem)));
		}

		/// <inheritdoc />
		protected override void OnHeaderChanged(object oldHeader, object newHeader)
		{
			base.OnHeaderChanged(oldHeader, newHeader);

			var old = oldHeader as ITreeViewItemViewModel;
			if (old != null)
			{
				old.PropertyChanged -= HeaderOnPropertyChanged;
			}

			_model = newHeader as ITreeViewItemViewModel;
			if (_model != null)
			{
				_model.PropertyChanged += HeaderOnPropertyChanged;
				IsExpanded = _model.IsExpanded;
				IsSelected = _model.IsSelected;
			}
		}

		private void HeaderOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "IsSelected":
					IsSelected = _model.IsSelected;
					break;
				case "IsExpanded":
					IsExpanded = _model.IsExpanded;
					break;
			}
		}

		/// <inheritdoc />
		protected override void OnExpanded(RoutedEventArgs e)
		{
			base.OnExpanded(e);
			if (_model != null)
				_model.IsExpanded = true;
		}

		/// <inheritdoc />
		protected override void OnCollapsed(RoutedEventArgs e)
		{
			base.OnCollapsed(e);
			if (_model != null)
				_model.IsExpanded = false;
		}

		/// <inheritdoc />
		protected override void OnSelected(RoutedEventArgs e)
		{
			base.OnSelected(e);
			if (_model != null)
				_model.IsSelected = true;
		}

		/// <inheritdoc />
		protected override void OnUnselected(RoutedEventArgs e)
		{
			base.OnUnselected(e);
			if (_model != null)
				_model.IsSelected = false;
		}

		/// <summary>
		///     Whether or not this item can be expanded.
		///     If it can be, then a clickable arrow appears next to the presented content.
		///     If not, then this arrow is hidden.
		/// </summary>
		public bool IsExpandable
		{
			get { return (bool) GetValue(IsExpandableProperty); }
			set { SetValue(IsExpandableProperty, value); }
		}

		/// <summary>
		///     The depth of this item.
		///     Items that are children of their <see cref="FlatTreeView" /> have a depth of 0.
		/// </summary>
		public int Depth
		{
			get
			{
				int depth = 0;
				FlatTreeViewItem item = this;
				while (item != null)
				{
					item = item.ParentItem;
					++depth;
				}
				return depth - 1;
			}
		}

		private FlatTreeViewItem ParentItem
		{
			get
			{
				DependencyObject dependencyObject = this;
				do
				{
					dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
				} while (!(dependencyObject is FlatTreeViewItem || dependencyObject is FlatTreeView));
				return dependencyObject as FlatTreeViewItem;
			}
		}

		/// <summary>
		///     Creates and returns a new Metrolib.Controls.FlatTreeViewItem container.
		/// </summary>
		/// <returns></returns>
		protected override DependencyObject GetContainerForItemOverride()
		{
			var item = new FlatTreeViewItem();
			var binding = new Binding("IsExpandable")
			{
				Source = this
			};
			BindingOperations.SetBinding(item, IsExpandableProperty, binding);
			return item;
		}

		/// <summary>
		///     Determines whether an object is a Metrolib.Controls.FlatTreeViewItem.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is FlatTreeViewItem;
		}
	}
}