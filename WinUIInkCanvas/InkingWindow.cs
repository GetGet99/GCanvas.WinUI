using Microsoft.UI;
using UnitedSets.Helpers;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Core;

namespace WinUIInkCanvas
{
    class InkingWindow
    {
        WindowsSystemDispatcherQueueHelper m_wsdqHelper;
        public CompositionWindow CompositionWindow { get; }
        public DispatcherQueue DispatcherQueue => Compositor.DispatcherQueue;
        public bool IsHitTestVisible
        {
            get => MainVisual.IsHitTestVisible;
            set => MainVisual.IsHitTestVisible = value;
        }
        public bool IsEraserMode
        {
            get => InkPresenter.InputProcessingConfiguration.Mode == InkInputProcessingMode.Erasing;
            set => InkPresenter.InputProcessingConfiguration.Mode = value ? InkInputProcessingMode.Erasing : InkInputProcessingMode.Inking;
        }
        ContainerVisual MainVisual;
        Compositor Compositor;
        InkDrawingAttributes DrawingAttributes;
        InkPresenter InkPresenter;
        InkPresenterRuler Ruler;
        public bool IsRulerVisible
        {
            get => Ruler.IsVisible; set => Ruler.IsVisible = value;
        }
        InkPresenterProtractor Protractor;
        public bool IsProtractorVisible
        {
            get => Protractor.IsVisible; set => Protractor.IsVisible = value;
        }
        public double? PenSize
        {
            get => DrawingAttributes.Size.Width;
            set
            {
                DrawingAttributes.Size = new(value ?? 1, value ?? 1);
                InkPresenter.UpdateDefaultDrawingAttributes(DrawingAttributes);
            }
        }
        public Color PenColor
        {
            get => DrawingAttributes.Color;
            set
            {
                DrawingAttributes.Color = value;
                InkPresenter.UpdateDefaultDrawingAttributes(DrawingAttributes);
            }
        }
        public bool HighlighterMode
        {
            get => DrawingAttributes.DrawAsHighlighter;
            set
            {
                DrawingAttributes.DrawAsHighlighter = value;
                InkPresenter.UpdateDefaultDrawingAttributes(DrawingAttributes);
            }
        }
        public InkingWindow() : this(false) { }
        public InkingWindow(bool InitializeDispatcherQueue)
        {
            if (InitializeDispatcherQueue)
            {
                m_wsdqHelper = new();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
            }
            CompositionWindow = new();
            var coreInkPresenterHost = new CoreInkPresenterHost();
            MainVisual = (Compositor = CompositionWindow.Compositor).CreateContainerVisual();

            coreInkPresenterHost.RootVisual = MainVisual;
            InkPresenter = coreInkPresenterHost.InkPresenter;
            Ruler = new(InkPresenter);
            Protractor = new(InkPresenter);
            CompositionWindow.Form.SizeChanged += delegate
            {
                UpdateSize();
            };
            UpdateSize();

            InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Pen;
            DrawingAttributes = InkPresenter.CopyDefaultDrawingAttributes();

            DrawingAttributes.Color = Colors.Black;

            DrawingAttributes.Size = new Size(10, 10);
            DrawingAttributes.DrawAsHighlighter = false;
            DrawingAttributes.PenTip = PenTipShape.Circle;

            InkPresenter.UpdateDefaultDrawingAttributes(DrawingAttributes);


            CompositionWindow.Child = MainVisual;
        }
        void UpdateSize()
        {
            MainVisual.Size = new(CompositionWindow.Form.Width * 100, CompositionWindow.Form.Height * 100);
        }
        public void Invoke(DispatcherQueueHandler handler) => DispatcherQueue.TryEnqueue(handler);

        public void ClearAllInk() => InkPresenter.StrokeContainer.Clear();
    }
}
