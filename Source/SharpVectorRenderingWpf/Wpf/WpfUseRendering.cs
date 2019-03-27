﻿using System;
using System.Xml;
using System.Windows;
using System.Windows.Media;

using SharpVectors.Dom.Svg;
using SharpVectors.Runtime;

namespace SharpVectors.Renderers.Wpf
{
    public sealed class WpfUseRendering : WpfRendering
    {
        #region Private Fields

        private DrawingGroup _drawGroup;

        #endregion

        #region Constructors and Destructor

        public WpfUseRendering(SvgElement element)
            : base(element)
        {
        }

        #endregion

        #region Public Methods

        public override void BeforeRender(WpfDrawingRenderer renderer)
        {
            base.BeforeRender(renderer);

            WpfDrawingContext context = renderer.Context;

            Geometry clipGeom   = this.ClipGeometry;
            Transform transform = this.Transform;

            SvgUseElement useElement = (SvgUseElement)_svgElement;

            if (transform == null && 
                (_svgElement.FirstChild != null && _svgElement.FirstChild == _svgElement.LastChild))
            {
                try
                {
                    // If none of the following attribute exists, an exception is thrown...
                    double x      = useElement.X.AnimVal.Value;
                    double y      = useElement.Y.AnimVal.Value;
                    double width  = useElement.Width.AnimVal.Value;
                    double height = useElement.Height.AnimVal.Value;
                    if (width > 0 && height > 0)
                    {
                        Rect elementBounds = new Rect(x, y, width, height);

                        // Try handling the cases of "symbol" and "svg" sources within the "use"...
                        XmlNode childNode = _svgElement.FirstChild;
                        string childName  = childNode.Name;
                        if (string.Equals(childName, "symbol", StringComparison.OrdinalIgnoreCase))
                        {
                            SvgSymbolElement symbolElement = (SvgSymbolElement)childNode;

                            this.FitToViewbox(context, symbolElement, elementBounds);
                        }
                    }

                    transform = this.Transform;
                }
                catch
                {                   	
                }
            }
            else if (transform != null)
            {
                try
                {
                    // If none of the following attribute exists, an exception is thrown...
                    double x      = useElement.X.AnimVal.Value;
                    double y      = useElement.Y.AnimVal.Value;
                    double width  = useElement.Width.AnimVal.Value;
                    double height = useElement.Height.AnimVal.Value;
                    if (width > 0 && height > 0)
                    {
                        Rect elementBounds = new Rect(x, y, width, height);

                        // Try handling the cases of "symbol" and "svg" sources within the "use"...
                        XmlNode childNode = _svgElement.FirstChild;
                        string childName = childNode.Name;
                        if (string.Equals(childName, "symbol", StringComparison.OrdinalIgnoreCase))
                        {
                            SvgSymbolElement symbolElement = (SvgSymbolElement)childNode;

                            this.FitToViewbox(context, symbolElement, elementBounds);
                        }
                    }

                    transform = Combine(this.Transform, transform, true);
                }
                catch
                {
                }
            }

            string elementId = this.GetElementName();

            float opacityValue = -1;

            string opacity = useElement.GetAttribute("opacity");
            if (string.IsNullOrWhiteSpace(opacity))
            {
                opacity = useElement.GetPropertyValue("opacity");
            }
            if (opacity != null && opacity.Length > 0)
            {
                opacityValue = (float)SvgNumber.ParseNumber(opacity);
                opacityValue = Math.Min(opacityValue, 1);
                opacityValue = Math.Max(opacityValue, 0);
            }

            if (clipGeom != null || transform != null || opacityValue >= 0 ||
                (!string.IsNullOrWhiteSpace(elementId) && !context.IsRegisteredId(elementId)))
            {
                _drawGroup = new DrawingGroup();

                DrawingGroup currentGroup = context.Peek();

                if (currentGroup == null)
                {
                    throw new InvalidOperationException("An existing group is expected.");
                }

                currentGroup.Children.Add(_drawGroup);
                context.Push(_drawGroup);

                if (clipGeom != null)
                {
                    _drawGroup.ClipGeometry = clipGeom;
                }

                if (transform != null)
                {
                    _drawGroup.Transform = transform;
                }

                if (opacityValue >= 0)
                {
                    _drawGroup.Opacity = opacityValue;
                }

                if (!string.IsNullOrWhiteSpace(elementId) && !context.IsRegisteredId(elementId))
                {
                    SvgObject.SetName(_drawGroup, elementId);

                    context.RegisterId(elementId);

                    if (context.IncludeRuntime)
                    {
                        SvgObject.SetId(_drawGroup, elementId);
                    }
                }
            }
        }

        public override void Render(WpfDrawingRenderer renderer)
        {
            base.Render(renderer);
        }

        public override void AfterRender(WpfDrawingRenderer renderer)
        {
            if (_drawGroup != null)
            {
                WpfDrawingContext context = renderer.Context;

                DrawingGroup currentGroup = context.Peek();

                if (currentGroup == null || currentGroup != _drawGroup)
                {
                    throw new InvalidOperationException("An existing group is expected.");
                }

                context.Pop();
            }

            base.AfterRender(renderer);
        }

        #endregion
    }
}
