//------------------------------------------------------------------------------
// <copyright file="TextAdornment1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows.Media;

namespace AlternatingLineColorVisualStudioExtension
{
    /// <summary>
    /// TextAdornment1 places red boxes behind all the "a"s in the editor window
    /// </summary>
    internal sealed class AlternatingLineColorTextAdornment
    {
        /// <summary>
        /// The layer of the adornment.
        /// </summary>
        private readonly IAdornmentLayer _layer;

        ///// <summary>
        ///// Text view where the adornment is created.
        ///// </summary>
        //private readonly IWpfTextView _view;

        /// <summary>
        /// Adornment brush.
        /// </summary>
        private readonly Brush _brush;

        ///// <summary>
        ///// Adornment pen.
        ///// </summary>
        //private readonly Pen _pen;

        private readonly string _layerName = "AlternatingLineTextAdornment";

        /// <summary>
        /// Initializes a new instance of the <see cref="AlternatingLineColorTextAdornment"/> class.
        /// </summary>
        /// <param name="textView">Text view to create the adornment for</param>
        public AlternatingLineColorTextAdornment(IWpfTextView textView)
        {
            if (textView == null)
                throw new ArgumentNullException("textView");

            _brush = new SolidColorBrush(Color.FromArgb(160, 194, 252, 233));
            _brush.Freeze();
            _layer = textView.GetAdornmentLayer(_layerName);

            textView.LayoutChanged += OnLayoutChanged;
            textView.ViewportWidthChanged += OnViewportWidthChanged;
            textView.ViewportLeftChanged += OnViewportLeftChanged;
        }

        private void OnViewportLeftChanged(object sender, EventArgs e)
        {
            IWpfTextView textView = (IWpfTextView)sender;

            foreach (var r in _layer.Elements)
            {
                Canvas.SetLeft((Rectangle)r.Adornment, textView.ViewportLeft);
            }
        }

        private void OnViewportWidthChanged(object sender, EventArgs e)
        {
            IWpfTextView textView = (IWpfTextView)sender;

            foreach (var r in _layer.Elements)
            {
                ((Rectangle)r.Adornment).Width = textView.ViewportWidth;
            }
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            IWpfTextView textView = (IWpfTextView)sender;
            if (e.OldSnapshot != e.NewSnapshot && e.OldSnapshot.Version.Changes.IncludesLineChanges)
            {
                _layer.RemoveAllAdornments();
                Refresh(textView, textView.TextViewLines);
            }
            else
            {
                Refresh(textView, e.NewOrReformattedLines);
            }
        }

        private void Refresh(IWpfTextView textView, IList<ITextViewLine> lines)
        {
            foreach (var line in lines)
            {
                int lineNumber = textView.TextSnapshot.GetLineNumberFromPosition(line.Extent.Start);
                if (lineNumber % 2 == 1)
                {
                    var rect = new Rectangle()
                    {
                        Height = line.Height,
                        Width = textView.ViewportWidth,
                        Fill = _brush
                    };

                    Canvas.SetLeft(rect, textView.ViewportLeft);
                    Canvas.SetTop(rect, line.Top);
                    _layer.AddAdornment(line.Extent, null, rect);
                }
            }
        }
    }
}
