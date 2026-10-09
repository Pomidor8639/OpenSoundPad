using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenSoundPad;

public partial class KnobControl : UserControl
{
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(KnobControl),
            new PropertyMetadata(0.0, OnPropertyChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(KnobControl),
            new PropertyMetadata(100.0, OnPropertyChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(KnobControl),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty DefaultValueProperty =
        DependencyProperty.Register(nameof(DefaultValue), typeof(double), typeof(KnobControl),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(KnobControl),
            new PropertyMetadata("", (d, e) => ((KnobControl)d).TitleText.Text = (string)e.NewValue));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(KnobControl),
            new PropertyMetadata("", (d, e) => ((KnobControl)d).RangeText.Text = (string)e.NewValue));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(KnobControl),
            new PropertyMetadata("", OnPropertyChanged));

    public static readonly DependencyProperty FormatProperty =
        DependencyProperty.Register(nameof(Format), typeof(string), typeof(KnobControl),
            new PropertyMetadata("0", OnPropertyChanged));

    public static readonly DependencyProperty IsBipolarProperty =
        DependencyProperty.Register(nameof(IsBipolar), typeof(bool), typeof(KnobControl),
            new PropertyMetadata(false, OnPropertyChanged));

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(nameof(Step), typeof(double), typeof(KnobControl),
            new PropertyMetadata(1.0));

    public event RoutedPropertyChangedEventHandler<double>? ValueChanged;

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double DefaultValue
    {
        get => (double)GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public bool IsBipolar
    {
        get => (bool)GetValue(IsBipolarProperty);
        set => SetValue(IsBipolarProperty, value);
    }

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    private enum DragMode
    {
        None,
        Detecting,
        Linear,
        Angular
    }

    private bool _isDragging;
    private DragMode _activeDragMode = DragMode.None;
    private Point _dragStartPos;
    private Point _lastPos;
    private double _internalValue;
    private double _lastAngle;

    private const double CenterX = 36.0;
    private const double CenterY = 36.0;
    private const double Radius = 27.0;
    private const double StartAngle = -135.0;
    private const double SweepAngle = 270.0;

    public KnobControl()
    {
        InitializeComponent();
        Loaded += (_, _) => UpdateVisuals();
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KnobControl k) k.UpdateVisuals();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is KnobControl k)
        {
            if (!k._isDragging)
            {
                k._internalValue = (double)e.NewValue;
            }
            k.UpdateVisuals();
            k.ValueChanged?.Invoke(k, new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue));
        }
    }

    private static Point AngleToPoint(double angleDeg)
    {
        double rad = angleDeg * Math.PI / 180.0;
        return new Point(CenterX + Radius * Math.Sin(rad), CenterY - Radius * Math.Cos(rad));
    }

    private static double PointToAngle(Point p)
    {
        double dx = p.X - CenterX;
        double dy = p.Y - CenterY;
        return Math.Atan2(dx, -dy) * (180.0 / Math.PI);
    }

    private static double AngleToFraction(double angleDeg)
    {
        if (angleDeg < -135.0) return 0.0;
        if (angleDeg > 135.0) return 1.0;
        return (angleDeg - StartAngle) / SweepAngle;
    }

    private static Geometry CreateArcGeometry(double fromAngle, double toAngle)
    {
        double sweep = toAngle - fromAngle;
        if (Math.Abs(sweep) < 0.3) return Geometry.Empty;

        bool isLarge = Math.Abs(sweep) > 180.0;
        SweepDirection dir = sweep >= 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;

        Point pStart = AngleToPoint(fromAngle);
        Point pEnd = AngleToPoint(toAngle);

        var figure = new PathFigure
        {
            StartPoint = pStart,
            IsClosed = false
        };
        figure.Segments.Add(new ArcSegment(pEnd, new Size(Radius, Radius), 0, isLarge, dir, true));

        var geom = new PathGeometry();
        geom.Figures.Add(figure);
        return geom;
    }

    private void UpdateVisuals()
    {
        if (TrackArc == null || ActiveArc == null || PointerRotation == null || ValueText == null) return;

        double range = Maximum - Minimum;
        if (range <= 0.0001) range = 1.0;

        double clampedVal = Math.Clamp(Value, Minimum, Maximum);
        double fraction = (clampedVal - Minimum) / range;
        double currentAngle = StartAngle + fraction * SweepAngle;

        // Фоновый трек (все 270 градусов)
        TrackArc.Data = CreateArcGeometry(StartAngle, StartAngle + SweepAngle);

        // Активный цветной трек
        if (IsBipolar)
        {
            double centerFraction = (0.0 - Minimum) / range;
            double centerAngle = StartAngle + centerFraction * SweepAngle;
            ActiveArc.Data = CreateArcGeometry(centerAngle, currentAngle);
        }
        else
        {
            ActiveArc.Data = CreateArcGeometry(StartAngle, currentAngle);
        }

        // Поворот указателя
        PointerRotation.Angle = currentAngle;

        // Текст значения
        string unitStr = string.IsNullOrEmpty(Unit) ? "" : " " + Unit;
        if (!string.IsNullOrEmpty(Format))
        {
            ValueText.Text = clampedVal.ToString(Format) + unitStr;
        }
        else
        {
            ValueText.Text = clampedVal.ToString("0.0") + unitStr;
        }
    }

    private void ApplyValue(double rawVal)
    {
        double clamped = Math.Clamp(rawVal, Minimum, Maximum);
        if (Step > 0.0001)
        {
            double steps = Math.Round((clamped - Minimum) / Step);
            clamped = Math.Clamp(Minimum + steps * Step, Minimum, Maximum);
        }
        Value = clamped;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        Focus();
        CaptureMouse();
        _isDragging = true;
        _dragStartPos = e.GetPosition(DialCanvas);
        _lastPos = _dragStartPos;
        _internalValue = Value;

        double dx = _dragStartPos.X - CenterX;
        double dy = _dragStartPos.Y - CenterY;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        // Если кликнули на дугу циферблата (кольцо шкалы) — мгновенно переставляем на это значение
        if (dist >= 18.0 && dist <= 42.0)
        {
            _activeDragMode = DragMode.Angular;
            _lastAngle = PointToAngle(_dragStartPos);
            double frac = AngleToFraction(_lastAngle);
            double range = Maximum - Minimum;
            _internalValue = Minimum + frac * range;
            ApplyValue(_internalValue);
        }
        else
        {
            _activeDragMode = DragMode.Detecting;
            _lastAngle = PointToAngle(_dragStartPos);
        }

        DialBody.Stroke = (Brush)FindResource("AccentGreen");
        DialBody.Fill = new SolidColorBrush(Color.FromRgb(0x1C, 0x1C, 0x1C));
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_isDragging || !IsMouseCaptured) return;

        Point current = e.GetPosition(DialCanvas);
        double range = Maximum - Minimum;
        if (range <= 0.0001) range = 1.0;

        bool isShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

        // Определение режима перемещения (линейный вертикальный или круговой)
        if (_activeDragMode == DragMode.Detecting)
        {
            double totalDist = (current - _dragStartPos).Length;
            if (totalDist >= 3.0)
            {
                double totalDy = Math.Abs(_dragStartPos.Y - current.Y);
                double totalDx = Math.Abs(current.X - _dragStartPos.X);

                if (totalDy >= 1.2 * totalDx)
                {
                    _activeDragMode = DragMode.Linear;
                }
                else
                {
                    _activeDragMode = DragMode.Angular;
                    _lastAngle = PointToAngle(current);
                }
            }
            else
            {
                return;
            }
        }

        if (_activeDragMode == DragMode.Linear)
        {
            // Плавное вертикальное перемещение (вверх = рост, вниз = спад)
            double dy = _lastPos.Y - current.Y;
            double dx = current.X - _lastPos.X;

            // Доминантная ось (горизонтальное смещение не сбивает вертикальный поворот)
            double delta = Math.Abs(dy) >= Math.Abs(dx) ? dy : dx;

            double sensitivity = isShift ? 600.0 : 180.0;
            double deltaVal = (delta / sensitivity) * range;

            _internalValue = Math.Clamp(_internalValue + deltaVal, Minimum, Maximum);
            ApplyValue(_internalValue);
        }
        else if (_activeDragMode == DragMode.Angular)
        {
            // Плавное круговое вращение циферблата
            double currentAngle = PointToAngle(current);
            double deltaAngle = currentAngle - _lastAngle;

            if (deltaAngle > 180.0) deltaAngle -= 360.0;
            else if (deltaAngle < -180.0) deltaAngle += 360.0;

            if (Math.Abs(deltaAngle) < 90.0)
            {
                double sensitivityFactor = isShift ? 0.25 : 1.0;
                double deltaVal = (deltaAngle / SweepAngle) * range * sensitivityFactor;

                _internalValue = Math.Clamp(_internalValue + deltaVal, Minimum, Maximum);
                ApplyValue(_internalValue);
            }

            _lastAngle = currentAngle;
        }

        _lastPos = current;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (_isDragging)
        {
            Point current = e.GetPosition(DialCanvas);
            double totalDist = (current - _dragStartPos).Length;

            // Клик без перемещения по циферблату переставляет значение
            if (totalDist < 3.0 && _activeDragMode == DragMode.Detecting)
            {
                double dx = current.X - CenterX;
                double dy = current.Y - CenterY;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                if (dist >= 12.0 && dist <= 42.0)
                {
                    double clickAngle = PointToAngle(current);
                    double frac = AngleToFraction(clickAngle);
                    double range = Maximum - Minimum;
                    _internalValue = Minimum + frac * range;
                    ApplyValue(_internalValue);
                }
            }

            _isDragging = false;
            _activeDragMode = DragMode.None;
            ReleaseMouseCapture();

            DialBody.Stroke = IsMouseOver ? new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)) : new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
            DialBody.Fill = IsMouseOver ? new SolidColorBrush(Color.FromRgb(0x18, 0x18, 0x18)) : new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x11));
        }
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        if (_isDragging)
        {
            _isDragging = false;
            _activeDragMode = DragMode.None;
            DialBody.Stroke = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
            DialBody.Fill = new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x11));
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        double range = Maximum - Minimum;
        double step = Step > 0.0001 ? Step : (range / 50.0);
        bool isShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        if (isShift && Step <= 0.0001) step /= 4.0;

        double delta = e.Delta > 0 ? step : -step;
        _internalValue = Math.Clamp(Value + delta, Minimum, Maximum);
        ApplyValue(_internalValue);
        e.Handled = true;
    }

    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);
        _internalValue = DefaultValue;
        ApplyValue(DefaultValue);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        double range = Maximum - Minimum;
        double step = Step > 0.0001 ? Step : (range / 50.0);
        bool isShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        if (isShift && Step <= 0.0001) step /= 4.0;

        if (e.Key == Key.Up || e.Key == Key.Right)
        {
            _internalValue = Math.Clamp(Value + step, Minimum, Maximum);
            ApplyValue(_internalValue);
            e.Handled = true;
        }
        else if (e.Key == Key.Down || e.Key == Key.Left)
        {
            _internalValue = Math.Clamp(Value - step, Minimum, Maximum);
            ApplyValue(_internalValue);
            e.Handled = true;
        }
        else if (e.Key == Key.PageUp)
        {
            _internalValue = Math.Clamp(Value + range * 0.1, Minimum, Maximum);
            ApplyValue(_internalValue);
            e.Handled = true;
        }
        else if (e.Key == Key.PageDown)
        {
            _internalValue = Math.Clamp(Value - range * 0.1, Minimum, Maximum);
            ApplyValue(_internalValue);
            e.Handled = true;
        }
        else if (e.Key == Key.Home)
        {
            _internalValue = Minimum;
            ApplyValue(Minimum);
            e.Handled = true;
        }
        else if (e.Key == Key.End)
        {
            _internalValue = Maximum;
            ApplyValue(Maximum);
            e.Handled = true;
        }
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (!_isDragging)
        {
            DialBody.Fill = new SolidColorBrush(Color.FromRgb(0x18, 0x18, 0x18));
            DialBody.Stroke = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (!_isDragging)
        {
            DialBody.Fill = new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x11));
            DialBody.Stroke = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22));
        }
    }
}
