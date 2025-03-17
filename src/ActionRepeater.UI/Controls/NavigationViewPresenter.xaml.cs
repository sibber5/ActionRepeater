using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace ActionRepeater.UI.Controls;

public sealed class NavigationViewPresenter : Control
{
    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register("Content", typeof(object), typeof(NavigationViewPresenter), new PropertyMetadata(null));

    public double TransitionOffset
    {
        get => (double)GetValue(TransitionOffsetProperty);
        set => SetValue(TransitionOffsetProperty, value);
    }

    public static readonly DependencyProperty TransitionOffsetProperty =
        DependencyProperty.Register("TransitionOffset", typeof(double), typeof(NavigationViewPresenter), new PropertyMetadata(200.0));

    private ContentPresenter? _contentPresenter;

    private readonly ContentThemeTransition _transition = new() { VerticalOffset = 0 };

    public NavigationViewPresenter()
    {
        this.DefaultStyleKey = typeof(NavigationViewPresenter);
        SizeChanged += NavigationViewPresenter_SizeChanged;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _contentPresenter = (ContentPresenter)GetTemplateChild("PART_ContentPresenter");
    }

    private void NavigationViewPresenter_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // when the content is resized, it plays the content transition for some reason. so this stops that
        if (e.PreviousSize != e.NewSize)
        { 
            _contentPresenter?.ContentTransitions.Clear();
        }
    }

    public void Navigate(object content, bool transitionRight, bool suppressTransition = false)
    {
        if (suppressTransition)
        {
            _contentPresenter?.ContentTransitions.Clear();
        }
        else
        {
            _transition.HorizontalOffset = transitionRight ? TransitionOffset : -TransitionOffset;
            if (_contentPresenter?.ContentTransitions.Count == 0) _contentPresenter.ContentTransitions.Add(_transition);
        }

        Content = content;
    }
}
