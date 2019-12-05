﻿using System;
using System.Diagnostics;
using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    public class ColumnMajorUniformToLargestGridLayout : NonVirtualizingLayout
    {
        public ColumnMajorUniformToLargestGridLayout()
        {
        }

        #region ColumnSpacing

        public static readonly DependencyProperty ColumnSpacingProperty =
            DependencyProperty.Register(
                nameof(ColumnSpacing),
                typeof(double),
                typeof(ColumnMajorUniformToLargestGridLayout),
                new FrameworkPropertyMetadata(OnColumnSpacingPropertyChanged));

        public double ColumnSpacing
        {
            get => (double)GetValue(ColumnSpacingProperty);
            set => SetValue(ColumnSpacingProperty, value);
        }

        private static void OnColumnSpacingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (ColumnMajorUniformToLargestGridLayout)sender;
            owner.OnColumnSpacingPropertyChanged(args);
        }

        private void OnColumnSpacingPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            InvalidateMeasure();
        }

        #endregion

        #region RowSpacing

        public static readonly DependencyProperty RowSpacingProperty =
            DependencyProperty.Register(
                nameof(RowSpacing),
                typeof(double),
                typeof(ColumnMajorUniformToLargestGridLayout),
                new FrameworkPropertyMetadata(OnRowSpacingPropertyChanged));

        public double RowSpacing
        {
            get => (double)GetValue(RowSpacingProperty);
            set => SetValue(RowSpacingProperty, value);
        }

        private static void OnRowSpacingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (ColumnMajorUniformToLargestGridLayout)sender;
            owner.OnRowSpacingPropertyChanged(args);
        }

        private void OnRowSpacingPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            InvalidateMeasure();
        }

        #endregion

        #region MaxColumns

        public static readonly DependencyProperty MaxColumnsProperty =
            DependencyProperty.Register(
                nameof(MaxColumns),
                typeof(int),
                typeof(ColumnMajorUniformToLargestGridLayout),
                new FrameworkPropertyMetadata(1, OnMaxColumnsPropertyChanged),
                ValidateMaxColumns);

        public int MaxColumns
        {
            get => (int)GetValue(MaxColumnsProperty);
            set => SetValue(MaxColumnsProperty, value);
        }

        private static void OnMaxColumnsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (ColumnMajorUniformToLargestGridLayout)sender;
            owner.OnMaxColumnsPropertyChanged(args);
        }

        private void OnMaxColumnsPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            InvalidateMeasure();
        }

        private static bool ValidateMaxColumns(object value)
        {
            return (int)value > 0;
        }

        #endregion

        protected internal override Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
        {
            var children = context.Children;
            if (children != null)
            {
                var maxColumns = Math.Max(1, MaxColumns);
                Debug.Assert(maxColumns > 0);
                var maxItemsPerColumn = (int)Math.Ceiling((double)children.Count / (double)maxColumns);

                Size calculateLargestChildSize()
                {
                    var largestChildWidth = 0.0;
                    var largestChildHeight = 0.0;
                    foreach (var child in children)
                    {
                        child.Measure(availableSize);
                        var desiredSize = child.DesiredSize;
                        if (desiredSize.Width > largestChildWidth)
                        {
                            largestChildWidth = desiredSize.Width;
                        }
                        if (desiredSize.Height > largestChildHeight)
                        {
                            largestChildHeight = desiredSize.Height;
                        }
                    }
                    return new Size(largestChildWidth, largestChildHeight);
                };
                m_largestChildSize = calculateLargestChildSize();

                var actualColumnCount = Math.Min(
                    maxColumns,
                    children.Count);
                return new Size(
                    (m_largestChildSize.Width * actualColumnCount) +
                    (ColumnSpacing * (actualColumnCount - 1)),
                    (m_largestChildSize.Height * maxItemsPerColumn) +
                    (RowSpacing * (maxItemsPerColumn - 1))
                );
            }
            return new Size(0, 0);
        }

        protected internal override Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize)
        {
            var children = context.Children;
            if (children != null)
            {
                var maxColumns = Math.Max(1, MaxColumns);
                Debug.Assert(maxColumns > 0);
                var itemCount = children.Count;
                var minitemsPerColumn = (int)Math.Floor((double)itemCount / (double)maxColumns);
                var numberOfColumnsWithExtraElements = (int)(itemCount % maxColumns);

                var columnSpacing = ColumnSpacing;
                var rowSpacing = RowSpacing;

                var horizontalOffset = 0.0;
                var verticalOffset = 0.0;
                var index = 0;
                var column = 0;
                foreach (var child in children)
                {
                    var desiredSize = child.DesiredSize;
                    child.Arrange(new Rect(horizontalOffset, verticalOffset, desiredSize.Width, desiredSize.Height));
                    if (column < numberOfColumnsWithExtraElements)
                    {
                        if (index % (minitemsPerColumn + 1) == minitemsPerColumn)
                        {
                            horizontalOffset += m_largestChildSize.Width + columnSpacing;
                            verticalOffset = 0.0;
                            column++;
                        }
                        else
                        {
                            verticalOffset += m_largestChildSize.Height + rowSpacing;
                        }
                    }
                    else
                    {
                        var indexAfterExtraLargeColumns = index - (numberOfColumnsWithExtraElements * (minitemsPerColumn + 1));
                        if (indexAfterExtraLargeColumns % minitemsPerColumn == minitemsPerColumn - 1)
                        {
                            horizontalOffset += m_largestChildSize.Width + columnSpacing;
                            verticalOffset = 0.0;
                            column++;
                        }
                        else
                        {
                            verticalOffset += m_largestChildSize.Height + rowSpacing;
                        }
                    }
                    index++;
                }

            }
            return finalSize;
        }

        Size m_largestChildSize;
    }
}
