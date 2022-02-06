﻿namespace Skyline.DataMiner.DeveloperCommunityLibrary.InteractiveAutomationToolkit
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using Skyline.DataMiner.Automation;

	/// <summary>
	///     A dialog represents a single window that can be shown.
	///     You can show widgets in the window by adding them to the dialog.
	///     The dialog uses a grid to determine the layout of its widgets.
	/// </summary>
	public abstract class Dialog : IDialog
	{
		private const string Auto = "auto";
		private const string Stretch = "*";

		private readonly Dictionary<Widget, IWidgetLayout> widgetLayouts = new Dictionary<Widget, IWidgetLayout>();

		private readonly Dictionary<int, string> columnDefinitions = new Dictionary<int, string>();
		private readonly Dictionary<int, string> rowDefinitions = new Dictionary<int, string>();

		private int height;
		private int maxHeight;
		private int maxWidth;
		private int minHeight;
		private int minWidth;
		private int width;
		private bool isEnabled = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="Dialog" /> class.
		/// </summary>
		/// <param name="engine"></param>
		protected Dialog(IEngine engine)
		{
			if (engine == null)
			{
				throw new ArgumentNullException("engine");
			}

			Engine = engine;
			width = -1;
			height = -1;
			MaxHeight = Int32.MaxValue;
			MinHeight = 1;
			MaxWidth = Int32.MaxValue;
			MinWidth = 1;
			RowCount = 0;
			ColumnCount = 0;
			Title = "Dialog";
			AllowOverlappingWidgets = false;
		}

		/// <inheritdoc />
		public bool AllowOverlappingWidgets { get; set; }

		/// <inheritdoc />
		public event EventHandler<EventArgs> Back;

		/// <inheritdoc />
		public event EventHandler<EventArgs> Forward;

		/// <inheritdoc />
		public event EventHandler<EventArgs> Interacted;

		/// <inheritdoc />
		public int ColumnCount { get; private set; }

		/// <inheritdoc />
		public IEngine Engine { get; private set; }

		/// <inheritdoc />
		public int Height
		{
			get
			{
				return height;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}

				height = value;
			}
		}

		/// <inheritdoc />
		public int MaxHeight
		{
			get
			{
				return maxHeight;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}

				maxHeight = value;
			}
		}

		/// <inheritdoc />
		public int MaxWidth
		{
			get
			{
				return maxWidth;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}

				maxWidth = value;
			}
		}

		/// <inheritdoc />
		public int MinHeight
		{
			get
			{
				return minHeight;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}

				minHeight = value;
			}
		}

		/// <inheritdoc />
		public int MinWidth
		{
			get
			{
				return minWidth;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}

				minWidth = value;
			}
		}

		/// <inheritdoc />
		public int RowCount { get; private set; }

		/// <inheritdoc />
		public bool IsEnabled
		{
			get
			{
				return isEnabled;
			}

			set
			{
				isEnabled = value;
				foreach (Widget widget in Widgets)
				{
					InteractiveWidget interactiveWidget = widget as InteractiveWidget;
					if (interactiveWidget != null && !(interactiveWidget is CollapseButton))
					{
						interactiveWidget.IsEnabled = isEnabled;
					}
				}
			}
		}

		/// <inheritdoc />
		public string Title { get; set; }

		/// <inheritdoc />
		public IReadOnlyCollection<Widget> Widgets
		{
			get
			{
				return widgetLayouts.Keys;
			}
		}

		/// <inheritdoc />
		public int Width
		{
			get
			{
				return width;
			}

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value");
				}

				width = value;
			}
		}

		/// <inheritdoc />
		public IDialog AddWidget(Widget widget, IWidgetLayout widgetLayout)
		{
			if (widget == null)
			{
				throw new ArgumentNullException("widget");
			}

			if (widgetLayouts.ContainsKey(widget))
			{
				throw new ArgumentException("Widget is already added to the dialog");
			}

			widgetLayouts.Add(widget, widgetLayout);

			SortedSet<int> rowsInUse;
			SortedSet<int> columnsInUse;
			this.FillRowsAndColumnsInUse(out rowsInUse, out columnsInUse);

			return this;
		}

		/// <inheritdoc />
		public IDialog AddWidget(
			Widget widget,
			int row,
			int column,
			HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment verticalAlignment = VerticalAlignment.Center)
		{
			AddWidget(widget, new WidgetLayout(row, column, horizontalAlignment, verticalAlignment));
			return this;
		}

		/// <inheritdoc />
		public IDialog AddWidget(
			Widget widget,
			int fromRow,
			int fromColumn,
			int rowSpan,
			int colSpan,
			HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
			VerticalAlignment verticalAlignment = VerticalAlignment.Center)
		{
			AddWidget(
				widget,
				new WidgetLayout(fromRow, fromColumn, rowSpan, colSpan, horizontalAlignment, verticalAlignment));
			return this;
		}

		/// <inheritdoc />
		public IWidgetLayout GetWidgetLayout(Widget widget)
		{
			CheckWidgetExists(widget);
			return widgetLayouts[widget];
		}

		/// <inheritdoc />
		public void RemoveWidget(Widget widget)
		{
			if (widget == null)
			{
				throw new ArgumentNullException("widget");
			}

			widgetLayouts.Remove(widget);

			SortedSet<int> rowsInUse;
			SortedSet<int> columnsInUse;
			this.FillRowsAndColumnsInUse(out rowsInUse, out columnsInUse);
		}

		/// <inheritdoc />
		public IDialog AddSection(Section section, SectionLayout layout)
		{
			foreach(Widget widget in section.Widgets)
			{
				IWidgetLayout widgetLayout = section.GetWidgetLayout(widget);
				AddWidget(
					widget,
					new WidgetLayout(
						widgetLayout.Row + layout.Row,
						widgetLayout.Column + layout.Column,
						widgetLayout.RowSpan,
						widgetLayout.ColumnSpan,
						widgetLayout.HorizontalAlignment,
						widgetLayout.VerticalAlignment));
			}

			return this;
		}

		/// <inheritdoc />
		public IDialog AddSection(Section section, int fromRow, int fromColumn)
		{
			return AddSection(section, new SectionLayout(fromRow, fromColumn));
		}

		/// <inheritdoc />
		public void SetColumnWidth(int column, int columnWidth)
		{
			if (column < 0) throw new ArgumentOutOfRangeException("column");
			if (columnWidth < 0) throw new ArgumentOutOfRangeException("columnWidth");

			if (columnDefinitions.ContainsKey(column)) columnDefinitions[column] = columnWidth.ToString();
			else columnDefinitions.Add(column, columnWidth.ToString());
		}

		/// <inheritdoc />
		public void SetColumnWidthAuto(int column)
		{
			if (column < 0) throw new ArgumentOutOfRangeException("column");

			if (columnDefinitions.ContainsKey(column)) columnDefinitions[column] = Auto;
			else columnDefinitions.Add(column, Auto);
		}

		/// <inheritdoc />
		public void SetColumnWidthStretch(int column)
		{
			if (column < 0) throw new ArgumentOutOfRangeException("column");

			if (columnDefinitions.ContainsKey(column)) columnDefinitions[column] = Stretch;
			else columnDefinitions.Add(column, Stretch);
		}

		/// <inheritdoc />
		public void SetRowHeight(int row, int rowHeight)
		{
			if (row < 0) throw new ArgumentOutOfRangeException("row");
			if (rowHeight <= 0) throw new ArgumentOutOfRangeException("rowHeight");

			if (rowDefinitions.ContainsKey(row)) rowDefinitions[row] = rowHeight.ToString();
			else rowDefinitions.Add(row, rowHeight.ToString());
		}

		/// <inheritdoc />
		public void SetRowHeightAuto(int row)
		{
			if (row < 0) throw new ArgumentOutOfRangeException("row");

			if (rowDefinitions.ContainsKey(row)) rowDefinitions[row] = Auto;
			else rowDefinitions.Add(row, Auto);
		}

		/// <inheritdoc />
		public void SetRowHeightStretch(int row)
		{
			if (row < 0) throw new ArgumentOutOfRangeException("row");

			if (rowDefinitions.ContainsKey(row)) rowDefinitions[row] = Stretch;
			else rowDefinitions.Add(row, Stretch);
		}

		/// <inheritdoc />
		public void SetWidgetLayout(Widget widget, IWidgetLayout widgetLayout)
		{
			CheckWidgetExists(widget);
			widgetLayouts[widget] = widgetLayout;
		}

		/// <inheritdoc />
		public void Show(bool requireResponse = true)
		{
			UIBuilder uib = Build();
			uib.RequireResponse = requireResponse;

			UIResults uir = Engine.ShowUI(uib);

			if (requireResponse)
			{
				LoadChanges(uir);
				RaiseResultEvents(uir);
			}
		}

		/// <inheritdoc />
		public void Clear()
		{
			widgetLayouts.Clear();
			RowCount = 0;
			ColumnCount = 0;
		}

		private static string AlignmentToUiString(HorizontalAlignment horizontalAlignment)
		{
			switch (horizontalAlignment)
			{
				case HorizontalAlignment.Center:
					return "Center";
				case HorizontalAlignment.Left:
					return "Left";
				case HorizontalAlignment.Right:
					return "Right";
				case HorizontalAlignment.Stretch:
					return "Stretch";
				default:
					throw new InvalidEnumArgumentException(
						"horizontalAlignment",
						(int)horizontalAlignment,
						typeof(HorizontalAlignment));
			}
		}

		private static string AlignmentToUiString(VerticalAlignment verticalAlignment)
		{
			switch (verticalAlignment)
			{
				case VerticalAlignment.Center:
					return "Center";
				case VerticalAlignment.Top:
					return "Top";
				case VerticalAlignment.Bottom:
					return "Bottom";
				case VerticalAlignment.Stretch:
					return "Stretch";
				default:
					throw new InvalidEnumArgumentException(
						"verticalAlignment",
						(int)verticalAlignment,
						typeof(VerticalAlignment));
			}
		}

		/// <summary>
		/// Checks if any visible widgets in the Dialog overlap.
		/// </summary>
		/// <exception cref="OverlappingWidgetsException">Thrown when two visible widgets overlap with each other.</exception>
		private void CheckIfVisibleWidgetsOverlap()
		{
			if (AllowOverlappingWidgets) return;

			foreach(Widget widget in widgetLayouts.Keys)
			{
				if (!widget.IsVisible) continue;

				IWidgetLayout widgetLayout = widgetLayouts[widget];
				for (int column = widgetLayout.Column; column < widgetLayout.Column + widgetLayout.ColumnSpan; column++)
				{
					for (int row = widgetLayout.Row; row < widgetLayout.Row + widgetLayout.RowSpan; row++)
					{
						foreach(Widget otherWidget in widgetLayouts.Keys)
						{
							if (!otherWidget.IsVisible || widget.Equals(otherWidget)) continue;

							IWidgetLayout otherWidgetLayout = widgetLayouts[otherWidget];
							if (column >= otherWidgetLayout.Column && column < otherWidgetLayout.Column + otherWidgetLayout.ColumnSpan && row >= otherWidgetLayout.Row && row < otherWidgetLayout.Row + otherWidgetLayout.RowSpan)
							{
								throw new OverlappingWidgetsException(String.Format("The widget overlaps with another widget in the Dialog on Row {0}, Column {1}, RowSpan {2}, ColumnSpan {3}", widgetLayout.Row, widgetLayout.Column, widgetLayout.RowSpan, widgetLayout.ColumnSpan));
							}
						}
					}
				}
			}
		}

		private string GetRowDefinitions(SortedSet<int> rowsInUse)
		{
			string[] definitions = new string[rowsInUse.Count];
			int currentIndex = 0;
			foreach (int rowInUse in rowsInUse)
			{
				string value;
				if (rowDefinitions.TryGetValue(rowInUse, out value))
				{
					definitions[currentIndex] = value;
				}
				else
				{
					definitions[currentIndex] = Auto;
				}

				currentIndex++;
			}

			return String.Join(";", definitions);
		}

		private string GetColumnDefinitions(SortedSet<int> columnsInUse)
		{
			string[] definitions = new string[columnsInUse.Count];
			int currentIndex = 0;
			foreach (int columnInUse in columnsInUse)
			{
				string value;
				if (columnDefinitions.TryGetValue(columnInUse, out value))
				{
					definitions[currentIndex] = value;
				}
				else
				{
					definitions[currentIndex] = Auto;
				}

				currentIndex++;
			}

			return String.Join(";", definitions);
		}

		private UIBuilder Build()
		{
			// Check rows and columns in use
			SortedSet<int> rowsInUse;
			SortedSet<int> columnsInUse;
			this.FillRowsAndColumnsInUse(out rowsInUse, out columnsInUse);

			// Check if visible widgets overlap and throw exception if this is the case
			CheckIfVisibleWidgetsOverlap();

			// Initialize UI Builder
			var uiBuilder = new UIBuilder
			{
				Height = Height,
				MinHeight = MinHeight,
				Width = Width,
				MinWidth = MinWidth,
				RowDefs = GetRowDefinitions(rowsInUse),
				ColumnDefs = GetColumnDefinitions(columnsInUse),
				Title = Title
			};

			KeyValuePair<Widget, IWidgetLayout> defaultKeyValuePair = default(KeyValuePair<Widget, IWidgetLayout>);
			int rowIndex = 0;
			int columnIndex = 0;
			foreach (int rowInUse in rowsInUse)
			{
				columnIndex = 0;
				foreach (int columnInUse in columnsInUse)
				{
					foreach (KeyValuePair<Widget, IWidgetLayout> keyValuePair in widgetLayouts.Where(x => x.Key.IsVisible && x.Key.Type != UIBlockType.Undefined && x.Value.Row.Equals(rowInUse) && x.Value.Column.Equals(columnInUse)))
					{
						if (keyValuePair.Equals(defaultKeyValuePair)) continue;

						// Can be removed once we retrieve all collapsed states from the UI
						TreeView treeView = keyValuePair.Key as TreeView;
						if (treeView != null) treeView.UpdateItemCache();

						UIBlockDefinition widgetBlockDefinition = keyValuePair.Key.BlockDefinition;
						IWidgetLayout widgetLayout = keyValuePair.Value;

						widgetBlockDefinition.Column = columnIndex;
						widgetBlockDefinition.ColumnSpan = widgetLayout.ColumnSpan;
						widgetBlockDefinition.Row = rowIndex;
						widgetBlockDefinition.RowSpan = widgetLayout.RowSpan;
						widgetBlockDefinition.HorizontalAlignment = AlignmentToUiString(widgetLayout.HorizontalAlignment);
						widgetBlockDefinition.VerticalAlignment = AlignmentToUiString(widgetLayout.VerticalAlignment);
						widgetBlockDefinition.Margin = widgetLayout.Margin.ToString();

						uiBuilder.AppendBlock(widgetBlockDefinition);
					}

					columnIndex++;
				}

				rowIndex++;
			}

			return uiBuilder;
		}

		/// <summary>
		/// Used to retrieve the rows and columns that are being used and updates the RowCount and ColumnCount properties based on the Widgets added to the dialog.
		/// </summary>
		/// <param name="rowsInUse">Collection containing the rows that are defined by the Widgets in the Dialog.</param>
		/// <param name="columnsInUse">Collection containing the columns that are defined by the Widgets in the Dialog.</param>
		private void FillRowsAndColumnsInUse(out SortedSet<int> rowsInUse, out SortedSet<int> columnsInUse)
		{
			rowsInUse = new SortedSet<int>();
			columnsInUse = new SortedSet<int>();
			foreach (KeyValuePair<Widget, IWidgetLayout> keyValuePair in this.widgetLayouts)
			{
				if (keyValuePair.Key.IsVisible && keyValuePair.Key.Type != UIBlockType.Undefined)
				{
					for (int i = keyValuePair.Value.Row; i < keyValuePair.Value.Row + keyValuePair.Value.RowSpan; i++)
					{
						rowsInUse.Add(i);
					}

					for (int i = keyValuePair.Value.Column; i < keyValuePair.Value.Column + keyValuePair.Value.ColumnSpan; i++)
					{
						columnsInUse.Add(i);
					}
				}
			}

			this.RowCount = rowsInUse.Count;
			this.ColumnCount = columnsInUse.Count;
		}

		// ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
		private void CheckWidgetExists(Widget widget)
		{
			if (widget == null)
			{
				throw new ArgumentNullException("widget");
			}

			if (!widgetLayouts.ContainsKey(widget))
			{
				throw new ArgumentException("Widget is not part of this dialog");
			}
		}

		private void LoadChanges(UIResults uir)
		{
			foreach (InteractiveWidget interactiveWidget in Widgets.OfType<InteractiveWidget>())
			{
				if (interactiveWidget.IsVisible)
				{
					interactiveWidget.LoadResult(uir);
				}
			}
		}

		private void RaiseResultEvents(UIResults uir)
		{
			if (Interacted != null)
			{
				Interacted(this, EventArgs.Empty);
			}

			if (uir.WasBack() && (Back != null))
			{
				Back(this, EventArgs.Empty);
				return;
			}

			if (uir.WasForward() && (Forward != null))
			{
				Forward(this, EventArgs.Empty);
				return;
			}

			// ToList is necessary to prevent InvalidOperationException when adding or removing widgets from a event handler.
			List<InteractiveWidget> intractableWidgets = Widgets.OfType<InteractiveWidget>()
				.Where(widget => widget.WantsOnChange).ToList();

			foreach (InteractiveWidget intractable in intractableWidgets)
			{
				intractable.RaiseResultEvents();
			}
		}
	}
}