﻿using Windows.UI.Xaml;

namespace SMPlayer.Triggers
{
    class ControlSizeTrigger : StateTriggerBase
    {
        //private variables
        private double _minWidth = -1, _minHeight = -1;
        private FrameworkElement _targetElement;
        private double _currentHeight, _currentWidth;
        //public properties to set from XAML
        public double MinHeight
        {
            get => _minHeight;
            set => _minHeight = value;
        }
        public double MinWidth
        {
            get => _minWidth;
            set => _minWidth = value;
        }
        public FrameworkElement TargetElement
        {
            get => _targetElement;
            set
            {
                if (_targetElement != null)
                {
                    _targetElement.SizeChanged -= _targetElement_SizeChanged;
                }
                _targetElement = value;
                _targetElement.SizeChanged += _targetElement_SizeChanged;
            }
        }
        //Handle event to get current values
        private void _targetElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _currentHeight = e.NewSize.Height;
            _currentWidth = e.NewSize.Width;
            UpdateTrigger();
        }
        //Logic to evaluate and apply trigger value
        private void UpdateTrigger()
        {
            //if target is set and either minHeight or minWidth is set, proceed
            if (_targetElement != null && (_minWidth >= 0 || _minHeight >= 0))
            {
                //if both minHeight and minWidth are set, then both conditions must be satisfied
                if (_minHeight >= 0 && _minWidth >= 0)
                {
                    SetActive((_currentHeight <= _minHeight) && (_currentWidth <= _minWidth));
                }
                //if only one of them is set, then only that condition needs to be satisfied
                else if (_minWidth >= 0)
                {
                    SetActive(_currentWidth <= _minWidth);
                }
                else
                {
                    SetActive(_currentHeight <= _minHeight);
                }
            }
            else
            {
                SetActive(false);
            }
        }
    }
}
