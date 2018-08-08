using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System.Windows.Media;

namespace AlternatingLineColorExtension
{
    /// <summary>
    /// AlternatingLineColorTextAdornment places red boxes behind all the "a"s in the editor window
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
        /// <param name="view">Text view to create the adornment for</param>
        public AlternatingLineColorTextAdornment(IWpfTextView textView)
        {
            if (textView == null)
                throw new ArgumentNullException("textView");

            var opacily = textView.Background.GetOpacity();
            _brush = new SolidColorBrush(Color.FromArgb(opacily, 194, 252, 233));
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
            //SetBrush(textView);

            foreach (var r in _layer.Elements)
            {
                ((Rectangle)r.Adornment).Width = textView.ViewportWidth;
            }
        }

        /// <summary>
        /// Handles whenever the text displayed in the view changes by adding the adornment to any reformatted lines
        /// </summary>
        /// <remarks><para>This event is raised whenever the rendered text displayed in the <see cref="ITextView"/> changes.</para>
        /// <para>It is raised whenever the view does a layout (which happens when DisplayTextLineContainingBufferPosition is called or in response to text or classification changes).</para>
        /// <para>It is also raised whenever the view scrolls horizontally or when its size changes.</para>
        /// </remarks>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            IWpfTextView textView = (IWpfTextView)sender;
            //SetBrush(textView);

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

        //private Brush GetBrush(IWpfTextView textView)
        //{
        //    byte opacity = GetOpacity(textView.Background);
        //    var brush = new SolidColorBrush(Color.FromArgb(opacity, 194, 252, 233));
        //    brush.Freeze();
        //    return brush;
        //}
    }
}
